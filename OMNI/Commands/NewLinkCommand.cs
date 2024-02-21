using OMNI.Models;
using OMNI.Views;
using OMNI.ViewModels;
using OMNI.QMS.ViewModel;
using System;
using System.Windows.Input;

namespace OMNI.Commands
{
    public class NewLinkCommand : ICommand
    {
        //TODO: Clean up this class after the implementation of the new user control
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            var o = parameter;
            if (parameter.GetType() == typeof(QIRFormViewModel))
            {
                o = ((QIRFormViewModel)parameter).Qir;
            }
            else if (parameter.GetType() == typeof(CMMSWorkOrderUCViewModel))
            {
                o = ((CMMSWorkOrderUCViewModel)parameter).WorkOrder;
            }
            return ((FormBase)o).IDNumber != null && !((FormBase)o).LinkExists();
        }
        public void Execute(object parameter)
        {
            var o = parameter;
            if (parameter.GetType() == typeof(QIRFormViewModel))
            {
                o = ((QIRFormViewModel)parameter).Qir;
            }
            else if (parameter.GetType() == typeof(CMMSWorkOrderUCViewModel))
            {
                o = ((CMMSWorkOrderUCViewModel)parameter).WorkOrder;
            }
            var flwv = new FormLinkWindowView { DataContext = new FormLinkWindowViewModel(o) };
            flwv.ShowDialog();
            if (parameter.GetType() == typeof(QIRFormViewModel))
            {
                ((QIRFormViewModel)parameter).UpdateUILinkList();
            }
            else if (parameter.GetType() == typeof(CMMSWorkOrderUCViewModel))
            {
                ((CMMSWorkOrderUCViewModel)parameter).UpdateUILinkList();
            }
        }
    }
}
