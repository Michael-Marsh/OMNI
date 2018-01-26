using OMNI.Models;
using OMNI.Views;
using OMNI.ViewModels;
using System;
using System.Windows.Input;

namespace OMNI.Commands
{
    public class NewLinkCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter) => ((FormBase)parameter).IDNumber != null && !((FormBase)parameter).LinkExistsAsync().Result;
        public void Execute(object parameter)
        {
            var flwv = new FormLinkWindowView { DataContext = new FormLinkWindowViewModel(parameter) };
            flwv.ShowDialog();
        }
    }
}
