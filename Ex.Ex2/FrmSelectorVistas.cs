using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComboBox = System.Windows.Forms.ComboBox;
using Point = System.Drawing.Point;
using View = Autodesk.Revit.DB.View;

namespace Ex.Ex2
{
    public partial class FrmSelectorVistas : System.Windows.Forms.Form
    {
        UIDocument uiDoc;
        Document doc;
        ICollection<Element> viewsCollector;
        List<CheckedListBox> cbVistas = new List<CheckedListBox>();

        ViewSheet vs;
        List<Viewport> viewports = new List<Viewport>();

        bool selectAll = true;

        public FrmSelectorVistas(UIDocument uiDoc, ViewSheet xvs)
        {
            InitializeComponent();
            this.uiDoc = uiDoc;
            doc = uiDoc.Document;
            viewsCollector = new FilteredElementCollector(doc).OfClass(typeof(View)).ToElements();
            vs = xvs;
        }

        private void FrmSelectorVistas_Load(object sender, EventArgs e)
        {
            GenerateComboBoxes();
            this.Size = new Size(Size.Width, 150 + ( 75 * ((cbVistas.Count-1) / 2)));
        }

        void GenerateComboBoxes()
        {
            List<string> viewTypes = GetViewTypes();
            for(int i = 0; i < viewTypes.Count; i++)
            {
                Label lb = new Label();
                CheckedListBox clb = new CheckedListBox();
                CheckBox cb = new CheckBox();

                int column = 80 * (i / 2);
                int row = 350 * (i % 2);

                lb.Location = new Point(5 + row, column + 5);
                lb.Size = new Size(170, 25);
                lb.Text = viewTypes[i];
                
                cb.Location = new Point(300 + row, column);
                cb.Size = new Size(23, 23);
                cb.Text = $"{i}";
                cb.Click += SelectAllList;

                clb.Name = $"cb{viewTypes[i]}";
                clb.Location = new Point(15 + row, column + 25);
                clb.CheckOnClick = true;
                clb.Size = new Size(300, 60);

                foreach(string v in GetViewsByViewType(viewTypes[i]))
                    clb.Items.Add(v);

                clb.SetItemChecked(0, true);

                Controls.Add(clb);
                cbVistas.Add(clb);
                Controls.Add(lb);
                Controls.Add(cb);
            }
        }

        void SelectAllList(object sender, EventArgs e)
        {
            
        }

        List<string> GetViewTypes()
        {
            List<string> views = new List<string>();

            foreach (Element element in viewsCollector)
            {
                View view = element as View;
                string viewType = view.ViewType.ToString();
                if (!views.Contains(viewType) && Viewport.CanAddViewToSheet(doc, vs.Id, view.Id))
                    views.Add(viewType);
            }

            return views;
        }

        List<string> GetViewsByViewType(string ViewType)
        {
            List<string> views = new List<string>();

            foreach (Element element in viewsCollector)
            {
                View view = element as View;
                if (view != null && view.ViewType.ToString() == ViewType && Viewport.CanAddViewToSheet(doc, vs.Id, view.Id))
                {
                    views.Add(view.Name);
                }
            }

            return views;
        }

        View GetViewByName(string name)
        {
            foreach (Element element in viewsCollector)
            {
                View view = element as View;
                if(view != null && view.Name == name)
                    return view;
            }

            return null;
        }

        public List<ElementId> GetViewSheets()
        {
            List<ElementId> vistasIds = new List<ElementId>();
            foreach (CheckedListBox box in cbVistas)
            {
                foreach (int i in box.CheckedIndices)
                    vistasIds.Add(GetViewByName(box.Items[i].ToString()).Id);
            }

            return vistasIds;
        }

        private void btViewSheet_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void btSelectAll_Click(object sender, EventArgs e)
        {

            foreach(CheckedListBox cb in cbVistas)
            {
                for(int i = 0; i < cb.Items.Count; i++)
                {
                    cb.SetItemChecked(i, selectAll);
                }
            }
            selectAll = !selectAll;
            if (!selectAll)
                btSelectAll.Text = "Unselect All";
            else
                btSelectAll.Text = "Select All";
        }
    }
}
