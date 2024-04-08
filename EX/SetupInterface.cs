using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Ex.Core;

namespace Ex
{
    public class SetupInterface
    {
        public void Initialize(UIControlledApplication app)
        {
            // Create ribbon tab.
            string tabName = "Mario";
            app.CreateRibbonTab(tabName);

            // Create ribbon panels
            var panelEx1 = app.CreateRibbonPanel(tabName, "Ex1");
            var panelEx2 = app.CreateRibbonPanel(tabName, "Ex2");
            var panelTest1 = app.CreateRibbonPanel(tabName, "Test1");

            // -------------- EX1 -------------- //
            RevitPushButton btEx1 = new RevitPushButton("Reseteador de parametros", panelEx1, 
                                                        "Descripcion", RemoveParamsCommand.GetPath(), 
                                                        "32_px.png", "windows_128_px.png");
            var bt1 = btEx1.Create();

            // -------------- EX2 -------------- //
            RevitPushButton btEx2 = new RevitPushButton("Selector de Vistas", panelEx2,
                                                        "Descripcion", ViewSelectorCommand.GetPath(),
                                                        "32_px.png", "windows_128_px.png");
            var bt2 = btEx2.Create();

            // -------------- TEST1 -------------- //
            RevitPushButton btTest1 = new RevitPushButton("Mover Objeto Seleccionado", panelTest1,
                                                        "Descripcion", MoveObjectCommand.GetPath(),
                                                        "32_px.png", "windows_128_px.png");
            var btt1 = btTest1.Create();
        }
    }
}
