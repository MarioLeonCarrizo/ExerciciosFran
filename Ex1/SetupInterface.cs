using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace Ex1
{
    public class SetupInterface
    {
        public void Initialize(UIControlledApplication app)
        {
            // Create ribbon tab.
            string tabName = "Mario";
            app.CreateRibbonTab(tabName);

            // Create the ribbon panels.
            var panelEx1 = app.CreateRibbonPanel(tabName, "Ex1");

            // Populate button data model.
            RevitPushButton button = new RevitPushButton("Reseteador de parametros", panelEx1, 
                                                        "Descripcion", LayerCommand.GetPath(), 
                                                        "32_px.png", "windows_128_px.png");

            // Create button from provided data.
            var Button = button.Create();
        }
    }
}
