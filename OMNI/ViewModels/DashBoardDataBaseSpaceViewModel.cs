using OMNI.Commands;
using OMNI.CustomControls;
using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using OMNI.Views;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    public class DashBoardDataBaseSpaceViewModel : ViewModelBase
    {
        #region Properies

        public bool QualityView { get { return CurrentUser.Quality; } }
        public bool CMMSView { get { return CurrentUser.CMMSAdmin || CurrentUser.CMMSCrew; } }
        public bool Training { get { return App.DataBase.Contains("Train"); } }
        public bool DeveloperView { get { return CurrentUser.Developer; } }
        public bool AccountingView { get { return CurrentUser.Accounting; } }
        public bool DataBaseOnline
        {
            get { return App.SqlConAsync.State == System.Data.ConnectionState.Open; }
            set { value = App.SqlConAsync.State == System.Data.ConnectionState.Open; OnPropertyChanged(nameof(DataBaseOnline)); }
        }
        private static int _progress;
        public static int Progress
        {
            get { return _progress; }
            set { _progress = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Progress))); }
        }
        private static bool _exporting;
        public static bool Exporting
        {
            get { return _exporting; }
            set { _exporting = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Exporting))); }
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;


        RelayCommand _exportCommand;
        RelayCommand _home;
        RelayCommand _search;
        RelayCommand _edit;

        #endregion

        /// <summary>
        /// DashBoard DataBase Space ViewModel Constructor
        /// </summary>
        public DashBoardDataBaseSpaceViewModel()
        {
            Exporting = false;
            Progress = 0;
            DataBaseOnline = false;
        }

        #region Export ICommand

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
                    new DataExportFilter { DataContext = new DataExportFilterViewModel() }.Show();
                    break;
                case "WOLog":

                    break;
            }
        }
        private bool ExportCanExecute(object parameter) => Exporting ? false : true;

        #endregion

        #region DataBase Edit ICommand

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

        #endregion

        #region Home ICommand

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

        #endregion

        #region Search ICommand

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

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _exportCommand = _home = _search = null;
            }
        }
    }
}