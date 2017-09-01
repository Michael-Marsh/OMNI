using Microsoft.Win32;
using OMNI.Commands;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
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
        public omniDataSet.documentindexDataTable Table { get; set; }
        public bool Delete { get; set; }

        RelayCommand _open;
        RelayCommand _help;
        RelayCommand _query;
        RelayCommand _add;
        RelayCommand _refresh;

        #endregion

        /// <summary>
        /// Document Index ViewModel Constructor
        /// </summary>
        public DocumentIndexViewModel()
        {
            Table = new omniDataSet.documentindexDataTable();
            using (var documentindexTableAdapter = new omniDataSetTableAdapters.documentindexTableAdapter())
            {
                documentindexTableAdapter.Fill(Table);
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
            var file = parameter as string;
            if (Delete && MessageBox.Show($"Do you want to permanently delete {file}?", "Delete Validation", MessageBoxButton.YesNo, MessageBoxImage.Stop) == MessageBoxResult.Yes)
            {
                OMNIDataBase.RemoveDocumentAsync(file);
                using (var documentindexTableAdapter = new omniDataSetTableAdapters.documentindexTableAdapter())
                {
                    Table.Clear();
                    documentindexTableAdapter.Fill(Table);
                }
                OnPropertyChanged(nameof(Table));
            }
            else
            {
                var ext = string.Empty;
                var extList = (from r in Table.AsEnumerable() where r.FileName == file select r.FileExtension).ToList();
                foreach (string item in extList)
                {
                    ext = item;
                }
                try
                {
                    Process.Start($"{Properties.Settings.Default.DocumentLocation}{file}{ext}");
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
            List<string> wiList = M2k.WorkInstructions(PartNumber);
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

        /// <summary>
        /// Add Document Command
        /// </summary>
        public ICommand AddCommand
        {
            get
            {
                if (_add == null)
                {
                    _add = new RelayCommand(AddExecute, AddCanExecute);
                }
                return _add;
            }
        }

        /// <summary>
        /// Add Document Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private void AddExecute(object parameter)
        {
            var fd = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf|Microsoft Word Files (.doc)|*.doc|All Files (*.*)|*.*",
                InitialDirectory = Properties.Settings.Default.DocumentLocation,
                Title = "Select a document to add.",
                DefaultExt = "*.pdf",
                Multiselect = true
            };
            fd.ShowDialog();
            var file = fd.SafeFileNames;
            if (file.Length != 0)
            {
                OMNIDataBase.DocumentInsertAsync(file);
            }
            using (var documentindexTableAdapter = new omniDataSetTableAdapters.documentindexTableAdapter())
            {
                Table.Clear();
                documentindexTableAdapter.Fill(Table);
            }
            OnPropertyChanged(nameof(Table));
        }
        private bool AddCanExecute(object parameter) => CurrentUser.Kaizen ? true : false;

        /// <summary>
        /// Refresh Document Index Database Command
        /// </summary>
        public ICommand RefreshCommand
        {
            get
            {
                if (_refresh == null)
                {
                    _refresh = new RelayCommand(RefreshExecute, RefreshCanExecute);
                }
                return _refresh;
            }
        }

        /// <summary>
        /// Refresh Document Index Database Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private void RefreshExecute(object parameter)
        {
            OMNIDataBase.RefreshDocumentIndexAsync();
            using (var documentindexTableAdapter = new omniDataSetTableAdapters.documentindexTableAdapter())
            {
                Table.Clear();
                documentindexTableAdapter.Fill(Table);
            }
            OnPropertyChanged(nameof(Table));
        }
        private bool RefreshCanExecute(object parameter) => CurrentUser.Kaizen ? true : false;

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
