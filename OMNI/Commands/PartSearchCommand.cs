using OMNI.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace OMNI.Commands
{
    public class PartSearchCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public void Execute(object parameter)
        {
            if (!string.IsNullOrEmpty(parameter?.ToString()))
            {
                var file = M2k.PartSearchAsync(parameter.ToString(), "WCCO").Result;
                try
                {
                    if (file == "Invalid")
                        ExceptionWindow.Show("Invalid Part Number", "The part number you have entered does not exist.\n Please double check your entry and try again.");
                    else if (file == "O")
                    {
                        ExceptionWindow.Show("Obsolete Part Number", "The part number you have entered has been obsoleted.");
                    }
                    else
                    {
                        Process.Start(Properties.Settings.Default.PrintLocation + file + Properties.Settings.Default.DefaultPrintExtension);
                    }
                }
                catch (Win32Exception)
                {
                    var eStatus = M2k.EngineeringStatus(parameter.ToString(), string.Empty);
                    switch (eStatus)
                    {
                        case "C":
                            ExceptionWindow.Show("Pending Change", "The part number you have entered is currently on Pending Change. \n Please call engineering for further support.");
                            break;
                        case "P":
                            ExceptionWindow.Show("Prototype Part", "The part number you have entered is a prototype and can only be viewed by engineering. \n Please call engineering for further support.");
                            break;
                        default:
                            ExceptionWindow.Show("Print Not Found", "The part number you have entered does exist but no print was found. \n Please call engineering for further support.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                }
            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
