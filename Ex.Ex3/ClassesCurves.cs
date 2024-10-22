using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ex.Ex3
{
    public class ArcInfo
    {
        public XYZ center;
        public XYZ dir;
        public XYZ middlePoint;
        public double width;
        public double radius;
        public Arc arc;
        public Line connectedLine;

        public double grados;
        public XYZ StartAngle;
        public XYZ EndAngle;
        public double SAngle;
        public double EAngle;

        public ArcInfo(Arc arc, double width = 0)
        {
            center = new XYZ(Math.Round(arc.Center.X, 3), Math.Round(arc.Center.Y, 3), Math.Round(arc.Center.Z, 3));
            dir = CalcularDireccion(arc.GetEndPoint(0), arc.GetEndPoint(1), center, Math.Round(arc.Radius, 3));
            middlePoint = arc.Center + new XYZ((arc.Radius / 2) * dir.X, (arc.Radius / 2) * dir.Y, 0);
            this.width = Math.Round(width, 2);
            radius = Math.Round(arc.Radius, 2);
            this.arc = arc;

            grados = CalcularGrados();
            StartAngle = arc.GetEndPoint(0) - arc.Center;
            EndAngle = arc.GetEndPoint(1) - arc.Center;
            SAngle = StartAngle.AngleTo(arc.XDirection);
            EAngle = EndAngle.AngleTo(arc.XDirection);
        }

        XYZ CalcularDireccion(XYZ a, XYZ b, XYZ center, double radius)
        {
            XYZ aPoint = (a - center) / radius, bPoint = (b - center) / radius;
            return new XYZ(Math.Round(aPoint.X), Math.Round(aPoint.Y), Math.Round(aPoint.Z)) + new XYZ(Math.Round(bPoint.X), Math.Round(bPoint.Y), Math.Round(bPoint.Z));
        }

        double CalcularGrados()
        {
            XYZ vectorInicio = arc.GetEndPoint(0) - arc.Center;
            XYZ vectorFin = arc.GetEndPoint(1) - arc.Center;
            double angulo = vectorInicio.AngleTo(vectorFin);
            return grados = Math.Round(angulo * (180 / Math.PI));
        }
    }

    public class LineInfo
    {
        public XYZ Start;
        public XYZ End;

        public XYZ StartF;
        public XYZ EndF;

        public double Up;
        public double Down;
        public double Left;
        public double Right;

        public XYZ Direction;
        public double Width;
        public double Length;

        public Line line;

        public LineInfo(Line line, double width = 0)
        {
            Start = new XYZ(Math.Round(line.GetEndPoint(0).X, 3), Math.Round(line.GetEndPoint(0).Y, 3), Math.Round(line.GetEndPoint(0).Z, 3));
            End = new XYZ(Math.Round(line.GetEndPoint(1).X, 3), Math.Round(line.GetEndPoint(1).Y, 3), Math.Round(line.GetEndPoint(1).Z, 3));

            Up = Math.Max(Start.Y, End.Y);
            Down = Math.Min(Start.Y, End.Y);
            Left = Math.Min(Start.X, End.X);
            Right = Math.Max(Start.X, End.X);

            StartF = new XYZ(Math.Round(Left, 3), Math.Round(Up, 3), Start.Z);
            EndF = new XYZ(Math.Round(Right, 3), Math.Round(Down, 3), End.Z);

            Direction = line.Direction;
            Width = Math.Round(width, 2);
            Length = Math.Round(Start.DistanceTo(End),3);

            this.line = line;
        }

        public XYZ MidPos() => (Start + End) / 2;
        public bool AreParallel(Line line) => Math.Abs(Math.Abs(Direction.DotProduct(line.Direction)) - 1) < 1;
        double MidCord(double a, double b) => (a + b) / 2;
    }
}
