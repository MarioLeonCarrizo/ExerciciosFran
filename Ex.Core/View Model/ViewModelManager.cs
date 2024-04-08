using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ex.Core.View_Model
{

    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }

    public class FamilyManagerMainPageViewModel : BaseViewModel
    {
        public ICommand SelectBtnCommand { get; set; }
        public ICommand MoveBtnCommand { get; set; }

        public FamilyManagerMainPageViewModel()
        {
            SelectBtnCommand = new RouteCommands(() => SelectModel());
            MoveBtnCommand = new RouteCommands(() => MoveModel());
        }

        void SelectModel()
        {

        }

        void MoveModel()
        {

        }
    }

    public class RouteCommands : ICommand
    {
        private Action mAction = null;
        public event EventHandler CanExecuteChanged = (sender, e) => { };

        public RouteCommands(Action action)
        {
            mAction = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            mAction();
        }
    }
}
