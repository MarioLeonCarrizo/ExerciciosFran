using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Ex.Ex3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Ex.Core
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class GeneradorMasterPlanos : IExternalCommand
    {
        Dictionary<double, string> wallsFamily = new Dictionary<double, string>();
        Dictionary<double, string> doorsFamily = new Dictionary<double, string>();
        Dictionary<double, string> wallsArcFamily = new Dictionary<double, string>();
        Dictionary<double, string> windowsFamily = new Dictionary<double, string>();

        List<Curve> curves = new List<Curve>();

        Document doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            Level level = CalculationManager.GetLevelByName(doc, doc.ActiveView.Name);

            curves = ObtenerCurve(doc);

            if (curves.Count == 0)
            {
                Message.Display("No hay lineas en la vista actual", WindowType.Warning);
                return Result.Cancelled;
            }

            List<Line> lines = curves.OfType<Line>().ToList();
            List<Arc> arcs = curves.OfType<Arc>().ToList();
            //-----=== Detectar Muros ===-----//
            //Obtenemos las lineas que pueden formar un muro y las que no
            SimplificarLineas(lines, out List<LineInfo> simplifiedLines, out List<Line> notSimpliedLine);
            //Juntamos las lineas de los muros para que se conecten
            CalculationManager.GroupLines(ref simplifiedLines);

            //-----=== Detectar Puertas ===-----//
            List<ArcInfo> doors = CalculationManager.SimplifyListArcs(arcs, ref notSimpliedLine, out List<ArcInfo> notSimpliedArc);

            //-----=== Detectar Muros Curvos ===-----//
            List<ArcInfo> wallsArc = FindWallArc(notSimpliedArc);

            //-----=== Detectar Ventanas ===-----//
            FindWindows(notSimpliedLine, out List<LineInfo> windows);

            //Crear Forms para ver la informacion optenida
            using (var window = new FrmInfoView(doc, simplifiedLines, windows, doors, wallsArc))
            {
                window.ShowDialog();

                if (window.DialogResult == DialogResult.Cancel)
                    return Result.Cancelled;

                wallsFamily = window.wallsFamily;
                doorsFamily = window.doorsFamily;
                windowsFamily = window.windowsFamily;
                wallsArcFamily = window.wallsArcFamily;
            }

            //-----=== Instanciar Objetos ===-----//
            List<Wall> walls = CreateWalls(doc, simplifiedLines, level);
            CreateDoors(doc, doors, walls, level);
            List<Wall> wallArcs = CreateWallsArc(doc, wallsArc, level);
            CreateWindows(doc, windows, walls, level);

            return Result.Succeeded;
        }

        /// <summary>
        /// Obtiene todas las lineas y arcos creados en la vista actual
        /// </summary>
        List<Curve> ObtenerCurve(Document doc)
        {
            List<Curve> curves = new List<Curve>();
            ICollection<Element> importedCADs = new FilteredElementCollector(doc).OfClass(typeof(CurveElement)).ToElements();

            foreach (CurveElement importedCAD in importedCADs)
            {
                if (importedCAD.Location is LocationCurve locationCurve)
                    curves.Add(locationCurve.Curve);
            }

            return curves;
        }

        void SimplificarLineas(List<Line> lines, out List<LineInfo> linesSimplified, out List<Line> linesNotSimplified)
        {
            linesNotSimplified = new List<Line>();
            List<Line> curves  = new List<Line>();
            linesSimplified    = new List<LineInfo>();

            for (int i = 0; i < lines.Count; i++)
            {
                Line currentLine = lines[i];
                if (!curves.Contains(lines[i]))
                {
                    //Encontrar Linea Paralela
                    CalculationManager.FindParalelLine(lines, i + 1, currentLine, out Line nextLine, out double dist);
                    if (nextLine != null)
                    {
                        LineInfo centeredLine = CalculationManager.CreateCenteredLine(currentLine, nextLine, dist);
                        linesSimplified.Add(centeredLine);
                        curves.Add(currentLine);
                        curves.Add(nextLine);
                    }
                }
            }

            foreach (Line line in lines)
            {
                if(!curves.Contains(line))
                    linesNotSimplified.Add(line);
            }
        }

        List<Wall> CreateWalls(Document doc, List<LineInfo> lines, Level level)
        {
            List<Wall> walls = new List<Wall>();
            // Iniciar una transacción para realizar cambios en el modelo
            using (Transaction trans = new Transaction(doc, "Create Walls"))
            {
                trans.Start();

                XYZ offset = new XYZ(0, 60, 0);
                foreach (LineInfo l in lines)
                {
                    WallType wallType = new FilteredElementCollector(doc)
                        .OfClass(typeof(WallType))
                        .Cast<WallType>()
                        .FirstOrDefault(x => x.Name == wallsFamily[l.Width]);

                    Wall wall = CreateWallFromLine(doc, l, wallType, level, offset);
                    walls.Add(wall);
                }

                doc.AutoJoinElements();
                doc.Regenerate();

                trans.Commit();
            }

            return walls;
        }

        /// <summary>Devuelve y crea un <see cref="Wall"/> segun los paramentros que se le envie</summary>
        Wall CreateWallFromLine(Document doc, LineInfo line, WallType wallType, Level level, XYZ offset) => Wall.Create(doc, Line.CreateBound(line.Start.Add(offset), line.End.Add(offset)), wallType.Id, level.Id, 10.0, 0, false, false);

        /// <summary>
        /// Genera puertas a partir de una lista de <see cref="Arc"/>
        /// </summary>
        /// <param name="doc">Documento en el que se generaran las puertas</param>
        /// <param name="arcs">Lista de Arcos con los que generara las puertas</param>
        /// <param name="walls">Lista de Muros en los que generara las puertas</param>
        /// <param name="level">Vista colocara las puertas</param>
        public void CreateDoors(Document doc, List<ArcInfo> arcs, List<Wall> walls, Level level)
        {
            using (Transaction trans = new Transaction(doc, "Create Doors"))
            {
                trans.Start();

                XYZ offset = new XYZ(0, 60, 0);
                foreach (ArcInfo arc in arcs)
                {
                    foreach (Wall wall in walls)
                    {
                        //Obtenemos los datos de posicion del muro y miramos si esta cerca de la puerta
                        LocationCurve locationCurve = wall.Location as LocationCurve;
                        double distancia = locationCurve.Curve.Distance(arc.middlePoint.Add(offset));
                        if (distancia < 3)
                        {
                            try
                            {
                                //Intentamos crear la puerta en el muro
                                FamilySymbol doorType = new FilteredElementCollector(doc)
                                    .OfClass(typeof(FamilySymbol))
                                    .OfCategory(BuiltInCategory.OST_Doors)
                                    .Cast<FamilySymbol>()
                                    .FirstOrDefault(x => x.Name == doorsFamily[arc.radius]);

                                FamilyInstance door = CreateDoorFromLine(doc, arc.middlePoint.Add(offset), doorType, wall, level);
                                XYZ wallDir = wall.Orientation;
                                XYZ doorDir = door.FacingOrientation;
                                //Rotamos la puerta en base a la direccion del muro y la direccion del arco
                                if (Math.Abs(wallDir.X) == 1)            //Puerta Horizontal 
                                {
                                    if (arc.dir.X == -1) door.flipFacing();
                                    if (arc.dir.Y == -1) door.flipHand();
                                }
                                else if (Math.Abs(wallDir.Y) == 1)       //Puerta Vertical
                                {
                                    if (arc.dir.X == 1) door.flipHand();
                                    if (arc.dir.Y == -1) door.flipFacing();
                                }
                                break;
                            }
                            catch
                            {
                                Message.Display("La Familia de la puerta no ha sido cargada en el proyecto", WindowType.Error);
                                return;
                            }
                        }
                    }
                }

                trans.Commit();
            }
        }

        /// <returns>Devuelve una Instancia de la Puerta Creada</returns>
        FamilyInstance CreateDoorFromLine(Document doc, XYZ middlePoint, FamilySymbol doorSymbol, Element wall, Level level) => doc.Create.NewFamilyInstance(middlePoint, doorSymbol, wall, level, StructuralType.NonStructural);

        List<ArcInfo> FindWallArc(List<ArcInfo> arcs)
        {
            List<ArcInfo> WallArcs = new List<ArcInfo>();
            List<ArcInfo> arcSimplified = new List<ArcInfo>();
            foreach (ArcInfo a in arcs)
            {
                ArcInfo nextArc = arcs.FirstOrDefault(x => a.center.DistanceTo(x.center) < 0.1f && x != a);

                if(nextArc != null && (!arcSimplified.Contains(a) && !arcSimplified.Contains(nextArc)))
                {
                    double radius = nextArc.radius + ((a.radius - nextArc.radius)/2);
                    //Arc newArc = Arc.Create(new Plane(),radius, a.StartAngle, a.EndAngle);
                    XYZ End0 = (a.arc.GetEndPoint(0) + nextArc.arc.GetEndPoint(0)) / 2;
                    XYZ End1 = (a.arc.GetEndPoint(1) + nextArc.arc.GetEndPoint(1)) / 2;
                    XYZ Center = (a.center + nextArc.center) / 2;

                    Arc newArc = Arc.Create(End0, End1, Center.Add(new XYZ(0, radius, 0)));
                    WallArcs.Add(new ArcInfo(newArc, Math.Abs(a.radius - nextArc.radius)));
                    arcSimplified.Add(nextArc);
                    arcSimplified.Add(a);
                }
            }
            return WallArcs;
        }

        List<Wall> CreateWallsArc(Document doc, List<ArcInfo> lines, Level level)
        {
            List<Wall> walls = new List<Wall>();
            // Iniciar una transacción para realizar cambios en el modelo
            using (Transaction trans = new Transaction(doc, "Create Walls"))
            {
                trans.Start();

                doc.AutoJoinElements();

                XYZ offset = new XYZ(0, 60, 0);
                foreach (ArcInfo l in lines)
                {
                    WallType wallType = new FilteredElementCollector(doc)
                        .OfClass(typeof(WallType))
                        .Cast<WallType>()
                        .FirstOrDefault(x => x.Name == wallsArcFamily[l.width]);

                    Wall wall = CreateWallFromArc(doc, l, wallType, level, offset);
                    walls.Add(wall);
                }

                doc.AutoJoinElements();
                doc.Regenerate();

                trans.Commit();
            }

            return walls;
        }

        Wall CreateWallFromArc(Document doc, ArcInfo arc, WallType wallType, Level level, XYZ offset)
        {
            int dirX = arc.dir.X < 0 ? -1 : 1;
            int dirY = arc.dir.Y < 0 ? -1 : 1;

            Arc offsetArc = Arc.Create(arc.center.Add(offset), arc.radius, arc.SAngle, arc.EAngle, new XYZ(dirX, 0, 0), new XYZ(0, dirY, 0));

            return Wall.Create(doc, offsetArc, wallType.Id, level.Id, 10.0, 0, false, false);
        }

        #region ----- Logica Ventana -----
        void FindWindows(List<Line> lines, out List<LineInfo> windows)
        {
            List<Line> curves = new List<Line>();
            windows = new List<LineInfo>();
            for (int i = 0; i < lines.Count; i++)
            {
                Line currentLine = lines[i];
                if (!curves.Contains(currentLine))
                {
                    //Encontrar Linea Paralela
                    CalculationManager.FindParalelLine(lines, i + 1, currentLine, out Line nextLine, out double dist, 20);
                    if (nextLine != null)
                    {
                        LineInfo Info1 = new LineInfo(currentLine);
                        LineInfo Info2 = new LineInfo(nextLine);

                        Line window = Line.CreateBound(Info1.MidPos(), Info2.MidPos());
                        windows.Add(new LineInfo(window, dist));
                        curves.Add(currentLine);
                        curves.Add(nextLine);
                    }
                }
            }
        }

        void CreateWindows(Document doc, List<LineInfo> windows, List<Wall> walls, Level level)
        {
            List<FamilyInstance> generatedWindows = new List<FamilyInstance>();
            using (Transaction trans = new Transaction(doc, "Create Windows"))
            {
                trans.Start();

                XYZ offset = new XYZ(0, 60, 0);
                foreach (LineInfo win in windows)
                {
                    foreach (Wall wall in walls)
                    {
                        //Obtenemos los datos de posicion del muro y miramos si esta cerca de la ventana
                        LocationCurve locationCurve = wall.Location as LocationCurve;
                        double distancia = locationCurve.Curve.Distance(win.MidPos().Add(offset));
                        if (distancia < 3)
                        {
                            try
                            {
                                //Intentamos crear la ventana en el muro
                                FamilySymbol windowsType = new FilteredElementCollector(doc)
                                    .OfClass(typeof(FamilySymbol))
                                    .OfCategory(BuiltInCategory.OST_Windows)
                                    .Cast<FamilySymbol>()
                                    .FirstOrDefault(x => x.Name == windowsFamily[win.Width]);

                                double Altura = windowsType.LookupParameter("Altura").AsDouble();

                                generatedWindows.Add(CreateWindowsFromLine(doc, win.MidPos().Add(offset + new XYZ(0,0,Altura)), windowsType, wall, level));
                                break;
                            }
                            catch
                            {
                                Message.Display("La Familia de la ventana no ha sido cargada en el proyecto", WindowType.Error);
                                return;
                            }
                        }
                    }
                }

                trans.Commit();
            }
        }

        FamilyInstance CreateWindowsFromLine(Document doc, XYZ middlePoint, FamilySymbol windowsSymbol, Element wall, Level level) => doc.Create.NewFamilyInstance(middlePoint, windowsSymbol, wall, level, StructuralType.NonStructural);

        #endregion
        public static string GetPath() => typeof(GeneradorMasterPlanos).Namespace + "." + nameof(GeneradorMasterPlanos);
    }
}
