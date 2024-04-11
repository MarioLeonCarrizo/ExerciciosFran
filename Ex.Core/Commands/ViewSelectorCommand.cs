using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Ex.Ex2;
using View = Autodesk.Revit.DB.View;

namespace Ex.Core
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class ViewSelectorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            ViewSheet vs;
            using (Transaction tx = new Transaction(doc, "Crear ViewSheet"))
            {
                tx.Start();

                vs = ViewSheet.Create(doc, ElementId.InvalidElementId);
                vs.Name = "Planos Generales";

                tx.Commit();
            }

            using (var window = new FrmSelectorVistas(uidoc, vs))
            {
                window.ShowDialog();

                if (window.DialogResult == DialogResult.Cancel)
                    return Result.Cancelled;

                // Generar la hoja de vistas con las vistas seleccionadas
                GenerateViewSheet(uidoc, vs, window.GetViewSheets());
            }

            return Result.Succeeded;
        }
        string InfoViews;
        /// <summary>
        /// Genera la hoja de vistas con los viewports correspondientes.
        /// </summary>
        /// <param name="uiDoc">UIDocument activo.</param>
        /// <param name="vs">Hojas de vistas generada</param>
        /// <param name="vistasIds">Lista de IDs de vistas seleccionadas.</param>
        void GenerateViewSheet(UIDocument uiDoc, ViewSheet vs, List<ElementId> vistasIds)
        {
            uiDoc.ActiveView = vs;

            using (Transaction tx = new Transaction(uiDoc.Document, "Generar ViewPorts"))
            {
                tx.Start();

                int rows = (int)Math.Round(Math.Sqrt(vistasIds.Count));
                for(int i =0;  i < vistasIds.Count; i++)
                {
                    if (Viewport.CanAddViewToSheet(uiDoc.Document, vs.Id, vistasIds[i]))
                    {
                        View vista = uiDoc.Document.GetElement(vistasIds[i]) as View;
                        XYZ ubi = new XYZ((i % rows) * 2.5f, -((i / rows) * 2.5f), 0);

                        CreateViewPort(uiDoc.Document, vs, vista, ubi);
                    }
                }

                vs.Document.Regenerate();

                var textNoteOptions = new TextNoteOptions
                {
                    VerticalAlignment = VerticalTextAlignment.Top,
                    HorizontalAlignment = HorizontalTextAlignment.Left,
                    TypeId = new FilteredElementCollector(uiDoc.Document).OfClass(typeof(TextElementType)).FirstOrDefault().Id,
                };

                var textNote = TextNote.Create(uiDoc.Document, uiDoc.ActiveView.Id, new XYZ(-2, 0, 0), InfoViews, textNoteOptions);

                tx.Commit();
            }
        }

        int MAX_AREA = 2;
        /// <summary>
        /// Crea un viewport en la hoja de vistas.
        /// </summary>
        /// <param name="doc">Documento activo.</param>
        /// <param name="vs">Hojas de vistas.</param>
        /// <param name="vista">Vista a añadir al viewport.</param>
        /// <param name="ubi">Ubicación del viewport.</param>
        void CreateViewPort(Document doc, ViewSheet vs, View vista, XYZ ubi)
        {
            BoundingBoxUV outline = vista.Outline;
            double area = (outline.Max.U - outline.Min.U) * (outline.Max.V - outline.Min.V);

            double scaleFactor = 100;
            if (area > MAX_AREA)
                scaleFactor = Math.Round(Math.Sqrt(area / MAX_AREA)) * 100;

            scaleFactor = Math.Min(150, Math.Max(75, scaleFactor));

            if (scaleFactor != 100 && vista.Scale != 0)
                vista.Scale = (int)scaleFactor;

            vista.Document.Regenerate();

            Viewport vp = Viewport.Create(doc, vs.Id, vista.Id, ubi);
            outline = vista.Outline;
            area = (outline.Max.U - outline.Min.U) * (outline.Max.V - outline.Min.V);
            InfoViews += $"{vista.Name,-60}\t-\t{area,-10}\t-\t{vista.Scale} \n";
        }

        public static string GetPath()
        {
            // Return constructed namespace path.
            return typeof(ViewSelectorCommand).Namespace + "." + nameof(ViewSelectorCommand);
        }
    }

    public class viewPortLayerData
    {
        public string Name { get; set; }
        public double Area { get; set; }
        public int Scale { get; set; }
    }
}
