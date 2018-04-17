using OMNI.Commands;
using OMNI.CustomControls;
using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// DashBoard Action Space ViewModel Interaction Logic
    /// </summary>
    public class DashBoardActionSpaceViewModel : CurrentUser, INotifyPropertyChanged, IDisposable
    {
        #region Properties

        public int? QIRNotice { get { return QualityNotice ? OMNIDataBase.CountNullValuesAsync("qir_notice", IdNumber.ToString()).Result : 0; } }
        public int? ExceptionInbox { get { return Developer ? OMNIDataBase.CountWithValuesAsync("exceptionlog", "Handled", "0").Result : 0; } }
        public int? CMMSInbox { get { return CMMSView ? OMNIDataBase.CountWithComparisonAsync("cmmsworkorder", "`Status` IN ('Pending', 'Reviewing')").Result : 0; } }
        public int? CMMSOpen { get { return CMMSView ? OMNIDataBase.CountWithComparisonAsync("cmmsworkorder", $"`Status`='Assigned'").Result : 0; } }
        public int? CMMSNotice { get { return CMMSView ? OMNIDataBase.CountWithComparisonAsync("cmmsworkorder", "`Status` NOT IN ('Completed', 'Denied')").Result : 0; } }
        public int? ITInbox { get { return IT ? OMNIDataBase.CountWithComparisonAsync("it_ticket_master", "`Status`='Pending' AND `Type`<>'Project'").Result : 0; } }
        public int? ITOpen { get { return IT ? OMNIDataBase.CountWithComparisonAsync("it_ticket_master", "`Status`='Assigned' AND `Type`<>'Project'").Result : 0; } }
        public int? ITProject { get { return IT ? OMNIDataBase.CountWithComparisonAsync("it_ticket_master", "`Type`<>'Ticket'").Result : 0; } }
        public bool CanUpdate { get { return App.IsUpdateAvailable; } }
        public bool CMMSView { get { return CMMS || CMMSCrew || CMMSAdmin; } }
        public bool ElevatedCMMSView { get { return CMMSAdmin || CMMSCrew; } }

        RelayCommand _action;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// DashBoard Action Space ViewModel Constructor
        /// </summary>
        public DashBoardActionSpaceViewModel()
        {
            //UpdateBoxValues();
            UpdateTimer.Add(UpdateBoxValues);
        }

        /// <summary>
        /// Update all inbox and notice values
        /// </summary>
        private void UpdateBoxValues()
        {
            OnPropertyChanged(nameof(QIRNotice));
            OnPropertyChanged(nameof(ExceptionInbox));
            OnPropertyChanged(nameof(CMMSNotice));
            OnPropertyChanged(nameof(CMMSInbox));
            OnPropertyChanged(nameof(CMMSOpen));
            OnPropertyChanged(nameof(CanUpdate));
            OnPropertyChanged(nameof(ITInbox));
            OnPropertyChanged(nameof(ITOpen));
            OnPropertyChanged(nameof(ITProject));
        }

        /// <summary>
        /// DashBoard Action Space Interface Command
        /// </summary>
        public ICommand ActionSpaceCommand
        {
            get
            {
                if (_action == null)
                    _action = new RelayCommand(ActionExecute);
                return _action;
            }
        }
        private void ActionExecute(object parameter)
        {
            var cmdAction = (DashBoardAction)Enum.Parse(typeof(DashBoardAction), parameter as string);
            var _tabItem = DashBoardTabControl.WorkSpace.IsOpen(cmdAction);
            if (_tabItem == null)
            {
                switch (cmdAction)

                {
                    case DashBoardAction.SubmitQIR:
                        DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.NewQIR);
                        break;
                    case DashBoardAction.MapForm:
                        MapForm.TypePDF();
                        break;
                    case DashBoardAction.CreateWO:
                        DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.NewCMMSWorkOrder);
                        break;
                    case DashBoardAction.UpdateWO:
                        DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.CMMSWorkOrderSearch);
                        break;
                    case DashBoardAction.ViewPart:
                        DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.PartViewer);
                        break;
                    case DashBoardAction.UserAccount:
                        DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.UserAccount);
                        break;
                    case DashBoardAction.UpdateInfo:
                        App.ManualUpdate();
                        break;
                    case DashBoardAction.UpdateOMNI:
                        App.Update();
                        break;
                    case DashBoardAction.UserSubmission:
                        DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.UserSubmissions);
                        break;
                    case DashBoardAction.DevTesting:
                        //Add in any method calls to test here
                        break;
                    case DashBoardAction.SubmitECR:
                        DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.NewECR);
                        break;
                    case DashBoardAction.UpdateECR:
                        DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.ECRSearch);
                        break;
                    case DashBoardAction.CreateTicket:
                        DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.NewTicket);
                        break;
                    case DashBoardAction.SearchTicket:
                        DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.TicketSearch);
                        break;
                    case DashBoardAction.ReviewImport:
                        ReviewImportViewModel.Open();
                        break;
                    case DashBoardAction.TapeMeasureCalibration:
                        break;
                    case DashBoardAction.SlitterCalibration:
                        DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.SlitterCalibration);
                        break;
                    case DashBoardAction.QIRNotice:
                        DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.QIRNotice);
                        break;
                    case DashBoardAction.ITNotice:
                        DashBoardTabControl.WorkSpace.Items.Add(DashBoardTabItem.ITNotice);
                        break;
                    case DashBoardAction.ITProject:
                    case DashBoardAction.Exception:
                    case DashBoardAction.CMMSAssigned:
                    case DashBoardAction.CMMSPending:
                    case DashBoardAction.CMMSOpen:
                    case DashBoardAction.CMMSClosed:
                    case DashBoardAction.PendingTicket:
                    case DashBoardAction.OpenTicket:
                    case DashBoardAction.ClosedTicket:
                        DashBoardTabControl.WorkSpace.AddActiveDataBaseTabItem(cmdAction);
                        break;
                }
                DashBoardTabControl.WorkSpace.SelectedIndex = DashBoardTabControl.WorkSpace.Items.Count > 0
                    ? DashBoardTabControl.WorkSpace.Items.Count - 1
                    : 0;
            }
            else
            {
                DashBoardTabControl.WorkSpace.SelectedItem = _tabItem;
            }
            OnPropertyChanged(nameof(DashBoardTabControl.WorkSpace));
        }

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
                _action = null;
                UpdateTimer.Remove(UpdateBoxValues);
            }
        }
    }
}