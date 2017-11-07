using OMNI.Commands;
using OMNI.CustomControls;
using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// CMMS Open Orders UserControl ViewModel Base Interaction Logic
    /// </summary>
    public class CMMSNoticeUCViewModelBase : ViewModelBase
    {
        #region Properties

        public DataTable NoticeTable { get; set; }
        private object selectedRow;
        public object SelectedRow
        {
            get { return selectedRow; }
            set { selectedRow = value; UpdateNotes(); }
        }
        public DataTable NotesTable { get; set; }
        public ObservableCollection<Users> CrewList { get; private set; }
        private string selectedCrewMember;
        public virtual string SelectedCrewMember
        {
            get { return selectedCrewMember; }
            set { selectedCrewMember = value; OnPropertyChanged(nameof(SelectedCrewMember)); }
        }
        public ICollectionView OpenOrdersView { get; set; }
        public string CurrentGroup { get; set; }
        public CMMSActionGridView Module { get; set; }
        public bool OpenOrders { get { return Module == CMMSActionGridView.Assigned; } }
        public bool Inbox { get { return Module == CMMSActionGridView.Pending; } }
        public bool Notice { get { return Module == CMMSActionGridView.OpenOrders; } }
        public bool Closed { get { return Module == CMMSActionGridView.Closed; } }
        public bool CanComplete { get { return ElevatedCMMSUser && !Closed && !Inbox; } }
        public bool CanDeny { get { return ElevatedCMMSUser && !Closed; } }
        public bool ElevatedCMMSUser { get { return CurrentUser.CMMSAdmin || CurrentUser.CMMSCrew; } }
        public bool AutoOff { get; set; }

        public virtual void CMMSNoticeOpenOrdersTick() { }
        public virtual void CMMSNoticePendingTick() { }
        public virtual void CMMSNoticeAssignedTick() { }
        public virtual void CMMSNoticeClosedTick() { }

        RelayCommand _open;
        RelayCommand _group;
        RelayCommand _refresh;
        RelayCommand _complete;
        RelayCommand _deny;
        RelayCommand _note;
        RelayCommand _email;
        RelayCommand _print;

        #endregion

        /// <summary>
        /// CMMS Open Orders UserControl ViewModel Constructor
        /// </summary>
        public CMMSNoticeUCViewModelBase()
        {
            CurrentGroup = "OverAll";
            if (NotesTable == null)
            {
                NotesTable = new DataTable();
            }
            if (CrewList == null)
            {
                CrewList = new ObservableCollection<Users>(Users.CMMSUserListAsync(true, addNone: false).Result);
                CrewList.Insert(0, Users.CreateCMMSUser("All", false));
            }
            SelectedCrewMember = CrewList.Any(o => o.FullName == CurrentUser.FullName) ? CurrentUser.FullName : "All";
            AutoOff = false;
            OnPropertyChanged(nameof(AutoOff));
        }

        /// <summary>
        /// Update the Notes Table based on SelectedRow
        /// </summary>
        public void UpdateNotes()
        {
            NotesTable = SelectedRow != null ? CMMSWorkOrder.LoadNotesAsync(Convert.ToInt32(((DataRowView)SelectedRow).Row[0])).Result : null;
            OnPropertyChanged(nameof(NotesTable));
        }

        /// <summary>
        /// Validates the Work Order completion
        /// </summary>
        /// <param name="wo">Work Order to validate</param>
        public static bool ValidateCompletion(CMMSWorkOrder wo)
        {
            if (wo.MachineDown)
            {
                var _result = MessageBox.Show("The machine is still marked as down and you are updating this work order as completed.\nWould you like to release the machine back to production?", "Machine Down Status", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
                switch (_result)
                {
                    case MessageBoxResult.Yes:
                        wo.MachineDown = false;
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
        /// Open Command
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

        /// <summary>
        /// Open Command Execution
        /// </summary>
        /// <param name="parameter">Work Order Number to open</param>
        private void OpenExecute(object parameter)
        {
            var woID = parameter != null ? Convert.ToInt32(parameter) : SelectedRow != null ? Convert.ToInt32(((DataRowView)SelectedRow).Row[0]) : 0;
            if (woID > 0)
            {
                DashBoardTabControl.WorkSpace.LoadCMMSWorkOrderTabItem(woID);
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
            if (header == "Asset")
            {
                header = nameof(WorkCenter);
            }
            if (header == "Part")
            {
                header = "GenericPart";
            }
            OpenOrdersView.GroupDescriptions.Clear();
            OpenOrdersView.GroupDescriptions.Add(new PropertyGroupDescription(header));
            OpenOrdersView.SortDescriptions.Add(new SortDescription(nameof(Priority), ListSortDirection.Ascending));
            CurrentGroup = header;
            AutoOff = true;
            OnPropertyChanged(nameof(AutoOff));
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
                OpenOrdersView = null;
                AutoOff = false;
                OnPropertyChanged(nameof(AutoOff));
            }
        }
        private bool RefreshCanExecute(object parameter) => AutoOff;

        /// <summary>
        /// Complete Command
        /// </summary>
        public ICommand CompleteCommand
        {
            get
            {
                if (_complete == null)
                {
                    _complete = new RelayCommand(CompleteExecute);
                }
                return _complete;
            }
        }

        /// <summary>
        /// Complete Command Execution
        /// </summary>
        /// <param name="parameter">Work Order Number to complete</param>
        public virtual void CompleteExecute(object parameter)
        {
            var woID = parameter != null ? Convert.ToInt32(parameter) : SelectedRow != null ? Convert.ToInt32(((DataRowView)SelectedRow).Row[0]) : 0;
            if (woID > 0)
            {
                var wo = CMMSWorkOrder.LoadAsync(woID).Result;
                if (ValidateCompletion(wo))
                {
                    if (!string.IsNullOrEmpty(OMNIDataBase.AddNoteAsync("cmms_work_order", wo.IDNumber).Result))
                    {
                        wo.Status = CMMSStatus.Completed;
                        wo.DateComplete = DateTime.Now;
                        wo.AttachedNotes = true;
                        wo.UpdateAsync();
                        using (BackgroundWorker bw = new BackgroundWorker())
                        {
                            try
                            {
                                bw.DoWork += new DoWorkEventHandler(
                                    delegate (object sender, DoWorkEventArgs e)
                                    {
                                        var email = Users.RetrieveEmailAddressAsync(wo.Submitter).Result;
                                        if (!email.Equals("Not on File", StringComparison.OrdinalIgnoreCase))
                                        {
                                            wo.ExportToPDF(true);
                                            EmailForm.SendwithAttachment(email, $"Your Maintenance Request # {wo.IDNumber} has been {wo.Status.ToString()}.\nPlease refer to your closed work orders in OMNI CMMS for more information.", $"WO # {wo.IDNumber} {wo.Status.ToString()}", $"{Properties.Settings.Default.omnitemp}{wo.IDNumber}.pdf");
                                        }
                                        if (File.Exists($"{Properties.Settings.Default.omnitemp}{wo.IDNumber}.pdf"))
                                        {
                                            File.Delete($"{Properties.Settings.Default.omnitemp}{wo.IDNumber}.pdf");
                                        }
                                    });
                                bw.RunWorkerAsync();
                                OnPropertyChanged(nameof(_complete.CanExecute));
                            }
                            catch (Exception f)
                            {
                                ExceptionWindow.Show("Unhandled Exception", f.Message, f);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deny Command
        /// </summary>
        public ICommand DenyCommand
        {
            get
            {
                if (_deny == null)
                {
                    _deny = new RelayCommand(DenyExecute);
                }
                return _deny;
            }
        }

        /// <summary>
        /// Deny Command Execution
        /// </summary>
        /// <param name="parameter">Work Order Number to deny</param>
        public virtual void DenyExecute(object parameter)
        {
            var woID = parameter != null ? Convert.ToInt32(parameter) : SelectedRow != null ? Convert.ToInt32(((DataRowView)SelectedRow).Row[0]) : 0;
            if (woID > 0)
            {
                var wo = CMMSWorkOrder.LoadAsync(woID).Result;
                if (!string.IsNullOrEmpty(OMNIDataBase.AddNoteAsync("cmms_work_order", wo.IDNumber).Result))
                {
                    wo.Status = CMMSStatus.Denied;
                    wo.DateAssigned = DateTime.MinValue;
                    wo.DateComplete = DateTime.Today;
                    wo.CrewAssigned = "None";
                    wo.Priority = "--Unassigned--";
                    wo.AttachedNotes = true;
                    wo.UpdateAsync();
                    var email = Users.RetrieveEmailAddressAsync(wo.Submitter).Result;
                    if (email != "Not on File")
                    {
                        EmailForm.SendwithoutAttachment(email, $"Your Maintenance Request # {wo.IDNumber} has been denied.\nPlease refer to your closed work orders in OMNI CMMS for more information.", $"WO # {wo.IDNumber} Denied");
                    }
                }
            }
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
            var wo = CMMSWorkOrder.LoadAsync(Convert.ToInt32(((DataRowView)SelectedRow).Row[0])).Result;
            if (!string.IsNullOrEmpty(OMNIDataBase.AddNoteAsync("cmms_work_order", wo.IDNumber).Result))
            {
                wo.AttachedNotes = true;
                wo.UpdateAsync();
                UpdateNotes();
            }
        }
        private bool NoteCanExecute(object parameter) => SelectedRow == null ? false : true;

        // <summary>
        /// E-mail CMMS Work Order Command
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
        /// E-mail CMMS Work Order Execution
        /// </summary>
        /// <param name="parameter">QIR Number</param>
        private void EmailExecute(object parameter)
        {
            var woNumber = string.Empty;
            woNumber = parameter == null ? ((DataRowView)SelectedRow).Row[0].ToString() : parameter.ToString();
            try
            {
                CMMSWorkOrder.LoadAsync(Convert.ToInt32(woNumber)).Result.ExportToPDF(true);
                EmailForm.ManualSend($"Work Order #{woNumber}", $"{Properties.Settings.Default.omnitemp}{woNumber}.pdf");
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

        // <summary>
        /// Print Work Order Command
        /// </summary>
        public ICommand PrintCommand
        {
            get
            {
                if (_print == null)
                {
                    _print = new RelayCommand(PrintExecute);
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
            var woNumber = parameter == null ? Convert.ToInt32(((DataRowView)SelectedRow).Row[0]) : Convert.ToInt32(parameter);
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
