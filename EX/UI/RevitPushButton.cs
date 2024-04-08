using Autodesk.Revit.UI;
using System;
using System.Reflection;

namespace Ex
{
    public class RevitPushButton
    {
        public static string Label { get; set; }
        public static RibbonPanel Panel { get; set; }
        public static string CommandNamespacePath { get; set; }
        public static string Tooltip { get; set; }
        public static string IconImageName { get; set; }
        public static string TooltipImageName { get; set; }

        public RevitPushButton(string label, RibbonPanel panel, string toolTip, string commandNameSpace, string iconImageName, string toolTipImage)
        {
            Label = label;
            Panel = panel;
            CommandNamespacePath = commandNameSpace;
            Tooltip = toolTip;
            IconImageName = iconImageName;
            TooltipImageName = toolTipImage;
        }

        public PushButton Create()
        {
            // The button name based on unique identifier.
            var btnDataName = Guid.NewGuid().ToString();

            // Sets the button data.
            var btnData = new PushButtonData(btnDataName, Label, Assembly.GetExecutingAssembly().Location, CommandNamespacePath)
            {
                ToolTip = Tooltip,
                LargeImage = ResourceImage.GetIcon(IconImageName),
                ToolTipImage = ResourceImage.GetIcon(TooltipImageName)
            };

            // Return created button and host it on panel provided in required data model.
            return Panel.AddItem(btnData) as PushButton;
        }
    }
}
