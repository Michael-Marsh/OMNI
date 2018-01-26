using MySql.Data.MySqlClient;
using OMNI.CustomControls;
using OMNI.Extensions;
using OMNI.Models;
using OMNI.QMS.Model;
using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Data;

namespace OMNI.ViewModels
{
    /// <summary>
    /// User Submissions UserControl ViewModel Interaction Logic
    /// </summary>
    public class FormSearchUCViewModel : ViewModelBase
    {
        #region Properties

        public DataTable Table { get; set; }
        public ICollectionView SubmissionView { get; set; }
        private DataRowView selectedRow;
        public DataRowView SelectedRow
        {
            get { return selectedRow; }
            set { if (value != null) { OpenForm(value); selectedRow = value = null; } OnPropertyChanged(nameof(SelectedRow)); }
        }

        #endregion

        /// <summary>
        /// User Submissions UserControl ViewModel Constructor
        /// </summary>
        public FormSearchUCViewModel()
        {
            LoadTableAsync();
        }

        /// <summary>
        /// User Submissions UserControl ViewModel Constructor
        /// </summary>
        /// <param name="searchFilter">DataTable Search Filter</param>
        public FormSearchUCViewModel(string searchFilter)
        {
            LoadTableAsync(searchFilter);
        }

        /// <summary>
        /// Load the Table for the view
        /// </summary>
        /// <param name="searchFilter">optional: DataTable Search Filter. default = null</param>
        public async void LoadTableAsync(string searchFilter = null)
        {
            Table = new DataTable();
            if (string.IsNullOrEmpty(searchFilter))
            {
                using (var adapter = new MySqlDataAdapter($"SELECT `WorkOrderNumber`, `Date` FROM `omni`.`cmmsworkorder` WHERE `Submitter`='{CurrentUser.FullName}'", App.ConAsync))
                {
                    var _table = new DataTable();
                    await adapter.FillAsync(_table);
                    _table.Columns[0].ColumnName = "FormNumber";
                    _table.Columns[1].ColumnName = "SubmitDate";
                    _table.Columns.Add("Type", typeof(string));
                    for (int i = 0; i < _table.Rows.Count; i++)
                    {
                        _table.Rows[i][2] = "CMMS";
                    }
                    Table.Merge(_table);
                }
                using (var adapter = new MySqlDataAdapter($"SELECT `QIRNumber`, `QIRDate`, `Type` FROM `omni`.`qir_master` WHERE `Submitter`='{CurrentUser.FullName}'", App.ConAsync))
                {
                    var _table = new DataTable();
                    await adapter.FillAsync(_table);
                    _table.Columns[0].ColumnName = "FormNumber";
                    _table.Columns[1].ColumnName = "SubmitDate";
                    Table.Merge(_table);
                }
                using (var adapter = new MySqlDataAdapter($"SELECT `TicketNumber`, `SubmitDate` FROM `omni`.`it_ticket_master` WHERE `Submitter`='{CurrentUser.FullName}'", App.ConAsync))
                {
                    var _table = new DataTable();
                    await adapter.FillAsync(_table);
                    _table.Columns[0].ColumnName = "FormNumber";
                    _table.Columns[1].ColumnName = "SubmitDate";
                    _table.Columns.Add("Type", typeof(string));
                    for (int i = 0; i < _table.Rows.Count; i++)
                    {
                        _table.Rows[i][2] = "Ticket";
                    }
                    Table.Merge(_table);
                }
            }
            else
            {
                using (var adapter = new MySqlDataAdapter($"SELECT * FROM `omni`.`cmmsworkorder`", App.ConAsync))
                {
                    var _table = new DataTable();
                    await adapter.FillAsync(_table);
                    var _tableView = _table.SearchToDataView(searchFilter);
                    _table = null;
                    _table = _tableView.ToTable(false, "WorkOrderNumber", "Date");
                    _table.Columns[0].ColumnName = "FormNumber";
                    _table.Columns[1].ColumnName = "SubmitDate";
                    _table.Columns.Add("Type", typeof(string));
                    for (int i = 0; i < _table.Rows.Count; i++)
                    {
                        _table.Rows[i][2] = "CMMS";
                    }
                    Table.Merge(_table);
                }
                using (var adapter = new MySqlDataAdapter($"SELECT * FROM `omni`.`qir_master`", App.ConAsync))
                {
                    var _table = new DataTable();
                    await adapter.FillAsync(_table);
                    var _tableView = _table.SearchToDataView(searchFilter);
                    _table = null;
                    _table = _tableView.ToTable(false, "QIRNumber", "QIRDate", "Type");
                    _table.Columns[0].ColumnName = "FormNumber";
                    _table.Columns[1].ColumnName = "SubmitDate";
                    Table.Merge(_table);
                }
                using (var adapter = new MySqlDataAdapter($"SELECT * FROM `omni`.`it_ticket_master`", App.ConAsync))
                {
                    var _table = new DataTable();
                    await adapter.FillAsync(_table);
                    var _tableView = _table.SearchToDataView(searchFilter);
                    _table = null;
                    _table = _tableView.ToTable(false, "TicketNumber", "SubmitDate");
                    _table.Columns[0].ColumnName = "FormNumber";
                    _table.Columns[1].ColumnName = "SubmitDate";
                    _table.Columns.Add("Type", typeof(string));
                    for (int i = 0; i < _table.Rows.Count; i++)
                    {
                        _table.Rows[i][2] = "Ticket";
                    }
                    Table.Merge(_table);
                }
            }
            SubmissionView = CollectionViewSource.GetDefaultView(Table);
            SubmissionView.GroupDescriptions.Add(new PropertyGroupDescription("Type"));
        }

        /// <summary>
        /// Opens the selected form
        /// </summary>
        /// <param name="drv">Selected DataRowView object</param>
        public static void OpenForm(DataRowView drv)
        {
            switch (drv.Row[2].ToString())
            {
                case nameof(QIR):
                case "QIREZ":
                    DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.LoadQIR(Convert.ToInt32(drv.Row[0])));
                    DashBoardTabControl.WorkSpace.SelectedIndex = DashBoardTabControl.WorkSpace.Items.Count - 1;
                    break;
                case "CMMS":
                    DashBoardTabControl.WorkSpace.LoadCMMSWorkOrderTabItem(Convert.ToInt32(drv.Row[0]));
                    break;
                case "Ticket":
                    DashBoardTabControl.WorkSpace.LoadITTicketTabItem(Convert.ToInt32(drv.Row[0]));
                    break;
            }
        }

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing) { }
        }
    }
}
