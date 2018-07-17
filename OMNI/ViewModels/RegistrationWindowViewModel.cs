using System.Data.SqlClient;
using OMNI.Commands;
using OMNI.Helpers;
using OMNI.Models;
using OMNI.Views;
using System.Windows;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    public class RegistrationWindowViewModel : ViewModelBase
    {
        #region Properties

        public string DomainName { get; set; }
        public string FullName { get; set; }
        public int? UserId { get; set; }
        public string Email { get; set; }
        public bool WCCO { get; set; }
        public bool CSI { get; set; }
        public Window Win { get; set; }

        RelayCommand _create;

        #endregion

        /// <summary>
        /// Registration Window ViewModel Constructor
        /// </summary>
        public RegistrationWindowViewModel()
        {
        }

        /// <summary>
        /// Register a new OMNI user.
        /// </summary>
        /// <param name="userName">Input User Name</param>
        public void RegisterUser(string userName)
        {
            DomainName = userName;
            Win = new RegistrationWindowView { DataContext = this };
            Win.ShowDialog();
        }

        #region Create User ICommand

        /// <summary>
        /// Create new OMNI user Interface Command
        /// </summary>
        public ICommand CreateCommand
        {
            get
            {
                if (_create == null)
                    _create = new RelayCommand(CreateExecuteAsync, CreateCanExecute);
                return _create;
            }
        }

        /// <summary>
        /// Create a new OMNI user Interface Command Execution
        /// </summary>
        /// <param name="parameter"></param>
        private async void CreateExecuteAsync(object parameter)
        {
            if (OMNIDataBase.Exists("users", "EmployeeNumber", UserId.ToString()))
            {
                ExceptionWindow.Show("Invalid User ID", "The user ID that you have entered already exists.\nPlease double check your entry and try again.\nIf you feel you have reached this message in error please contact IT.");
            }
            else
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase}
                                                        INSERT INTO users (DomainName, FullName, AccountName, EmployeeNumber, eMail, Site) Values (@p1, @p2, @p3, @p4, @p5, @p6)", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", DomainName);
                    cmd.Parameters.AddWithValue("p2", FullName);
                    if (FullName.Contains(" "))
                    {
                        cmd.Parameters.AddWithValue("p3", FullName.Substring(0, FullName.IndexOf(" ")));
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("p3", FullName);
                    }
                    cmd.Parameters.AddWithValue("p4", UserId);
                    if (!string.IsNullOrEmpty(Email))
                        cmd.Parameters.AddWithValue("p5", Email);
                    else
                        cmd.Parameters.AddWithValue("p5", "Not on File");
                    if (WCCO)
                        cmd.Parameters.AddWithValue("p6", nameof(WCCO));
                    else
                        cmd.Parameters.AddWithValue("p6", nameof(CSI));
                    await cmd.ExecuteNonQueryAsync();
                }
                Win.Close();
            }
        }
        private bool CreateCanExecute(object parameter) => string.IsNullOrEmpty(FullName) || UserId == null
                ? false
                : true;

        #endregion

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                Win = null;
                _create = null;
            }
        }
    }
}
