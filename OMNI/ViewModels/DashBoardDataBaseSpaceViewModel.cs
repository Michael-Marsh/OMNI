using OMNI.Commands;
using OMNI.CustomControls;
using OMNI.Enumerations;
using OMNI.Extensions;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    public class DashBoardDataBaseSpaceViewModel : OMNIDataTable, INotifyPropertyChanged, IDisposable
    {
        #region Properies

        public omniDataSet omniDS { get; set; }

        public bool QualityView { get { return CurrentUser.Quality; } }
        public bool CMMSView { get { return CurrentUser.CMMSAdmin || CurrentUser.CMMSCrew; } }
        public bool Training { get { return MainWindowViewModel.TrainingStatus; } }
        public bool DeveloperView { get { return CurrentUser.Developer; } }
        public bool DataBaseOnline
        {
            get { return App.ConConnected; }
            set { value = App.ConConnected; OnPropertyChanged(nameof(DataBaseOnline)); }
        }

        RelayCommand _exportCommand;
        RelayCommand _home;
        RelayCommand _search;
        RelayCommand _edit;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// DashBoard DataBase Space ViewModel Constructor
        /// </summary>
        public DashBoardDataBaseSpaceViewModel()
        {
            if (omniDS == null)
            {
                omniDS = new omniDataSet();
            }
            Exporting = false;
            Progress = 0;
            DataBaseOnline = false;
        }

        /// <summary>
        /// DashBoard DataBase Export Command
        /// </summary>
        public ICommand ExportCommand
        {
            get
            {
                if (_exportCommand == null)
                    _exportCommand = new RelayCommand(ExportExecute, ExportCanExecute);
                return _exportCommand;
            }
        }

        /// <summary>
        /// DashBoard DataBase Export Command Execution
        /// </summary>
        /// <param name="parameter">Table to export</param>
        private void ExportExecute(object parameter)
        {
            var action = parameter as string;
            switch (action)
            {
                case "QIRMaster":
                    using (var qirmasterviewTableAdapter = new omniDataSetTableAdapters.qir_master_viewTableAdapter())
                    {
                        qirmasterviewTableAdapter.Fill(omniDS.qir_master_view);
                        var dt = omniDS.qir_master_view.Copy();
                        ExportToExcel(dt, "QIR Master");
                    }
                    break;
                case "WOLog":
                    using (var cmmsworkorderviewTableAdapter = new omniDataSetTableAdapters.cmmsworkorderviewTableAdapter())
                    {
                        cmmsworkorderviewTableAdapter.Fill(omniDS.cmmsworkorderview);
                        var dt = omniDS.cmmsworkorderview.Copy();
                        ExportToExcel(dt, "CMMS WO Log");
                    }
                    break;
            }
        }
        private bool ExportCanExecute(object parameter) => Exporting ? false : true;

        /// <summary>
        /// DashBoard DataBase Edit Command
        /// </summary>
        public ICommand DataBaseEditCommand
        {
            get
            {
                if (_edit == null)
                    _edit = new RelayCommand(DataBaseEditExecute);
                return _edit;
            }
        }

        /// <summary>
        /// DashBoard DataBase Edit Command Execution
        /// </summary>
        /// <param name="parameter">DashBoardDataBase</param>
        private void DataBaseEditExecute(object parameter)
        {
            DashBoardTabControl.WorkSpace.AddDataBaseEditorTabItem((DashBoardDataBase)Enum.Parse(typeof(DashBoardDataBase), parameter.ToString()));
        }

        /// <summary>
        /// DashBoard Home Command
        /// </summary>
        public ICommand HomeCommand
        {
            get
            {
                if (_home == null)
                    _home = new RelayCommand(HomeExecute);
                return _home;
            }
        }

        /// <summary>
        /// DashBoard Home Command Execution
        /// </summary>
        /// <param name="parameter">Empty object</param>
        private void HomeExecute(object parameter)
        {
            DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.Home);
            DashBoardTabControl.WorkSpace.SelectedIndex = 0;
            foreach (Action action in UpdateTimer.UpdateTimerTick.GetInvocationList())
            {
                if (!action.Method.Name.Equals("UpdateBoxValues"))
                {
                    UpdateTimer.UpdateTimerTick -= action;
                }
            }
        }

        /// <summary>
        /// Search Command
        /// </summary>
        public ICommand SearchCommand
        {
            get
            {
                if (_search == null)
                    _search = new RelayCommand(SearchExecute, SearchCanExecute);
                return _search;
            }
        }

        /// <summary>
        /// Search Command Execution
        /// </summary>
        /// <param name="parameter">Search string</param>
        private void SearchExecute(object parameter)
        {
            DashBoardTabControl.WorkSpace.AddSearchResultsTabItem(parameter.ToString());
        }
        private bool SearchCanExecute(object parameter) => string.IsNullOrWhiteSpace(parameter as string) || (parameter as string).IndexOfAny(@"'<>()`~&%$#@[]*".ToCharArray()) >= 0 ? false : true;

        /// <summary>
        /// Reflects changes from the ViewModel properties to the View
        /// </summary>
        /// <param name="propertyName">Property Name</param>
        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        /// <summary>
        /// Object Disposal
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                omniDS.Dispose();
                _exportCommand = _home = _search = null;
            }
        }
    }
}
