using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ex.Core
{
    public enum WindowType
    {
        Information = 0,
        Warning = 1,
        Error = 2,
    }
    public class Message
    {
        public static void Display(string message, WindowType type)
        {
            string title = "";
            var icon = TaskDialogIcon.TaskDialogIconNone;

            // Customize window based on type of message.
            switch (type)
            {
                case WindowType.Information:
                    title = "~ INFORMATION ~";
                    icon = TaskDialogIcon.TaskDialogIconInformation;
                    break;
                case WindowType.Warning:
                    title = "~ WARNING ~";
                    icon = TaskDialogIcon.TaskDialogIconWarning;
                    break;
                case WindowType.Error:
                    title = "~ ERROR ~";
                    icon = TaskDialogIcon.TaskDialogIconError;
                    break;
                default:
                    break;
            }

            // Construct window to display specified message.
            var window = new TaskDialog(title)
            {
                MainContent = message,
                MainIcon = icon,
                CommonButtons = TaskDialogCommonButtons.Ok
            };
            window.Show();
        }
    }
}
