using System;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.UI;

namespace Ex.Core
{
    public partial class MoveObjectManagerMainPage : Page, IDisposable, IDockablePaneProvider
    {
        public MoveObjectManagerMainPage()
        {
            //InitializeComponent();

            DataContext = new FamilyManagerMainPageViewModel();
        }

        public void Dispose() => Dispose();

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this as FrameworkElement;
            data.InitialState = new DockablePaneState { DockPosition = DockPosition.Right, };
        }
    }
}
