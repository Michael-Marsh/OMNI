using OMNI.Commands;
using OMNI.CustomControls;
using OMNI.Models;
using OMNI.QMS.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// Pareto Chart UserControl ViewModel Interaction Logic
    /// </summary>
    public class ParetoChartUCViewModel : ViewModelBase
    {
        #region Properties

        public List<QIRChart> DataCollection { get; set; }
        public List<KeyValuePair<string, int>> X_AxisCount { get; set; }
        public List<int> Y_AxisCount { get; set; }
        public List<KeyValuePair<string, int>> X_AxisCost { get; set; }
        public List<double> Y_AxisCost { get; set; }
        public int MaxCount { get; set; }
        public int MaxCost { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Filter { get; set; }
        public List<int> ParetoPercentage { get; set; }
        private int selectedPercentage;
        public int SelectedPercentage
        {
            get { return selectedPercentage; }
            set
            {
                selectedPercentage = value;
                OnPropertyChanged(nameof(SelectedPercentage));
                BuildChart(Year, Month, Filter, value);
            }
        }
        public ObservableCollection<WorkCenter> WorkCenterList { get; private set; }
        private int? selectedWorkCenter;
        public int? SelectedWorkCenter
        {
            get { return selectedWorkCenter; }
            set
            {
                selectedWorkCenter = value;
                OnPropertyChanged(nameof(SelectedWorkCenter));
                BuildChart(Year, Month, Filter, SelectedPercentage);
            }
        }
        public bool NoChart { get; set; }
        public bool Chart { get; set; }
        private object _selectedCost;
        public object SelectedCost
        {
            get { return _selectedCost; }
            set
            {
                if (value != _selectedCost && value != null)
                {
                    CostResultsTable = QIRChart.GetResultsAsync(((KeyValuePair<string, int>)value).Key, SelectedWorkCenter, Month, Year).Result;
                }
                _selectedCost = value;
                SelectedQIRNumberCost = null;
                OnPropertyChanged(nameof(CostResultsTable));
                OnPropertyChanged(nameof(SelectedCost));
            }
        }
        public DataTable CostResultsTable { get; set; }
        private object _selectedQIRNumberCost;
        public object SelectedQIRNumberCost
        {
            get { return _selectedQIRNumberCost; }
            set { if (value != _selectedQIRNumberCost && value != null) { OpenQIR(Convert.ToInt32(((DataRowView)value).Row[0])); } _selectedQIRNumberCost = value; OnPropertyChanged(nameof(SelectedQIRNumberCost)); }
        }

        private object _selectedCount;
        public object SelectedCount
        {
            get { return _selectedCount; }
            set
            {
                if (value != _selectedCount && value != null)
                {
                    CountResultsTable = QIRChart.GetResultsAsync(((KeyValuePair<string, int>)value).Key, SelectedWorkCenter, Month, Year).Result;
                }
                _selectedCount = value;
                OnPropertyChanged(nameof(CountResultsTable));
                OnPropertyChanged(nameof(SelectedCount));
            }
        }
        public DataTable CountResultsTable { get; set; }
        private object _selectedQIRNumberCount;
        public object SelectedQIRNumberCount
        {
            get { return _selectedQIRNumberCount; }
            set { if (value != _selectedQIRNumberCount && value != null) { OpenQIR(Convert.ToInt32(((DataRowView)value).Row[0])); } _selectedQIRNumberCount = value; OnPropertyChanged(nameof(SelectedQIRNumberCount)); }
        }

        RelayCommand _refresh;

        #endregion

        /// <summary>
        /// Pareto Chart UserControl ViewModel Constructor
        /// </summary>
        public ParetoChartUCViewModel(int year, int month, string filter)
        {
            Year = year;
            Month = month;
            Filter = filter;
            WorkCenterList = new ObservableCollection<WorkCenter>(WorkCenter.GetListAsync(Enumerations.WorkCenterType.QMS).Result);
            WorkCenterList.Insert(0, new WorkCenter { IDNumber = 0, Name = "All" });
            SelectedWorkCenter = 0;
            if (ParetoPercentage == null)
            {
                ParetoPercentage = new List<int>();
                for (int i = 50; i <= 80; i = i + 5)
                {
                    ParetoPercentage.Add(i);
                }
            }
            SelectedPercentage = ParetoPercentage.Last();
        }

        /// <summary>
        /// Build pareto charts
        /// </summary>
        /// <param name="chartYear">Chart year filter</param>
        /// <param name="chartMonth">Chart month filter</param>
        /// <param name="chartFilter">Chart select filter statement</param>
        /// <param name="percentage">Pareto Chart Percentage</param>
        public void BuildChart(int chartYear, int chartMonth, string chartFilter, int percentage)
        {
            DataCollection = QIRChart.NCMDataAsync("Count", chartYear, chartMonth, chartFilter, SelectedWorkCenter, percentage).Result;
            if (DataCollection == null)
            {
                NoChart = true;
                OnPropertyChanged(nameof(NoChart));
                Chart = false;
                OnPropertyChanged(nameof(Chart));
                SelectedQIRNumberCost = null;
                SelectedCost = null;
                SelectedCount = null;
                CostResultsTable = null;
                OnPropertyChanged(nameof(CostResultsTable));
                CountResultsTable = null;
                OnPropertyChanged(nameof(CountResultsTable));
            }
            else
            {
                NoChart = false;
                OnPropertyChanged(nameof(NoChart));
                Chart = true;
                OnPropertyChanged(nameof(Chart));
                X_AxisCount = new List<KeyValuePair<string, int>>();
                Y_AxisCount = new List<int>();
                foreach (QIRChart o in DataCollection)
                {
                    X_AxisCount.Add(new KeyValuePair<string, int>(o.NCMCode, (int)o.Absolute));
                    Y_AxisCount.Add(o.Cumulative);
                }
                Y_AxisCount = Y_AxisCount.OrderBy(i => i).ToList();
                MaxCount = X_AxisCount.Sum(o => o.Value);
                DataCollection = QIRChart.NCMDataAsync("Cost", chartYear, chartMonth, chartFilter, SelectedWorkCenter, SelectedPercentage).Result;
                X_AxisCost = new List<KeyValuePair<string, int>>();
                Y_AxisCost = new List<double>();
                foreach (QIRChart o in DataCollection)
                {
                    X_AxisCost.Add(new KeyValuePair<string, int>(o.NCMCode, (int)o.Absolute));
                    Y_AxisCost.Add(o.Cumulative);
                }
                Y_AxisCost = Y_AxisCost.OrderBy(i => i).ToList();
                MaxCost = X_AxisCost.Sum(o => o.Value);
                OnPropertyChanged(nameof(X_AxisCount));
                OnPropertyChanged(nameof(Y_AxisCount));
                OnPropertyChanged(nameof(X_AxisCost));
                OnPropertyChanged(nameof(Y_AxisCost));
                OnPropertyChanged(nameof(MaxCount));
                OnPropertyChanged(nameof(MaxCost));
                SelectedQIRNumberCost = null;
                SelectedCost = null;
                SelectedCount = null;
                CostResultsTable = null;
                OnPropertyChanged(nameof(CostResultsTable));
                CountResultsTable = null;
                OnPropertyChanged(nameof(CountResultsTable));
            }
        }

        /// <summary>
        /// Open a Selected QIR
        /// </summary>
        /// <param name="qirNumber"></param>
        public static void OpenQIR(int qirNumber)
        {
            DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.LoadQIR(qirNumber));
            DashBoardTabControl.WorkSpace.SelectedIndex = DashBoardTabControl.WorkSpace.Items.Count - 1;
        }

        /// <summary>
        /// All Chart Command
        /// </summary>
        public ICommand AllCommand
        {
            get
            {
                if (_refresh == null)
                {
                    _refresh = new RelayCommand(AllExecute, AllCanExecute);
                }
                return _refresh;
            }
        }

        /// <summary>
        /// All Chart Execution
        /// </summary>
        /// <param name="parameter">Empty object</param>
        private void AllExecute(object parameter)
        {
            SelectedWorkCenter = 0;
            SelectedPercentage = ParetoPercentage.Last();
            BuildChart(Year, Month, Filter, SelectedPercentage);
        }
        private bool AllCanExecute(object parameter) => SelectedWorkCenter > 0 ? true : false;

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                DataCollection = null;
                X_AxisCount = null;
                Y_AxisCount = null;
                X_AxisCost = null;
                Y_AxisCost = null;
                _refresh = null;
                WorkCenterList = null;
            }
        }
    }
}
