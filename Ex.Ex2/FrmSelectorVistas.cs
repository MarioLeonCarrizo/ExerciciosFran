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
        Document doc;
        ICollection<Element> viewsCollector;
        public FrmSelectorVistas(UIDocument uiDoc)
        {
            InitializeComponent();
            doc = uiDoc.Document;
            viewsCollector = new FilteredElementCollector(doc).OfClass(typeof(View)).ToElements();
        }

        private void FrmSelectorVistas_Load(object sender, EventArgs e)
        {
            GenerateComboBoxes();
            //cbViews.DataSource = GetViews();
        }

        void GenerateComboBoxes()
        {
            List<string> viewTypes = GetViewTypes();
            for(int i = 0; i < viewTypes.Count; i++)
            {
                Label lb = new Label();
                ComboBox cb = new ComboBox();

                lb.Location = new Point(5, 50 * i + 5);
                lb.Size = new Size(300, 25);
                lb.Text = viewTypes[i];

                cb.Location = new Point(15, 50 * i + 26);
                cb.Size = new Size(300, 15);
                cb.DataSource = GetViewsByViewType(viewTypes[i]);

                Controls.Add(cb);
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
                if (!views.Contains(viewType))
                {
                    views.Add(viewType);
                }
            }

            return views;
        }

        List<string> GetViewsByViewType(string ViewType)
        {
            List<string> views = new List<string>();

            foreach (Element element in viewsCollector)
            {
                View view = element as View;
                if (view != null && view.ViewType.ToString() == ViewType)
                {
                    views.Add(view.Name);
                }
            }

            return views;
        }

        View GetViewByName(string name)
        {
            ICollection<Element> elements = new FilteredElementCollector(doc).OfClass(typeof(View)).ToElements();
            foreach (Element element in elements)
            {
                View view = element as View;
                if(view != null && view.Name == name)
                    return view;
            }

            return null;
        }

        void ShowViewPlan(View plan)
        {
            pbPlanoView.Image = GetViewImage(plan);
        }

        Image GetViewImage(View view)
        {
            View viewGraphics = doc.GetElement(view.GetTypeId()) as View;
            XYZ origin = new XYZ(0,0,0);
            XYZ viewDir = new XYZ(0,0,1);

            ViewOrientation3D viewOri = new ViewOrientation3D(origin,viewDir, new XYZ(0,1,0));
            BoundingBoxXYZ crop = view.CropBox;

            ImageExportOptions opts = new ImageExportOptions();
            opts.FilePath = "C:\\";
            opts.ZoomType = ZoomFitType.FitToPage;
            opts.FitDirection = FitDirectionType.Horizontal;
            opts.PixelSize = 100;

            doc.ExportImage(opts);
            Image img = Image.FromFile(opts.FilePath);
            File.Delete(opts.FilePath);

            return img;
        }
    }
}
