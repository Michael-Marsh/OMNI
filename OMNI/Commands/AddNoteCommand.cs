using OMNI.Models;
using System;
using System.Windows.Input;

namespace OMNI.Commands
{
    public sealed class AddNoteCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter) => ((FormBase)parameter)?.IDNumber != null && App.ConConnected;
        public void Execute(object parameter)
        {
            ((FormBase)parameter).AddNote();
        }
    }
}
