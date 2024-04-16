using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using System.Xaml;
using static System.Windows.Forms.LinkLabel;
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
                    else if(curve is Arc arc)
                    {
                        InfoViews += $"Line Arc: Start Point: {{{startPoint.X,-7}\t-\t{startPoint.Y,-7}}}  " +
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
                    if(curve.Length > 0.35f)
                        curves.Add(curve);
                }
            }

            return curves;
        }

        List<Curve> SimplifyListCurves(List<Curve> xLines)
        {
            List<Curve> curves = new List<Curve>();
            HashSet<Curve> simplifiedCurves = new HashSet<Curve>();

            List<Line> lines = xLines.OfType<Line>().ToList();

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
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns>Devuelve una <see cref="Line"/> entre dos lineas</returns>
        Line CreateCenteredLine(Line line1, Line line2)
        {
            LineInfo Info1 = new LineInfo(line1);
            LineInfo Info2 = new LineInfo(line2);

            XYZ midPoint = MidPos(line1.Origin, line2.Origin);

            Line cl;
            cl = Line.CreateBound(new XYZ(MidCord(Info1.Left, Info2.Left), MidCord(Info1.Up, Info2.Up), 0),
                                  new XYZ(MidCord(Info1.Right, Info2.Right), MidCord(Info1.Down, Info2.Down), 0));
            //if (Math.Abs(Math.Round(line1.Direction.X, 1)) == 1f) // Horizontal line
            //{
            //    cl = Line.CreateBound(new XYZ(MidCord(Info1.Left, Info2.Left), midPoint.Y, 0),
            //                           new XYZ(MidCord(Info1.Right, Info2.Right), midPoint.Y, 0));
            //}
            //else if (Math.Abs(Math.Round(line1.Direction.Y, 1)) == 1f) // Vertical line
            //{
            //    cl = Line.CreateBound(new XYZ(midPoint.X, MidCord(Info1.Up, Info2.Up), 0),
            //                          new XYZ(midPoint.X, MidCord(Info1.Down, Info2.Down), 0));
            //}
            //else
            //{
            //    cl = Line.CreateBound(new XYZ(MidCord(Info1.Left, Info2.Left), MidCord(Info1.Up, Info2.Up), 0),
            //                          new XYZ(MidCord(Info1.Right, Info2.Right), MidCord(Info1.Down, Info2.Down), 0));
            //}

            return Line.CreateBound(cl.GetEndPoint(0) - (cl.Direction/20),
                                    cl.GetEndPoint(1) + (cl.Direction/20));
        }

        //-------- Calcular Parametros --------//
        /// <returns><see langword="true"/> en caso de que las dos lineas sean paralelas</returns>
        bool AreParallel(Line line1, Line line2) => Math.Abs(Math.Abs(line1.Direction.DotProduct(line2.Direction)) - 1) < 1;
        /// <returns><see langword="true"/> en caso de que los dos puntos esten cerca</returns>
        bool IsClose(XYZ a, XYZ b) => a.DistanceTo(b) < 1;  //Saber si dos puntos estas cerca
        /// <returns>La media entre dos coordenadas</returns>
        XYZ MidPos(XYZ a, XYZ b) => (a + b) / 2;            //Saber la media entre dos posiciones
        /// <returns> La media entre dos variables</returns>
        double MidCord(double a, double b) => (a + b) / 2;  //Saber la media entre dos variables
        //-------------------------------------//


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

    class LineInfo
    {
        public XYZ Start;
        public XYZ End;

        public double Up;
        public double Down;
        public double Left;
        public double Right;

        public LineInfo(Line line)
        {
            Start = line.GetEndPoint(0);
            End = line.GetEndPoint(1);

            Up = Math.Max(Start.Y, End.Y);
            Down = Math.Min(Start.Y, End.Y);
            Left = Math.Min(Start.X, End.X);
            Right = Math.Max(Start.X, End.X);
        }
    }
}
