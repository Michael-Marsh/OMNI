using OMNI.Commands;
using OMNI.CustomControls;
using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// CMMS Work Order UserControl ViewModel Interaction Logic
    /// </summary>
    public class CMMSWorkOrderUCViewModel : ViewModelBase
    {
        #region Properties

        public CMMSWorkOrder WorkOrder { get; set; }
        public ObservableCollection<LinkedForms> FormLinks { get; set; }
        public bool SearchMode { get; set; }
        public bool SearchEntered { get; set; }
        public bool SearchHide { get; set; }
        public bool Running { get; set; }
        public double Fade { get; set; }
        private string completeCommandType;
        public string CompleteCommandType
        {
            get
            { return completeCommandType; }
            set
            {
                value = WorkOrder != null && (WorkOrder.Status == CMMSStatus.Completed || WorkOrder.Status == CMMSStatus.Denied) ? "Re-Open" : "Complete";
                completeCommandType = value;
                OnPropertyChanged(nameof(CompleteCommandType));
            }

        }
        public string CompleteCommandTT
        {
            get { return CompleteCommandType == "Complete" ? "Complete work order\nA note will be required and the work order will be marked as closed." : "Re-Open the work order for completion."; }
        }
        public FormAction ActionTaken
        {
            get { return CommandType == FormCommand.Submit && Running ? FormAction.Submitted : CommandType == FormCommand.Update && Running ? FormAction.Updated : FormAction.None; }
        }
        public bool AssignView
        {
            get { return WorkOrder.DateAssigned != DateTime.MinValue ? true : false; }
        }
        public bool ClosedView
        {
            get { return WorkOrder.DateComplete != DateTime.MinValue ? true : false; }
        }
        public string FinishedStatus
        {
            get { return WorkOrder.DateComplete != DateTime.MinValue ? WorkOrder.Status.ToString() : string.Empty; }
        }
        private int? _search;
        public int? SearchIDNumber
        {
            get { return _search; }
            set
            {
                if (_search != value)
                {
                    ((TabItem)(DashBoardTabControl.WorkSpace).SelectedItem).Header = "Update Work Order";
                    SearchEntered = false;
                    OnPropertyChanged(nameof(SearchEntered));
                    CommandType = FormCommand.Search;
                    OnPropertyChanged(nameof(CommandType));
                    IsDateRequested = false;
                    OnPropertyChanged(nameof(IsDateRequested));
                    OnPropertyChanged(nameof(CanDeny));
                    WorkOrder = null;
                    WorkOrder = new CMMSWorkOrder();
                    CompleteCommandType = string.Empty;
                }
                _search = value;
            }
        }
        public FormCommand CommandType { get; set; }
        public ObservableCollection<CMMSGLAccount> WorkCenterList { get; private set; }
        private CMMSGLAccount _workCenter;
        public CMMSGLAccount SelectedWorkCenter
        {
            get { return _workCenter; }
            set { _workCenter = value; WorkOrder.Workcenter = value?.Description; OnPropertyChanged(nameof(SelectedWorkCenter)); OnPropertyChanged(nameof(GLAccount)); }
        }
        public BindingList<Users> CrewList { get; set; }
        public bool PriorityView { get { return WorkOrder.Status == CMMSStatus.Pending || WorkOrder.Status == CMMSStatus.Reviewing ? false : true; } }
        public ObservableCollection<Priority> PriorityList { get; private set; }
        public string SelectedPriority
        {
            get { return WorkOrder.Priority; }
            set { WorkOrder.Priority = value; OnPropertyChanged(nameof(SelectedPriority)); }
        }
        public BindingList<AttachedDocuments> DocumentList { get; set; }
        public string GLAccount
        {
            get { return SelectedWorkCenter != null ? CurrentUser.Site == "WCCO" ? $"01-00-{SelectedWorkCenter.GLAccount}" : $"02-00-{SelectedWorkCenter.GLAccount}" : null; }
        }

        public DataTable NotesTable { get; set; }
        private bool _isDateRequested;
        public bool IsDateRequested
        {
            get { return _isDateRequested; }
            set { _isDateRequested = RequestCompletionDate(value); OnPropertyChanged(nameof(IsDateRequested)); OnPropertyChanged(nameof(WorkOrder)); }
        }
        public bool CanEdit { get { return (CurrentUser.FullName == WorkOrder.Submitter || CurrentUser.CMMSAdmin) && !IsClosed; } }
        public bool CanAssign { get { return (CurrentUser.CMMSAdmin || CurrentUser.CMMSCrew) && !IsClosed; } }
        public bool CanDeny { get { return (CurrentUser.CMMSCrew || CurrentUser.CMMSAdmin) && SearchEntered; } }
        public bool Assigned { get { return !IsClosed ? CurrentUser.CMMS && CrewList?.Count > 0 : CrewList?.Count > 0; } }
        private bool isLoaded;
        public bool IsLoaded
        {
            get { return isLoaded; }
            set { if (value) { UpdateNotes(); LoadFormAsync(); } isLoaded = value = false; }
        }
        public bool IsClosed { get { return WorkOrder == null || WorkOrder.Status.Equals(CMMSStatus.Completed) || WorkOrder.Status.Equals(CMMSStatus.Denied); } }
        public string CurrentSite { get { return CurrentUser.Site; } }


        RelayCommand _submit;
        RelayCommand _note;
        RelayCommand _docAttach;
        RelayCommand _docOpen;
        RelayCommand _complete;
        RelayCommand _deny;
        RelayCommand _print;

        #endregion

        /// <summary>
        /// CMMS Work Order UserControl ViewModel Constructor
        /// </summary>
        public CMMSWorkOrderUCViewModel()
        {
            if (WorkOrder == null)
            {
                WorkOrder = new CMMSWorkOrder();
            }
            if (FormLinks == null)
            {
                FormLinks = new ObservableCollection<LinkedForms>(WorkOrder.FormLinkList);
            }
            SelectedWorkCenter = !string.IsNullOrEmpty(WorkOrder.Workcenter) ? WorkCenterList.FirstOrDefault(o => o.Description == WorkOrder.Workcenter) : null;
            WorkOrder.Date = DateTime.Now;
            WorkOrder.Submitter = CurrentUser.FullName;
            WorkOrder.Status = CMMSStatus.Pending;
            CompleteCommandType = string.Empty;
            LoadAsync();
            DocumentList = new BindingList<AttachedDocuments>();
            DocumentList.ListChanged += DocumentListChanged;
            SelectedPriority = "--Unassigned--";
            CommandType = FormCommand.Submit;
            SearchEntered = true;
            SearchMode = false;
            SearchHide = true;
            Running = false;
            OnPropertyChanged(nameof(PriorityView));
            OnPropertyChanged(nameof(AssignView));
            UpdateNotes();
        }

        /// <summary>
        /// Async Loading of the large items in the form
        /// </summary>
        private async void LoadAsync()
        {
            WorkCenterList = new ObservableCollection<CMMSGLAccount>(await CMMSGLAccount.CMMSGLAccountListAsync());
            if (CurrentUser.CMMSAdmin || CurrentUser.CMMSCrew)
            {
                CrewList = new BindingList<Users>(await Users.CMMSUserListAsync(true, addNone: false));
                CrewList.ListChanged += CrewListChanged;
            }
            PriorityList = new ObservableCollection<Priority>(await Priority.GetListAsync(true));
        }

        /// <summary>
        /// Change Event for CrewList
        /// </summary>
        /// <param name="sender">CrewList object</param>
        /// <param name="e">CrewList change events</param>
        private void CrewListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                if (WorkOrder.Status == CMMSStatus.Denied && CrewList[e.NewIndex].Selected)
                {
                    var result = MessageBox.Show("This work order is currently marked as 'Denied'.\nSelecting 'Yes' will reopen the work order.\n Selecting 'No' will cancel the assignment", "Assignment of Denied Work Order", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            WorkOrder.Status = CMMSStatus.Assigned;
                            WorkOrder.DateComplete = DateTime.MinValue;
                            break;
                        case MessageBoxResult.No:
                            CrewList[e.NewIndex].Selected = false;
                            break;
                    }
                }
                if (CrewList[e.NewIndex].Selected)
                {
                    if (WorkOrder.Status == CMMSStatus.Pending || WorkOrder.Status == CMMSStatus.Reviewing)
                    {
                        WorkOrder.Status = CMMSStatus.Assigned;
                    }
                    if (WorkOrder.DateAssigned == DateTime.MinValue)
                    {
                        WorkOrder.DateAssigned = DateTime.Today;
                    }
                    if (WorkOrder.Priority == "--Unassigned--")
                    {
                        SelectedPriority = "Standard";
                    }
                    if (WorkOrder.IDNumber > 0)
                    {
                        WorkOrder.CrewAssigned = BuildCrew();
                        WorkOrder.Update();
                        TakeAction();
                    }
                }
                else if (!CrewList[e.NewIndex].Selected && WorkOrder.Status != CMMSStatus.Denied)
                {
                    var _check = false;
                    foreach (var item in CrewList)
                    {
                        if (item.Selected)
                        {
                            _check = true;
                        }
                    }
                    if (!_check)
                    {
                        WorkOrder.DateAssigned = DateTime.MinValue;
                        SelectedPriority = "--Unassigned--";
                        WorkOrder.Status = CMMSStatus.Pending;
                    }
                    WorkOrder.CrewAssigned = BuildCrew();
                    WorkOrder.Update();
                    TakeAction();
                }
            }
            OnPropertyChanged(nameof(WorkOrder));
            OnPropertyChanged(nameof(AssignView));
            OnPropertyChanged(nameof(PriorityView));
            OnPropertyChanged(nameof(ClosedView));
        }

        /// <summary>
        /// Change Event for DocumentList
        /// </summary>
        /// <param name="sender">DocumentList object</param>
        /// <param name="e">DocumentList change events</param>
        private void DocumentListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor.Name == "Attached")
            {
                if (File.Exists($"{Properties.Settings.Default.CMMSDocumentLocation}{DocumentList[e.NewIndex].FileName}"))
                {
                    File.Delete($"{Properties.Settings.Default.CMMSDocumentLocation}{DocumentList[e.NewIndex].FileName}");
                }
                WorkOrder.RemoveDocument(DocumentList[e.NewIndex].FileName);
                DocumentList.RemoveAt(e.NewIndex);
            }

        }

        /// <summary>
        /// Update the notes attached to the QIR
        /// </summary>
        public void UpdateNotes()
        {
            if (WorkOrder.IDNumber != null)
            {
                NotesTable = CMMSWorkOrder.LoadNotes((int)WorkOrder.IDNumber);
                OnPropertyChanged(nameof(NotesTable));
            }
        }

        /// <summary>
        /// Prepares the Crew List for database submission
        /// </summary>
        /// <returns>CrewMembersAssigned as string</returns>
        public string BuildCrew()
        {
            var builder = new System.Text.StringBuilder();
            if (CrewList != null)
            {
                foreach (var item in CrewList)
                {
                    if (item.Selected)
                    {
                        builder.Append($"{item.FullName}/");
                    }
                    if (item.Selected && WorkOrder.DateAssigned == null)
                    {
                        WorkOrder.DateAssigned = DateTime.Now;
                        WorkOrder.Status = CMMSStatus.Assigned;
                    }
                }
            }
            return builder.ToString().Length == 0
                    ? "None"
                    : builder.ToString().Remove(builder.Length - 1, 1);
        }

        /// <summary>
        /// Validates the Work Order status
        /// </summary>
        /// <param name="statusChange">Status the work order is changing to</param>
        public bool ValidateStatus(CMMSStatus statusChange)
        {
            if (statusChange == CMMSStatus.Completed && WorkOrder.MachineDown)
            {
                var _result = MessageBox.Show("The machine is still marked as down and you are updating this work order as completed.\nWould you like to release the machine back to production?", "Machine Down Status", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
                switch (_result)
                {
                    case MessageBoxResult.Yes:
                        WorkOrder.MachineDown = false;
                        return true;
                    case MessageBoxResult.No:
                        MessageBox.Show("This work order will be updated to complete and the machine will remain down.", "Machine Down Status", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return true;
                    default: return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Loads the form when a search is performed
        /// </summary>
        public async void LoadFormAsync()
        {
            SearchEntered = true;
            OnPropertyChanged(nameof(SearchEntered));
            CrewList = CurrentUser.CMMSCrew || CurrentUser.CMMSAdmin ? new BindingList<Users>(await Users.CMMSUserListAsync(false, (int)WorkOrder?.IDNumber, false)) : new BindingList<Users>(Users.CMMSAssignedCrewList((int)WorkOrder?.IDNumber));
            CrewList.ListChanged += CrewListChanged;
            OnPropertyChanged(nameof(CrewList));
            CommandType = FormCommand.Update;
            OnPropertyChanged(nameof(SelectedPriority));
            SelectedWorkCenter = !string.IsNullOrEmpty(WorkOrder.Workcenter) ? WorkCenterList.FirstOrDefault(o => o.Description == WorkOrder.Workcenter) : null;
            OnPropertyChanged(nameof(SelectedWorkCenter));
            OnPropertyChanged(nameof(AssignView));
            OnPropertyChanged(nameof(ClosedView));
            OnPropertyChanged(nameof(FinishedStatus));
            OnPropertyChanged(nameof(PriorityView));
            UpdateNotes();
            DocumentList = new BindingList<AttachedDocuments>(await AttachedDocuments.CreateDocListAsync((int)WorkOrder.IDNumber));
            DocumentList.ListChanged += DocumentListChanged;
            OnPropertyChanged(nameof(DocumentList));
            OnPropertyChanged(nameof(CommandType));
            CompleteCommandType = string.Empty;
            OnPropertyChanged(nameof(CompleteCommandTT));
            IsDateRequested = WorkOrder.RequestDate > DateTime.MinValue ? true : false;
            OnPropertyChanged(nameof(CanEdit));
            OnPropertyChanged(nameof(CanDeny));
            OnPropertyChanged(nameof(CanAssign));
            OnPropertyChanged(nameof(Assigned));
            OnPropertyChanged(nameof(IsClosed));
            if (!FormBase.FormChangeInProgress && WorkOrder.LinkExists())
            {
                var noUseYet = WorkOrder.GetLinkList();
            }
        }

        /// <summary>
        /// Request a completion date property validation
        /// </summary>
        /// <param name="isRequested">boolean value for user request of a completion date</param>
        /// <returns>Validated boolean value for requested completion date</returns>
        public bool RequestCompletionDate(bool isRequested)
        {
            if (!isRequested)
            {
                WorkOrder.RequestDate = DateTime.MinValue;
                WorkOrder.RequestedDateReason = null;
                return false;
            }
            else if (isRequested && WorkOrder.RequestDate != DateTime.MinValue)
            {
                return true;
            }
            WorkOrder.RequestDate = DateTime.Today.AddDays(7);
            WorkOrder.RequestedDateReason = string.Empty;
            return true;
        }

        /// <summary>
        /// Displays the action taken by the user.
        /// </summary>
        public void TakeAction()
        {
            Running = true;
            OnPropertyChanged(nameof(ActionTaken));
            OnPropertyChanged(nameof(ClosedView));
            OnPropertyChanged(nameof(AssignView));
            OnPropertyChanged(nameof(FinishedStatus));
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                try
                {
                    bw.DoWork += new DoWorkEventHandler(
                        delegate (object sender, DoWorkEventArgs e)
                        {
                            for (double i = 1.0; i >= 0.0; i -= 0.0000002)
                            {
                                Fade = i;
                                OnPropertyChanged(nameof(Fade));
                            }
                        });
                    bw.RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                }
            }
            Running = false;
        }

        /// <summary>
        /// TODO: move to its own usercontrol
        /// </summary>
        public void UpdateUILinkList()
        {
            if (WorkOrder != null)
            {
                FormLinks = new ObservableCollection<LinkedForms>(WorkOrder.FormLinkList);
                OnPropertyChanged(nameof(WorkOrder));
            }
        }

        #region Attach Document ICommand

        /// <summary>
        /// Attach Document Command
        /// </summary>
        public ICommand AttachDocumentCommand
        {
            get
            {
                if (_docAttach == null)
                {
                    _docAttach = new RelayCommand(AttachDocumentExecute, AttachDocumentCanExecute);
                }
                return _docAttach;
            }
        }

        /// <summary>
        /// Attach Document Command Execution
        /// </summary>
        /// <param name="parameter">Name of Document</param>
        private void AttachDocumentExecute(object parameter)
        {
            var _temp = WorkOrder.AttachDocument();
            if (_temp != null)
            {
                foreach (var item in _temp)
                {
                    DocumentList.Add(item);
                }
            }
            OnPropertyChanged(nameof(DocumentList));
        }
        private bool AttachDocumentCanExecute(object parameter) => IsClosed || CommandType.Equals(FormCommand.Search)
            ? false
            : true;

        #endregion

        #region Open Document ICommand

        /// <summary>
        /// Open Document Command
        /// </summary>
        public ICommand OpenDocumentCommand
        {
            get
            {
                if (_docOpen == null)
                {
                    _docOpen = new RelayCommand(OpenDocumentExecute);
                }
                return _docOpen;
            }
        }

        /// <summary>
        /// Open Document Command Execution
        /// </summary>
        /// <param name="parameter">Name of Document</param>
        private static void OpenDocumentExecute(object parameter)
        {
            if (File.Exists(parameter.ToString()))
            {
                try
                {
                    Process.Start(parameter.ToString());
                }
                catch (Exception ex)
                {
                    ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                }
            }
        }

        #endregion

        #region Add Note ICommand

        /// <summary>
        /// Add Note Command
        /// </summary>
        public ICommand NoteCommand
        {
            get
            {
                if (_note == null)
                {
                    _note = new RelayCommand(NoteExecute, NoteCanExecute);
                }
                return _note;
            }
        }

        /// <summary>
        /// Add Note Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private void NoteExecute(object parameter)
        {
            if (!string.IsNullOrEmpty(OMNIDataBase.AddNote("cmms_work_order", WorkOrder.IDNumber)))
            {
                WorkOrder.AttachedNotes = true;
            }
            UpdateNotes();
        }
        private bool NoteCanExecute(object parameter) => WorkOrder == null || WorkOrder.IDNumber == null || IsClosed
            ? false
            : true;

        #endregion

        #region Deny Work Order ICommand

        /// <summary>
        /// Deny a work order Command
        /// </summary>
        public ICommand DenyCommand
        {
            get
            {
                if (_deny == null)
                {
                    _deny = new RelayCommand(DenyExecute, DenyCanExecute);
                }
                return _deny;
            }
        }

        /// <summary>
        /// Deny work order Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private void DenyExecute(object parameter)
        {
            if (!string.IsNullOrEmpty(OMNIDataBase.AddNote("cmms_work_order", WorkOrder.IDNumber)))
            {
                WorkOrder.Status = CMMSStatus.Denied;
                SelectedPriority = "Unassigned";
                WorkOrder.DateAssigned = DateTime.MinValue;
                WorkOrder.DateComplete = DateTime.Today;
                WorkOrder.AttachedNotes = true;
                UpdateNotes();
                foreach (var item in CrewList)
                {
                    item.Selected = false;
                }
                OnPropertyChanged(nameof(CrewList));
                WorkOrder.Update();
                TakeAction();
            }
            OnPropertyChanged(nameof(AssignView));
            OnPropertyChanged(nameof(ClosedView));
            CompleteCommandType = string.Empty;
            OnPropertyChanged(nameof(CompleteCommandTT));
            OnPropertyChanged(nameof(WorkOrder));
            OnPropertyChanged(nameof(IsClosed));
            OnPropertyChanged(nameof(CanAssign));
            OnPropertyChanged(nameof(CanEdit));
        }
        private bool DenyCanExecute(object parameter) => IsClosed || WorkOrder.IDNumber == null || CommandType.Equals(FormCommand.Search)
            ? false
            : true;

        #endregion

        #region Submit Work Order ICommand

        /// <summary>
        /// Submit Command
        /// </summary>
        public ICommand SubmitCommand
        {
            get
            {
                if (_submit == null)
                {
                    _submit = new RelayCommand(SubmitExecute, SubmitCanExecute);
                }
                return _submit;
            }
        }

        /// <summary>
        /// Submit Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private void SubmitExecute(object parameter)
        {
            if (CommandType == FormCommand.Submit)
            {
                WorkOrder.CrewAssigned = BuildCrew();
                ValidateStatus(WorkOrder.Status);
                WorkOrder.IDNumber = WorkOrder.Submit();
                if (DocumentList.Count > 0)
                {
                    foreach (var file in DocumentList)
                    {
                        WorkOrder.SubmitAttachDocument(file.FileName);
                        File.Copy(file.FilePath, $"{Properties.Settings.Default.CMMSDocumentLocation}{file.FileName}", true);
                        file.FilePath = $"{Properties.Settings.Default.CMMSDocumentLocation}{file.FileName}";
                    }
                }
                ((TabItem)DashBoardTabControl.WorkSpace.SelectedItem).Header = WorkOrder.IDNumber;
                TakeAction();
                CommandType = FormCommand.Update;
                OnPropertyChanged(nameof(CommandType));
            }
            else if (CommandType == FormCommand.Update)
            {
                if (!string.IsNullOrEmpty(OMNIDataBase.AddNote("cmms_work_order", WorkOrder.IDNumber)))
                {
                    WorkOrder.CrewAssigned = BuildCrew();
                    ValidateStatus(WorkOrder.Status);
                    WorkOrder.AttachedNotes = true;
                    UpdateNotes();
                    WorkOrder.Update();
                    TakeAction();
                }
            }
            else
            {
                WorkOrder = CMMSWorkOrder.LoadAsync(SearchIDNumber).Result;
                if (WorkOrder != null)
                {
                    ((TabItem)DashBoardTabControl.WorkSpace.SelectedItem).Header = WorkOrder.IDNumber;
                    LoadFormAsync();
                }
                else
                {
                    WorkOrder = new CMMSWorkOrder();
                }
            }
            OnPropertyChanged(nameof(WorkOrder));
        }
        private bool SubmitCanExecute(object parameter) =>
            CommandType == FormCommand.Search && string.IsNullOrWhiteSpace(SearchIDNumber?.ToString())
                ? false
                : CommandType != FormCommand.Search && (string.IsNullOrWhiteSpace(WorkOrder?.Description) || string.IsNullOrWhiteSpace(WorkOrder?.Workcenter) || Running || IsClosed)
                    ? false
                    : IsDateRequested && (WorkOrder?.RequestedDateReason.Length <= 5 || (WorkOrder?.RequestDate < DateTime.Now.Date && CommandType == FormCommand.Submit))
                        ? false
                : true;

        #endregion

        #region Complete Work Order ICommand

        /// <summary>
        /// Complete Work Order Command
        /// </summary>
        public ICommand CompleteCommand
        {
            get
            {
                if (_complete == null)
                {
                    _complete = new RelayCommand(CompleteExecute, CompleteCanExecute);
                }
                return _complete;
            }
        }

        /// <summary>
        /// Complete Work Order Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private void CompleteExecute(object parameter)
        {
            if (CompleteCommandType == "Complete" && ValidateStatus(CMMSStatus.Completed))
            {
                if (!string.IsNullOrEmpty(OMNIDataBase.AddNote("cmms_work_order", WorkOrder.IDNumber)))
                {
                    WorkOrder.Status = CMMSStatus.Completed;
                    WorkOrder.DateComplete = DateTime.Now;
                    WorkOrder.AttachedNotes = true;
                    WorkOrder.Update();
                    TakeAction();
                    UpdateNotes();
                    OnPropertyChanged(nameof(WorkOrder));
                }
            }
            else if (CompleteCommandType == "Re-Open" && WorkOrder.Status == CMMSStatus.Completed)
            {
                WorkOrder.Status = CMMSStatus.Assigned;
                WorkOrder.DateComplete = DateTime.MinValue;
                WorkOrder.Update();
                OnPropertyChanged(nameof(WorkOrder));
            }
            else
            {
                WorkOrder.Status = CMMSStatus.Pending;
                WorkOrder.DateComplete = DateTime.MinValue;
                WorkOrder.Update();
                OnPropertyChanged(nameof(WorkOrder));
            }
            OnPropertyChanged(nameof(ClosedView));
            OnPropertyChanged(nameof(CompleteCommandTT));
            CompleteCommandType = string.Empty;
            OnPropertyChanged(nameof(IsClosed));
            OnPropertyChanged(nameof(CanEdit));
            OnPropertyChanged(nameof(CanAssign));
            OnPropertyChanged(nameof(Assigned));
        }
        private bool CompleteCanExecute(object parameter) =>
            CompleteCommandType == "Complete" && (WorkOrder?.Status != CMMSStatus.Assigned || string.IsNullOrEmpty(WorkOrder.Workcenter) || string.IsNullOrEmpty(WorkOrder.Description) || WorkOrder?.IDNumber == null || IsClosed || Running)
                ? false
                : CompleteCommandType == "Re-Open" && IsClosed && CommandType.Equals(FormCommand.Search)
                    ? false
                    : true;

        #endregion

        #region Print Work Order ICommand

        // <summary>
        /// Print Work Order Command
        /// </summary>
        public ICommand PrintCommand
        {
            get
            {
                if (_print == null)
                {
                    _print = new RelayCommand(PrintExecute, PrintCanExecute);
                }
                return _print;
            }
        }

        /// <summary>
        /// Print CMMS Work Order Execution
        /// </summary>
        /// <param name="parameter">QIR Number</param>
        private void PrintExecute(object parameter)
        {
            var woNumber = WorkOrder.IDNumber;
            try
            {
                CMMSWorkOrder.LoadAsync(woNumber).Result.ExportToPDF(true);
                PrintForm.FromPDF($"{Properties.Settings.Default.omnitemp}{woNumber}.pdf");
                if (File.Exists($"{Properties.Settings.Default.omnitemp}{woNumber}.pdf"))
                {
                    File.Delete($"{Properties.Settings.Default.omnitemp}{woNumber}.pdf");
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
            }
        }
        private bool PrintCanExecute(object parameter) => WorkOrder?.IDNumber > 0;

        #endregion

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                WorkCenterList = null;
                CrewList = null;
                PriorityList = null;
                WorkOrder = null;
                NotesTable?.Dispose();
            }
        }
    }
}
