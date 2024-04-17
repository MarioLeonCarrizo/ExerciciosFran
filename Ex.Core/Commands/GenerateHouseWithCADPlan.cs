using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using static System.Windows.Forms.LinkLabel;
using Line = Autodesk.Revit.DB.Line;

namespace Ex.Core
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class GenerateHouseWithCADPlan : IExternalCommand
    {
        List<Curve> curves = new List<Curve>();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            using (Transaction tx = new Transaction(doc, "Generar ViewPorts"))
            {
                tx.Start();

                var textNoteOptions = new TextNoteOptions
                {
                    VerticalAlignment = VerticalTextAlignment.Top,
                    HorizontalAlignment = HorizontalTextAlignment.Left,
                    TypeId = new FilteredElementCollector(doc).OfClass(typeof(TextElementType)).FirstOrDefault().Id,
                };

                var textNote = TextNote.Create(doc, uidoc.ActiveView.Id, new XYZ(0, -1, 0), ObtenerInformacionCAD(doc), textNoteOptions);

                tx.Commit();
            }
            Element wallType = new FilteredElementCollector(doc).OfClass(typeof(WallType)).FirstOrDefault(x => x.Name == "Genérico - 300 mm");
            FamilySymbol doorType = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
                                                                     .OfCategory(BuiltInCategory.OST_Doors)
                                                                     .OfType<FamilySymbol>()
                                                                     .FirstOrDefault(x => x.Name == "0915 x 2134mm");

            curves = ObtenerCurve(doc);
            List<Wall> walls = CreateWalls(doc, SimplifyListLines(GetLines(curves)), wallType as WallType, GetLevelByName(doc, doc.ActiveView.Name));
            CreateDoors(doc, GetArcs(curves), walls, doorType, GetLevelByName(doc, doc.ActiveView.Name));

            return Result.Succeeded;
        }

        string ObtenerInformacionCAD(Document doc)
        {
            string InfoViews = "";
            // Obtener todos los elementos importados de CAD en el documento
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> importedCADs = collector.OfClass(typeof(CurveElement)).ToElements();

            foreach (CurveElement importedCAD in importedCADs)
            {
                if (importedCAD.Location is LocationCurve locationCurve)
                {
                    Curve curve = locationCurve.Curve;
                    // Accedemos a la informacion
                    XYZ startPoint = curve.GetEndPoint(0);
                    XYZ endPoint = curve.GetEndPoint(1);
                    double length = curve.Length;

                    // Redondea los resultados
                    startPoint = new XYZ(Math.Round(startPoint.X, 3), Math.Round(startPoint.Y, 3), Math.Round(startPoint.Z, 3));
                    endPoint = new XYZ(Math.Round(endPoint.X, 3), Math.Round(endPoint.Y, 3), Math.Round(endPoint.Z, 3));
                    length = Math.Round(length, 3);

                    if (curve is Line line)
                    {
                        InfoViews += $"Line Wall: Start Point: {{{startPoint.X,-7}\t-\t{startPoint.Y,-7}}}  " +
                                     $"\t  |\tEnd Point: {{{endPoint.X,-7}\t-\t{endPoint.Y,-7}}}  " +
                                     $"\t  |\tLenght: {length}  " +
                                     $"\t  |\tDirection: {Math.Round(line.Direction.X, 1)},{Math.Round(line.Direction.Y, 1)}\n";
                    }
                    else if (curve is Arc arc)
                    {
                        InfoViews += $"Line Door: Start Point: {{{startPoint.X,-7}\t-\t{startPoint.Y,-7}}}  " +
                                     $"\t  |\tEnd Point: {{{endPoint.X,-7}\t-\t{endPoint.Y,-7}}}  " +
                                     $"\t  |\tLenght: {length} \n";
                    }
                }
            }

            return InfoViews;
        }

        List<Curve> ObtenerCurve(Document doc)
        {
            List<Curve> curves = new List<Curve>();
            // Obtener todos los elementos importados de CAD en el documento
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> importedCADs = collector.OfClass(typeof(CurveElement)).ToElements();

            foreach (CurveElement importedCAD in importedCADs)
            {
                if (importedCAD.Location is LocationCurve locationCurve)
                {
                    Curve curve = locationCurve.Curve;
                    if (curve.Length > 0.35f)
                        curves.Add(curve);
                }
            }

            return curves;
        }

        List<Line> GetLines(List<Curve> curves) => curves.OfType<Line>().ToList();
        List<Arc> GetArcs(List<Curve> curves) => curves.OfType<Arc>().ToList();

        List<Line> SimplifyListLines(List<Line> lines)
        {
            List<Line> curves = new List<Line>();
            HashSet<Curve> simplifiedCurves = new HashSet<Curve>();

            for (int i = 0; i < lines.Count; i++)
            {
                Line currentLine = lines[i];
                if (!simplifiedCurves.Contains(lines[i]))
                {
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

            return curves;
        }

        /// <returns><see cref="Line"/> paralela a <paramref name="origin"/></returns>
        Line FindParalelLine(List<Line> lines, int startIndex, Line origin)
        {
            LineInfo infoOrigin = new LineInfo(origin);
            for (int i = startIndex; i < lines.Count; i++)
            {
                Line line = lines[i];
                if (IsClose(MidPos(line.GetEndPoint(0), line.GetEndPoint(1)), MidPos(infoOrigin.Start, infoOrigin.End)) && AreParallel(origin, line))
                    return line;
            }
            return null;
        }

        /// <summary>
        /// Crea la <see cref="Line"/> centrada entre las dos lineas que se le envian teniendo en cuenta su horientacion
        /// </summary>
        /// <returns>Devuelve una <see cref="Line"/> entre dos lineas</returns>
        Line CreateCenteredLine(Line line1, Line line2)
        {
            LineInfo Info1 = new LineInfo(line1);
            LineInfo Info2 = new LineInfo(line2);

            Line cl;
            cl = Line.CreateBound(new XYZ(MidCord(Info1.Left, Info2.Left), MidCord(Info1.Up, Info2.Up), 0),
                                  new XYZ(MidCord(Info1.Right, Info2.Right), MidCord(Info1.Down, Info2.Down), 0));

            return Line.CreateBound(cl.GetEndPoint(0) - (cl.Direction / 4),
                                    cl.GetEndPoint(1) + (cl.Direction / 4));
        }

        //-------- Calcular Parametros --------//
        /// <returns><see langword="true"/> en caso de que las dos lineas sean paralelas</returns>
        bool AreParallel(Line line1, Line line2) => Math.Abs(Math.Abs(line1.Direction.DotProduct(line2.Direction)) - 1) < 1;
        /// <returns><see langword="true"/> en caso de que los dos puntos esten cerca</returns>
        bool IsClose(XYZ a, XYZ b) => a.DistanceTo(b) < 3;
        /// <returns>La media entre dos coordenadas</returns>
        XYZ MidPos(XYZ a, XYZ b) => (a + b) / 2;
        /// <returns> La media entre dos variables</returns>
        double MidCord(double a, double b) => (a + b) / 2;
        //-------------------------------------//

        List<Wall> CreateWalls(Document doc, List<Line> lines, WallType wallType, Level level)
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

        public void CreateDoors(Document doc, List<Arc> arcs, List<Wall> walls ,FamilySymbol doorSymbol, Level level)
        {
            using (Transaction trans = new Transaction(doc, "Create Doors"))
            {
                trans.Start();

                XYZ offset = new XYZ(0, 60, 0);
                foreach (Arc arc in arcs)
                {
                    FamilyInstance door;
                    XYZ center = new XYZ(Math.Round(arc.Center.X, 3), Math.Round(arc.Center.Y, 3), Math.Round(arc.Center.Z, 3));
                    XYZ dir = CalcularDireccion(arc.GetEndPoint(0), arc.GetEndPoint(1), center, Math.Round(arc.Radius, 3));
                    XYZ middlePoint = arc.Center + new XYZ((arc.Radius / 2) * dir.X, (arc.Radius / 2) * dir.Y, 0) + offset;

                    foreach (Wall wall in walls)
                    {
                        LocationCurve locationCurve = wall.Location as LocationCurve;
                        Curve curveWall = locationCurve.Curve;
                        double distancia = curveWall.Distance(middlePoint);
                        if (distancia < 3)
                        {
                            door = CreateDoorFromLine(doc, middlePoint, doorSymbol, wall, level);
                            XYZ wallDir = wall.Orientation;
                            XYZ doorDir = door.FacingOrientation;
                            if(Math.Abs(wallDir.X) == 1)            //Puerta Vertical 
                            {
                                if(dir.X == -1) door.flipFacing();
                                if(dir.Y == -1) door.flipHand();
                            }
                            else if(Math.Abs(wallDir.Y) == 1)       //Puerta Vertical
                            {
                                if (dir.X == 1) door.flipHand();
                                if (dir.Y == -1) door.flipFacing();
                            }
                            break;
                        }
                    }
                }

                trans.Commit();
            }
        }

        FamilyInstance CreateDoorFromLine(Document doc, XYZ middlePoint, FamilySymbol doorSymbol, Element wall, Level level)
        {
            return doc.Create.NewFamilyInstance(middlePoint, doorSymbol, wall, level, StructuralType.NonStructural);
        }

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
