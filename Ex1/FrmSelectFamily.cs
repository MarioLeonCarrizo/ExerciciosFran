using System;
using System.Windows.Forms;
using System.Collections.Generic;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Linq;
using Autodesk.Revit.Creation;
using System.Xml.Linq;
using System.Windows.Media;
using System.Security.Cryptography;
using System.Collections.ObjectModel;

namespace Ex.Ex1
{
    public partial class FrmSelectFamily : System.Windows.Forms.Form
    {
        Autodesk.Revit.DB.Document doc = null;
        public FrmSelectFamily(UIDocument uIDoc)
        {
            InitializeComponent();

            doc = uIDoc.Document;  
        }

        private List<string> GetAllCategories()
        {
            List<string> familiesName = new List<string>();
            IEnumerable<Element> elements = new FilteredElementCollector(doc).OfClass(typeof(ElementType)).WhereElementIsElementType().ToElements();

            foreach (Element f in elements)
            {
                if (f.Category != null)
                    familiesName.Add(f.Category.Name);
            }

            familiesName.Sort();

            return familiesName.Distinct().ToList();
        }

        private List<string> GetAllFamilies()
        {
            List<string> familiesName = new List<string>();

            ICollection<Element> families = new FilteredElementCollector(doc).OfClass(typeof(Family)).ToElements();
            IEnumerable<Element> elements = new FilteredElementCollector(doc).OfClass(typeof(ElementType)).WhereElementIsElementType().ToElements();
            
            foreach(Element f in elements)
            {
                //if (f.Category != null)
                //    familiesName.Add(f.Category.Name);

                if (f is ElementType elementType)
                    if(!string.IsNullOrEmpty(elementType.FamilyName))
                    familiesName.Add(elementType.FamilyName.ToString());
            }

            familiesName.Sort();

            return familiesName.Distinct().ToList();
        }

        private List<string> GetAllTypes(string nameFamily)
        {
            List<string> typesName = new List<string>();

            //ICollection<Element> families = new FilteredElementCollector(doc).OfClass(typeof(ElementType)).ToElements();
            //ElementType targetFamily = families.Cast<ElementType>().FirstOrDefault(f => f.FamilyName == nameFamily);

            //if (targetFamily == null)
            //    return null;

            
            ICollection<Element> elements = new FilteredElementCollector(doc).WhereElementIsElementType().ToElements();
            IEnumerable<Element> types = elements.Cast<ElementType>().Where(f => f.FamilyName == nameFamily);

            //ICollection<ElementId> typeIds = targetFamily.GetValidTypes();
            foreach (Element type in types)
            {
                typesName.Add(type.Name);

                //if (type is FamilySymbol familySymbol)
                //    typesName.Add(familySymbol.Name);
                //else if (type is ElementType elementType)
                //    typesName.Add(elementType.Name);
            }

            return typesName;
        }

        private void FrmSelectFamily_Load(object sender, EventArgs e)
        {
            cbCategoria.DataSource = GetAllCategories();
            cbFamilies.DataSource = GetAllFamilies();
        }

        private void cbFamilies_SelectedIndexChanged(object sender, EventArgs e)
        {
            string aux = cbFamilies.Items[cbFamilies.SelectedIndex].ToString();
            cbTypes.DataSource = GetAllTypes(cbFamilies.Items[cbFamilies.SelectedIndex].ToString());
        }

        private void cbTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowInfo();
        }

        void ShowInfo()
        {
            lbInfo.Text = "";
            List<Element> instances = GetInstances();
            foreach (Element instance in instances)
                lbInfo.Text += instance.Name + "-" + instance.Id.IntegerValue + " | ";
        }

        public List<Element> GetInstances()
        {
            string typeName = cbTypes.Items[cbTypes.SelectedIndex].ToString();
            
            var elementCollector = new FilteredElementCollector(doc).WhereElementIsNotElementType().Where(f => f.Name == typeName);

            return elementCollector.ToList();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }
    }
}
