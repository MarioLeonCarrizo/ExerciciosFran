using Autodesk.Revit.DB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ex.Ex3
{
    public partial class FrmSelectWalls : System.Windows.Forms.Form
    {
        Document doc;
        public FrmSelectWalls(Document doc)
        {
            InitializeComponent();
            this.doc = doc;
        }

        private void FrmSelectWalls_Load(object sender, EventArgs e)
        {
            cbWall.DataSource = GetWallTypes(doc);
            cbDoor.DataSource = GetDoorTypes(doc);
            cbWindows.DataSource = GetWindowsTypes(doc);
        }

        List<string> GetWallTypes(Document doc)
        {
            List<string> wallTypesList = new List<string>();

            // Get all wall types from the document
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(WallType));

            foreach (WallType wallType in collector)
            {
                wallTypesList.Add(wallType.Name);
            }

            return wallTypesList;
        }

        List<string> GetDoorTypes(Document doc)
        {
            List<string> wallTypesList = new List<string>();

            // Get all wall types from the document
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Doors).OfType<FamilySymbol>();

            foreach (FamilySymbol doorType in collector)
            {
                wallTypesList.Add(doorType.Name);
            }

            return wallTypesList;
        }

        List<string> GetWindowsTypes(Document doc)
        {
            List<string> wallTypesList = new List<string>();

            // Get all wall types from the document
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Windows).OfType<FamilySymbol>();

            foreach (FamilySymbol doorType in collector)
            {
                wallTypesList.Add(doorType.Name);
            }

            return wallTypesList;
        }

        private void btCrear_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        public WallType GetWall() => new FilteredElementCollector(doc).OfClass(typeof(WallType)).OfType<WallType>().FirstOrDefault(x => x.Name == cbWall.Items[cbWall.SelectedIndex].ToString());
        public FamilySymbol GetDoor() => new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).OfType<FamilySymbol>().FirstOrDefault(x => x.Name == cbDoor.Items[cbDoor.SelectedIndex].ToString());
        public FamilySymbol GetWindows() => new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).OfType<FamilySymbol>().FirstOrDefault(x => x.Name == cbWindows.Items[cbWindows.SelectedIndex].ToString());
        public bool CreateCheckNote() => cbTextNote.Checked;
    }
}
