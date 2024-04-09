using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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

                GenerateViewSheet(uidoc, vs, window.GetViewSheets());
            }

            return Result.Succeeded;
        }

        int MAX_AREA = 20;
        void GenerateViewSheet(UIDocument uiDoc, ViewSheet vs, List<ElementId> vistasIds)
        {
            uiDoc.ActiveView = vs;

            using (Transaction tx = new Transaction(uiDoc.Document, "Generar ViewPorts"))
            {
                tx.Start();

                XYZ ubi = new XYZ(0, 0, 0);
                for(int i =0;  i < vistasIds.Count; i++)
                {
                    // Verificar el área del plano
                    View vista = uiDoc.Document.GetElement(vistasIds[i]) as View;

                    if (Viewport.CanAddViewToSheet(uiDoc.Document, vs.Id, vistasIds[i]))
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

                        ubi = new XYZ((i % 3) * 2, -((i / 3) * 2), 0);

                        Viewport vp = Viewport.Create(uiDoc.Document, vs.Id, vistasIds[i], ubi);
                    }
                }

                vs.Document.Regenerate();

                tx.Commit();
            }
        }

        public static string GetPath()
        {
            // Return constructed namespace path.
            return typeof(ViewSelectorCommand).Namespace + "." + nameof(ViewSelectorCommand);
        }
    }
}
