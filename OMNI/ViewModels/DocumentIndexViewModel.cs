using OMNI.Commands;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// Document Index ViewModel Interaction Logic
    /// </summary>
    public class DocumentIndexViewModel : ViewModelBase
    {
        #region Properties

        public string PartNumber { get; set; }
        public bool ModifyAccess
        {
            get { return CurrentUser.Kaizen; }
        }
        private string _search;
        public string Search
        {
            get { return _search; }
            set { Table.DefaultView.RowFilter = $"FileName LIKE '%{value}%'"; _search = value; }
        }
        public DataTable Table { get; set; }

        RelayCommand _open;
        RelayCommand _help;
        RelayCommand _query;

        #endregion

        /// <summary>
        /// Document Index ViewModel Constructor
        /// </summary>
        public DocumentIndexViewModel()
        {
            if (Table == null)
            {
                Table = OMNIDataBase.GetDocumentIndex();
            }
        }

        #region View Command Interfaces

        /// <summary>
        /// Open Command
        /// </summary>
        public ICommand OpenCommand
        {
            get
            {
                if (_open == null)
                {
                    _open = new RelayCommand(OpenExecute);
                }
                return _open;
            }
        }

        /// <summary>
        /// Open Command Execution
        /// </summary>
        /// <param name="parameter">File to open</param>
        private void OpenExecute(object parameter)
        {
            var file = Directory.GetFiles(Properties.Settings.Default.DocumentLocation, $"{parameter}.*", SearchOption.TopDirectoryOnly).FirstOrDefault();
            try
            {
                Process.Start(file);
            }
            catch (Win32Exception)
            {
                ExceptionWindow.Show("Unable to Locate File", "The file that you have tried to open does not exist or was entered incorrectly.\nPlease contact the Standards Manager for further assistance.");
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
            }
        }

        /// <summary>
        /// Help Command
        /// </summary>
        public ICommand HelpCommand
        {
            get
            {
                if (_help == null)
                {
                    _help = new RelayCommand(HelpExecute);
                }
                return _help;
            }
        }

        /// <summary>
        /// Open Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private void HelpExecute(object parameter)
        {
            try
            {
                Process.Start(Properties.Settings.Default.DocumentIndexHelpFile);
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
            }
        }

        /// <summary>
        /// Query Command
        /// </summary>
        public ICommand QueryCommand
        {
            get
            {
                if (_query == null)
                {
                    _query = new RelayCommand(QueryExecute, QueryCanExecute);
                }
                return _query;
            }
        }

        /// <summary>
        /// Query Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private void QueryExecute(object parameter)
        {
            var wiList = M2k.WorkInstructions(PartNumber);
            if (wiList != null && wiList.Count > 0)
            {
                var filter = "FileName IN (";
                var builder = new System.Text.StringBuilder();
                builder.Append(filter);
                foreach (string wi in wiList)
                {
                    builder.Append($"'{Path.GetFileNameWithoutExtension(wi)}',");
                }
                filter = builder.ToString();
                filter += ")";
                Table.DefaultView.RowFilter = filter;
                PartNumber = string.Empty;
                OnPropertyChanged(nameof(PartNumber));
            }
            else
            {
                ExceptionWindow.Show("Empty Query", "The part number you have entered is either invalid, \n or there is no work instructions attached to it. \n If you feel you have reached this window in error double check your entry and try again.");
            }
        }
        private bool QueryCanExecute(object parameter) => string.IsNullOrEmpty(PartNumber)
                ? false
                : true;

        #endregion

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                Table.Dispose();
                _open = null;
                _help = null;
                _query = null;
            }
        }
    }
}
