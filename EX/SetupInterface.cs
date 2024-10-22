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
            var panelEx3 = app.CreateRibbonPanel(tabName, "Ex3");
            var panelEx4 = app.CreateRibbonPanel(tabName, "Ex4");
            var panelEx5 = app.CreateRibbonPanel(tabName, "Ex5");

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

            // -------------- EX3 -------------- //
            RevitPushButton btEx3 = new RevitPushButton("Obtener Medidas\nArchivo CAD", panelEx3,
                                                        "Descripcion", ViewInformationCADCommand.GetPath(),
                                                        "32_px.png", "windows_128_px.png");
            var bt3 = btEx3.Create();
            // -------------- EX4 -------------- //
            RevitPushButton btEx4 = new RevitPushButton("Generar Casa\nArchivo CAD", panelEx4,
                                                        "Descripcion", GenerateHouseWithCADPlan.GetPath(),
                                                        "32_px.png", "windows_128_px.png");
            var bt4 = btEx4.Create();
            // -------------- Final -------------- //
            RevitPushButton btEx5 = new RevitPushButton("Generar Estructura\nArchivo CAD Master", panelEx5,
                                                        "Descripcion", GeneradorMasterPlanos.GetPath(),
                                                        "32_px.png", "windows_128_px.png");
            var bt5 = btEx5.Create();
        }
    }
}
