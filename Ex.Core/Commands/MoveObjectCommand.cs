using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Ex.Ex1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ex.Core
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class MoveObjectCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Application context.
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            var dpid = new DockablePaneId(new Guid("E646DEBC-798E-429E-88DE-BD3270DCDD76"));
            var dp = commandData.Application.GetDockablePane(dpid);
            //dp.Show();

            return Result.Succeeded;
        }

        public static string GetPath()
        {
            // Return constructed namespace path.
            return typeof(MoveObjectCommand).Namespace + "." + nameof(MoveObjectCommand);
        }
    }
}
