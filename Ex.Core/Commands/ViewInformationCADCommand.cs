using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using System.Xaml;
using Line = Autodesk.Revit.DB.Line;

namespace Ex.Core
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class ViewInformationCADCommand : IExternalCommand
    {
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
            Element wallType = new FilteredElementCollector(doc).OfClass(typeof(WallType)).First(x => x.Name == "Interior - Partición 79 mm (1-hr)");
            List<Curve> curves = ObtenerCurve(doc);
            CreateWalls(doc, SimplifyListCurves(curves), wallType as WallType, GetLevelByName(doc, doc.ActiveView.Name));

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
                    Line line = curve as Line;
                    // Accedemos a la informacion
                    XYZ startPoint = curve.GetEndPoint(0);
                    XYZ endPoint = curve.GetEndPoint(1);
                    double length = curve.Length;

                    // Redondea los resultados
                    startPoint = new XYZ(Math.Round(startPoint.X, 3), Math.Round(startPoint.Y, 3), Math.Round(startPoint.Z, 3));
                    endPoint = new XYZ(Math.Round(endPoint.X, 3), Math.Round(endPoint.Y, 3), Math.Round(endPoint.Z, 3));
                    length = Math.Round(length, 3);

                    InfoViews += $"Start Point: {{{startPoint.X,-6} - {startPoint.Y,-6}}}" +
                                 $"\t\tEnd Point: {{{endPoint.X,-6} - {endPoint.Y,-6}}}" +
                                 $"\t\tLenght: {length} " +
                                 $"\t\tDirection: {line.Direction}\n";
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
                    if(curve.Length > 0.35f)
                        curves.Add(curve);
                }
            }

            return curves;
        }

        List<Curve> SimplifyListCurves(List<Curve> lines)
        {
            List<Curve> curves = new List<Curve>();
            HashSet<Curve> simplifiedCurves = new HashSet<Curve>();

            for(int i = 0; i < lines.Count; i++)
            {
                Line currentLine = lines[i] as Line;
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
                    else
                    {
                        curves.Add(currentLine);
                        simplifiedCurves.Add(currentLine);
                    }
                }
            }

            return curves;
        }

        /// <returns><see cref="Line"/> paralela a <paramref name="origin"/></returns>
        Line FindParalelLine(List<Curve> lines, int startIndex, Line origin)
        {
            for (int i = startIndex; i < lines.Count; i++)
            {
                Line line = lines[i] as Line;
                if (IsClose(MidPos(line.GetEndPoint(0), line.GetEndPoint(1)), MidPos(origin.GetEndPoint(0), origin.GetEndPoint(1))) && AreParallel(origin, line))
                    return line;
            }
            return null;
        }

        /// <summary>
        /// Crea la <see cref="Line"/> centrada entre las dos lineas que se le envian teniendo en cuenta su horientacion
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns>Devuelve una <see cref="Line"/> entre dos lineas</returns>
        Line CreateCenteredLine(Line line1, Line line2)
        {
            //Line1Points
            XYZ line1Start = line1.GetEndPoint(0);
            XYZ line1End = line1.GetEndPoint(1);
            //Line2 Points
            XYZ line2Start = line2.GetEndPoint(0);
            XYZ line2End = line2.GetEndPoint(1);

            XYZ midPoint = MidPos(line1.Origin, line2.Origin);

            Line cl;
            if (Math.Abs(line1.Direction.X) == 1) // Horizontal line
            {
                cl =  Line.CreateBound(new XYZ(MidCord(line1Start.X, line2End.X), midPoint.Y, 0),
                                        new XYZ(MidCord(line1End.X, line2Start.X), midPoint.Y,   0));
            }
            else // Vertical line
            {
                cl = Line.CreateBound(new XYZ(midPoint.X, MidCord(line1Start.Y, line2End.Y), 0),
                                        new XYZ(midPoint.X, MidCord(line1End.Y, line2Start.Y),   0));
            }

            return Line.CreateBound(cl.GetEndPoint(0) - (cl.Direction/10),
                                    cl.GetEndPoint(1) + (cl.Direction/10));
        }

        //---- Calcular Parametros ----//
        /// <returns><see langword="true"/> en caso de que las dos lineas sean paralelas</returns>
        bool AreParallel(Line line1, Line line2) => Math.Abs(Math.Abs(line1.Direction.DotProduct(line2.Direction)) - 1) < 1;
        /// <returns><see langword="true"/> en caso de que los dos puntos esten cerca</returns>
        bool IsClose(XYZ a, XYZ b) => a.DistanceTo(b) < 1;  //Saber si dos puntos estas cerca
        /// <returns>La media entre dos coordenadas</returns>
        XYZ MidPos(XYZ a, XYZ b) => (a + b) / 2;            //Saber la media entre dos posiciones
        /// <returns> La media entre dos variables</returns>
        double MidCord(double a, double b) => (a + b) / 2;  //Saber la media entre dos variables
        //----------------------------//


        public void CreateWalls(Document doc, List<Curve> curves, WallType wallType, Level level)
        {
            // Iniciar una transacción para realizar cambios en el modelo
            using (Transaction trans = new Transaction(doc, "Create Walls"))
            {
                trans.Start();

                doc.AutoJoinElements();

                XYZ offset = new XYZ(0,20,0);
                foreach (Curve curve in curves)
                {
                    // Convertir la curva a una línea
                    if (curve is Line l)
                        CreateWallFromLine(doc, l, wallType, level, offset);
                }

                doc.AutoJoinElements();

                // Completar la transacción
                trans.Commit();
            }
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

        public static string GetPath() => typeof(ViewInformationCADCommand).Namespace + "." + nameof(ViewInformationCADCommand);
    }

    class CombinationLines
    {
        List<Line> Lines = new List<Line>();
        bool IsClosed;

        public CombinationLines(List<Line> lines, bool isClosed)
        {
            Lines = lines;
            IsClosed = isClosed;
        }

        public List<XYZ> SimplifiedPoints()
        {
            List<XYZ> newPoints = new List<XYZ>();

            for (int i = 0; i < Lines.Count; i++)
            {
                Line currentLine = Lines[i];
                Line nextLine = Lines[i + 1];

                XYZ newPoint;
                XYZ offset;

                if (currentLine.Direction.X != 0)
                    offset = new XYZ(currentLine.Direction.X * 0.5f, nextLine.Direction.Y * 0.5f, 0);
                else
                    offset = new XYZ(nextLine.Direction.X * 0.5f, currentLine.Direction.Y * 0.5f, 0);

                newPoint = currentLine.GetEndPoint(1) + offset;
                newPoints.Add(newPoint);
            }

            return newPoints;
        }
    }
}
