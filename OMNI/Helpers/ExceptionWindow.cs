using OMNI.Models;
using OMNI.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace OMNI.Helpers
{
    /// <summary>
    /// Exception Window Logic
    /// </summary>
    public class ExceptionWindow
    {
        #region Properties

        public static string Title { get; set; }
        public static string Message { get; set; }
        public static bool ShowDetails { get; set; }
        public static string Source { get; set; }

        #endregion

        /// <summary>
        /// Display Exception Window.
        /// </summary>
        /// <param name="windowTitle">Window Title</param>
        /// <param name="exceptionMessage">Exception Message to display.</param>
        /// <param name="exceptionSource">optional: Exception Object to display as more details. Default = null</param>
        /// <param name="methodName">optional: Method name that threw the exception. Default = null</param>
        public static void Show(string windowTitle, string exceptionMessage, object exceptionSource = null, string methodName = null)
        {
            if (exceptionSource == null)
            {
                ShowDetails = false;
            }
            else
            {
                ShowDetails = true;
                Source = exceptionSource.ToString();
                var ex = (Exception)exceptionSource;
                if (App.SqlConAsync?.State == System.Data.ConnectionState.Open)
                {
                    OMNIException.SendtoLogAsync(ex.Source, ex.StackTrace, ex.Message, methodName);
                }
            }
            Title = windowTitle;
            Message = exceptionMessage;
            new ExceptionWindowView().ShowDialog();
        }
    }

    /// <summary>
    /// Ok Command Logic for Exception Window View
    /// </summary>
    public class OkCommand : ICommand
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
            var win = parameter as Window;
            win.Close();
        }
    }
}
