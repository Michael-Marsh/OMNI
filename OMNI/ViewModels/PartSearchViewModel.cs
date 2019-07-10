using OMNI.Commands;
using OMNI.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// Part Search ViewModel Interaction Logic
    /// </summary>
    public class PartSearchViewModel : ViewModelBase
    {
        #region Properties

        public string PartNumber { get; set; }
        bool searching;
        public bool Searching
        {
            get { return searching; }
            set { searching = value; OnPropertyChanged(nameof(Searching)); }
        }
        string asyncText;
        public string AsyncText
        {
            get { return asyncText; }
            set { asyncText = value; OnPropertyChanged(nameof(AsyncText)); }
        }
        public bool Wcco { get; set; }
        public bool Csi { get; set; }
        public bool Both { get; set; }

        RelayCommand _printSearch;

        #endregion

        /// <summary>
        /// Part Search ViewModel Constructor
        /// </summary>
        public PartSearchViewModel()
        {
            Wcco = true;
        }

        /// <summary>
        /// Search Command
        /// </summary>
        public ICommand SearchCommand
        {
            get
            {
                if (_printSearch == null)
                {
                    _printSearch = new RelayCommand(SearchExecuteAsync, SearchCanExecute);
                }
                return _printSearch;
            }
        }

        /// <summary>
        /// Search Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private async void SearchExecuteAsync(object parameter)
        {
            var _tempPartNumber = PartNumber.ToUpper();
            PartNumber = string.Empty;
            OnPropertyChanged(nameof(PartNumber));
            var file = string.Empty;
            var company = Wcco ? nameof(Wcco) : Csi ? nameof(Csi) : nameof(Both);
            while (string.IsNullOrEmpty(file))
            {
                Searching = true;
                AsyncText = "Searching...";
                file = await M2k.SQLPartSearchAsync(_tempPartNumber, company);
            }
            AsyncText = "Loading...";
            try
            {
                if (file == "Invalid")
                {
                    if (File.Exists(Properties.Settings.Default.PrintLocation + _tempPartNumber + Properties.Settings.Default.DefaultPrintExtension))
                    {
                        Process.Start(Properties.Settings.Default.PrintLocation + _tempPartNumber + Properties.Settings.Default.DefaultPrintExtension);
                    }
                    else
                    {
                        ExceptionWindow.Show("Invalid Part Number", "The part number you have entered does not exist.\n Please double check your entry and try again.");
                    }
                }
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
                var eStatus = M2k.EngineeringStatus(PartNumber, string.Empty);
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
            Searching = false;
        }
        private bool SearchCanExecute(object parameter) => !string.IsNullOrWhiteSpace(PartNumber);

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                _printSearch = null;
            }
        }
    }
}
