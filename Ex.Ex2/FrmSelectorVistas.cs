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
                CheckedListBox cb = new CheckedListBox();

                int column = 80 * (i / 2);
                int row = 350 * (i % 2);

                lb.Location = new Point(5 + row, column + 5);
                lb.Size = new Size(300, 25);
                lb.Text = viewTypes[i];

                cb.Name = $"cb{viewTypes[i]}";
                cb.Location = new Point(15 + row, column + 25);
                cb.CheckOnClick = true;
                cb.Size = new Size(300, 60);

                foreach(string v in GetViewsByViewType(viewTypes[i]))
                    cb.Items.Add(v);

                cb.SetItemChecked(0, true);

                Controls.Add(cb);
                cbVistas.Add(cb);
                Controls.Add(lb);
            }
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

        int MAX_AREA = 20;
        void GenerateViewSheet()
        {
            uiDoc.ActiveView = vs;

            using (Transaction tx = new Transaction(doc, "Generar ViewPorts"))
            {
                tx.Start();

                List<ElementId> vistasIds = new List<ElementId>();
                foreach (CheckedListBox box in cbVistas)
                {
                    foreach(int i in box.CheckedIndices)
                        vistasIds.Add(GetViewByName(box.Items[i].ToString()).Id);
                }
                    

                foreach(Viewport vp in viewports)
                    vs.DeleteViewport(vp);
                viewports.Clear();

                XYZ ubi = new XYZ(0, 0, 0);
                foreach (ElementId vistaId in vistasIds)
                {
                    // Verificar el área del plano
                    View vista = doc.GetElement(vistaId) as View;

                    if (Viewport.CanAddViewToSheet(doc, vs.Id, vistaId))
                    {
                        BoundingBoxUV outline = vista.Outline;
                        double area = (outline.Max.U - outline.Min.U) * (outline.Max.V - outline.Min.V);

                        // Definir el factor de escala
                        int scaleFactor = 100;
                        if (area > MAX_AREA)
                            scaleFactor = (int)Math.Round(Math.Sqrt(MAX_AREA / area) * 100);

                        if (scaleFactor != 100)
                            vista.Scale = scaleFactor;

                        vista.Document.Regenerate();

                        int i = viewports.Count;
                        ubi = new XYZ((i % 3) * 2, -((i / 3) * 2), 0);                        

                        Viewport vp = Viewport.Create(doc, vs.Id, vistaId, ubi);

                        viewports.Add(vp);
                    }
                } 

                vs.Document.Regenerate();

                tx.Commit();
            }
        }

        private void btViewSheet_Click(object sender, EventArgs e)
        {
            //GenerateViewSheet();
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void btSelectAll_Click(object sender, EventArgs e)
        {
            foreach(CheckedListBox cb in cbVistas)
            {
                for(int i = 0; i < cb.Items.Count; i++)
                {
                    cb.SetItemChecked(i, true);
                }
            }
        }
    }
}
