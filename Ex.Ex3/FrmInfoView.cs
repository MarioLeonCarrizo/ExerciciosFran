using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using Form = System.Windows.Forms.Form;
using Point = System.Drawing.Point;

namespace Ex.Ex3
{
    public partial class FrmInfoView : Form
    {
        List<string> wallType;
        List<string> doorType;
        List<string> windowType;

        public Dictionary<double, string> wallsFamily = new Dictionary<double, string>();
        List<LineInfo> walls;

        public Dictionary<double, string> doorsFamily = new Dictionary<double, string>();
        List<ArcInfo> doors;

        public Dictionary<double, string> wallsArcFamily = new Dictionary<double, string>();
        List<ArcInfo> wallsArc;

        public Dictionary<double, string> windowsFamily = new Dictionary<double, string>();
        List<LineInfo> windows;

        Document doc;
        int selectedMode = 0; // 0 = Walls // 1 = Doors // 2 = Windows
        public FrmInfoView(Document doc, List<LineInfo> walls, List<LineInfo> windows, List<ArcInfo> doors, List<ArcInfo> wallsArc)
        {
            InitializeComponent();
            this.doc = doc;
            this.walls = walls;
            this.windows = windows;
            this.doors = doors;
            this.wallsArc = wallsArc;

            wallType = GetWallTypes();
            doorType = GetDoorTypes();
            windowType = GetWindowsTypes();

            List<string> wallsWidth = walls.GroupBy(line => line.Width).Select(group => $"{group.Key}").ToList();
            List<string> doorsWidth = doors.GroupBy(line => line.radius).Select(group => $"{group.Key}").ToList();
            List<string> windowsWidth = windows.GroupBy(line => line.Width).Select(group => $"{group.Key}").ToList();
            List<string> wallsArcWidth = wallsArc.GroupBy(line => line.width).Select(group => $"{group.Key}").ToList();

            lbWidths.DataSource = wallsWidth;

            wallsFamily = SetFamilySimbol(wallsWidth, "Walls");
            doorsFamily = SetFamilySimbol(doorsWidth, "Doors");
            windowsFamily = SetFamilySimbol(windowsWidth, "Windows");
            wallsArcFamily = SetFamilySimbol(wallsArcWidth, "Walls");

            SelectorType();
        }

        private void pnlDraw_Paint(object sender, PaintEventArgs e)
        {
            // Define un factor de escala
            int scale = 4;
            float selectedWidth = float.Parse(lbWidths.SelectedItem.ToString().Replace(",", "."), CultureInfo.InvariantCulture);

            // Obtiene el centro del panel
            Point panelCenter = new Point(pnlDraw.Width / (2 * scale), pnlDraw.Height / (2 * scale));

            // Calcula el desplazamiento para centrar las líneas
            float offsetX = panelCenter.X, offsetY = panelCenter.Y;

            // Crea objetos Pen para diferentes colores de línea
            Pen redPen = new Pen(Color.Red);
            Pen blackPen = new Pen(Color.Black);
            Pen greenPen = new Pen(Color.Green);
            Pen bluePen = new Pen(Color.Blue);

            // Crea un objeto Graphics desde el evento Paint del panel
            Graphics g = e.Graphics;

            // ---- DIBUJAR WALLS ---- //
            foreach (var line in walls)
            {
                float startX = offsetX + ((float)line.Start.X * scale);
                float startY = offsetY + ((float)line.Start.Y * scale);
                float endX = offsetX + ((float)line.End.X * scale);
                float endY = offsetY + ((float)line.End.Y * scale);

                Pen pen = (float)line.Width == selectedWidth && selectedMode == 0 ? redPen : blackPen;
                g.DrawLine(pen, startX, startY, endX, endY);
            }

            // ---- DIBUJAR DOORS ---- //
            foreach (var arc in doors)
            {
                float centerX = offsetX + ((float)arc.center.X * scale);
                float centerY = offsetY + ((float)arc.center.Y * scale);
                float radius = ((float)arc.radius * scale);
                float X = centerX - radius;
                float Y = centerY - radius;

                float startX = offsetX + ((float)arc.connectedLine.GetEndPoint(0).X * scale);
                float startY = offsetY + ((float)arc.connectedLine.GetEndPoint(0).Y * scale);
                float SAngle = (float)Math.Round(arc.SAngle * (180 / Math.PI));
                float EAngle = (float)Math.Round(arc.EAngle * (180 / Math.PI));
                float sweepAngle = EAngle - SAngle;
                RectangleF rect = new RectangleF(X, Y, radius * 2, radius * 2);

                Pen pen = (float)arc.radius == selectedWidth && selectedMode == 1 ? redPen : greenPen;
                g.DrawArc(pen, rect, arc.dir.Y < 0 && arc.dir.X > 0 ? SAngle - 90 : SAngle, sweepAngle);
                g.DrawLine(pen, startX, startY, centerX, centerY);
            }

            // ---- DIBUJAR WALLS ARC ---- //
            foreach (var arc in wallsArc)
            {
                float centerX = offsetX + ((float)arc.center.X * scale);
                float centerY = offsetY + ((float)arc.center.Y * scale);
                float radius = ((float)arc.radius * scale);
                float X = centerX - radius;
                float Y = centerY - radius;
                RectangleF rect = new RectangleF(X, Y, radius * 2, radius * 2);

                Pen pen = (float)arc.width == selectedWidth && selectedMode == 3 ? redPen : blackPen;
                float sweetAngle = arc.dir.Y < 0 ? -(float)arc.grados : (float)arc.grados;
                g.DrawArc(pen, rect, 0, sweetAngle);
            }

            // ---- DIBUJAR WINDOWS ---- //
            foreach (var line in windows)
            {
                float startX = offsetX + ((float)line.Start.X * scale);
                float startY = offsetY + ((float)line.Start.Y * scale);
                float endX = offsetX + ((float)line.End.X * scale);
                float endY = offsetY + ((float)line.End.Y * scale);

                Pen pen = (float)line.Width == selectedWidth && selectedMode == 2 ? redPen : bluePen;
                g.DrawLine(pen, startX, startY, endX, endY);
            }
        }

        private void lbWidth_SelectedIndexChanged(object sender, EventArgs e) => SelectorType();

        void SelectorType()
        {
            if (lbWidths.SelectedIndex == -1) return;

            double selectedWidth = float.Parse(lbWidths.SelectedItem.ToString().Replace(",", "."), CultureInfo.InvariantCulture);
            if (selectedMode == 0)
            {
                lbNameType.Text = "Wall Type";
                cbTypes.DataSource = wallType;

                if (wallsFamily.Count == 0)
                    return;
                
                int index = cbTypes.FindString(wallsFamily[Math.Round(selectedWidth, 2)]);
                cbTypes.SelectedIndex = index;
            }
            else if (selectedMode == 1)
            {
                lbNameType.Text = "Door Type";
                cbTypes.DataSource = doorType;

                if (doorsFamily.Count == 0)
                    return;

                int index = cbTypes.FindString(doorsFamily[Math.Round(selectedWidth, 2)]);
                cbTypes.SelectedIndex = index;
            }
            else if (selectedMode == 2)
            {
                lbNameType.Text = "Window Type";
                cbTypes.DataSource = windowType;

                if (windowsFamily.Count == 0)
                    return;

                int index = cbTypes.FindString(windowsFamily[Math.Round(selectedWidth, 2)]);
                cbTypes.SelectedIndex = index;
            }
            else if (selectedMode == 3)
            {
                lbNameType.Text = "Wall Arc Type";
                cbTypes.DataSource = wallType;

                if (wallsArcFamily.Count == 0)
                    return;

                int index = cbTypes.FindString(wallsArcFamily[Math.Round(selectedWidth, 2)]);
                cbTypes.SelectedIndex = index;
            }

            pnlDraw.Invalidate();
        }

        Dictionary<double, string> SetFamilySimbol(List<string> widths, string type)
        {
            Dictionary<double, string> values = new Dictionary<double, string>();

            if(type == "Walls")
            {
                foreach(string w in widths)
                {
                    double selectedWidth = float.Parse(w.Replace(",", "."), CultureInfo.InvariantCulture);
                    List<WallType> collector = new FilteredElementCollector(doc).OfClass(typeof(WallType)).Cast<WallType>().ToList();
                    // Buscar el WallType con el ancho más cercano
                    WallType wallType = collector.OrderBy(x => Math.Abs(Math.Round(x.Width, 2) - selectedWidth)).FirstOrDefault();
                    double width = Math.Round(selectedWidth, 2);
                    if (wallType != null)
                        values.Add(width, wallType.Name);
                }
            }
            else if (type == "Doors")
            {
                foreach (string w in widths)
                {
                    float selectedWidth = float.Parse(w.Replace(",", "."), CultureInfo.InvariantCulture);
                    List<FamilySymbol> collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Doors).OfType<FamilySymbol>().Cast<FamilySymbol>().ToList();
                    FamilySymbol doorType = collector.OrderBy(x => Math.Abs(x.get_Parameter(BuiltInParameter.DOOR_WIDTH).AsDouble() - selectedWidth)).FirstOrDefault();
                    double width = Math.Round(selectedWidth, 2);
                    if (doorType != null)
                        values.Add(width, doorType.Name);
                }
            }
            else if (type == "Windows")
            {
                foreach (string w in widths)
                {
                    float selectedWidth = float.Parse(w.Replace(",", "."), CultureInfo.InvariantCulture);
                    List<FamilySymbol> collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Windows).OfType<FamilySymbol>().Cast<FamilySymbol>().ToList();
                    FamilySymbol windowType = collector.OrderBy(x => Math.Abs(x.get_Parameter(BuiltInParameter.WINDOW_WIDTH).AsDouble() - selectedWidth)).FirstOrDefault();
                    double width = Math.Round(selectedWidth, 2);
                    if (windowType != null)
                        values.Add(width, windowType.Name);
                }
            }

            return values;
        }

        private void cbTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cbTypes.SelectedIndex == -1) return;

            double selectedWidth = float.Parse(lbWidths.SelectedItem.ToString().Replace(",", "."), CultureInfo.InvariantCulture);
            if (selectedMode == 0)
            {
                if (wallsFamily.Count == 0) return;
                wallsFamily[Math.Round(selectedWidth,2)] = cbTypes.SelectedItem.ToString();
            }
            else if (selectedMode == 1)
            {
                if (doorsFamily.Count == 0) return;
                doorsFamily[Math.Round(selectedWidth, 2)] = cbTypes.SelectedItem.ToString();
            }
            else if (selectedMode == 2)
            {
                if (windowsFamily.Count == 0) return;
                windowsFamily[Math.Round(selectedWidth, 2)] = cbTypes.SelectedItem.ToString();
            }
            else if (selectedMode == 3)
            {
                if (windowsFamily.Count == 0) return;
                windowsFamily[Math.Round(selectedWidth, 2)] = cbTypes.SelectedItem.ToString();
            }
        }

        #region ----- Search List Family Types -----
        List<string> GetWallTypes()
        {
            List<string> wallTypesList = new List<string>();

            // Get all wall types from the document
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(WallType));

            foreach (WallType wallType in collector)
                wallTypesList.Add(wallType.Name);

            return wallTypesList;
        }

        List<string> GetDoorTypes()
        {
            List<string> wallTypesList = new List<string>();

            // Get all wall types from the document
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol));
            collector.OfCategory(BuiltInCategory.OST_Doors).OfType<FamilySymbol>();

            foreach (FamilySymbol doorType in collector)
                wallTypesList.Add(doorType.Name);

            return wallTypesList;
        }

        List<string> GetWindowsTypes()
        {
            List<string> wallTypesList = new List<string>();

            // Get all wall types from the document
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol));
            collector.OfCategory(BuiltInCategory.OST_Windows).OfType<FamilySymbol>();

            foreach (FamilySymbol doorType in collector)
                wallTypesList.Add(doorType.Name);

            return wallTypesList;
        }
        #endregion

        #region ----- Change Mode Manger -----
        private void btWalls_Click(object sender, EventArgs e) => SetMode(0);
        private void btDoors_Click(object sender, EventArgs e) => SetMode(1);
        private void btWindows_Click(object sender, EventArgs e) => SetMode(2);
        private void btWallsArc_Click(object sender, EventArgs e) => SetMode(3);

        void SetMode(int i)
        {
            selectedMode = i;
            switch (i)
            {
                case 0:
                    lbWidths.DataSource = walls.GroupBy(line => line.Width).Select(group => $"{group.Key}").ToList();
                    break;
                case 1:
                    lbWidths.DataSource = doors.GroupBy(line => line.radius).Select(group => $"{group.Key}").ToList();
                    break;
                case 2:
                    lbWidths.DataSource = windows.GroupBy(line => line.Width).Select(group => $"{group.Key}").ToList();
                    break;
                case 3:
                    lbWidths.DataSource = wallsArc.GroupBy(line => line.width).Select(group => $"{group.Key}").ToList();
                    break;
            }

            SelectorType();
        }
        #endregion

        private void btCreate_Click(object sender, EventArgs e)
        {
            this.Close();
            DialogResult = DialogResult.OK;
        }
    }
}
