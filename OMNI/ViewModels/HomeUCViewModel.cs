using OMNI.Commands;
using OMNI.CustomControls;
using OMNI.Enumerations;
using OMNI.QMS.Enumeration;
using OMNI.Helpers;
using OMNI.Models;
using OMNI.QMS.Model;
using OMNI.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;

namespace OMNI.ViewModels
{
    public class HomeUCViewModel : ViewModelBase
    {
        #region Properties

        public bool QualityView
        {
            get { return CurrentUser.Quality; }
        }
        public bool SlitterView
        {
            get { return CurrentUser.SlitterLead; }
        }
        public bool AccountingView
        {
            get { return CurrentUser.Accounting; }
        }
        public bool CMMSView { get { return CurrentUser.CMMS || CurrentUser.CMMSAdmin || CurrentUser.CMMSCrew; } }
        public List<Month> MonthList { get; set; }
        private string selectedInternalMonth;
        public string SelectedInternalMonth
        {
            get { return selectedInternalMonth; }
            set
            {
                if (!Loading && SelectedInternalMonth != value) QIRValues.UpdateMTDMetrics(value, SelectedInternalYear, QMSMetricType.Internal);
                selectedInternalMonth = value;
                OnPropertyChanged(nameof(QIRValues));
                OnPropertyChanged(nameof(SelectedInternalMonth));
            }
        }
        private string selectedIncomingMonth;
        public string SelectedIncomingMonth
        {
            get { return selectedIncomingMonth; }
            set
            {
                if (!Loading && selectedIncomingMonth != value) QIRValues.UpdateMTDMetrics(value, SelectedIncomingYear, QMSMetricType.Incoming);
                selectedIncomingMonth = value;
                OnPropertyChanged(nameof(QIRValues));
                OnPropertyChanged(nameof(SelectedIncomingMonth));
            }
        }
        public List<int> YearList { get; set; }
        private int selectedInternalYear;
        public int SelectedInternalYear
        {
            get { return selectedInternalYear; }
            set
            {
                if (!Loading && selectedInternalYear != value) QIRValues.UpdateMTDMetrics(SelectedInternalMonth, value, QMSMetricType.Internal);
                selectedInternalYear = value;
                OnPropertyChanged(nameof(QIRValues));
                OnPropertyChanged(nameof(SelectedInternalYear));
            }
        }
        private int selectedIncomingYear;
        public int SelectedIncomingYear
        {
            get { return selectedIncomingYear; }
            set
            {
                if (!Loading && selectedIncomingYear != value) QIRValues.UpdateMTDMetrics(SelectedIncomingMonth, value, QMSMetricType.Incoming);
                selectedIncomingYear = value;
                OnPropertyChanged(nameof(QIRValues));
                OnPropertyChanged(nameof(SelectedIncomingYear));
            }
        }
        private string selectedMonthSales;
        public string SelectedMonthSales
        {
            get { return selectedMonthSales; }
            set { if (!Loading && selectedMonthSales != value) { UpdateSales(value, SelectedYearSales); } selectedMonthSales = value; OnPropertyChanged(nameof(SelectedMonthSales)); }
        }
        private int selectedYearSales;
        public int SelectedYearSales
        {
            get { return selectedYearSales; }
            set { if (!Loading && selectedYearSales != value) { UpdateSales(SelectedMonthSales, value); } selectedYearSales = value; OnPropertyChanged(nameof(SelectedYearSales)); }
        }
        private string selectedWorkOrderMonth;
        public string SelectedWorkOrderMonth
        {
            get { return selectedWorkOrderMonth; }
            set { selectedWorkOrderMonth = value; if (!Loading) { UpdateCMMS(); } OnPropertyChanged(nameof(SelectedWorkOrderMonth)); }
        }
        private int selectedWorkOrderYear;
        public int SelectedWorkOrderYear
        {
            get { return selectedWorkOrderYear; }
            set { selectedWorkOrderYear = value; if (!Loading) { UpdateCMMS(); } OnPropertyChanged(nameof(SelectedWorkOrderYear)); }
        }
        private string selectedTicketMonth;
        public string SelectedTicketMonth
        {
            get { return selectedTicketMonth; }
            set { selectedTicketMonth = value; if (!Loading) { UpdateHDT(); } OnPropertyChanged(nameof(SelectedTicketMonth)); }
        }
        private int selectedTicketYear;
        public int SelectedTicketYear
        {
            get { return selectedTicketYear; }
            set { selectedTicketYear = value; if (!Loading) { UpdateHDT(); } OnPropertyChanged(nameof(SelectedTicketYear)); }
        }
        public int MonthlySales { get; set; }
        public bool SalesFirm { get; set; }
        public int TMInService { get; set; }
        public int TMOverDue { get; set; }
        public int TMAlmostDue { get; set; }
        public int WorkOrderYTDSubmissions { get; set; }
        public int WorkOrderYTDCompletedCount { get; set; }
        public int WorkOrderYTDResponseTime { get; set; }
        public int WorkOrderMTDSubmissions { get; set; }
        public int WorkOrderMTDCompletedCount { get; set; }
        public int WorkOrderMTDResponseTime { get; set; }
        public int TicketYTDSubmissions { get; set; }
        public int TicketYTDCompletedCount { get; set; }
        public int TicketYTDResponseTime { get; set; }
        public int TicketMTDSubmissions { get; set; }
        public int TicketMTDCompletedCount { get; set; }
        public int TicketMTDResponseTime { get; set; }
        public QIRMetric QIRValues { get; set; }
        public bool Loading { get; set; }

        RelayCommand _update;
        RelayCommand _charts;
        RelayCommand _cal;
        RelayCommand _calLog;
        RelayCommand _metric;

        #endregion

        /// <summary>
        /// DashBoard Home UC ViewModel Constructor
        /// </summary>
        public HomeUCViewModel()
        {
            if (MonthList == null)
            {
                MonthList = Enum.GetValues(typeof(Month)).Cast<Month>().ToList();
            }
            if (YearList == null)
            {
                YearList = new List<int>();
                for (int i = 2015; i < DateTime.Now.Year + 1; i++)
                {
                    YearList.Add(i);
                }
            }
            Loading = true;
            SelectedInternalMonth = SelectedIncomingMonth = SelectedWorkOrderMonth = SelectedTicketMonth = SelectedMonthSales = DateTime.Now.ToString("MMMM");
            SelectedInternalYear = SelectedIncomingYear = SelectedWorkOrderYear = SelectedTicketYear = SelectedYearSales = DateTime.Now.Year;
            Task.Run(() => UpdateSales(SelectedMonthSales, SelectedYearSales));
            Task.Run(() => UpdateCMMS());
            Task.Run(() => UpdateHDT());
        }

        /// <summary>
        /// Update selected sales
        /// </summary>
        /// <param name="month">Selected Month</param>
        /// <param name="year">Selected Year</param>
        public void UpdateSales(string month, int year)
        {
            if (!string.IsNullOrEmpty(month) && year != 0)
            {
                var i = OMNIDataBase.MonthlySalesAsync(month, year).Result;
                MonthlySales = Convert.ToBoolean(i[1]) ? i[0] : M2k.GetLiveSales($"{DateTime.Today.Month}-1-{DateTime.Today.Year}", DateTime.Today.ToString("MM-dd-yyyy"));
                if (QIRValues == null)
                {
                    QIRValues = new QIRMetric(MonthlySales);
                    OnPropertyChanged(nameof(QIRValues));
                }
                SalesFirm = Convert.ToBoolean(i[1]);
                OnPropertyChanged(nameof(MonthlySales));
                OnPropertyChanged(nameof(SalesFirm));
                Loading = false;
                OnPropertyChanged(nameof(Loading));
            }
        }

        /// <summary>
        /// Update Tape Measure Metrics
        /// </summary>
        public void UpdateTapeMeasureMetrics()
        {
            if (CurrentUser.Quality)
            {
                try
                {
                    TMInService = (int)OMNIDataBase.CountWithComparison("caltapemeasure", "`Status`='In Service'");
                    TMOverDue = Convert.ToInt32(OMNIDataBase.CountWithComparison("caltapemeasure", $"`CalDueDate`<'{DateTime.Today.ToString("yyyy-MM-dd")}'"));
                    TMAlmostDue = Convert.ToInt32(OMNIDataBase.CountWithComparison("caltapemeasure", $"`CalDueDate`>'{DateTime.Today.ToString("yyyy-MM-dd")}' AND `CalDueDate`<'{DateTime.Today.AddMonths(1).ToString("yyyy-MM-dd")}'"));
                }
                catch (InvalidOperationException)
                {
                    ExceptionWindow.Show("No Calibration Data", "OMNI is currently uable to load the current tape measure metrics.\nIf you feel you have reached this message in error please contact IT.");
                }
                catch (Exception ex)
                {
                    ExceptionWindow.Show("Unhandled Exception", "Unable to load current calibration metrics.", ex);
                }
            }
        }

        /// <summary>
        /// Update the Monthly Sales Command
        /// </summary>
        public ICommand UpdateSalesCommand
        {
            get
            {
                if (_update == null)
                {
                    _update = new RelayCommand(UpdateSalesExecute);
                }
                return _update;
            }
        }

        /// <summary>
        /// Update Sales Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private static void UpdateSalesExecute(object parameter)
        {
            var updateWindow = new UpdateSalesWindowView();
            updateWindow.ShowDialog();
        }

        /// <summary>
        /// Update CMMS Metrics
        /// </summary>
        private void UpdateCMMS()
        {
            WorkOrderYTDCompletedCount = CMMSWorkOrder.GetMetrics(MetricType.Completed);
            OnPropertyChanged(nameof(WorkOrderYTDCompletedCount));
            WorkOrderYTDSubmissions = CMMSWorkOrder.GetMetrics(MetricType.Submission);
            OnPropertyChanged(nameof(WorkOrderYTDSubmissions));
            WorkOrderYTDResponseTime = CMMSWorkOrder.GetMetrics(MetricType.ResponseTime);
            OnPropertyChanged(nameof(WorkOrderYTDResponseTime));
            WorkOrderMTDCompletedCount = CMMSWorkOrder.GetMetrics(MetricType.Completed, DateTime.ParseExact(SelectedWorkOrderMonth, "MMMM", CultureInfo.CurrentCulture).Month, SelectedWorkOrderYear);
            OnPropertyChanged(nameof(WorkOrderMTDCompletedCount));
            WorkOrderMTDSubmissions = CMMSWorkOrder.GetMetrics(MetricType.Submission, DateTime.ParseExact(SelectedWorkOrderMonth, "MMMM", CultureInfo.CurrentCulture).Month, SelectedWorkOrderYear);
            OnPropertyChanged(nameof(WorkOrderMTDSubmissions));
            WorkOrderMTDResponseTime = CMMSWorkOrder.GetMetrics(MetricType.ResponseTime, DateTime.ParseExact(SelectedWorkOrderMonth, "MMMM", CultureInfo.CurrentCulture).Month, SelectedWorkOrderYear);
            OnPropertyChanged(nameof(WorkOrderMTDResponseTime));
        }

        /// <summary>
        /// Update HDT Metrics
        /// </summary>
        private void UpdateHDT()
        {
            TicketYTDCompletedCount = ITTicket.GetMetrics(MetricType.Completed);
            OnPropertyChanged(nameof(TicketYTDCompletedCount));
            TicketYTDSubmissions = ITTicket.GetMetrics(MetricType.Submission);
            OnPropertyChanged(nameof(TicketYTDSubmissions));
            TicketYTDResponseTime = ITTicket.GetMetrics(MetricType.ResponseTime);
            OnPropertyChanged(nameof(TicketYTDResponseTime));
            TicketMTDCompletedCount = ITTicket.GetMetrics(MetricType.Completed, DateTime.ParseExact(SelectedTicketMonth, "MMMM", CultureInfo.CurrentCulture).Month, SelectedTicketYear);
            OnPropertyChanged(nameof(TicketMTDCompletedCount));
            TicketMTDSubmissions = ITTicket.GetMetrics(MetricType.Submission, DateTime.ParseExact(SelectedTicketMonth, "MMMM", CultureInfo.CurrentCulture).Month, SelectedTicketYear);
            OnPropertyChanged(nameof(TicketMTDSubmissions));
            TicketMTDResponseTime = ITTicket.GetMetrics(MetricType.ResponseTime, DateTime.ParseExact(SelectedTicketMonth, "MMMM", CultureInfo.CurrentCulture).Month, SelectedTicketYear);
            OnPropertyChanged(nameof(TicketMTDResponseTime));
        }

        /// <summary>
        /// Chart Viewer Command
        /// </summary>
        public ICommand ViewChartCommand
        {
            get
            {
                if (_charts == null)
                {
                    _charts = new RelayCommand(ViewChartExecute);
                }
                return _charts;
            }
        }

        /// <summary>
        /// Chart Viewer Command Execution
        /// </summary>
        /// <param name="parameter">Type of Chart</param>
        private void ViewChartExecute(object parameter)
        {
            var _year = 0;
            var _month = 0;
            switch (parameter as string)
            {
                case "InternalYTD":
                    _year = DateTime.Today.Year;
                    break;
                case "InternalMTD":
                    _year = SelectedInternalYear;
                    _month = DateTime.ParseExact(SelectedInternalMonth, "MMMM", CultureInfo.CurrentCulture).Month;
                    break;
                case "IncomingYTD":
                    _year = DateTime.Today.Year;
                    break;
                case "IncomingMTD":
                    _year = SelectedIncomingYear;
                    _month = DateTime.ParseExact(SelectedIncomingMonth, "MMMM", CultureInfo.CurrentCulture).Month;
                    break;
            }
            DashBoardTabControl.WorkSpace.LoadParetoChartTabItem(_year, _month, parameter as string);
        }

        /// <summary>
        /// Cal Review Command
        /// </summary>
        public ICommand ViewCalCommand
        {
            get
            {
                if (_cal == null)
                {
                    _cal = new RelayCommand(ViewCalExecute);
                }
                return _cal;
            }
        }

        /// <summary>
        /// Cal Review Command Execution
        /// </summary>
        /// <param name="parameter">Filter</param>
        private void ViewCalExecute(object parameter)
        {
            var db = parameter as string;
            if (db == "TapeOverDue" && TMOverDue > 0)
            {
                DashBoardTabControl.WorkSpace.AddDataBaseReadOnlyTabItem("Cal Tape Review", $"CalDueDate<#{DateTime.Today.ToString("yyyy-MM-dd")}#");
            }
            else if (db == "TapeAlmostDue" && TMAlmostDue > 0)
            {
                DashBoardTabControl.WorkSpace.AddDataBaseReadOnlyTabItem("Cal Tape Review", $"`Due Date`>#{DateTime.Today.ToString("yyyy-MM-dd")}# AND `Due Date`<=#{DateTime.Today.AddMonths(1).ToString("yyyy-MM-dd")}#");
            }
            else
            {
                ExceptionWindow.Show("Up to date!", "All devices are up to date!");
            }
        }

        /// <summary>
        /// Cal Log View Command
        /// </summary>
        public ICommand ViewCalLogCommand
        {
            get
            {
                if (_calLog == null)
                {
                    _calLog = new RelayCommand(ViewCalLogExecute);
                }
                return _calLog;
            }
        }

        /// <summary>
        /// Cal Log View Command Execution
        /// </summary>
        /// <param name="parameter">Filter</param>
        private static void ViewCalLogExecute(object parameter)
        {
            switch (parameter as string)
            {
                case "TML":
                    DashBoardTabControl.WorkSpace.AddDataBaseReadOnlyTabItem("Cal Tape Review");
                    DataBaseUCViewModel.WorkCenterView = true;
                    break;
                case "IL":
                    //TODO: finish DashBoardTab.AddDataBaseView("Cal Tape Review");
                    DataBaseUCViewModel.WorkCenterView = true;
                    break;
            }
        }

        /// <summary>
        /// Metric Rollback Command
        /// </summary>
        public ICommand MetricCommand
        {
            get
            {
                if (_metric == null)
                {
                    _metric = new RelayCommand(MetricExecute);
                }
                return _metric;
            }
        }

        /// <summary>
        /// Metric Rollback Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private void MetricExecute(object parameter)
        {
            var _year = DateTime.Now.Year;
            var _yearStart = string.Empty;
            var _yearEnd = string.Empty;
            if (SelectedIncomingMonth == "January")
            {
                _yearStart = DateTime.Parse($"{DateTime.Now.AddYears(-1).Year}-01-01").ToString("yyyy-MM-dd");
                _yearEnd = DateTime.Parse($"{DateTime.Now.AddYears(-1).Year}-{DateTime.Now.AddMonths(-1).Month}-{DateTime.DaysInMonth(DateTime.Now.AddYears(-1).Year, DateTime.Now.AddMonths(-1).Month)}").ToString("yyyy-MM-dd");
                SelectedIncomingYear = SelectedInternalYear = SelectedYearSales = _year = DateTime.Now.AddYears(-1).Year;
            }
            else
            {
                _yearStart = DateTime.Parse($"{DateTime.Now.Year}-01-01").ToString("yyyy-MM-dd");
                _yearEnd = DateTime.Parse($"{DateTime.Now.Year}-{DateTime.Now.AddMonths(-1).Month}-{DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month)}").ToString("yyyy-MM-dd");
            }
            SelectedIncomingMonth = null;
            SelectedIncomingMonth = DateTime.Now.AddMonths(-1).ToString("MMMM");
            SelectedInternalMonth = null;
            SelectedInternalMonth = DateTime.Now.AddMonths(-1).ToString("MMMM");
            SelectedMonthSales = null;
            SelectedMonthSales = DateTime.Now.AddMonths(-1).ToString("MMMM");
        }

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                MonthList = null;
                YearList = null;
            }
        }
    }
}
