using OMNI.Helpers;
using OMNI.Models;
using OMNI.QMS.ViewModel;
using OMNI.ViewModels;
using System;
using System.Windows.Input;

namespace OMNI.Commands
{
    public class DeleteLinkCommand : ICommand
    {
        //TODO: Clean up the class when the new user control is built
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            if (parameter.GetType() == typeof(QIRFormViewModel))
            {
                parameter = ((QIRFormViewModel)parameter).Qir;
            }
            else if (parameter.GetType() == typeof(CMMSWorkOrderUCViewModel))
            {
                parameter = ((CMMSWorkOrderUCViewModel)parameter).WorkOrder;
            }
            return ((FormBase)parameter).IDNumber != null && ((FormBase)parameter).LinkExists();
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
            if (!((FormBase)o).DeleteLink())
            {
                ExceptionWindow.Show("Link Failure", "Unable to link this form at this time.\nPlease Contact IT for further assistance.");
            }
            else
            {
                ExceptionWindow.Show("Link Deleted", "Sucessfully deleted all links to this form and reconfigured the stage count!");
            }
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
