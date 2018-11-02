using OMNI.Commands;
using OMNI.CustomControls;
using OMNI.Enumerations;
using OMNI.HDT.Enumeration;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// IT Form UserControl ViewModel Logic
    /// </summary>
    public class ITFormUCViewModel : ViewModelBase
    {
        #region Properties

        public ITTicket Ticket { get; set; }
        public ObservableCollection<LinkedForms> FormLinks { get; set; }
        private int? searchIDNumber;
        public int? SearchIDNumber
        {
            get { return searchIDNumber; }
            set { if (!searchIDNumber.Equals(value) && PrimaryCommandType.Equals(FormCommand.Update)) { ClearForm(); } searchIDNumber = value; OnPropertyChanged(nameof(SearchIDNumber)); }
        }
        private bool searchMode;
        public bool SearchMode
        {
            get { return searchMode; }
            set { searchMode = value; OnPropertyChanged(nameof(SearchMode)); }
        }
        private FormCommand primaryCommandType;
        public FormCommand PrimaryCommandType
        {
            get { return primaryCommandType; }
            set { primaryCommandType = value; OnPropertyChanged(nameof(PrimaryCommandType)); if (PrimaryCommandType.Equals(FormCommand.Search)) { SearchMode = true; } OnPropertyChanged(nameof(PrimaryCommandTT)); }
        }
        public string PrimaryCommandTT { get { return CommandTT(PrimaryCommandType); } }
        private FormCommand secondaryCommandType;
        public FormCommand SecondaryCommandType
        {
            get { return secondaryCommandType; }
            set { secondaryCommandType = value; OnPropertyChanged(nameof(SecondaryCommandType)); OnPropertyChanged(nameof(SecondaryCommandTT)); }
        }
        public string SecondaryCommandTT { get { return CommandTT(SecondaryCommandType); } }
        public ObservableCollection<WorkCenter> LocationList { get; set; }
        public string SelectedLocation
        {
            get { return Ticket.Location; }
            set { Ticket.Location = value; OnPropertyChanged(nameof(SelectedLocation)); }
        }
        public ObservableCollection<TicketSubject> TypeList { get; set; }
        public TicketSubject SelectedType
        {
            get { return Ticket.Subject == null ? null : TypeList.FirstOrDefault(o => o.Title == Ticket.Subject.Title); }
            set { if (Ticket.Subject == null || Ticket.Subject.Title != value.Title) { Ticket.Subject = value; } OnPropertyChanged(nameof(SelectedType)); }
        }
        public ObservableCollection<Priority> PriorityList { get; set; }
        public Priority SelectedPriority
        {
            get { return Ticket.Priority == null ? null : PriorityList.FirstOrDefault(o => o.Level == Ticket.Priority.Level); }
            set { if (Ticket.Priority == null || Ticket.Priority.Level != value.Level) { Ticket.Priority = value; } OnPropertyChanged(nameof(SelectedPriority)); }
        }
        public ObservableCollection<ITProject> ProjectList { get; set; }
        private ITProject selectedProject;
        public ITProject SelectedProject
        {
            get { return selectedProject; }
            set { if (value != null) { selectedProject = value; ProjectCommand.Execute(2); } selectedProject = value; OnPropertyChanged(nameof(SelectedProject)); }
        }
        public bool IsDateRequested
        {
            get { return Ticket.RequestDate == DateTime.MinValue ? false : true; }
            set { Ticket.RequestDate = !value ? DateTime.MinValue : DateTime.Today.AddDays(7); OnPropertyChanged(nameof(IsDateRequested)); OnPropertyChanged(nameof(Ticket)); }
        }
        private bool AssignmentInProgress;
        private bool AttachmentInProgress;
        public bool LoadFromNotice { set { if (value) { LoadForm(); } } }
        public bool IsTransactionRunning { get; set; }
        public double Fade { get; set; }
        public string CommandActionTaken { get; set; }
        public string MemberAssigned;

        RelayCommand _submit;
        RelayCommand _note;
        RelayCommand _docAttach;
        RelayCommand _openDoc;
        RelayCommand _project;

        #endregion

        /// <summary>
        /// IT Form UserControl ViewModel Constructor
        /// </summary>
        public ITFormUCViewModel()
        {
            if (Ticket == null)
            {
                Ticket = new ITTicket();
            }
            if (LocationList == null)
            {
                LocationList = new ObservableCollection<WorkCenter>(WorkCenter.GetListAsync(WorkCenterType.HDT).Result);
            }
            if (TypeList == null)
            {
                TypeList = new ObservableCollection<TicketSubject>(TicketSubject.GetListAsync().Result);
            }
            if (PriorityList == null)
            {
                PriorityList = new ObservableCollection<Priority>(Priority.GetListAsync(true).Result);
            }
            if (ProjectList == null)
            {
                ProjectList = new ObservableCollection<ITProject>(ITProject.GetListAsync().Result);
            }
            if (FormLinks == null)
            {
                FormLinks = new ObservableCollection<LinkedForms>(Ticket.FormLinkList);
            }
            Ticket.AssignedTo.ListChanged += AssignedToChanged;
            Ticket.DocumentList.ListChanged += DocumentListChanged;
            PrimaryCommandType = FormCommand.Submit;
            SecondaryCommandType = FormCommand.Complete;
            AssignmentInProgress = false;
            AttachmentInProgress = false;
            LoadFromNotice = false;
            IsTransactionRunning = false;
        }

        /// <summary>
        /// Changes in ticket.AssignedTo, tracked to update live assignments
        /// </summary>
        /// <param name="sender">ITTeamMember</param>
        /// <param name="e">BindingList Change Events</param>
        public void AssignedToChanged(object sender, ListChangedEventArgs e)
        {
            var _tempStatus = Ticket.Status;
            var _tempAssignDate = Ticket.AssignedTo[e.NewIndex].AssignDate;
            var _tempAssignStatus = Ticket.AssignedTo[e.NewIndex].Assigned;
            var _tempPriority = Ticket.Priority;
            if (e.ListChangedType == ListChangedType.ItemChanged && !AssignmentInProgress)
            {
                AssignmentInProgress = true;
                if (_tempAssignStatus)
                {
                    Ticket.Status = TicketStatus.Create("Assigned");
                    Ticket.AssignedTo[e.NewIndex].AssignDate = DateTime.Today;
                    if (Ticket.Priority.Level == 6)
                    {
                        Ticket.Priority = PriorityList.First(o => o.Level == 3);
                    }
                }
                else
                {
                    Ticket.AssignedTo[e.NewIndex].AssignDate = DateTime.MinValue;
                    var _validate = false;
                    foreach (var member in Ticket.AssignedTo)
                    {
                        if (member.Assigned)
                        {
                            _validate = true;
                        }
                    }
                    if (!_validate)
                    {
                        Ticket.Status = TicketStatus.Create("Pending");
                        Ticket.Priority = Priority.Create(6, "--Unassigned--");
                    }
                }
                MemberAssigned = Ticket.AssignedTo[e.NewIndex].Name;
                if (!ITTeamMember.AssignmentTransactionAsync(Ticket.AssignedTo[e.NewIndex], Convert.ToInt32(Ticket.IDNumber), Ticket.AssignedTo[e.NewIndex].Assigned).Result)
                {
                    ExceptionWindow.Show("Assignment Interference", "OMNI is currently not able to process your assignment request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact the IT OMNI Developer.");
                    Ticket.Status = _tempStatus;
                    Ticket.AssignedTo[e.NewIndex].AssignDate = _tempAssignDate;
                    Ticket.AssignedTo[e.NewIndex].Assigned = _tempAssignStatus;
                    Ticket.Priority = _tempPriority;
                }
                SubmitCommand.Execute(FormCommand.Assigned);
                OnPropertyChanged(nameof(Ticket));
                OnPropertyChanged(nameof(SelectedPriority));
                AssignmentInProgress = false;
            }
        }

        /// <summary>
        /// Changes in ticket.DocumentList, tracked to update live attachments
        /// </summary>
        /// <param name="sender">ITDocument</param>
        /// <param name="e">BindingList Change Events</param>
        public void DocumentListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged && !Ticket.DocumentList[e.NewIndex].IsSelected && !AttachmentInProgress)
            {
                AttachmentInProgress = true;
                if (!Ticket.RemoveAttachmentAsync(e.NewIndex).Result)
                {
                    ExceptionWindow.Show("File Attachment Failed", "OMNI is currently not able to process your file attachment request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact IT directly.");
                }
                AttachmentInProgress = false;
            }
        }

        /// <summary>
        /// Clear a loaded IT Form
        /// </summary>
        public void ClearForm()
        {
            Ticket = new ITTicket();
            OnPropertyChanged(nameof(Ticket));
            PrimaryCommandType = FormCommand.Search;
            OnPropertyChanged(nameof(IsDateRequested));
            ((TabItem)(DashBoardTabControl.WorkSpace).SelectedItem).Header = "Ticket Search";
        }

        /// <summary>
        /// Load a Form if opened from an outside source
        /// </summary>
        public void LoadForm()
        {
            Ticket.AssignedTo.ListChanged += AssignedToChanged;
            Ticket.DocumentList.ListChanged += DocumentListChanged;
            LoadFromNotice = false;
            PrimaryCommandType = FormCommand.Update;
            SecondaryCommandType = Ticket.CompletionDate == DateTime.MinValue ? FormCommand.Complete : FormCommand.ReOpen;
            Ticket.NotesTable = ITTicket.GetNotesDataTable(Convert.ToInt32(Ticket.IDNumber));
            if (Ticket.FormLinkList.Count > 0)
            {
                UpdateUILinkList();
            }
        }

        /// <summary>
        /// TODO: move to its own usercontrol
        /// </summary>
        public void UpdateUILinkList()
        {
            if (Ticket != null)
            {
                if (Ticket.LinkExists())
                {
                    Ticket.GetLinkList();
                }
                else
                {
                    Ticket.FormLinkList.Clear();
                }
                FormLinks = new ObservableCollection<LinkedForms>(Ticket.FormLinkList);
                OnPropertyChanged(nameof(Ticket));
                OnPropertyChanged(nameof(FormLinks));
            }
        }

        /// <summary>
        /// Form Command Tool tips
        /// </summary>
        /// <param name="commandType">Command Type</param>
        /// <returns>Tool Tip as string</returns>
        public string CommandTT(FormCommand commandType)
        {
            switch (commandType)
            {
                case FormCommand.Submit:
                    return $"Submit this ticket to the database.";
                case FormCommand.Update:
                    return $"Update HDT# {Ticket.IDNumber}.";
                case FormCommand.Search:
                    return $"Search for a specific ticket based on it's HDT#";
                case FormCommand.Complete:
                    return $"Complete HDT# {Ticket.IDNumber}";
                case FormCommand.ReOpen:
                    return $"Re-Open HDT# {Ticket.IDNumber}.";
            }
            return null;
        }

        /// <summary>
        /// Displays the action taken by the user.
        /// </summary>
        /// <param name="commandAction">Command Action Taken as string</param>
        /// <param name="emailSubmitter">Email the action taken to the submitter</param>
        /// <param name="emailTeam">Email the action taken to the IT Team</param>
        /// <param name="emailAssignee">Email the action taken to the IT Team Member that was assigned</param>
        public void TakeAction(string commandAction, bool emailSubmitter, bool emailTeam, bool emailAssignee)
        {
            if (!IsTransactionRunning)
            {
                IsTransactionRunning = true;
                CommandActionTaken = commandAction;
                OnPropertyChanged(nameof(CommandActionTaken));
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
                                IsTransactionRunning = false;
                            });
                        bw.RunWorkerAsync();
                    }
                    catch (Exception ex)
                    {
                        ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                    }
                }
                Task.Run(delegate
                {
                    if (emailSubmitter)
                    {
                        var _tempEmail = Users.RetrieveEmailAddress(Ticket.Submitter);
                        var emailBody = $"Action has been taken on your ticket #{Ticket.IDNumber}, log into OMNI to see a detailed view the changes.";
                        switch (commandAction)
                        {
                            case "Note Added":
                                emailBody += $"\n\nNote Description:\n{Ticket.NotesTable.Rows[Ticket.NotesTable.Rows.Count - 1]["Note"]}";
                                break;
                            case "Completed":
                                emailBody += $"\n\nClosing Notes:\n{Ticket.NotesTable.Rows[Ticket.NotesTable.Rows.Count - 1]["Note"]}";
                                break;
                            case "Re-Opened":
                                emailBody += $"\n\nReasoning:\n{Ticket.NotesTable.Rows[Ticket.NotesTable.Rows.Count - 1]["Note"]}";
                                break;
                        }
                        if (_tempEmail != "Not on File")
                        {
                            EmailForm.SendwithoutAttachment(_tempEmail, emailBody, $"HDT #{Ticket.IDNumber} {commandAction}");
                        }
                    }
                    if (emailTeam)
                    {
                        var emailBody = $"Ticket #{Ticket.IDNumber} was just {commandAction} by {Ticket.Submitter}.";
                        emailBody += Ticket.Location == "Provided Contact" ? $"\n\nCurrently at {Ticket.Location} [{Ticket.POC}] there is an issue with {Ticket.Subject.Title}." : $"\n\nCurrently at {Ticket.Location} there is an issue with {Ticket.Subject.Title}.";
                        emailBody += $"\n\nReported Description:\n{Ticket.Description}";
                        if (Ticket.IAR)
                        {
                            emailBody += $"\n\n{Ticket.Submitter} has requested IT's immediate assistance.";
                        }
                        EmailForm.SendwithoutAttachment("itsupport@wccobelt.com", emailBody, $"HDT #{Ticket.IDNumber} {commandAction}");
                    }
                    if (emailAssignee)
                    {
                        EmailForm.SendwithoutAttachment(Users.RetrieveEmailAddress(MemberAssigned), $"Ticket #{Ticket?.IDNumber} was just {commandAction} to {MemberAssigned}, please log into OMNI to review.", $"HDT #{Ticket?.IDNumber} {commandAction}");
                        MemberAssigned = string.Empty;
                    }
                });
            }
        }

        #region View Command Interfaces

        /// <summary>
        /// Submit Form Command
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
        /// Submit Form Execution
        /// </summary>
        /// <param name="parameter">typeof(FormCommand)</param>
        private void SubmitExecute(object parameter)
        {
            var _tempSubmit = parameter == null ? PrimaryCommandType : (FormCommand)Enum.Parse(typeof(FormCommand), parameter.ToString());
            switch (_tempSubmit)
            {
                case FormCommand.Submit:
                    if (!Ticket.SubmitAsync().Result)
                    {
                        ExceptionWindow.Show("Submission Interference", "OMNI is currently not able to process your submission request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact IT directly.");
                    }
                    else
                    {
                        SearchIDNumber = -1;
                        ((TabItem)(DashBoardTabControl.WorkSpace).SelectedItem).Header = Ticket.IDNumber;
                        PrimaryCommandType = FormCommand.Update;
                        OnPropertyChanged(nameof(Ticket));
                        TakeAction("Submitted", false, true, false);
                    }
                    break;
                case FormCommand.Update:
                    if (!Ticket.Update())
                    {
                        ExceptionWindow.Show("Update Failed", "OMNI is currently not able to process your update request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact IT directly.");
                    }
                    else
                    {
                        TakeAction("Updated", true, false, false);
                    }
                    break;
                case FormCommand.Assigned:
                    if (!Ticket.Update())
                    {
                        ExceptionWindow.Show("Update Failed", "OMNI is currently not able to process your update request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact IT directly.");
                    }
                    else
                    {
                        TakeAction("Assigned", true, false, true);
                    }
                    break;
                case FormCommand.Search:
                    Ticket = ITTicket.GetITTicketAsync((int)SearchIDNumber).Result;
                    if (Ticket.IDNumber == 0)
                    {
                        ExceptionWindow.Show("Invalid Entry", "The ticket number you have entered does not exist.\nPlease double check your entry and try again.");
                    }
                    else
                    {
                        PrimaryCommandType = FormCommand.Update;
                        SecondaryCommandType = Ticket.CompletionDate == DateTime.MinValue ? FormCommand.Complete : FormCommand.ReOpen;
                        OnPropertyChanged(nameof(IsDateRequested));
                        OnPropertyChanged(nameof(SelectedType));
                        OnPropertyChanged(nameof(SelectedPriority));
                        OnPropertyChanged(nameof(SelectedLocation));
                        ((TabItem)(DashBoardTabControl.WorkSpace).SelectedItem).Header = Ticket.IDNumber;
                        Ticket.NotesTable = ITTicket.GetNotesDataTable(Convert.ToInt32(Ticket.IDNumber));
                        OnPropertyChanged(nameof(Ticket));
                        Ticket.AssignedTo.ListChanged += AssignedToChanged;
                        Ticket.DocumentList.ListChanged += DocumentListChanged;
                        if (!FormBase.FormChangeInProgress && Ticket.LinkExists())
                        {
                            var noUseYet = Ticket.GetLinkList();
                        }
                    }
                    break;
                case FormCommand.Complete:
                    if (Ticket.AddNotes())
                    {
                        SecondaryCommandType = FormCommand.ReOpen;
                        Ticket.CompletionDate = DateTime.Today;
                        Ticket.Status = TicketStatus.Create("Closed");
                        if (!Ticket.Update())
                        {
                            ExceptionWindow.Show("Update Failed", "OMNI is currently not able to process your update request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact IT directly.");
                            SecondaryCommandType = FormCommand.Complete;
                            Ticket.CompletionDate = DateTime.MinValue;
                            Ticket.Status.Title = "Assigned";
                            OnPropertyChanged(nameof(Ticket));
                        }
                        else
                        {
                            OnPropertyChanged(nameof(Ticket));
                            TakeAction("Completed", true, false, false);
                        }
                    }
                    break;
                case FormCommand.ReOpen:
                    if (Ticket.AddNotes())
                    {
                        var _tempDate = Ticket.CompletionDate;
                        var _tempStatus = Ticket.Status.Title;
                        SecondaryCommandType = FormCommand.Complete;
                        Ticket.CompletionDate = DateTime.MinValue;
                        Ticket.Status.Title = Ticket.AssignedTo.Count(o => o.Assigned) > 0 ? "Assigned" : "Pending";
                        if (!Ticket.Update())
                        {
                            ExceptionWindow.Show("Update Failed", "OMNI is currently not able to process your update request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact IT directly.");
                            SecondaryCommandType = FormCommand.ReOpen;
                            Ticket.CompletionDate = _tempDate;
                            Ticket.Status.Title = _tempStatus;
                            OnPropertyChanged(nameof(Ticket));
                        }
                        else
                        {
                            OnPropertyChanged(nameof(Ticket));
                            TakeAction("Re-Opened", true, true, false);
                        }
                    }
                    break;
                case FormCommand.Deny:
                    if (Ticket.AddNotes())
                    {
                        var _tempStatus = Ticket.Status.Title;
                        SecondaryCommandType = FormCommand.ReOpen;
                        Ticket.CompletionDate = DateTime.Today;
                        Ticket.Status = TicketStatus.Create("Denied");
                        if (!Ticket.Update())
                        {
                            ExceptionWindow.Show("Update Failed", "OMNI is currently not able to process your update request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact IT directly.");
                            SecondaryCommandType = FormCommand.Complete;
                            Ticket.CompletionDate = DateTime.MinValue;
                            Ticket.Status.Title = _tempStatus;
                            OnPropertyChanged(nameof(Ticket));
                        }
                        else
                        {
                            OnPropertyChanged(nameof(Ticket));
                            TakeAction("Denied", true, false, false);
                        }
                    }
                    break;
            }
        }
        private bool SubmitCanExecute(object parameter)
        {
            var _tempValidate = parameter == null ? PrimaryCommandType : (FormCommand)Enum.Parse(typeof(FormCommand), parameter.ToString());
            if (!IsTransactionRunning)
            {
                switch (_tempValidate)
                {
                    case FormCommand.Submit:
                    case FormCommand.Update:
                        return Ticket != null && Ticket.CompletionDate == DateTime.MinValue && Ticket.Subject != null && !string.IsNullOrWhiteSpace(Ticket.Location) && !string.IsNullOrWhiteSpace(Ticket.Description) && (!IsDateRequested || (IsDateRequested && Ticket.RequestReason?.Length >= 20)) && (Ticket.Project == null || !string.IsNullOrEmpty(Ticket.Project.Title));
                    case FormCommand.Search:
                        return SearchIDNumber != null;
                    case FormCommand.Complete:
                        return Ticket?.IDNumber != null && Ticket?.Status.Title == "Assigned" && CurrentUser.ITTeam;
                    case FormCommand.Deny:
                        return Ticket?.IDNumber != null && Ticket?.CompletionDate == DateTime.MinValue && CurrentUser.ITTeam;
                    default:
                        return true;
                }
            }
            return false;
        }

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
            if (Ticket.AddNotes())
            {
                TakeAction("Note Added", true, false, false);
                OnPropertyChanged(nameof(Ticket));
            }
        }
        private bool NoteCanExecute(object parameter) => Ticket?.IDNumber != null && Ticket?.CompletionDate == DateTime.MinValue;

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
            if (!Ticket.AddAttachmentAsync(false).Result)
            {
                ExceptionWindow.Show("File Attachment Failed", "OMNI is currently not able to process your file attachment request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact IT directly.");
            }
            OnPropertyChanged(nameof(Ticket));
        }
        private bool AttachDocumentCanExecute(object parameter) => (PrimaryCommandType == FormCommand.Submit || PrimaryCommandType == FormCommand.Update) && SecondaryCommandType == FormCommand.Complete;

        /// <summary>
        /// Open Document Command
        /// </summary>
        public ICommand OpenDocumentCommand
        {
            get
            {
                if (_openDoc == null)
                {
                    _openDoc = new RelayCommand(OpenDocumentExecute);
                }
                return _openDoc;
            }
        }

        /// <summary>
        /// Open Document command Execution
        /// </summary>
        /// <param name="parameter">File path to open</param>
        private void OpenDocumentExecute(object parameter)
        {
            if (Ticket != null)
            {
                Process.Start($"{parameter.ToString()}");
            }
        }

        /// <summary>
        /// Ticket to project manipulation commmand
        /// </summary>
        public ICommand ProjectCommand
        {
            get
            {
                if (_project == null)
                {
                    _project = new RelayCommand(ProjectExecute, ProjectCanExecute);
                }
                return _project;
            }
        }

        /// <summary>
        /// Project command execution
        /// </summary>
        /// <param name="parameter">Type of command to execute</param>
        private void ProjectExecute(object parameter)
        {
            switch (Convert.ToInt32(parameter))
            {
                case 0:
                    if (!Ticket.NewProjectAsync().Result)
                    {
                        ExceptionWindow.Show("Update Failed", "OMNI is currently not able to process your update request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact IT directly.");
                        Ticket.Project = new ITProject();
                    }
                    break;
                case 1:
                    if (!Ticket.RevertToTicketAsync().Result)
                    {
                        ExceptionWindow.Show("Update Failed", "OMNI is currently not able to process your update request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact IT directly.");
                        Ticket.Project = null;
                    }
                    break;
                case 2:
                    if (SelectedProject != null)
                    {
                        if (!Ticket.SaveAsTaskAsync(SelectedProject.ID).Result)
                        {
                            ExceptionWindow.Show("Update Failed", "OMNI is currently not able to process your update request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact IT directly.");
                        }
                    }
                    break;
            }
            OnPropertyChanged(nameof(Ticket));
        }
        private bool ProjectCanExecute(object parameter)
        {
            switch (Convert.ToInt32(parameter))
            {
                case 0:
                    return Ticket.Type == TicketType.Ticket && PrimaryCommandType != FormCommand.Search && Ticket.CompletionDate == DateTime.MinValue;
                case 1:
                    return Ticket.IDNumber != null && Ticket.Type != TicketType.Ticket && Ticket.CompletionDate == DateTime.MinValue;
                case 2:
                    return Ticket.IDNumber != null && Ticket.Type == TicketType.Ticket && Ticket.CompletionDate == DateTime.MinValue;
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                Ticket = null;
                TypeList = null;
                LocationList = null;
            }
        }
    }
}
