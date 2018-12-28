using OMNI.Commands;
using OMNI.Enumerations;
using OMNI.Extensions;
using OMNI.Models;
using OMNI.QMS.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// DataBase UserControl ViewModel Interaction Logic
    /// </summary>
    public class DataBaseUCViewModel : ViewModelBase
    {
        #region Properties

        public DataTable Table { get; set; }
        public string Header { get; set; }
        public List<object> FilterList { get; set; }
        public string Column { get; set; }
        private string _filterItem;
        public string SelectedFilterItem
        {
            get { return _filterItem; }
            set { _filterItem = value; if (!string.IsNullOrEmpty(value)) { FilterTable(value); } }
        }
        public ObservableCollection<WorkCenter> WorkCenterList { get; private set; }
        private static bool workcenterView;
        public static bool WorkCenterView
        {
            get { return workcenterView; }
            set { workcenterView = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(WorkCenterView))); }
        }
        private int selectedWorkCenter;
        public int SelectedWorkCenter
        {
            get { return selectedWorkCenter; }
            set { selectedWorkCenter = value; OnPropertyChanged(nameof(SelectedWorkCenter)); }
        }

        public ObservableCollection<Users> CrewList { get; private set; }
        private bool crewView;
        public bool CrewView
        {
            get { return crewView; }
            set
            {
                crewView = value;
                OnPropertyChanged(nameof(CrewView));
            }
        }
        private string selectedmember;
        public string SelectedCrewMember
        {
            get { return selectedmember; }
            set
            {
                selectedmember = value;
                OnPropertyChanged(nameof(SelectedCrewMember));
                if (value == "None")
                {
                    Table.DefaultView.RowFilter = $"([Completed] > #{CompleteDate}# AND [Completed] < #{CompleteDate.AddDays(1)}#)";
                }
                else
                {
                    Table.DefaultView.RowFilter = $"([Completed] > #{CompleteDate}# AND [Completed] < #{CompleteDate.AddDays(1)}#) AND [ActiveCrew] LIKE '%{value}%'";
                }
            }
        }

        private DateTime cDate;
        public DateTime CompleteDate
        {
            get { return cDate; }
            set
            {
                cDate = value;
                OnPropertyChanged(nameof(CompleteDate));
                if (SelectedCrewMember == "None")
                {
                    Table.DefaultView.RowFilter = $"([Completed] > #{value}# AND [Completed] < #{value.AddDays(1)}#)";
                }
                else
                {
                    Table.DefaultView.RowFilter = $"([Completed] > #{value}# AND [Completed] < #{value.AddDays(1)}#) AND [ActiveCrew] LIKE '%{SelectedCrewMember}%'";
                }
            }
        }

        RelayCommand _filter;
        RelayCommand _clearFilter;
        RelayCommand _clearList;

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion

        public DataBaseUCViewModel(DataTable table)
        {
            Table = table.Copy();
            OnPropertyChanged(nameof(Table));
            WorkCenterView = false;
        }

        /// <summary>
        /// DataBase UserControl ViewModel Constructor
        /// </summary>
        /// <param name="tableName">Name of DataBase Table to Load</param>
        public DataBaseUCViewModel(string tableName, string filterQuery = null, string directFilter = null)
        {
            if (Table == null)
            {
                Table = new DataTable();
            }
            var _fail = false;
            foreach (var v in Enum.GetValues(typeof(DashBoardDataBase)))
            {
                if (((DashBoardDataBase)Enum.Parse(typeof(DashBoardDataBase), v.ToString())).GetDescription() == tableName)
                {
                    LoadTable(tableName);
                    CrewList = new ObservableCollection<Users>(Users.CMMSUserListAsync(true).Result);
                    SelectedCrewMember = CrewList[0].FullName;
                    CrewView = true;
                    CompleteDate = DateTime.Today;
                    _fail = true;
                }
            }
            if (string.IsNullOrEmpty(filterQuery) && filterQuery != "skip" && !_fail)
            {
                LoadTable(tableName);
                Header = tableName;
            }
            else if (string.IsNullOrEmpty(directFilter) && !_fail)
            {
                LoadTable(tableName);
                var dv = Table.SearchToDataView(filterQuery);
                Table =  dv == null || dv.Count == 0 ? null : dv.ToTable();
            }
            else if (!_fail)
            {
                LoadTable(tableName);
                Table.DefaultView.RowFilter = directFilter;
            }
            FilterList = new List<object>();
            WorkCenterList = new ObservableCollection<WorkCenter>(WorkCenter.GetListAsync(WorkCenterType.QMS).Result);
            WorkCenterView = false;
        }

        /// <summary>
        /// Filters the current table
        /// </summary>
        /// <param name="filterParameter">Value to filter the specfied column</param>
        public void FilterTable(string filterParameter)
        {
            Table.DefaultView.RowFilter = string.Empty;
            Table.DefaultView.RowFilter = $"[{Column}]='{filterParameter}'";
        }

        /// <summary>
        /// Load specified DataTable
        /// </summary>
        /// <param name="table">Name of DataBase Table to Load</param>
        private void LoadTable(string table)
        {
            switch (table)
            {

                case "Cal Tape Review":
                    //TODO: Add in calibration tape measure view
                    break;
                case "QIR Master":
                    Table = QIR.GetTableData(Convert.ToDateTime("10-1-17"),Convert.ToDateTime("11-1-17"));
                    break;
                case "Work Order Log":
                    //TODO: Add in the work order log table
                    break;
                case "CMMS Metrics":
                    Table = CMMSWorkOrder.GetUserMetrics();
                    break;
            }
        }

        /// <summary>
        /// Filter Command
        /// </summary>
        public ICommand FilterCommand
        {
            get
            {
                if (_filter == null)
                {
                    _filter = new RelayCommand(FilterExecute);
                }
                return _filter;
            }
        }

        /// <summary>
        /// Filter Command Execution
        /// </summary>
        /// <param name="parameter">Header Content</param>
        private void FilterExecute(object parameter)
        {
            Column = parameter as string;
            FilterList.Clear();
            FilterList = (from r in Table.AsEnumerable() select r[Column]).Distinct().ToList();
            {
                FilterList.Sort();
            }
            OnPropertyChanged(nameof(FilterList));
        }

        /// <summary>
        /// Clear Filter Command
        /// </summary>
        public ICommand ClearFilterCommand
        {
            get
            {
                if (_clearFilter == null)
                {
                    _clearFilter = new RelayCommand(ClearFilterExecute);
                }
                return _clearFilter;
            }
        }

        /// <summary>
        /// Clear Filter Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private void ClearFilterExecute(object parameter)
        {
            Table.DefaultView.RowFilter = string.Empty;
        }

        /// <summary>
        /// Clear List Command
        /// </summary>
        public ICommand ClearListCommand
        {
            get
            {
                if (_clearList == null)
                {
                    _clearList = new RelayCommand(ClearListExecute);
                }
                return _clearList;
            }
        }

        /// <summary>
        /// Clear List Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private void ClearListExecute(object parameter)
        {
            FilterList.Clear();
        }

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                _clearFilter = null;
                _clearList = null;
                _filter = null;
                Table?.Dispose();
                FilterList = null;
            }
        }
    }
}

