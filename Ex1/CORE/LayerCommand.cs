﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ex1
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class LayerCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Application context.
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            // Check if we are in the Revit project , not in family one.
            if (doc.IsFamilyDocument)
            {
                Message.Display("Can't use command in family document", WindowType.Warning);
                return Result.Cancelled;
            }

            // Get access to current view.
            var activeView = uidoc.ActiveView;

            // Check if Text Note can be created in currently active project view.
            bool canCreateTextNoteInView = false;
            switch (activeView.ViewType)
            {
                case ViewType.FloorPlan:
                    canCreateTextNoteInView = true;
                    break;
                case ViewType.CeilingPlan:
                    canCreateTextNoteInView = true;
                    break;
                case ViewType.Detail:
                    canCreateTextNoteInView = true;
                    break;
                case ViewType.Elevation:
                    canCreateTextNoteInView = true;
                    break;
                case ViewType.Section:
                    canCreateTextNoteInView = true;
                    break;
            }

            // Collector for data provided in window.
            List<Element> instances = new List<Element>();

            using (var window = new FrmSelectFamily(uidoc))
            {
                window.ShowDialog();

                if (window.DialogResult == DialogResult.Cancel)
                    return Result.Cancelled;

                instances = window.GetInstances();
            }

            // Open Revit document transaction to edit all Instance Properties.
            using (var transaction = new Transaction(doc))
            {
                transaction.Start("Remove Parameters Command");

                foreach(Element element in instances)
                {
                    ParameterSet parameter = element.Parameters;
                    foreach (Parameter param in parameter)
                    {
                        if (!param.IsReadOnly)
                        {
                            if (param.Definition.ParameterType == ParameterType.Text)
                                param.Set("");
                            else if (param.Definition.ParameterType == ParameterType.Integer || param.Definition.ParameterType == ParameterType.YesNo)
                                param.Set(0);
                        }
                    }
                }

                transaction.Commit();
            }

            return Result.Succeeded;
        }

        public static string GetPath()
        {
            // Return constructed namespace path.
            return typeof(LayerCommand).Namespace + "." + nameof(LayerCommand);
        }
    }

    public class LayersCommandData
    {
        public bool Name { get; set; }
        public ElementId TextTypeId { get; set; }
    }
}
