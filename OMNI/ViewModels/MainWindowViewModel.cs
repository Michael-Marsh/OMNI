<<<<<<< HEAD
﻿using OMNI.Commands;
using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using OMNI.QMS.View;
using OMNI.QMS.ViewModel;
using OMNI.Views;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// MainWindow ViewModel
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties

        private bool loggedIn;
        public bool LoggedIn
        {
            get { return loggedIn; }
            set { value = string.IsNullOrEmpty(CurrentUser.FullName) ? false : true; loggedIn = value; OnPropertyChanged(nameof(LoggedIn)); }
        }

        private bool loggedOut;
        public bool LoggedOut
        {
            get { return loggedOut; }
            set { value = string.IsNullOrEmpty(CurrentUser.FullName) ? true : false; loggedOut = value; OnPropertyChanged(nameof(LoggedOut)); }
        }

        public bool Quality
        {
            get { return CurrentUser.Quality; }
            set { value = CurrentUser.Quality; OnPropertyChanged(nameof(Quality)); }
        }
        public bool QualityNotice
        {
            get { return CurrentUser.QualityNotice; }
            set { value = CurrentUser.QualityNotice; OnPropertyChanged(nameof(QualityNotice)); }
        }
        public bool Kaizen
        {
            get { return CurrentUser.Kaizen; }
            set { value = CurrentUser.Kaizen; OnPropertyChanged(nameof(Kaizen)); }
        }
        public bool CMMS
        {
            get { return CurrentUser.CMMS; }
            set { value = CurrentUser.CMMS; OnPropertyChanged(nameof(CMMS)); }
        }
        public bool IT
        {
            get { return CurrentUser.IT; }
            set { value = CurrentUser.IT; OnPropertyChanged(nameof(IT)); }
        }
        public bool Engineering
        {
            get { return CurrentUser.Engineering; }
            set { value = CurrentUser.Engineering; OnPropertyChanged(nameof(Engineering)); }
        }
        public bool Admin
        {
            get { return CurrentUser.Admin; }
            set { value = CurrentUser.Admin; OnPropertyChanged(nameof(Admin)); }
        }
        public bool Developer
        {
            get { return CurrentUser.Developer; }
            set { value = CurrentUser.Developer; OnPropertyChanged(nameof(Developer)); }
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }
        public string UserAccountName
        {
            get { return CurrentUser.AccountName; }
            set { value = CurrentUser.AccountName; OnPropertyChanged(nameof(UserAccountName)); }
        }
        public string TrainingMode { get { return App.TrainingStatus ? "Turn Training Off" : "Turn Training On"; } }
        public bool Training
        {
            get { return App.TrainingStatus; }
        }
        private bool _update;
        public bool UpdateAvailable
        {
            get { return _update; }
            set { _update = value; OnPropertyChanged(nameof(UpdateAvailable)); }
        }
        public int AutoClose { get; set; }
        public string TimeLeft
        {
            get { return AutoClose / 60 < 1 ? "less than a minute" : AutoClose / 60 == 1 ? "1 minute" : $"{AutoClose / 60} minutes"; }
        }
        public static Action MainWindowUpdateTick { get; set; }
        public bool DataBaseOnline
        {
            get { return App.ConConnected; }
            set { value = App.ConConnected; OnPropertyChanged(nameof(DataBaseOnline)); }
        }

        RelayCommand _login;
        RelayCommand _workSpace;
        RelayCommand _default;

        #endregion

        /// <summary>
        /// Main Window ViewModel Constructor
        /// </summary>
        public MainWindowViewModel()
        {
            var _test = UseSSO();
            if (!string.IsNullOrEmpty(_test) && CurrentUser.ExistsAsync(_test.Substring(_test.LastIndexOf('\\') + 1)).Result)
            {
                CurrentUser.LogInAsync(_test.Substring(_test.LastIndexOf('\\') + 1));
                RefreshView(true);
            }
            else
            {
                LoggedOut = true;
            }
            if (MainWindowUpdateTick == null)
            {
                MainWindowUpdateTick = new Action(MainWindowTick);
                UpdateTimer.UpdateTimerTick += MainWindowUpdateTick;
            }
            AutoClose = 999;
        }

        public string UseSSO()
        {
            return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        }

        /// <summary>
        /// Main Window action update tick
        /// </summary>
        public void MainWindowTick()
        {
            if (!UpdateAvailable)
            {
                UpdateAvailable = App.IsUpdateAvailable;
            }
            if (UpdateAvailable)
            {
                if (AutoClose == 0)
                {
                    Application.Current.Shutdown();
                }
                AutoClose = AutoClose == 999 ? 300 : AutoClose - 15;
            }
        }

        /// <summary>
        /// Refresh MainWindow View
        /// </summary>
        /// <param name="sso">Optional: Was SSO used</param>
        public void RefreshView(bool sso = false)
        {
            if (!sso)
            {
                ((MainWindowView)MainWindowView.MainWindow).Password_pwbx.Clear();
                ((MainWindowView)MainWindowView.MainWindow).UserName_tbx.Focus();
            }
            LoggedIn = LoggedOut = Quality = QualityNotice = Kaizen = CMMS = IT = Engineering = Admin = Developer = false;
            UserName = Password = UserAccountName = string.Empty;
            OnPropertyChanged(nameof(UserName));
        }

        /// <summary>
        /// Log In Command
        /// </summary>
        public ICommand LogInCommand
        {
            get
            {
                if (_login == null)
                    _login = new RelayCommand(LogInExecute, LogInCanExecute);
                return _login;
            }
        }

        /// <summary>
        /// Log In Command Execution
        /// </summary>
        /// <param name="parmeter">Empty Object</param>
        private void LogInExecute(object parmeter)
        {
            if (int.TryParse(UserName, out int empID))
            {
                UserName = OMNIDataBase.UserDomainNameFromIDAsync(empID).Result;
            }
            if (CurrentUser.Validate(UserName, Password))
            {
                if (CurrentUser.ExistsAsync(UserName).Result)
                {
                    CurrentUser.LogInAsync(UserName);
                }
                else
                {
                    using (var registrationWindowViewModel = new RegistrationWindowViewModel())
                    {
                        registrationWindowViewModel.RegisterUser(UserName);
                        CurrentUser.LogInAsync(UserName);
                    }
                }
            }
            else
            {
                MessageBox.Show("The User Name or Password you have enter is invalid.", "Invalid Credentials", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            RefreshView();
        }
        private bool LogInCanExecute(object parameter) => (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
                ? false
                : true;

        /// <summary>
        /// Work Space Command
        /// </summary>
        public ICommand WorkSpaceCommand
        {
            get
            {
                if (_workSpace == null)
                    _workSpace = new RelayCommand(WorkExecute, WorkCanExecute);
                return _workSpace;
            }
        }

        /// <summary>
        /// Work Space Command Execution
        /// </summary>
        /// <param name="parmeter">Command to call</param>
        private void WorkExecute(object parmeter)
        {
            var action = parmeter as string;
            switch (action)
            {
                case "QIREZ":
                    new QIREZFormWindowView { DataContext = new QIRFormViewModel(true) }.ShowDialog();
                    break;
                case "DashBoard":
                    try
                    {
                        if (DashBoardWindowView.DashBoardView != null)
                        {
                            DashBoardWindowView.DashBoardView.WindowState = WindowState.Maximized;
                            DashBoardWindowView.DashBoardView.Focus();
                            MainWindowView.MainWindow.WindowState = WindowState.Minimized;
                        }
                        else
                        {
                            new DashBoardWindowView().Show();
                            MainWindowView.MainWindow.WindowState = WindowState.Minimized;
                            UpdateTimer.Remove(MainWindowUpdateTick);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                    }
                    break;
                case "Turn Training On":
                    App.TrainingStatus = true;
                    OnPropertyChanged(nameof(TrainingMode));
                    OnPropertyChanged(nameof(Training));
                    break;
                case "Turn Training Off":
                    App.TrainingStatus = false;
                    OnPropertyChanged(nameof(TrainingMode));
                    OnPropertyChanged(nameof(Training));
                    break;
                case "LogOut":
                    CurrentUser.LogOut();
                    App.TrainingStatus = false;
                    OnPropertyChanged(nameof(TrainingMode));
                    OnPropertyChanged(nameof(Training));
                    RefreshView();
                    break;
            }
        }
        private bool WorkCanExecute(object parameter) => LoggedOut
                ? false
                : true;

        /// <summary>
        /// Default Command
        /// </summary>
        public ICommand DefaultCommand
        {
            get
            {
                if (_default == null)
                    _default = new RelayCommand(DefaultExecute);
                return _default;
            }
        }

        /// <summary>
        /// Default Command Execution
        /// </summary>
        /// <param name="parmeter">Command to call</param>
        private void DefaultExecute(object parmeter)
        {
            var action = parmeter as string;
            switch (action)
            {
                case "PartSearch":
                    if (!OMNIWindow<PartSearchWindowView>.IsOpen())
                    {
                        new PartSearchWindowView().Show();
                    }
                    else
                    {
                        OMNIWindow<PartSearchWindowView>.Focus();
                    }
                    break;
                case "QIRSearch":
                    if (!OMNIWindow<QIRSearchWindowView>.IsOpen())
                    {
                        new QIRSearchWindowView().Show();
                    }
                    else
                    {
                        OMNIWindow<QIRSearchWindowView>.Focus();
                    }
                    break;
                case "DocumentIndex":
                    if (!OMNIWindow<DocumentIndexWindowView>.IsOpen())
                    {
                        new DocumentIndexWindowView { DataContext = new DocumentIndexViewModel() }.Show();
                    }
                    else
                    {
                        OMNIWindow<DocumentIndexWindowView>.Focus();
                    }
                    break;
                case "Calculators":
                    if (!OMNIWindow<CalculatorsWindowView>.IsOpen())
                    {
                        new CalculatorsWindowView().Show();
                    }
                    else
                    {
                        OMNIWindow<CalculatorsWindowView>.Focus();
                    }
                    break;
                case "PlateSearch":
                    if (!OMNIWindow<PlateSearchWindowView>.IsOpen())
                    {
                        new PlateSearchWindowView().Show();
                    }
                    else
                    {
                        OMNIWindow<PlateSearchWindowView>.Focus();
                    }
                    break;
            }
        }

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                UpdateTimer.Remove(MainWindowUpdateTick);
                _default = null;
                _login = null;
                _workSpace = null;
            }
        }
    }
}
=======
﻿using OMNI.Commands;
using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using OMNI.QMS.View;
using OMNI.QMS.ViewModel;
using OMNI.Views;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// MainWindow ViewModel
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties

        private bool loggedIn;
        public bool LoggedIn
        {
            get { return loggedIn; }
            set { value = string.IsNullOrEmpty(CurrentUser.FullName) ? false : true; loggedIn = value; OnPropertyChanged(nameof(LoggedIn)); }
        }

        private bool loggedOut;
        public bool LoggedOut
        {
            get { return loggedOut; }
            set { value = string.IsNullOrEmpty(CurrentUser.FullName) ? true : false; loggedOut = value; OnPropertyChanged(nameof(LoggedOut)); }
        }

        public bool Quality
        {
            get { return CurrentUser.Quality; }
            set { value = CurrentUser.Quality; OnPropertyChanged(nameof(Quality)); }
        }
        public bool QualityNotice
        {
            get { return CurrentUser.QualityNotice; }
            set { value = CurrentUser.QualityNotice; OnPropertyChanged(nameof(QualityNotice)); }
        }
        public bool Kaizen
        {
            get { return CurrentUser.Kaizen; }
            set { value = CurrentUser.Kaizen; OnPropertyChanged(nameof(Kaizen)); }
        }
        public bool CMMS
        {
            get { return CurrentUser.CMMS; }
            set { value = CurrentUser.CMMS; OnPropertyChanged(nameof(CMMS)); }
        }
        public bool IT
        {
            get { return CurrentUser.IT; }
            set { value = CurrentUser.IT; OnPropertyChanged(nameof(IT)); }
        }
        public bool Engineering
        {
            get { return CurrentUser.Engineering; }
            set { value = CurrentUser.Engineering; OnPropertyChanged(nameof(Engineering)); }
        }
        public bool Admin
        {
            get { return CurrentUser.Admin; }
            set { value = CurrentUser.Admin; OnPropertyChanged(nameof(Admin)); }
        }
        public bool Developer
        {
            get { return CurrentUser.Developer; }
            set { value = CurrentUser.Developer; OnPropertyChanged(nameof(Developer)); }
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }
        public string UserAccountName
        {
            get { return CurrentUser.AccountName; }
            set { value = CurrentUser.AccountName; OnPropertyChanged(nameof(UserAccountName)); }
        }
        public string TrainingMode { get { return App.TrainingStatus ? "Turn Training Off" : "Turn Training On"; } }
        public bool Training
        {
            get { return App.TrainingStatus; }
        }
        private bool _update;
        public bool UpdateAvailable
        {
            get { return _update; }
            set { _update = value; OnPropertyChanged(nameof(UpdateAvailable)); }
        }
        public int AutoClose { get; set; }
        public string TimeLeft
        {
            get { return AutoClose / 60 < 1 ? "less than a minute" : AutoClose / 60 == 1 ? "1 minute" : $"{AutoClose / 60} minutes"; }
        }
        public static Action MainWindowUpdateTick { get; set; }
        public bool DataBaseOnline
        {
            get { return App.ConConnected; }
            set { value = App.ConConnected; OnPropertyChanged(nameof(DataBaseOnline)); }
        }

        RelayCommand _login;
        RelayCommand _workSpace;
        RelayCommand _default;

        #endregion

        /// <summary>
        /// Main Window ViewModel Constructor
        /// </summary>
        public MainWindowViewModel()
        {
            var _test = UseSSO();
            if (!string.IsNullOrEmpty(_test) && CurrentUser.ExistsAsync(_test.Substring(_test.LastIndexOf('\\') + 1)).Result)
            {
                CurrentUser.LogInAsync(_test.Substring(_test.LastIndexOf('\\') + 1));
                RefreshView(true);
            }
            else
            {
                LoggedOut = true;
            }
            if (MainWindowUpdateTick == null)
            {
                MainWindowUpdateTick = new Action(MainWindowTick);
                UpdateTimer.UpdateTimerTick += MainWindowUpdateTick;
            }
            AutoClose = 999;
        }

        public string UseSSO()
        {
            return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        }

        /// <summary>
        /// Main Window action update tick
        /// </summary>
        public void MainWindowTick()
        {
            if (!UpdateAvailable)
            {
                UpdateAvailable = App.IsUpdateAvailable;
            }
            if (UpdateAvailable)
            {
                if (AutoClose == 0)
                {
                    Application.Current.Shutdown();
                }
                AutoClose = AutoClose == 999 ? 300 : AutoClose - 15;
            }
        }

        /// <summary>
        /// Refresh MainWindow View
        /// </summary>
        /// <param name="sso">Optional: Was SSO used</param>
        public void RefreshView(bool sso = false)
        {
            if (!sso)
            {
                ((MainWindowView)MainWindowView.MainWindow).Password_pwbx.Clear();
                ((MainWindowView)MainWindowView.MainWindow).UserName_tbx.Focus();
            }
            LoggedIn = LoggedOut = Quality = QualityNotice = Kaizen = CMMS = IT = Engineering = Admin = Developer = false;
            UserName = Password = UserAccountName = string.Empty;
            OnPropertyChanged(nameof(UserName));
        }

        /// <summary>
        /// Log In Command
        /// </summary>
        public ICommand LogInCommand
        {
            get
            {
                if (_login == null)
                    _login = new RelayCommand(LogInExecute, LogInCanExecute);
                return _login;
            }
        }

        /// <summary>
        /// Log In Command Execution
        /// </summary>
        /// <param name="parmeter">Empty Object</param>
        private void LogInExecute(object parmeter)
        {
            if (int.TryParse(UserName, out int empID))
            {
                UserName = OMNIDataBase.UserDomainNameFromIDAsync(empID).Result;
            }
            if (CurrentUser.Validate(UserName, Password))
            {
                if (CurrentUser.ExistsAsync(UserName).Result)
                {
                    CurrentUser.LogInAsync(UserName);
                }
                else
                {
                    using (var registrationWindowViewModel = new RegistrationWindowViewModel())
                    {
                        registrationWindowViewModel.RegisterUser(UserName);
                        CurrentUser.LogInAsync(UserName);
                    }
                }
            }
            else
            {
                MessageBox.Show("The User Name or Password you have enter is invalid.", "Invalid Credentials", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            RefreshView();
        }
        private bool LogInCanExecute(object parameter) => (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
                ? false
                : true;

        /// <summary>
        /// Work Space Command
        /// </summary>
        public ICommand WorkSpaceCommand
        {
            get
            {
                if (_workSpace == null)
                    _workSpace = new RelayCommand(WorkExecute, WorkCanExecute);
                return _workSpace;
            }
        }

        /// <summary>
        /// Work Space Command Execution
        /// </summary>
        /// <param name="parmeter">Command to call</param>
        private void WorkExecute(object parmeter)
        {
            var action = parmeter as string;
            switch (action)
            {
                case "QIREZ":
                    new QIREZFormWindowView { DataContext = new QIRFormViewModel(true) }.ShowDialog();
                    break;
                case "DashBoard":
                    try
                    {
                        if (DashBoardWindowView.DashBoardView != null)
                        {
                            DashBoardWindowView.DashBoardView.WindowState = WindowState.Maximized;
                            DashBoardWindowView.DashBoardView.Focus();
                            MainWindowView.MainWindow.WindowState = WindowState.Minimized;
                        }
                        else
                        {
                            new DashBoardWindowView().Show();
                            MainWindowView.MainWindow.WindowState = WindowState.Minimized;
                            UpdateTimer.Remove(MainWindowUpdateTick);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                    }
                    break;
                case "Turn Training On":
                    App.TrainingStatus = true;
                    OnPropertyChanged(nameof(TrainingMode));
                    OnPropertyChanged(nameof(Training));
                    break;
                case "Turn Training Off":
                    App.TrainingStatus = false;
                    OnPropertyChanged(nameof(TrainingMode));
                    OnPropertyChanged(nameof(Training));
                    break;
                case "LogOut":
                    CurrentUser.LogOut();
                    App.TrainingStatus = false;
                    OnPropertyChanged(nameof(TrainingMode));
                    OnPropertyChanged(nameof(Training));
                    RefreshView();
                    break;
            }
        }
        private bool WorkCanExecute(object parameter) => LoggedOut
                ? false
                : true;

        /// <summary>
        /// Default Command
        /// </summary>
        public ICommand DefaultCommand
        {
            get
            {
                if (_default == null)
                    _default = new RelayCommand(DefaultExecute);
                return _default;
            }
        }

        /// <summary>
        /// Default Command Execution
        /// </summary>
        /// <param name="parmeter">Command to call</param>
        private void DefaultExecute(object parmeter)
        {
            var action = parmeter as string;
            switch (action)
            {
                case "PartSearch":
                    if (!OMNIWindow<PartSearchWindowView>.IsOpen())
                    {
                        new PartSearchWindowView().Show();
                    }
                    else
                    {
                        OMNIWindow<PartSearchWindowView>.Focus();
                    }
                    break;
                case "QIRSearch":
                    if (!OMNIWindow<QIRSearchWindowView>.IsOpen())
                    {
                        new QIRSearchWindowView().Show();
                    }
                    else
                    {
                        OMNIWindow<QIRSearchWindowView>.Focus();
                    }
                    break;
                case "DocumentIndex":
                    if (!OMNIWindow<DocumentIndexWindowView>.IsOpen())
                    {
                        new DocumentIndexWindowView { DataContext = new DocumentIndexViewModel() }.Show();
                    }
                    else
                    {
                        OMNIWindow<DocumentIndexWindowView>.Focus();
                    }
                    break;
                case "Calculators":
                    if (!OMNIWindow<CalculatorsWindowView>.IsOpen())
                    {
                        new CalculatorsWindowView().Show();
                    }
                    else
                    {
                        OMNIWindow<CalculatorsWindowView>.Focus();
                    }
                    break;
                case "PlateSearch":
                    if (!OMNIWindow<PlateSearchWindowView>.IsOpen())
                    {
                        new PlateSearchWindowView().Show();
                    }
                    else
                    {
                        OMNIWindow<PlateSearchWindowView>.Focus();
                    }
                    break;
            }
        }

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                UpdateTimer.Remove(MainWindowUpdateTick);
                _default = null;
                _login = null;
                _workSpace = null;
            }
        }
    }
}
>>>>>>> origin/master
