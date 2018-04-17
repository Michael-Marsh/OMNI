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
            set { DocumentTable.DefaultView.RowFilter = $"FileName LIKE '%{value}%'"; _search = value; }
        }
        public string DocumentFolder
        {
            get { return CSISite ? Properties.Settings.Default.CSIDocumentLocation : Properties.Settings.Default.WCCODocumentLocation; }
        }
        private bool _csiSite;
        public bool CSISite
        {
            get { return _csiSite; }
            set { _csiSite = value; _wccoSite = !value; OnPropertyChanged(nameof(CSISite)); OnPropertyChanged(nameof(WCCOSite)); DocumentTable = LoadDocumentTable(); OnPropertyChanged(nameof(DocumentTable)); }
        }
        private bool _wccoSite;
        public bool WCCOSite
        {
            get { return _wccoSite; }
            set { _wccoSite = value; _csiSite = !value; OnPropertyChanged(nameof(WCCOSite)); OnPropertyChanged(nameof(CSISite)); DocumentTable = LoadDocumentTable(); OnPropertyChanged(nameof(DocumentTable)); }
        }
        public DataTable DocumentTable { get; set; }

        RelayCommand _open;
        RelayCommand _help;
        RelayCommand _query;

        #endregion

        /// <summary>
        /// Document Index ViewModel Constructor
        /// </summary>
        public DocumentIndexViewModel()
        {
            if (DocumentTable == null)
            {
                DocumentTable = LoadDocumentTable();
            }
            switch (Environment.UserDomainName)
            {
                case "AD":
                    WCCOSite = true;
                    break;
                case "CSI":
                    CSISite = true;
                    break;
                default:
                    WCCOSite = true;
                    break;
            }
        }

        #region View Command Interfaces

        public DataTable LoadDocumentTable()
        {
            using (DataTable dt = new DataTable())
            {
                using (var dataColumn = new DataColumn("FileName"))
                {
                    dt.Columns.Add(dataColumn);
                }
                using (var dataColumn = new DataColumn("FileExtension"))
                {
                    dt.Columns.Add(dataColumn);
                }
                foreach (var file in Directory.EnumerateFiles(DocumentFolder))
                {
                    if (file.IndexOf("~") == -1 && !Path.GetExtension(file).Equals(".db"))
                    {
                        var _tempRow = dt.NewRow();
                        _tempRow["FileName"] = Path.GetFileNameWithoutExtension(file);
                        _tempRow["FileExtension"] = Path.GetExtension(file);
                        dt.Rows.Add(_tempRow);
                    }
                }
                return dt;
            }
        }

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
            var file = Directory.GetFiles(DocumentFolder, $"{parameter}.*", SearchOption.TopDirectoryOnly).FirstOrDefault();
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
            var wiList = M2k.GetWorkInstructions(PartNumber);
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
                DocumentTable.DefaultView.RowFilter = filter;
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
                DocumentTable.Dispose();
                _open = null;
                _help = null;
                _query = null;
            }
        }
    }
}
