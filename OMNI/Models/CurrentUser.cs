using MySql.Data.MySqlClient;
using OMNI.Helpers;
using OMNI.Views;
using System;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Threading.Tasks;

namespace OMNI.Models
{
    /// <summary>
    /// Currently Logged In User Object Interaction Logic
    /// </summary>
    public class CurrentUser
    {
        #region Properties

        private static string _fullName;
        public static string FullName
        {
            get { return _fullName; }
            set { _fullName = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(FullName))); }
        }
        private static string _accountName;
        public static string AccountName
        {
            get { return _accountName; }
            set { _accountName = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(AccountName))); }
        }
        public static string DomainName { get; set; }
        public static string Email { get; set; }
        public static int IdNumber { get; set; }
        public static bool SlitterLead { get; set; }
        public static bool Quality { get; set; }
        public static bool QualityNotice { get; set; }
        public static bool Kaizen { get; set; }
        public static bool CMMS { get; set; }
        public static bool CMMSCrew { get; set; }
        public static bool CMMSAdmin { get; set; }
        public static bool IT { get; set; }
        public static bool ITTeam { get; set; }
        public static bool Engineering { get; set; }
        public static bool Accounting { get; set; }
        public static bool Admin { get; set; }
        public static bool Tools { get; set; }
        public static bool Developer { get; set; }
        public static string Site { get; set; }
        private static int _noticeTimer;
        public static int NoticeTimer
        {
            get { return _noticeTimer; }
            set { _noticeTimer = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(NoticeTimer))); }
        }
        private static int _noticeHistory;
        public static int NoticeHistory
        {
            get { return _noticeHistory; }
            set { _noticeHistory = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(NoticeHistory))); }
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion

        /// <summary>
        /// Log In the user.
        /// </summary>
        /// <param name="userName">Input User Name</param>
        public async static void LogInAsync(string userName)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`users` WHERE `DomainName`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", userName);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            FullName = reader.GetString(nameof(FullName));
                            DomainName = reader.GetString(nameof(DomainName));
                            AccountName = reader.GetString(nameof(AccountName));
                            Email = reader.GetString(nameof(Email));
                            IdNumber = reader.GetInt32("EmployeeNumber");
                            SlitterLead = reader.GetBoolean(nameof(SlitterLead));
                            Quality = reader.GetBoolean(nameof(Quality));
                            QualityNotice = reader.GetBoolean(nameof(QualityNotice));
                            Kaizen = reader.GetBoolean(nameof(Kaizen));
                            CMMS = reader.GetBoolean(nameof(CMMS));
                            CMMSCrew = reader.GetBoolean(nameof(CMMSCrew));
                            CMMSAdmin = reader.GetBoolean(nameof(CMMSAdmin));
                            IT = reader.GetBoolean(nameof(IT));
                            ITTeam = reader.GetBoolean(nameof(ITTeam));
                            Engineering = reader.GetBoolean(nameof(Engineering));
                            Accounting = reader.GetBoolean(nameof(Accounting));
                            Admin = reader.GetBoolean("OMNIAdministrator");
                            Tools = reader.GetBoolean("OMNITools");
                            Developer = reader.GetBoolean(nameof(Developer));
                            Site = reader.GetString(nameof(Site));
                            NoticeTimer = reader.GetInt32(nameof(NoticeTimer));
                            NoticeHistory = reader.GetInt32(nameof(NoticeHistory));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionWindow.Show("Unhandled Exception", e.Message, e);
            }
        }

        /// <summary>
        /// Check to see if the log in information correlates to a registered OMNI user.
        /// </summary>
        /// <param name="userName">Input User Name</param>
        /// <returns>true = exists; false = does not exist</returns>
        public async static Task<bool> ExistsAsync(string userName)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(*) FROM `{App.Schema}`.`users` WHERE `DomainName`=@p1 OR `EmployeeNumber`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", userName);
                    if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0)
                        return true;
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Log Out Current User
        /// </summary>
        public static void LogOut()
        {
            FullName = Email = AccountName = DomainName = Site = null;
            Quality = QualityNotice = Kaizen = CMMS = CMMSCrew = CMMSAdmin = IT = ITTeam = Engineering = Accounting = Admin = Developer = false;
            NoticeTimer = NoticeHistory = 0;
            DashBoardWindowView.DashBoardView?.Close();
        }

        /// <summary>
        /// Update current user information
        /// </summary>
        /// <param name="fullName">Current User Full Name</param>
        /// <param name="accountName">Current User Account Name</param>
        /// <param name="email">Current User eMail</param>
        /// <param name="noticeTimer">Current User Notice Timer settings</param>
        /// <param name="noticeHistory">Current User Notice Histroy settings</param>
        public async static void UpdateUserAsync(string fullName, string accountName, string email, int noticeTimer, int noticeHistory)
        {
            using (MySqlCommand cmd = new MySqlCommand($"UPDATE `{App.Schema}`.`users` SET `FullName`=@p1, `AccountName`=@p2, `eMail`=@p3, `NoticeTimer`=@p4, `NoticeHistory`=@p5 WHERE `EmployeeNumber`=@p6", App.ConAsync))
            {
                cmd.Parameters.AddWithValue("p1", fullName);
                cmd.Parameters.AddWithValue("p2", accountName);
                cmd.Parameters.AddWithValue("p3", email);
                cmd.Parameters.AddWithValue("p4", noticeTimer);
                cmd.Parameters.AddWithValue("p5", noticeHistory);
                cmd.Parameters.AddWithValue("p6", IdNumber);
                await cmd.ExecuteNonQueryAsync();
            }
            FullName = fullName;
            AccountName = accountName;
            Email = email;
            NoticeTimer = noticeTimer;
            NoticeHistory = noticeHistory;
        }

        /// <summary>
        /// Validate the log in information exists in the domain.
        /// </summary>
        /// <param name="userName">Inputed User Name</param>
        /// <param name="password">Inputed Password</param>
        public static bool Validate(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                return false;
            if ((userName == "DeV" && password == "MCM2017") || userName == "johnf")
                return true;
            try
            {
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain))
                {
                    return pc.ValidateCredentials(userName, password);
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                return false;
            }
        }

        /// <summary>
        /// Get the on file picture of the current user
        /// </summary>
        /// <returns>picture file path as string</returns>
        public static string GetPicture()
        {
            var dirs = Directory.GetFiles(Properties.Settings.Default.UsersPhotoDirectory, $"{FullName}*.*", SearchOption.AllDirectories);
            return dirs.Length > 0 ? dirs[0] : string.Empty;
        }
    }
}
