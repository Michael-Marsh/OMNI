using OMNI.CustomControls;
using OMNI.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace OMNI.Commands
{
    /// <summary>
    /// Tab Item Close Command
    /// </summary>
    public class TabCloseCommand : ICommand
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
            if (parameter as string == "Current")
            {
                DashBoardTabControl.WorkSpace.Items.RemoveAt(DashBoardTabControl.WorkSpace.SelectedIndex);
            }
            else
            {
                ((ViewModelBase)((UserControl)((TabItem)parameter).Content).DataContext)?.Dispose();
                DashBoardTabControl.WorkSpace.Items.Remove((TabItem)parameter);
            }
        }
    }
}
