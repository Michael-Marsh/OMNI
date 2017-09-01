using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Windows.Input;

namespace OMNI.Commands
{
    public class DeleteLinkCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter) => ((FormBase)parameter).IDNumber != null && ((FormBase)parameter).LinkExistsAsync().Result;
        public void Execute(object parameter)
        {
            if (!((FormBase)parameter).DeleteLinkAsync().Result)
            {
                ExceptionWindow.Show("Link Failure", "Unable to link this form at this time.\nPlease Contact IT for further assistance.");
            }
            else
            {
                ExceptionWindow.Show("Link Deleted", "Sucessfully deleted all links to this form and reconfigured the stage count!");
            }
        }
    }
}
