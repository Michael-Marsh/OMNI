using OMNI.Commands;
using OMNI.CustomControls;
using OMNI.Models;
using System.Windows.Controls;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// User Account UserControl ViewModel Interaction Logic
    /// </summary>
    public class UserAccountUCViewModel : ViewModelBase
    {
        #region Properties

        public int IDNumber
        {
            get { return CurrentUser.IdNumber; }
        }
        private string fullName;
        public string FullName
        {
            get { return fullName; }
            set { fullName = value; OnPropertyChanged(nameof(FullName)); }
        }
        private string accountName;
        public string AccountName
        {
            get { return accountName; }
            set { accountName = value; OnPropertyChanged(nameof(accountName)); }
        }
        private string email;
        public string Email
        {
            get { return email; }
            set { email = value; OnPropertyChanged(nameof(Email)); }
        }
        private int noticeTimer;
        public int NoticeTimer
        {
            get { return noticeTimer; }
            set { noticeTimer = value; OnPropertyChanged(nameof(NoticeTimer)); }
        }
        private int noticeHistory;
        public int NoticeHistory
        {
            get { return noticeHistory; }
            set { noticeHistory = value; OnPropertyChanged(nameof(NoticeHistory)); }
        }
        public string UserPicture { get { return CurrentUser.GetPicture(); } }

        RelayCommand _save;

        #endregion

        /// <summary>
        /// User Account UserControl ViewModel Constructor
        /// </summary>
        public UserAccountUCViewModel()
        {
            FullName = CurrentUser.FullName;
            Email = CurrentUser.Email;
            AccountName = CurrentUser.AccountName;
            NoticeTimer = CurrentUser.NoticeTimer;
            NoticeHistory = CurrentUser.NoticeHistory;
        }

        /// <summary>
        /// Save Changes Command
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if (_save == null)
                {
                    _save = new RelayCommand(SaveExecute);
                }
                return _save;
            }
        }

        /// <summary>
        /// Save Changes Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private void SaveExecute(object parameter)
        {
            CurrentUser.UpdateUserAsync(FullName, AccountName, Email, NoticeTimer, NoticeHistory);
            ((TabItem)(DashBoardTabControl.WorkSpace.SelectedItem)).Header = $"{AccountName} Account";
        }

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing) { }
        }
    }
}
