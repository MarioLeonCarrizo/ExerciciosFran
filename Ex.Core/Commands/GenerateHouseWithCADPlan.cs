using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Ex.Ex2;
using Ex.Ex3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Shapes;
using static System.Windows.Forms.LinkLabel;
using Line = Autodesk.Revit.DB.Line;

namespace Ex.Core
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class GenerateHouseWithCADPlan : IExternalCommand
    {
        WallType wallType = null;
        FamilySymbol doorType = null;
        List<Curve> curves = new List<Curve>();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            curves = ObtenerCurve(doc);

            if(curves.Count == 0)
            {
                Message.Display("No hay lineas en la vista actual", WindowType.Warning);
                return Result.Cancelled;
            }

            //Crear Forms para seleccionar familia muros y puertas
            using (var window = new FrmSelectWalls(doc))
            {
                window.ShowDialog();

                if (window.DialogResult == DialogResult.Cancel)
                    return Result.Cancelled;

                wallType = window.GetWall();
                doorType = window.GetDoor();

                if(window.CreateCheckNote())
                {
                    //Crear Nota con toda la informacion de la lista de lineas y arcos
                    using (Transaction tx = new Transaction(doc, "Generar Text Note"))
                    {
                        tx.Start();

                        var textNoteOptions = new TextNoteOptions
                        {
                            VerticalAlignment = VerticalTextAlignment.Top,
                            HorizontalAlignment = HorizontalTextAlignment.Left,
                            TypeId = new FilteredElementCollector(doc).OfClass(typeof(TextElementType)).FirstOrDefault().Id,
                        };

                        var textNote = TextNote.Create(doc, uidoc.ActiveView.Id, new XYZ(0, -1, 0), ObtenerInformacionCAD(curves), textNoteOptions);

                        tx.Commit();
                    }
                }
            }

            //Instanciar Muros
            List<Wall> walls = CreateWalls(doc, SimplifyListLines(GetLines(curves)), GetLevelByName(doc, doc.ActiveView.Name));
            //Instanciar Puertas
            CreateDoors(doc, GetArcs(curves), walls, GetLevelByName(doc, doc.ActiveView.Name));

            return Result.Succeeded;
        }

        /// <summary>
        /// Obtiene todas las lineas y arcos creados en la vista actual
        /// </summary>
        /// <returns>Devuelve una lista con todas las lineas y arcos del Documento</returns>
        List<Curve> ObtenerCurve(Document doc)
        {
            List<Curve> curves = new List<Curve>();
            ICollection<Element> importedCADs = new FilteredElementCollector(doc).OfClass(typeof(CurveElement)).ToElements();

            foreach (CurveElement importedCAD in importedCADs)
            {
                if (importedCAD.Location is LocationCurve locationCurve)
                {
                    Curve curve = locationCurve.Curve;
                    curves.Add(curve);
                }
            }

            return curves;
        }

        string ObtenerInformacionCAD(List<Curve> curvesCAD)
        {
            string InfoViews = "";
            foreach (Curve curve in curvesCAD)
            {
                // Accedemos a la informacion
                XYZ startPoint = curve.GetEndPoint(0);
                XYZ endPoint = curve.GetEndPoint(1);
                double length = curve.Length;

                // Redondea los resultados
                startPoint = new XYZ(Math.Round(startPoint.X, 3), Math.Round(startPoint.Y, 3), Math.Round(startPoint.Z, 3));
                endPoint = new XYZ(Math.Round(endPoint.X, 3), Math.Round(endPoint.Y, 3), Math.Round(endPoint.Z, 3));
                length = Math.Round(length, 3);

                if (curve is Line line)
                    InfoViews += $"Line Wall: ";
                else if (curve is Arc arc)
                    InfoViews += $"Line Door: ";

                InfoViews += $"Start Point: {{{startPoint.X,-7}\t-\t{startPoint.Y,-7}}}  " +
                             $"\t  |\tEnd Point: {{{endPoint.X,-7}\t-\t{endPoint.Y,-7}}}  " +
                             $"\t  |\tLenght: {length} \n";
            }

            return InfoViews;
        }

        /// <summary>
        /// A partir de una lista de <see cref="Curve"/>, devuelve solo las que son de tipo <see cref="Line"/>
        /// </summary>
        List<Line> GetLines(List<Curve> curves) => curves.OfType<Line>().ToList();
        /// <summary>
        /// A partir de una lista de <see cref="Curve"/>, devuelve solo las que son de tipo <see cref="Arc"/>
        /// </summary>
        List<Arc> GetArcs(List<Curve> curves) => curves.OfType<Arc>().ToList();

        List<Line> SimplifyListLines(List<Line> lines)
        {
            List<Line> curves = new List<Line>();                       //Lineas Generadas
            HashSet<Curve> simplifiedCurves = new HashSet<Curve>();     //Lineas Detectadas Para Generar Linea

            for (int i = 0; i < lines.Count; i++)
            {
                Line currentLine = lines[i];
                if (!simplifiedCurves.Contains(lines[i]))
                {
                    //Encontrar Linea Paralela
                    Line nextLine = FindParalelLine(lines, i + 1, currentLine);

                    if (nextLine != null)
                    {
                        Line centeredLine = CreateCenteredLine(currentLine, nextLine);
                        curves.Add(centeredLine);
                        simplifiedCurves.Add(currentLine);
                        simplifiedCurves.Add(nextLine);
                    }
                }
            }

            return GroupLines(curves);
        }

        /// <returns><see cref="Line"/> paralela a <paramref name="origin"/></returns>
        Line FindParalelLine(List<Line> lines, int startIndex, Line origin)
        {
            LineInfo infoOrigin = new LineInfo(origin);
            for (int i = startIndex; i < lines.Count; i++)
            {
                Line line = lines[i];
                LineInfo lineInfo = new LineInfo(line);
                if (IsClose(lineInfo.MidPos(), infoOrigin.MidPos()) && AreParallel(origin, line))
                    return line;
            }
            return null;
        }

        /// <summary>
        /// Crea la <see cref="Line"/> centrada entre las dos lineas que se le envian teniendo en cuenta su horientacion
        /// </summary>
        /// <returns>Devuelve una <see cref="Line"/> entre dos lineas</returns>
        Line CreateCenteredLine(Line l1, Line l2)
        {
            LineInfo Info1 = new LineInfo(l1);
            LineInfo Info2 = new LineInfo(l2);

            Line cl = Line.CreateBound(new XYZ(MidCord(Info1.Left, Info2.Left), MidCord(Info1.Up, Info2.Up), 0),
                                       new XYZ(MidCord(Info1.Right, Info2.Right), MidCord(Info1.Down, Info2.Down), 0));            

            return Line.CreateBound(cl.GetEndPoint(0), cl.GetEndPoint(1));
        }

        List<Line> GroupLines(List<Line> curves)
        {
            List<Line> lines = new List<Line>();
            for(int i = 0; i < curves.Count; i++)
            {
                //Guardamos informacion de la linea actual
                Line curve = curves[i];
                XYZ curveS = curve.GetEndPoint(0);
                XYZ curveE = curve.GetEndPoint(1);

                //Buscamos si hay una linea cerca, pero descartamos si hace esquina
                Line closeLineS = curves.FirstOrDefault(x => curve.Distance(x.Project(curveS).XYZPoint) < 0.1f &&
                                               (curveS.DistanceTo(x.GetEndPoint(0)) < 0.001f || curveS.DistanceTo(x.GetEndPoint(1)) < 0.001f) && x != curve);
                Line closeLineE = curves.FirstOrDefault(x => curve.Distance(x.Project(curveE).XYZPoint) < 0.1f &&
                                               (curveE.DistanceTo(x.GetEndPoint(0)) < 0.001f || curveE.DistanceTo(x.GetEndPoint(1)) < 0.001f) && x != curve);

                //Calculamos la distancia entre la linea actual y la cercana
                double distanceS = closeLineS != null ? Math.Round(curve.Distance(closeLineS.Project(curveS).XYZPoint), 2) : 1;
                double distanceE = closeLineE != null ? Math.Round(curve.Distance(closeLineE.Project(curveE).XYZPoint), 2) : 1;

                //Calculamos la direccion y distancia que tiene que recorrer la linea para unirse con el muro
                XYZ offsetS = distanceS != 0 ? curves[i].Direction / distanceS : XYZ.Zero;
                XYZ offsetE = distanceE != 0 ? curves[i].Direction / distanceE : XYZ.Zero;

                //Creamos una nueva linea apartir de la anterior sumandole los offsets
                lines.Add(Line.CreateBound(curveS - offsetS/1.8f, curveE + offsetE/1.8f));
            }

            return lines;
        }

        //-------- Calcular Parametros --------//
        /// <returns><see langword="true"/> en caso de que las dos lineas sean paralelas</returns>
        bool AreParallel(Line line1, Line line2) => Math.Abs(Math.Abs(line1.Direction.DotProduct(line2.Direction)) - 1) < 1;
        /// <returns><see langword="true"/> en caso de que los dos puntos esten cerca</returns>
        bool IsClose(XYZ a, XYZ b) => a.DistanceTo(b) < 3;
        /// <returns> La media entre dos variables</returns>
        double MidCord(double a, double b) => (a + b) / 2;
        //-------------------------------------//

        /// <summary>
        /// Crea muros a partir de la lineas en el nivel especificado
        /// </summary>
        /// <param name="doc">Documento actual</param>
        /// <param name="lines">Lista de lineas de referencia</param>
        /// <param name="level">Nivel en el que se generaran</param>
        /// <returns>Devuelve una lista de <see cref="Wall"/> instanciada</returns>
        List<Wall> CreateWalls(Document doc, List<Line> lines, Level level)
        {
            List<Wall> walls = new List<Wall>();
            // Iniciar una transacción para realizar cambios en el modelo
            using (Transaction trans = new Transaction(doc, "Create Walls"))
            {
                trans.Start();

                XYZ offset = new XYZ(0, 60, 0);
                foreach (Line l in lines)
                {
                    Wall wall = CreateWallFromLine(doc, l, wallType, level, offset);
                    walls.Add(wall);
                }

                doc.AutoJoinElements();
                doc.Regenerate();

                trans.Commit();
            }

            return walls;
        }

        /// <summary>
        /// Devuelve y crea un <see cref="Wall"/> segun los paramentros que se le envie
        /// </summary>
        Wall CreateWallFromLine(Document doc, Line line, WallType wallType, Level level, XYZ offset)
        {
            Line offsetLine = Line.CreateBound(line.GetEndPoint(0).Add(offset), line.GetEndPoint(1).Add(offset));
            return Wall.Create(doc, offsetLine, wallType.Id, level.Id, 10.0, 0, false, false);
        }

        /// <summary>
        /// Genera puertas a partir de una lista de <see cref="Arc"/>
        /// </summary>
        /// <param name="doc">Documento en el que se generaran las puertas</param>
        /// <param name="arcs">Lista de Arcos con los que generara las puertas</param>
        /// <param name="walls">Lista de Muros en los que generara las puertas</param>
        /// <param name="level">Vista colocara las puertas</param>
        public void CreateDoors(Document doc, List<Arc> arcs, List<Wall> walls, Level level)
        {
            using (Transaction trans = new Transaction(doc, "Create Doors"))
            {
                trans.Start();

                XYZ offset = new XYZ(0, 60, 0);
                foreach (Arc arc in arcs)
                {
                    //Guardamos toda la informacion necessario del arco seleccionado
                    XYZ center = new XYZ(Math.Round(arc.Center.X, 3), Math.Round(arc.Center.Y, 3), Math.Round(arc.Center.Z, 3));
                    XYZ dir = CalcularDireccion(arc.GetEndPoint(0), arc.GetEndPoint(1), center, Math.Round(arc.Radius, 3));
                    XYZ middlePoint = arc.Center + new XYZ((arc.Radius / 2) * dir.X, (arc.Radius / 2) * dir.Y, 0) + offset;

                    foreach (Wall wall in walls)
                    {
                        //Obtenemos los datos de posicion del muro y miramos si esta cerca de la puerta
                        LocationCurve locationCurve = wall.Location as LocationCurve;
                        double distancia = locationCurve.Curve.Distance(middlePoint);
                        if (distancia < 3)
                        {
                            try
                            {
                                //Intentamos crear la puerta en el muro
                                FamilyInstance door = CreateDoorFromLine(doc, middlePoint, doorType, wall, level);
                                XYZ wallDir = wall.Orientation;
                                XYZ doorDir = door.FacingOrientation;
                                //Rotamos la puerta en base a la direccion del muro y la direccion del arco
                                if (Math.Abs(wallDir.X) == 1)            //Puerta Vertical 
                                {
                                    if (dir.X == -1) door.flipFacing();
                                    if (dir.Y == -1) door.flipHand();
                                }
                                else if (Math.Abs(wallDir.Y) == 1)       //Puerta Vertical
                                {
                                    if (dir.X == 1) door.flipHand();
                                    if (dir.Y == -1) door.flipFacing();
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
        FamilyInstance CreateDoorFromLine(Document doc, XYZ middlePoint, FamilySymbol doorSymbol, Element wall, Level level)
        {
            return doc.Create.NewFamilyInstance(middlePoint, doorSymbol, wall, level, StructuralType.NonStructural);
        }

        /// <returns>Devuelve la dirreccion del arco</returns>
        XYZ CalcularDireccion(XYZ a, XYZ b, XYZ center, double radius)
        {
            XYZ aPoint = (a - center) / radius;
            XYZ bPoint = (b - center) / radius;

            return new XYZ(Math.Round(aPoint.X), Math.Round(aPoint.Y), Math.Round(aPoint.Z)) +
                   new XYZ(Math.Round(bPoint.X), Math.Round(bPoint.Y), Math.Round(bPoint.Z));
        }

        /// <summary>
        /// Devuelve el <see cref="Autodesk.Revit.DB.Level"/> segun <paramref name="levelName"/>
        /// </summary>
        Level GetLevelByName(Document doc, string levelName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> levels = collector.OfClass(typeof(Level)).ToElements();

            foreach (Element level in levels)
                if (level.Name == levelName)
                    return level as Level;

            return null; // Si no se encuentra ningún nivel con el nombre dado
        }

        public static string GetPath() => typeof(GenerateHouseWithCADPlan).Namespace + "." + nameof(GenerateHouseWithCADPlan);
    }
}
