using System;
using System.Windows;
using System.Windows.Input;

namespace OMNI.Commands
{
    /// <summary>
    /// Form Window Home Command
    /// </summary>
    public class FormHomeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var _formWindow = parameter as Window;
            _formWindow.Close();
        }
    }
}
