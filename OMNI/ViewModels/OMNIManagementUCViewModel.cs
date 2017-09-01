using MySql.Data.MySqlClient;
using OMNI.Commands;
using OMNI.Enumerations;
using OMNI.Extensions;
using OMNI.Models;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// Privilege Management UserControl ViewModel Interaction Logic
    /// </summary>
    public class OMNIManagementUCViewModel : ViewModelBase
    {
        #region Properties

        public DataTable Table { get; set; }
        public List<OMNIDataBase> TableList { get; set; }
        private string selectedTable;
        public string SelectedTable { get { return selectedTable; } set { selectedTable = value; LoadTableAsync(); OnPropertyChanged(nameof(selectedTable)); } }
        public MySqlCommandBuilder TableCommandBuilder { get; set; }
        public MySqlDataAdapter TableDataAdapter { get; set; }
        public DataRowView SelectedRow { get; set; }
        public bool Changes { get; set; }
        private string searchBox;
        public string SearchBox
        {
            get { return searchBox; }
            set { if (!string.IsNullOrWhiteSpace(value)) { Table.Search(value); } else { Table.DefaultView.RowFilter = string.Empty; } searchBox = value; OnPropertyChanged(nameof(SearchBox)); }
        }
        public bool DeveloperView { get; set; }

        RelayCommand _commit;

        #endregion

        /// <summary>
        /// Privilege Management UserControl ViewModel Constructor
        /// </summary>
        public OMNIManagementUCViewModel()
        {
            Changes = false;
        }

        /// <summary>
        /// Privilege Management UserControl ViewModel Constructor
        /// <param name="tableName">Name of table to load.</param>
        /// </summary>
        public OMNIManagementUCViewModel(DashBoardDataBase tableName)
        {
            if (tableName.Equals(DashBoardDataBase.OMNIManagement))
            {
                TableList = OMNIDataBase.GetTableNameList();
                DeveloperView = CurrentUser.Developer;
            }
            else
            {
                SelectedTable = tableName.ToString();
                DeveloperView = false;
            }
            Changes = false;
        }

        /// <summary>
        /// Detects any changes made to Table and saves them for future commitment.
        /// </summary>
        /// <param name="sender">DataRow</param>
        /// <param name="e">DataRow Changed Events</param>
        private void TableRowChanged(object sender, DataRowChangeEventArgs e)
        {
            Changes = true;
            OnPropertyChanged(nameof(Changes));
        }

        /// <summary>
        /// Detects any changes made to Table and saves them for future commitment.
        /// </summary>
        /// <param name="sender">DataColumn</param>
        /// <param name="e">DataColumn Changed Events</param>
        private void TableColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            Changes = true;
            OnPropertyChanged(nameof(Changes));
        }

        /// <summary>
        /// Load a selected table into DataTable for viewing and editing
        /// </summary>
        private async void LoadTableAsync()
        {
            if (Changes && MessageBox.Show("All changes will be lost if you navigate away from this table.\nWould you like to save your changes now?", "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                CommitExecuteAsync(string.Empty);
            }
            Table = null;
            Table = new DataTable();
            TableDataAdapter = new MySqlDataAdapter($"SELECT * FROM `{App.Schema}`.`{SelectedTable}`", App.ConAsync);
            TableCommandBuilder = new MySqlCommandBuilder(TableDataAdapter);
            await TableDataAdapter.FillSchemaAsync(Table, SchemaType.Source);
            foreach (DataColumn col in Table.Columns)
            {
                if (col.DataType == typeof(sbyte))
                {
                    col.DataType = typeof(bool);
                }
            }
            await TableDataAdapter.FillAsync(Table);
            SearchBox = string.Empty;
            Changes = false;
            OnPropertyChanged(nameof(Changes));
            Table.AcceptChanges();
            OnPropertyChanged(nameof(Table));
            Table.RowChanged += TableRowChanged;
            Table.ColumnChanged += TableColumnChanged;
        }

        /// <summary>
        /// Commit Changes Command
        /// </summary>
        public ICommand CommitChangesCommand
        {
            get
            {
                if (_commit == null)
                {
                    _commit = new RelayCommand(CommitExecuteAsync, CommitCanExecute);
                }
                return _commit;
            }
        }

        /// <summary>
        /// Commit Changes Command Execution
        /// </summary>
        /// <param name="parameter"></param>
        private async void CommitExecuteAsync(object parameter)
        {
            TableDataAdapter.UpdateCommand = TableCommandBuilder.GetUpdateCommand();
            await TableDataAdapter.UpdateAsync(Table);
            Table.AcceptChanges();
            Changes = false;
            OnPropertyChanged(nameof(Changes));
        }
        private bool CommitCanExecute(object parameter) => Changes;

        /// <summary>
        /// Object Disposable
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                TableDataAdapter.Dispose();
                TableDataAdapter = null;
                TableCommandBuilder.Dispose();
                TableCommandBuilder = null;
            }
        }
    }
}
