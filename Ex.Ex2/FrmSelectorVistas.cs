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

        /// <summary>
        /// Constructor del formulario.
        /// </summary>
        /// <param name="uiDoc">UIDocument actual en Revit.</param>
        /// <param name="xvs">ViewSheet a la que se agregarán las vistas seleccionadas.</param>
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

        /// <summary>
        /// Genera los ComboBoxes y CheckedListBoxes para cada tipo de vista disponible.
        /// </summary>
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
                cb.Text = $"cb{viewTypes[i]}";
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

        /// <summary>
        /// Maneja el evento Click de los CheckBoxes para seleccionar o deseleccionar todos los items de los CheckedListBoxes correspondientes.
        /// </summary>
        /// <param name="sender">CheckBox que disparó el evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        void SelectAllList(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb != null)
            {
                CheckedListBox clb = cbVistas.First(x => x.Name.Contains(cb.Text));
                for (int i = 0; i < clb.Items.Count; i++)
                    clb.SetItemChecked(i, cb.Checked);
            }
        }

        /// <summary>
        /// Obtiene una lista de todos los tipos de vista compatibles con la ViewSheet.
        /// </summary>
        /// <returns>Lista de tipos de vista.</returns>
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

        /// <summary>
        /// Obtiene una lista de vistas según el tipo de vista especificado.
        /// </summary>
        /// <param name="ViewType">El tipo de vista a filtrar.</param>
        /// <returns>Lista de nombres de vistas del tipo especificado.</returns>
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

        /// <summary>
        /// Obtiene una vista según su nombre.
        /// </summary>
        /// <param name="name">Nombre de la vista.</param>
        /// <returns>Vista correspondiente al nombre especificado.</returns>
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

        /// <summary>
        /// Obtiene los ElementId de las vistas seleccionadas para agregar a la ViewSheet.
        /// </summary>
        /// <returns>Lista de ElementId de vistas seleccionadas.</returns>
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
