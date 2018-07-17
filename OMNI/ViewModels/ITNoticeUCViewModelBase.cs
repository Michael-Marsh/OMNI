using OMNI.Commands;
using OMNI.CustomControls;
using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// IT Notice UserControl ViewModel Base Interaction Logic
    /// </summary>
    public class ITNoticeUCViewModelBase : ViewModelBase
    {
        #region Properties

        public DataTable NoticeTable { get; set; }
        private object selectedRow;
        public object SelectedRow
        {
            get { return selectedRow; }
            set { if (selectedRow != value) { UpdateActionSpace(value); } selectedRow = value; OnPropertyChanged(nameof(SelectedRow)); }
        }
        public DataTable NotesTable { get; set; }
        public DataTable AssignmentTable { get; set; }
        public bool IsAssigned { get; set; }
        public bool CanDeny { get { return CurrentUser.ITTeam && !Closed; } }
        public bool CanComplete { get { return Open && CurrentUser.ITTeam; } }
        public bool CanAssign { get { return CurrentUser.ITTeam && Module == ITNotice.Unassigned; } }
        public ICollectionView OpenTicketsView { get; set; }
        public string CurrentGroup { get; set; }
        public ObservableCollection<ITTeamMember> TeamList { get; set; }
        private string selectedTeamMember;
        public virtual string SelectedTeamMember
        {
            get { return selectedTeamMember; }
            set { selectedTeamMember = value; SelectedRow = null; OnPropertyChanged(nameof(SelectedTeamMember)); }
        }
        public bool IsLiveUpdateOn { get; set; }
        private ITNotice module;
        public ITNotice Module
        {
            get { return module; }
            set { module = value; OnPropertyChanged(nameof(Module)); }
        }
        //TODO: Figure out why the view does not accept the above Enum and has to be set manually with the below 3 booleans
        public bool Unassigned { get { return Module == ITNotice.Unassigned; } }
        public bool Open { get { return Module == ITNotice.Open; } }
        public bool Closed { get { return Module == ITNotice.Closed; } }

        RelayCommand _group;
        RelayCommand _open;
        RelayCommand _refresh;
        RelayCommand _deny;
        RelayCommand _complete;
        RelayCommand _email;
        RelayCommand _assign;
        RelayCommand _note;

        #endregion

        /// <summary>
        /// IT Notice UserControl ViewModel Constructor
        /// </summary>
        public ITNoticeUCViewModelBase()
        {
            CurrentGroup = "OverAll";
            if (TeamList == null)
            {
                TeamList = new ObservableCollection<ITTeamMember>(ITTeamMember.GetListAsync(true).Result);
            }
            SelectedTeamMember = TeamList.Any(o => o.Name == CurrentUser.FullName) ? CurrentUser.FullName : TeamList.First().Name;
            IsLiveUpdateOn = true;
        }

        /// <summary>
        /// Update the Notice Action Space when a new ticket is selected
        /// </summary>
        /// <param name="selectedValue"></param>
        public void UpdateActionSpace(object selectedValue)
        {
            if (selectedValue != null)
            {
                NotesTable = ITTicket.GetNotesDataTable(Convert.ToInt32(((DataRowView)selectedValue).Row[0]));
                AssignmentTable = ITTicket.GetAssignmentDataTable(Convert.ToInt32(((DataRowView)selectedValue).Row[0]));
                IsAssigned = AssignmentTable.Rows.Count > 0;
                OnPropertyChanged(nameof(IsAssigned));
                OnPropertyChanged(nameof(NotesTable));
                OnPropertyChanged(nameof(AssignmentTable));
            }
            else
            {
                NotesTable = null;
                AssignmentTable = null;
                IsAssigned = false;
                OnPropertyChanged(nameof(IsAssigned));
                OnPropertyChanged(nameof(NotesTable));
                OnPropertyChanged(nameof(AssignmentTable));
            }
        }

        #region View Command Interfaces

        /// <summary>
        /// Open IT Form Command
        /// </summary>
        public ICommand OpenCommand
        {
            get
            {
                if (_open == null)
                {
                    _open = new RelayCommand(OpenExecute);
                }
                return _open;
            }
        }

        private void OpenExecute(object parameter)
        {
            var itFormNumber = parameter != null ? Convert.ToInt32(parameter) : SelectedRow != null ? Convert.ToInt32(((DataRowView)SelectedRow).Row[0]) : 0;
            if (itFormNumber != 0)
            {
                DashBoardTabControl.WorkSpace.LoadITTicketTabItem(itFormNumber);
            }
        }

        /// <summary>
        /// Group Command
        /// </summary>
        public ICommand GroupCommand
        {
            get
            {
                if (_group == null)
                {
                    _group = new RelayCommand(GroupExecute);
                }
                return _group;
            }
        }

        /// <summary>
        /// Group Command Execution
        /// </summary>
        /// <param name="parameter">Column Header</param>
        public virtual void GroupExecute(object parameter)
        {
            var header = parameter as string;
            header = header.Replace(" ", "");
            header = header.Replace("#", "Number");
            if (header == "Date")
            {
                header = "SubmitDate";
            }
            OpenTicketsView.GroupDescriptions.Clear();
            OpenTicketsView.GroupDescriptions.Add(new PropertyGroupDescription(header));
            CurrentGroup = header;
            IsLiveUpdateOn = false;
            OnPropertyChanged(nameof(IsLiveUpdateOn));
        }

        /// <summary>
        /// Refresh Command
        /// </summary>
        public ICommand RefreshCommand
        {
            get
            {
                if (_refresh == null)
                {
                    _refresh = new RelayCommand(RefreshExecute, RefreshCanExecute);
                }
                return _refresh;
            }
        }

        /// <summary>
        /// Refresh Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        public virtual void RefreshExecute(object parameter)
        {
            if (parameter == null)
            {
                CurrentGroup = "OverAll";
                OpenTicketsView = null;
                IsLiveUpdateOn = true;
                OnPropertyChanged(nameof(IsLiveUpdateOn));
            }
        }
        private bool RefreshCanExecute(object parameter) => !IsLiveUpdateOn;

        /// <summary>
        /// Deny Command
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
        /// Deny Command Execution
        /// </summary>
        /// <param name="parameter">ITTicket.IDNumber</param>
        private void DenyExecute(object parameter)
        {
            var _idNumber = parameter == null
                ? Convert.ToInt32(((DataRowView)SelectedRow).Row[0])
                : Convert.ToInt32(parameter);
            var ticket = ITTicket.GetITTicketAsync(_idNumber).Result;
            if (ticket.AddNotes())
            {
                var _tempStatus = ticket.Status.Title;
                ticket.CompletionDate = DateTime.Today;
                ticket.Status = TicketStatus.Create("Denied");
                if (!ticket.Update())
                {
                    ExceptionWindow.Show("Update Failed", "OMNI is currently not able to process your update request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact IT directly.");
                    ticket.CompletionDate = DateTime.MinValue;
                    ticket.Status.Title = _tempStatus;
                }
            }
        }
        private bool DenyCanExecute(object parameter) => CanDeny && (parameter != null || SelectedRow != null);

        /// <summary>
        /// Complete Command
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
        /// Complete Command Execution
        /// </summary>
        /// <param name="parameter">ITTicket.IDNumber</param>
        private void CompleteExecute(object parameter)
        {
            var _idNumber = parameter == null
                ? Convert.ToInt32(((DataRowView)SelectedRow).Row[0])
                : Convert.ToInt32(parameter);
            var ticket = ITTicket.GetITTicketAsync(_idNumber).Result;
            if (ticket.AddNotes())
            {
                var _tempStatus = ticket.Status.Title;
                ticket.CompletionDate = DateTime.Today;
                ticket.Status = new TicketStatus { Title = nameof(Closed) };
                if (!ticket.Update())
                {
                    ExceptionWindow.Show("Update Failed", "OMNI is currently not able to process your update request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact IT directly.");
                    ticket.CompletionDate = DateTime.MinValue;
                    ticket.Status.Title = _tempStatus;
                }
            }
        }
        private bool CompleteCanExecute(object parameter) => CanComplete && (parameter != null || SelectedRow != null);

        // <summary>
        /// E-mail IT Ticket Command
        /// </summary>
        public ICommand EmailCommand
        {
            get
            {
                if (_email == null)
                {
                    _email = new RelayCommand(EmailExecute);
                }
                return _email;
            }
        }

        /// <summary>
        /// E-mail IT Ticket Execution
        /// </summary>
        /// <param name="parameter">Ticket Number</param>
        private void EmailExecute(object parameter)
        {
            var ticketID = parameter == null ? Convert.ToInt32(((DataRowView)SelectedRow).Row[0]) : Convert.ToInt32(parameter);
            var ticketSubmitter = parameter == null ? ((DataRowView)SelectedRow).Row[3].ToString() : ITTicket.GetITTicketAsync(Convert.ToInt32(parameter)).Result.Submitter;
            try
            {
                var reciever = Users.RetrieveEmailAddress(ticketSubmitter);
                EmailForm.ManualSend(reciever, $"Ticket Number #{ticketID}", false);
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
            }
        }

        /// <summary>
        /// Assign Command
        /// </summary>
        public ICommand AssignCommand
        {
            get
            {
                if (_assign == null)
                {
                    _assign = new RelayCommand(AssignExecute, AssignCanExecute);
                }
                return _assign;
            }
        }

        /// <summary>
        /// Assign Command Execution
        /// </summary>
        /// <param name="parameter">ITTicket.IDNumber</param>
        private void AssignExecute(object parameter)
        {
            var _ticket = parameter == null ? ITTicket.GetITTicketAsync(Convert.ToInt32(((DataRowView)SelectedRow).Row[0])).Result : ITTicket.GetITTicketAsync(Convert.ToInt32(parameter)).Result;
            _ticket.Status.Title = "Assigned";
            _ticket.Priority = Priority.Create("Standard");
            if (!_ticket.Update())
            {
                ExceptionWindow.Show("Update Failed", "OMNI is currently not able to process your update request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact IT directly.");
            }
            else
            {
                if (!ITTeamMember.AssignmentTransactionAsync(new ITTeamMember { Name = CurrentUser.FullName, AssignDate = DateTime.Now, Assigned = true }, Convert.ToInt32(_ticket.IDNumber), true).Result)
                {
                    ExceptionWindow.Show("Assignment Interference", "OMNI is currently not able to process your assignment request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact the IT OMNI Developer.");
                }
            }
            RefreshCommand.Execute(null);
        }
        private bool AssignCanExecute(object parameter) => CanAssign && (parameter != null || SelectedRow != null);

        /// <summary>
        /// Note Command
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
        /// Note Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        public virtual void NoteExecute(object parameter)
        {
            if (parameter == null)
            {
                var ticket = ITTicket.GetITTicketAsync(Convert.ToInt32(((DataRowView)SelectedRow).Row[0])).Result;
                if (ticket.AddNotes())
                {
                    OnPropertyChanged(nameof(NotesTable));
                }
            }
        }
        private bool NoteCanExecute(object parameter) => SelectedRow != null && Module != ITNotice.Closed;

        #endregion

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {

            }
        }
    }
}
