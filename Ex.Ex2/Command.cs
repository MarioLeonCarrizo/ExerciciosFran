using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Ex.Ex2
{
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            throw new NotImplementedException();
        }

        public static string GetPath()
        {
            // Return constructed namespace path.
            return typeof(Command).Namespace + "." + nameof(Command);
        }
    }
}
