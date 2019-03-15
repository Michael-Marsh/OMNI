using OMNI.Enumerations;
using OMNI.Extensions;
using OMNI.LCR.View;
using OMNI.LCR.ViewModel;
using OMNI.Models;
using OMNI.QMS.Calibration.View;
using OMNI.QMS.View;
using OMNI.QMS.ViewModel;
using OMNI.ViewModels;
using OMNI.Views;
using System;
using System.Windows.Controls;

namespace OMNI.CustomControls
{
    /// <summary>
    /// DashBoard TabControl Interaction Logic
    /// </summary>
    public class DashBoardTabControl : TabControl
    {
        /// <summary>
        /// Currently active DashBoardTabControl object
        /// </summary>
        public static DashBoardTabControl WorkSpace { get; set; }

        /// <summary>
        /// DashBoard TabControl Constructor
        /// </summary>
        public DashBoardTabControl()
        {
            if (WorkSpace == null)
            {
                WorkSpace = this;
                WorkSpace.Items.Add(DashBoardTabItem.Home);
            }
        }

        /// <summary>
        /// Get a TabItem based on the Tab Header
        /// </summary>
        /// <param name="tabHeader">Tab Item Header</param>
        /// <returns>TabItem Object</returns>
        public static TabItem GetTabItem(string tabHeader)
        {
            try
            {
                foreach (TabItem t in WorkSpace.Items)
                {
                    if (t.Header.ToString() == tabHeader)
                    {
                        return t;
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// DashBoard TabControl Extensions
    /// </summary>
    public static class DashBoardTabControlExtensions
    {
        /// <summary>
        /// Check to see if the TabItem is currently open
        /// </summary>
        /// <param name="tabControl">TabControl to check for open TabItem</param>
        /// <param name="header">TabItem header as string</param>
        /// <returns>True = Open; False = Not open</returns>
        public static TabItem IsOpen(this DashBoardTabControl tabControl, string header)
        {
            foreach (TabItem t in tabControl.Items)
            {
                if (t.Header.ToString() == header)
                {
                    return t;
                }
            }
            return null;
        }

        /// <summary>
        /// Check to see if the TabItem is currently open
        /// </summary>
        /// <param name="tabControl">TabControl to check for open TabItem</param>
        /// <param name="header">TabItem header as DashBoardAction</param>
        /// <returns>True = Open; False = Not open</returns>
        public static TabItem IsOpen(this DashBoardTabControl tabControl, DashBoardAction header)
        {
            try
            {
                foreach (TabItem t in tabControl.Items)
                {
                    if (t.Header.ToString() == header.GetDescription())
                    {
                        return t;
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Check to see if the TabItem is currently open
        /// </summary>
        /// <param name="tabControl">TabControl to check for open TabItem</param>
        /// <param name="header">TabItem header as DashBoardDataBase</param>
        /// <returns>True = Open; False = Not open</returns>
        public static TabItem IsOpen(this DashBoardTabControl tabControl, DashBoardDataBase header)
        {
            foreach (TabItem t in tabControl.Items)
            {
                if (t.Header.ToString() == header.GetDescription())
                {
                    return t;
                }
            }
            return null;
        }

        /// <summary>
        /// Add a New OMNI DataBase Management Tab to DashBoard WorkSpace
        /// </summary>
        /// <param name="tabControl">TabControl to check for open TabItem</param>
        /// <param name="dbEnum">Specific database to load.</param>
        public static void AddDataBaseEditorTabItem(this DashBoardTabControl tabControl, DashBoardDataBase dbEnum)
        {
            var _tabItem = tabControl.IsOpen(dbEnum);
            if (_tabItem == null)
            {
                var item = new TabItem { Header = dbEnum.GetDescription(), Content = new OMNIManagementUCView { DataContext = new OMNIManagementUCViewModel(dbEnum) } as UserControl };
                tabControl.Items.Add(item);
                tabControl.SelectedItem = item;
            }
            else
            {
                tabControl.SelectedItem = _tabItem;
            }
        }

        /// <summary>
        /// Add a New Pareto Chart View Tab to DashBoard WorkSpace
        /// </summary>
        /// <param name="tabControl">TabControl to check for open TabItem</param>
        /// <param name="year">Year to use for query</param>
        /// <param name="month">Month to use for query</param>
        /// <param name="filter">Type of Pareto chart</param>
        public static void LoadParetoChartTabItem(this DashBoardTabControl tabControl, int year, int month, string filter)
        {
            var _tabItem = tabControl.IsOpen($"{filter} QIR Charts");
            if (_tabItem == null)
            {
                var item = new TabItem { Header = $"{filter} QIR Charts", Content = new ParetoChartUCView { DataContext = new ParetoChartUCViewModel(year, month, filter) } as UserControl };
                tabControl.Items.Add(item);
                tabControl.SelectedItem = item;
            }
            else
            {
                tabControl.SelectedItem = _tabItem;
            }
        }

        /// <summary>
        /// Add a New DataBase View Tab to DashBoard WorkSpace
        /// </summary>
        /// <param name="tabControl">TabControl to check for open TabItem</param>
        /// <param name="dataBase">Table name to load.</param>
        /// <param name="filter">optional: Filter to apply on load.</param>
        /// <param name="table">optional: DataTable object to load.</param>
        public static void AddDataBaseReadOnlyTabItem(this DashBoardTabControl tabControl, string dataBase, string filter = null, System.Data.DataTable table = null)
        {
            var item = table != null
                ? new TabItem { Header = table.TableName, Content = new DataBaseUCView { DataContext = new DataBaseUCViewModel(table) } as UserControl }
                : string.IsNullOrEmpty(filter)
                    ? new TabItem { Header = dataBase, Content = new DataBaseUCView { DataContext = new DataBaseUCViewModel(dataBase) } as UserControl }
                    : new TabItem { Header = dataBase, Content = new DataBaseUCView { DataContext = new DataBaseUCViewModel(dataBase, "skip", filter) } as UserControl };
            tabControl.Items.Add(item);
            tabControl.SelectedItem = item;
        }

        /// <summary>
        /// Add a New Active DataBase View Tab to DashBoard WorkSpace
        /// </summary>.
        /// <param name="tabControl">TabControl to check for open TabItem</param>
        /// <param name="dataBase">Active DataBase to load.</param>
        public static void AddActiveDataBaseTabItem(this DashBoardTabControl tabControl, DashBoardAction dataBase)
        {
            var Item = new TabItem { Header = dataBase.GetDescription() };
            switch (dataBase)
            {
                case DashBoardAction.Exception:
                    Item.Content = new ExceptionLogUCView() as UserControl;
                    break;
                case DashBoardAction.CMMSAssigned:
                    Item.Content = new CMMSOpenOrdersUCView { DataContext = new CMMSNoticeAssignedUCViewModel() } as UserControl;
                    break;
                case DashBoardAction.CMMSPending:
                    Item.Content = new CMMSOpenOrdersUCView { DataContext = new CMMSNoticePendingUCViewModel() } as UserControl;
                    break;
                case DashBoardAction.CMMSOpen:
                    Item.Content = new CMMSOpenOrdersUCView { DataContext = new CMMSNoticeOpenOrdersUCViewModel() } as UserControl;
                    break;
                case DashBoardAction.CMMSClosed:
                    Item.Content = new CMMSOpenOrdersUCView { DataContext = new CMMSNoticeClosedUCViewModel() } as UserControl;
                    break;
                case DashBoardAction.PendingTicket:
                    Item.Content = new ITTicketNoticeUCView { DataContext = new ITNoticeUnassignedUCViewModel() } as UserControl;
                    break;
                case DashBoardAction.OpenTicket:
                    Item.Content = new ITTicketNoticeUCView { DataContext = new ITNoticeOpenUCViewModel() } as UserControl;
                    break;
                case DashBoardAction.ClosedTicket:
                    Item.Content = new ITTicketNoticeUCView { DataContext = new ITNoticeClosedUCViewModel() } as UserControl;
                    break;
                case DashBoardAction.ITProject:
                    Item.Content = new ITProjectListUCView() as UserControl;
                    break;

            }
            tabControl.Items.Add(Item);
            tabControl.SelectedItem = Item;
        }

        /// <summary>
        /// Add a New Search Results Tab to DashBoard WorkSpace
        /// </summary>
        /// <param name="tabControl">TabControl to check for open TabItem</param>
        /// <param name="query">Search string</param>
        public static void AddSearchResultsTabItem(this DashBoardTabControl tabControl, string query)
        {
            var item = new TabItem { Header = $"{query} Results", Content = new FormSearchUCView { DataContext = new FormSearchUCViewModel(query) } as UserControl };
            tabControl.Items.Add(item);
            tabControl.SelectedItem = item;
        }

        /// <summary>
        /// Add a New CMMS Work Order Form Tab to DashBoard WorkSpace
        /// </summary>
        /// <param name="tabControl">TabControl to check for open TabItem</param>
        /// <param name="cmmsWorkOrderNumber">CMMS Work Order Number to load</param>
        /// <param name="searchMode">Search Mode as bool. True = load dynamic / False = load static</param>
        public static void LoadCMMSWorkOrderTabItem(this DashBoardTabControl tabControl, int cmmsWorkOrderNumber, bool searchMode = false)
        {
            var _tabItem = tabControl.IsOpen(cmmsWorkOrderNumber.ToString());
            if (_tabItem == null)
            {
                var item = new TabItem { Header = cmmsWorkOrderNumber.ToString(), Content = new CMMSWorkOrderUCView { DataContext = new CMMSWorkOrderUCViewModel { WorkOrder = CMMSWorkOrder.LoadAsync(cmmsWorkOrderNumber).Result, IsLoaded = true, SearchMode = searchMode } } as UserControl };
                tabControl.Items.Add(item);
                tabControl.SelectedItem = item;
            }
            else
            {
                tabControl.SelectedItem = _tabItem;
            }
        }

        /// <summary>
        /// Add a New IT Ticket Form Tab to DashBoard WorkSpace
        /// </summary>
        /// <param name="tabControl">TabControl to check for open TabItem</param>
        /// <param name="itTicketNumber">IT Ticket Number to load</param>
        /// <param name="searchMode">Search Mode as bool. True = load dynamic / False = load static</param>
        public static void LoadITTicketTabItem(this DashBoardTabControl tabControl, int itTicketNumber, bool searchMode = false)
        {
            var _tabItem = tabControl.IsOpen(itTicketNumber.ToString());
            if (_tabItem == null)
            {
                var item = new TabItem { Header = itTicketNumber.ToString(), Content = new ITFormUCView(FormCommand.Update) { DataContext = new ITFormUCViewModel { Ticket = ITTicket.GetITTicketAsync(itTicketNumber).Result, SearchIDNumber = itTicketNumber, SearchMode = searchMode, LoadFromNotice = true } } as UserControl };
                tabControl.Items.Add(item);
                tabControl.SelectedItem = item;
            }
            else
            {
                tabControl.SelectedItem = _tabItem;
            }
        }

        /// <summary>
        /// Add a new IAP file review Tab to the DashBoard WorkSpace
        /// </summary>
        /// <param name="tabControl">TabControl to check for open TabItem</param>
        /// <param name="companyName">Company Name to review</param>
        public static void AddNewIAPView(this DashBoardTabControl tabControl, string companyName)
        {
            var _tabItem = tabControl.IsOpen($"{companyName} IAP");
            if (_tabItem == null)
            {
                var item = new TabItem { Header = $"{companyName} IAP", Content = new IAP.View { DataContext = new IAP.ViewModel(companyName) } as UserControl };
                tabControl.Items.Add(item);
                tabControl.SelectedItem = item;
            }
            else
            {
                tabControl.SelectedItem = _tabItem;
            }
        }
    }

    /// <summary>
    /// Default DashBoard TabControl TabItems
    /// </summary>
    public class DashBoardTabItem
    {
        #region Properties

        /// <summary>
        /// Load the DashBoard WorkSpace Home Tab
        /// </summary>
        public static TabItem Home
        {
            get
            {
                DashBoardTabControl.WorkSpace.Items.Clear();
                return new TabItem { Header = nameof(Home), Content = new HomeUCView() as UserControl };
            }
        }

        /// <summary>
        /// Add a New User Account Tab to DashBoard WorkSpace
        /// </summary>
        public static TabItem UserAccount
        {
            get
            {
                return new TabItem { Header = $"{CurrentUser.AccountName} Account", Content = new UserAccountUCView() as UserControl };
            }
        }

        /// <summary>
        /// Add a New CMMS Part Viewer Tab to DashBoard WorkSpace
        /// </summary>
        public static TabItem PartViewer
        {
            get
            {
                return new TabItem { Header = "Part Management", Content = new CMMSPartManagementUCView() as UserControl };
            }
        }

        /// <summary>
        /// Add a New CMMS Work Order Form Tab to DashBoard WorkSpace
        /// </summary>
        public static TabItem NewCMMSWorkOrder
        {
            get
            {
                return new TabItem { Header = "New Work Order", Content = new CMMSWorkOrderUCView() as UserControl };
            }
        }

        /// <summary>
        /// Add a New Update CMMS Work Order Form Tab to DashBoard WorkSpace
        /// </summary>
        public static TabItem CMMSWorkOrderSearch
        {
            get
            {
                return new TabItem { Header = "Work Order Search", Content = new CMMSWorkOrderUCView { DataContext = new CMMSWorkOrderUCViewModel { SearchMode = true, SearchEntered = false, SearchHide = false, CommandType = FormCommand.Search } } as UserControl };
            }
        }

        /// <summary>
        /// Add a New User Submissions Tab to DashBoard WorkSpace
        /// </summary>
        public static TabItem UserSubmissions
        {
            get
            {
                return new TabItem { Header = $"{CurrentUser.AccountName} Submissions", Content = new FormSearchUCView() as UserControl };
            }
        }

        /// <summary>
        /// Add a New ECR Form Tab to DashBoard WorkSpace
        /// </summary>
        public static TabItem NewECR
        {
            get
            {
                return new TabItem { Header = "New ECR", Content = new ECRFormUCView(FormCommand.Submit) as UserControl };
            }
        }

        /// <summary>
        /// Add a New Update ECR Form Tab to DashBoard WorkSpace
        /// </summary>
        public static TabItem ECRSearch
        {
            get
            {
                return new TabItem { Header = "ECR Search", Content = new ECRFormUCView(FormCommand.Search) { DataContext = new ECRFormUCViewModel { CommandType = FormCommand.Search } } as UserControl };
            }
        }

        /// <summary>
        /// Add a New Ticket Form Tab to DashBoard WorkSpace
        /// </summary>
        public static TabItem NewTicket
        {
            get
            {
                return new TabItem { Header = "New Ticket", Content = new ITFormUCView(FormCommand.Submit) as UserControl };
            }
        }

        /// <summary>
        /// Add a New Update Ticket Form Tab to DashBoard WorkSpace
        /// </summary>
        public static TabItem TicketSearch
        {
            get
            {
                return new TabItem { Header = "Ticket Search", Content = new ITFormUCView(FormCommand.Search) { DataContext = new ITFormUCViewModel { PrimaryCommandType = FormCommand.Search } } as UserControl };
            }
        }

        /// <summary>
        /// Add a New Counter Calibration Tab to the DashBoard WorkSpace
        /// </summary>
        public static TabItem CounterCal
        {
            get
            {
                return new TabItem { Header = "Counter Cal.", Content = new CounterCalView() as UserControl };
            }
        }

        /// <summary>
        /// Add a New Tapemeasure Calibration Tab to the DashBoard WorkSpace
        /// </summary>
        public static TabItem TapeCal
        {
            get
            {
                return new TabItem { Header = "Tape Cal.", Content = new TapeMeasureCalView() as UserControl };
            }
        }

        /// <summary>
        /// Add a New QIR Form Tab to DashBoard WorkSpace
        /// </summary>
        public static TabItem NewQIR
        {
            get
            {
                return new TabItem { Header = "New QIR", Content = new QIRFormView { DataContext = new QMS.ViewModel.QIRFormViewModel() } as UserControl };
            }
        }

        /// <summary>
        /// Add a New QIR Notice Tab to DashBoard WorkSpace
        /// </summary>
        public static TabItem QIRNotice
        {
            get
            {
                var qirNotice = new NoticeView { DataContext = new QIRNoticeViewModel() };
                qirNotice.FilterGridView.Children.Add(new QIRNoticeFilterView());
                qirNotice.NoticeGridView.Children.Add(new QIRNoticeDataView());
                return new TabItem { Header = DashBoardAction.QIRNotice.GetDescription(), Content = qirNotice as UserControl };
            }
        }

        /// <summary>
        /// Add a New QIR Form Tab to DashBoard WorkSpace
        /// </summary>
        /// <param name="idNumber">QIR ID Number</param>
        public static TabItem LoadQIR(int? idNumber)
        {
            return new TabItem { Header = $"QIR {idNumber}", Content = new QIRFormView { DataContext = new QIRFormViewModel(new QMS.Model.QIR(idNumber, true)) } as UserControl };
        }

        /// <summary>
        /// Add a New QIR Notice Tab to DashBoard WorkSpace
        /// </summary>
        public static TabItem ITNotice
        {
            get
            {
                var itNotice = new NoticeView { DataContext = new HDT.ViewModel.ITNoticeViewModel() };
                //itNotice.FilterGridView.Children.Add(new QIRNoticeFilterView());
                itNotice.NoticeGridView.Children.Add(new HDT.View.ITNoticeDataView());
                return new TabItem { Header = DashBoardAction.ITNotice.GetDescription(), Content = itNotice as UserControl };
            }
        }

        /// <summary>
        /// Add a New QIR Form Tab to DashBoard WorkSpace
        /// </summary>
        /// <param name="idNumber">Ticket ID Number</param>
        public static TabItem LoadHDT(int? idNumber)
        {
            return new TabItem { Header = $"Ticket {idNumber}" };
        }

        /// <summary>
        /// Add a New Call Log Report Form Tab to DashBoard WorkSpace
        /// </summary>
        public static TabItem NewCallReport
        {
            get
            {
                return new TabItem { Header = $"New Call Report", Content = new CallReport { DataContext = new CallReportVM() } as UserControl };
            }
        }

        #endregion
    }
}
