using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;

namespace Ex.Ex3
{
    public static class CalculationManager
    {
        //------------- Calcular Parametros -------------//
        /// <returns><see langword="true"/> en caso de que los dos puntos esten cerca</returns>
        public static bool IsClose(XYZ a, XYZ b, out double dist, double maxdist = 2)
        {
            dist = a.DistanceTo(b);
            return dist < maxdist;
        }
        /// <returns> La media entre dos variables</returns>
        static double MidCord(double a, double b) => (a + b) / 2;
        //-----------------------------------------------//

        #region ---------------------------------- CALCULOS - MUROS ----------------------------------
        /// <returns><see cref="Line"/> paralela a <paramref name="origin"/></returns>
        public static void FindParalelLine(List<Line> lines, int startIndex, Line origin, out Line ParalelLine, out double dist, double maxdist = 2)
        {
            LineInfo infoOrigin = new LineInfo(origin);
            dist = -1;
            ParalelLine = null;
            for (int i = startIndex; i < lines.Count; i++)
            {
                Line line = lines[i];
                LineInfo lineInfo = new LineInfo(line);
                if (IsClose(lineInfo.MidPos(), infoOrigin.line.Project(lineInfo.MidPos()).XYZPoint, out dist, maxdist) && infoOrigin.AreParallel(line))
                {
                    ParalelLine = line;
                    return;
                }
            }
        }

        /// <summary>
        /// Crea la <see cref="Line"/> centrada entre las dos lineas que se le envian teniendo en cuenta su horientacion
        /// </summary>
        public static LineInfo CreateCenteredLine(Line l1, Line l2, double dist)
        {
            LineInfo Info1 = new LineInfo(l1);
            LineInfo Info2 = new LineInfo(l2);

            Line cl;
            if (Info1.Direction.X < 0 && Info1.Direction.Y < 0 || Info1.Direction.X > 0 && Info1.Direction.Y > 0)
            {
                cl = Line.CreateBound(new XYZ(MidCord(Info1.Left, Info2.Left), MidCord(Info1.Down, Info2.Down), 0),
                                       new XYZ(MidCord(Info1.Right, Info2.Right), MidCord(Info1.Up, Info2.Up), 0));
            }
            else
            {
                cl = Line.CreateBound(new XYZ(MidCord(Info1.Left, Info2.Left), MidCord(Info1.Up, Info2.Up), 0),
                                       new XYZ(MidCord(Info1.Right, Info2.Right), MidCord(Info1.Down, Info2.Down), 0));
            }            

            return new LineInfo(Line.CreateBound(cl.GetEndPoint(0), cl.GetEndPoint(1)), dist);
        }

        public static void GroupLines(ref List<LineInfo> curves)
        {
            List<LineInfo> lines = new List<LineInfo>();
            for (int i = 0; i < curves.Count; i++)
            {
                //Guardamos informacion de la linea actual
                LineInfo curve = curves[i];
                XYZ curveS = curve.Start;
                XYZ curveE = curve.End;

                XYZ offsetS = ModifyLine(curves, curve.line, curveS, i);
                XYZ offsetE = ModifyLine(curves, curve.line, curveE, i);

                //Creamos una nueva linea apartir de la anterior sumandole los offsets
                lines.Add(new LineInfo(Line.CreateBound(curveS - offsetS / 1.8f, curveE + offsetE / 1.8f), curve.Width));
            }

            curves = lines;
        }

        static XYZ ModifyLine(List<LineInfo> curves, Line curve, XYZ point, int i)
        {
            //Buscamos si hay una linea cerca, pero descartamos si hace esquina
            LineInfo closeLine = curves.FirstOrDefault(x => curve.Distance(x.line.Project(point).XYZPoint) < 0.1f &&
                                            //Buscamos que no se connecte y que no sea el mismo
                                            (point.DistanceTo(x.Start) < 0.001f || point.DistanceTo(x.End) < 0.001f) && x.line != curve);

            //Calculamos la distancia entre la linea actual y la cercana
            double distanceS = closeLine != null ? Math.Round(curve.Distance(closeLine.line.Project(point).XYZPoint), 2) : 1;

            //Calculamos la direccion y distancia que tiene que recorrer la linea para unirse con el muro
            return distanceS != 0 ? curves[i].Direction / distanceS : XYZ.Zero;
        }
        #endregion

        #region ---------------------------------- CALCULOS - Puertas ----------------------------------
        public static List<ArcInfo> SimplifyListArcs(List<Arc> arc, ref List<Line> notSimpliedLine, out List<ArcInfo> notDoors)
        {
            List<ArcInfo> arcs = new List<ArcInfo>();                       //Lineas Generadas
            notDoors = new List<ArcInfo>();                                 //Lineas No Generadas

            foreach(Arc a in arc)
            {
                Line cLine = notSimpliedLine.FirstOrDefault(x => a.Center.DistanceTo(x.Project(x.GetEndPoint(0)).XYZPoint) < 0.1f ||
                                                                 a.Center.DistanceTo(x.Project(x.GetEndPoint(1)).XYZPoint) < 0.1f);
                if (cLine != null)
                {
                    notSimpliedLine.Remove(cLine);
                    ArcInfo aI = new ArcInfo(a);
                    aI.connectedLine = cLine;
                    arcs.Add(aI);
                }
            }

            foreach (Arc a in arc)
            {
                if (!arcs.Contains(new ArcInfo(a)))
                    notDoors.Add(new ArcInfo(a));
            }

            return arcs;
        }
        #endregion

        #region ---------------------------------- CALCULOS - Muros Curvos ----------------------------------
        #endregion

        #region ---------------------------------- CALCULOS - Ventanas ----------------------------------
        #endregion

        /// <summary>
        /// Devuelve el <see cref="Autodesk.Revit.DB.Level"/> segun <paramref name="levelName"/>
        /// </summary>
        public static Level GetLevelByName(Document doc, string levelName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> levels = collector.OfClass(typeof(Level)).ToElements();

            foreach (Element level in levels)
                if (level.Name == levelName)
                    return level as Level;

            return null; // Si no se encuentra ningún nivel con el nombre dado
        }
    }
}
