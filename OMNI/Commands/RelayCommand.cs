using System;
using System.Diagnostics;
using System.Windows.Input;

namespace OMNI.Commands
{
    /// <summary>
    /// Dynamic Relay Commannd
    /// </summary>
    public class RelayCommand : ICommand
    {
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;
        /// <summary>
        /// Relay Command
        /// </summary>
        /// <param name="execute">Execution Action</param>
        public RelayCommand(Action<object> execute)
            :this()
        {
            _execute = execute;
        }

        /// <summary>
        /// Relay Command
        /// </summary>
        /// <param name="execute">Execution Action</param>
        /// <param name="canExecute">Can Execute bool</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
            :this()
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public RelayCommand()
        {
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
