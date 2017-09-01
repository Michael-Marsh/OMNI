using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;

namespace OMNI.Models
{
    /// <summary>
    /// OMNI Users Object Interaction Logic
    /// </summary>
    public class Users : INotifyPropertyChanged
    {
        #region Properties

        public int UserID { get; set; }
        public string FullName { get; set; }
        public string DomainName { get; set; }
        public int EmployeeNumber { get; set; }
        public string Email { get; set; }
        public bool SlitterLead { get; set; }
        public bool Quality { get; set; }
        public bool QualityNotice { get; set; }
        public bool Kaizen { get; set; }
        public bool CMMS { get; set; }
        public bool CMMSCrew { get; set; }
        public bool CMMSAdmin { get; set; }
        public bool IT { get; set; }
        public bool ITTeam { get; set; }
        public bool Engineering { get; set; }
        public bool Accounting { get; set; }
        public bool Admin { get; set; }
        public bool Developer { get; set; }
        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; OnPropertyChanged(nameof(Selected)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        /// <summary>
        /// OMNI User Object Creation
        /// </summary>
        /// <param name="userID">User ID</param>
        /// <param name="fullName">User Full Name</param>
        /// <param name="domainName">User Domain Name</param>
        /// <param name="employeeNumber">User Employee Number</param>
        /// <param name="eMail">User Email</param>
        /// <param name="slitterLead">Slitter Lead privilage</param>
        /// <param name="quality">Quality privilage</param>
        /// <param name="qualityNotice">Quality Notice privilage</param>
        /// <param name="kaizen">Kaizen privilage</param>
        /// <param name="cmms">OMNI CMMS privilage</param>
        /// <param name="cmmsCrew">CMMS Crew privilage</param>
        /// <param name="cmmsAdmin">CMMS Administrator privilage</param>
        /// <param name="it">IT privilage</param>
        /// <param name="itTeam">IT Team Member privilage</param>
        /// <param name="engineering">Engineering privilage</param>
        /// <param name="accounting">Accounting privilage</param>
        /// <param name="admin">OMNI Administrator privilage</param>
        /// <param name="developer">OMNI Developer privilage</param>
        /// <returns>New User Object</returns>
        public static Users CreateUser(int userID, string fullName, string domainName, int employeeNumber, string eMail, bool slitterLead, bool quality, bool qualityNotice, bool kaizen, bool cmms, bool cmmsCrew, bool cmmsAdmin, bool it, bool itTeam, bool engineering, bool accounting, bool admin, bool developer)
        {
            return new Users
            {
                UserID = userID,
                FullName = fullName,
                DomainName = domainName,
                EmployeeNumber = employeeNumber,
                Email = eMail,
                SlitterLead = slitterLead,
                Quality = quality,
                QualityNotice = qualityNotice,
                Kaizen = kaizen,
                CMMS = cmms,
                CMMSCrew = cmmsCrew,
                CMMSAdmin = cmmsAdmin,
                IT = it,
                ITTeam = itTeam,
                Engineering = engineering,
                Accounting = accounting,
                Admin = admin,
                Developer = developer
            };
        }

        /// <summary>
        /// OMNI CMMS User Object Creation
        /// </summary>
        /// <param name="fullname">User Full Name</param>
        /// <param name="isSelected">Used to assign multiple crew members to the same workorder</param>
        /// <returns>New CMMS User Object</returns>
        public static Users CreateCMMSUser(string fullname, bool isSelected)
        {
            return new Users { FullName = fullname, Selected = isSelected };
        }

        /// <summary>
        /// OMNI IT Team User Object Creation
        /// </summary>
        /// <param name="fullname">User Full Name</param>
        /// <param name="isSelected">Used to assign multiple crew members to the same workorder</param>
        /// <returns>New IT Team User Object</returns>
        public static Users CreateTeamUser(string fullname, bool isSelected)
        {
            return new Users { FullName = fullname, Selected = isSelected };
        }

        /// <summary>
        /// User Object List Creation
        /// </summary>
        /// <returns>New user object list</returns>
        public async static Task<List<Users>> UserListAsync()
        {
            var _userList = new List<Users>();
            using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`users`", App.ConAsync))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        _userList.Add(CreateUser(reader.GetInt16(nameof(UserID)),
                           reader.GetString(nameof(DomainName)),
                           reader.GetString(nameof(FullName)),
                           reader.GetInt32(nameof(EmployeeNumber)),
                           reader.GetString("eMail"),
                           reader.GetBoolean(nameof(SlitterLead)),
                           reader.GetBoolean(nameof(Quality)),
                           reader.GetBoolean(nameof(QualityNotice)),
                           reader.GetBoolean(nameof(Kaizen)),
                           reader.GetBoolean(nameof(CMMS)),
                           reader.GetBoolean(nameof(CMMSCrew)),
                           reader.GetBoolean(nameof(CMMSAdmin)),
                           reader.GetBoolean(nameof(IT)),
                           reader.GetBoolean(nameof(ITTeam)),
                           reader.GetBoolean(nameof(Engineering)),
                           reader.GetBoolean(nameof(Accounting)),
                           reader.GetBoolean("OMNIAdministrator"),
                           reader.GetBoolean(nameof(Developer))));
                    }
                }
            }
            return _userList;
        }

        /// <summary>
        /// CMMS User Object List Creation
        /// </summary>
        /// <param name="newList">true = new list / false = loaded list</param>
        /// <param name="workOrder">optional: CMMS Work order to load</param>
        /// <param name="addNone">optional: (default) true = add None to the list / false = do not add None to the list</param>
        /// <returns>New CMMS user object list</returns>
        public async static Task<List<Users>> CMMSUserListAsync(bool newList, int workOrder = 0, bool addNone = true)
        {
            var _cmmsUserList = new List<Users>();
            if (addNone)
            {
                _cmmsUserList.Add(CreateCMMSUser("None", false));
            }
            using (MySqlCommand cmd = new MySqlCommand($"SELECT FullName FROM `{App.Schema}`.`users` WHERE CMMSCrew=1", App.ConAsync))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        _cmmsUserList.Add(CreateCMMSUser(reader.GetString(nameof(FullName)), false));
                    }
                }
            }
            if (!newList)
            {
                var _assigned = string.Empty;
                using (MySqlCommand cmd = new MySqlCommand($"SELECT CrewMembersAssigned FROM `{App.Schema}`.`cmmsworkorder` WHERE WorkOrderNumber=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", workOrder);
                    _assigned = (await cmd.ExecuteScalarAsync()).ToString();
                }
                var counter = 0;
                foreach (var item in _cmmsUserList)
                {
                    if (_assigned.Contains(item.FullName))
                    {
                        _cmmsUserList[counter].Selected = true;
                    }
                    counter++;
                }
            }
            return _cmmsUserList;
        }

        /// <summary>
        /// Assigned CMMS Crew Object List Creation
        /// </summary>
        /// <param name="workOrder">Work Order number to load list from</param>
        /// <returns>CMMS crew members assigned as user object list</returns>
        public async static Task<List<Users>> CMMSAssignedCrewListAsync(int workOrder)
        {
            var _cmmsUserList = new List<Users>();
            var _crewAssigned = string.Empty;
            using (MySqlCommand cmd = new MySqlCommand($"SELECT CrewMembersAssigned FROM `{App.Schema}`.`cmmsworkorder` WHERE WorkOrderNumber=@p1", App.ConAsync))
            {
                cmd.Parameters.AddWithValue("p1", workOrder);
                _crewAssigned = (await cmd.ExecuteScalarAsync()).ToString();
            }
            var crewArray = _crewAssigned.Split('/');
            foreach (string s in crewArray)
            {
                _cmmsUserList.Add(CreateCMMSUser(s, true));
            }
            return _cmmsUserList;
        }

        /// <summary>
        /// Retrieve a users e-mail address
        /// </summary>
        /// <param name="userFullName">Submitter Full Name</param>
        /// <returns>e-mail address</returns>
        public async static Task<string> RetrieveEmailAddressAsync(string userFullName)
        {
            try
            {
                while (App.ConAsync.State.Equals(ConnectionState.Fetching))
                { }
                using (MySqlCommand cmd = new MySqlCommand($"`{App.Schema}`.`query_user_eMail`", App.ConAsync) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@fullName", userFullName);
                    return (await cmd.ExecuteScalarAsync()).ToString();
                }
            }
            catch (Exception)
            {
                return "Not on File";
            }
        }
    }
}
