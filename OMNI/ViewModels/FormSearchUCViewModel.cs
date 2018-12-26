using OMNI.CustomControls;
using OMNI.Extensions;
using OMNI.Models;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
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
            set { if (value != null) { OpenForm(value); } selectedRow = value; OnPropertyChanged(nameof(SelectedRow)); }
        }

        #endregion

        /// <summary>
        /// User Submissions UserControl ViewModel Constructor
        /// </summary>
        public FormSearchUCViewModel()
        {
            LoadTable();
        }

        /// <summary>
        /// User Submissions UserControl ViewModel Constructor
        /// </summary>
        /// <param name="searchFilter">DataTable Search Filter</param>
        public FormSearchUCViewModel(string searchFilter)
        {
            LoadTable(searchFilter);
        }

        /// <summary>
        /// Load the Table for the view
        /// </summary>
        /// <param name="searchFilter">optional: DataTable Search Filter. default = null</param>
        public void LoadTable(string searchFilter = null)
        {
            Table = new DataTable();
            if (string.IsNullOrEmpty(searchFilter))
            {
                using (var adapter = new SqlDataAdapter($@"USE {App.DataBase};
                                                        SELECT [WorkOrderNumber], [Date] FROM [cmmsworkorder] WHERE [Submitter]='{CurrentUser.FullName}';", App.SqlConAsync))
                {
                    var _table = new DataTable();
                    adapter.Fill(_table);
                    _table.Columns[0].ColumnName = "FormNumber";
                    _table.Columns[1].ColumnName = "SubmitDate";
                    _table.Columns.Add("Type", typeof(string));
                    for (int i = 0; i < _table.Rows.Count; i++)
                    {
                        _table.Rows[i][2] = "CMMS";
                    }
                    Table.Merge(_table);
                }
                using (var adapter = new SqlDataAdapter($@"USE {App.DataBase};
                                                           SELECT [QIRNumber], [QIRDate], [Type] FROM [qir_master] WHERE [Submitter]='{CurrentUser.FullName}';", App.SqlConAsync))
                {
                    var _table = new DataTable();
                    adapter.Fill(_table);
                    _table.Columns[0].ColumnName = "FormNumber";
                    _table.Columns[1].ColumnName = "SubmitDate";
                    Table.Merge(_table);
                }
                using (var adapter = new SqlDataAdapter($@"USE {App.DataBase};
                                                            SELECT [TicketNumber], [SubmitDate] FROM [it_ticket_master] WHERE [Submitter]='{CurrentUser.FullName}';", App.SqlConAsync))
                {
                    var _table = new DataTable();
                    adapter.Fill(_table);
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
                using (var adapter = new SqlDataAdapter($@"USE {App.DataBase}
                                                            SELECT * FROM [cmmsworkorder]", App.SqlConAsync))
                {
                    var _table = new DataTable();
                    adapter.Fill(_table);
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
                using (var adapter = new SqlDataAdapter($@"USE {App.DataBase};
                                                            SELECT * FROM [qir_master];", App.SqlConAsync))
                {
                    var _table = new DataTable();
                    adapter.Fill(_table);
                    var _tableView = _table.SearchToDataView(searchFilter);
                    _table = null;
                    _table = _tableView.ToTable(false, "QIRNumber", "QIRDate", "Type");
                    _table.Columns[0].ColumnName = "FormNumber";
                    _table.Columns[1].ColumnName = "SubmitDate";
                    Table.Merge(_table);
                }
                using (var adapter = new SqlDataAdapter($@"USE {App.DataBase};
                                                            SELECT * FROM [it_ticket_master];", App.SqlConAsync))
                {
                    var _table = new DataTable();
                    adapter.Fill(_table);
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
            if (drv != null)
            {
                switch (drv.Row.ItemArray[2].ToString())
                {
                    case "QIR":
                    case "QIREZ":
                        DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.LoadQIR(Convert.ToInt32(drv.Row.ItemArray[0])));
                        DashBoardTabControl.WorkSpace.SelectedIndex = DashBoardTabControl.WorkSpace.Items.Count - 1;
                        break;
                    case "CMMS":
                        DashBoardTabControl.WorkSpace.LoadCMMSWorkOrderTabItem(Convert.ToInt32(drv.Row.ItemArray[0]));
                        break;
                    case "Ticket":
                        DashBoardTabControl.WorkSpace.LoadITTicketTabItem(Convert.ToInt32(drv.Row.ItemArray[0]));
                        break;
                }
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
