using Microsoft.Win32;
using MySql.Data.MySqlClient;
using OMNI.Enumerations;
using OMNI.Extensions;
using OMNI.HDT.Enumeration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMNI.Models
{
    /// <summary>
    /// IT Ticket Object
    /// </summary>
    public class ITTicket : FormBase, IChangeTracking
    {
        #region Properties

        public string Location { get; set; }
        public TicketSubject Subject { get; set; }
        public TicketType Type { get; set; }
        public DateTime RequestDate { get; set; }
        public string RequestReason { get; set; }
        public string Description { get; set; }
        public bool IAR { get; set; }
        public TicketStatus Status { get; set; }
        public Priority Priority { get; set; }
        public bool Confidential { get; set; }
        public DateTime CompletionDate { get; set; }
        public BindingList<ITTeamMember> AssignedTo { get; set; }
        public bool CanAssign { get { return CurrentUser.ITTeam && CompletionDate == DateTime.MinValue; } }
        public bool CanEdit { get { return (CurrentUser.ITTeam || CurrentUser.FullName == Submitter) && CompletionDate == DateTime.MinValue; } }
        public DataTable NotesTable { get; set; }
        public string POC { get; set; }
        public BindingList<ITDocument> DocumentList { get; set; }
        public ITProject Project { get; set; }

        public bool IsChanged
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        public ITTicket()
        {
            if (AssignedTo == null)
            {
                AssignedTo = ITTeamMember.GetBindingListAsync(false).Result;
            }
            Submitter = CurrentUser.FullName;
            Date = DateTime.Today;
            Status = TicketStatus.Create("Pending");
            if (DocumentList == null)
            {
                DocumentList = new BindingList<ITDocument>();
            }
            Type = TicketType.Ticket;
            FormModule = Module.HDT;
        }

        /// <summary>
        /// Load an ITTicket Object from the database based on an ID Number
        /// </summary>
        /// <param name="idNumber">IT Ticket Number</param>
        /// <returns>ITTicket Object</returns>
        public async static Task<ITTicket> GetITTicketAsync(int idNumber)
        {
            var _ticket = new ITTicket();
            var _tempPriority = string.Empty;
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`it_ticket_master` WHERE `TicketNumber`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", idNumber);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            _ticket.IDNumber = idNumber;
                            _ticket.Submitter = reader.GetString(nameof(Submitter));
                            _ticket.Date = reader.GetDateTime("SubmitDate");
                            _ticket.Location = reader.GetString(nameof(Location));
                            _ticket.Subject = new TicketSubject { Title = reader.GetString(nameof(Subject)) };
                            _ticket.Type = (TicketType)Enum.Parse(typeof(TicketType), reader.GetString(nameof(Type)));
                            _ticket.RequestDate = reader.GetDateTime("RequestCompletionDate");
                            _ticket.RequestReason = reader.IsDBNull(7) ? string.Empty : reader.GetString("RequestCompletionReason");
                            _ticket.Description = reader.GetString(nameof(Description));
                            _ticket.IAR = reader.GetBoolean(nameof(IAR));
                            _ticket.Status = TicketStatus.Create(reader.GetString(nameof(Status)));
                            _tempPriority = reader.GetString(nameof(Priority));
                            _ticket.Confidential = reader.GetBoolean(nameof(Confidential));
                            _ticket.CompletionDate = reader.GetDateTime("DateCompleted");
                            _ticket.POC = reader.IsDBNull(14) ? string.Empty : reader.GetString(nameof(POC));
                        }
                    }
                    _ticket.Priority = _tempPriority == "--Unassigned--" ? Priority.Create(6, "--Unassigned--") : Priority.Create(_tempPriority);
                }
                _ticket.AssignedTo = await ITTeamMember.GetBindingListAsync(false).ConfigureAwait(false);
                if (_ticket.Priority.Level != 6)
                {
                    using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`it_ticket_assignment` WHERE `TicketNumber`=@p1", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", _ticket.IDNumber);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                _ticket.AssignedTo.First(o => o.Name == reader.GetString("TeamMember")).Assigned = true;
                                _ticket.AssignedTo.First(o => o.Name == reader.GetString("TeamMember")).AssignDate = reader.GetDateTime("AssignmentDate");
                            }
                        }
                    }
                }
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`it_ticket_documents` WHERE `TicketNumber`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", _ticket.IDNumber);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            _ticket.DocumentList.Add(new ITDocument { FileName = reader.GetString("FilePath"), FilePath = $"{Properties.Settings.Default.HDTDocumentLocation}{reader.GetString("FilePath")}", IsSelected = true });
                        }
                    }
                }
                if (_ticket.Type == TicketType.Project)
                {
                    using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`it_projects` WHERE `TicketNumber`=@p1", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", _ticket.IDNumber);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                _ticket.Project = new ITProject { ID = reader.GetInt32("TicketNumber"), Title = reader.GetString("Title"), DDD = reader.GetDateTime("DropDeadDate"), PromiseDate = reader.GetDateTime("PromiseDate") };
                            }
                        }
                    }
                }
                else if (_ticket.Type == TicketType.Task)
                {
                    using (MySqlCommand cmd = new MySqlCommand($"SELECT `TicketNumber` FROM `{App.Schema}`.`it_project_tasks` WHERE `TicketNumber`=@p1", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", _ticket.IDNumber);
                        _ticket.IDNumber = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                }
                return _ticket;
            }
            catch (Exception)
            {
                return new ITTicket();
            }
        }

        /// <summary>
        /// IT Ticket Notice DataTable
        /// </summary>
        /// <param name="noticeType">Type of ITNotice to load</param>
        /// <returns>NoticeTable</returns>
        public async static Task<DataTable> GetNoticeDataTableAsync(ITNotice noticeType)
        {
            var _where = string.Empty;
            _where = !noticeType.Equals(ITNotice.Closed) ? $"`Status`='{noticeType.GetDescription()}' AND `Type`<>'Project'" : $"(`Status`='{noticeType.GetDescription()}' OR `Status`='Denied') AND `DateCompleted`>={DateTime.Today.AddMonths(-1).ToString("yyyy-MM-dd")}";
            if (!CurrentUser.ITTeam)
            {
                _where += $" AND (`Confidential`=0 OR (`Confidential`=1 AND `Submitter`='{CurrentUser.FullName}'))";
            }
            using (DataTable dt = new DataTable())
            {
                try
                {
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter($"SELECT * FROM `{App.Schema}`.`it_ticket_master` WHERE {_where}", App.ConAsync))
                    {
                        await adapter.FillAsync(dt).ConfigureAwait(false);
                        return dt;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// IT Ticket Notice DataTable filtered for selected team memebers
        /// </summary>
        /// <param name="teamMember">Selected Team Member</param>
        /// <returns>NoticeTable</returns>
        public async static Task<DataTable> GetNoticeDataTableAsync(string teamMember)
        {
            try
            {
                var _where = string.Empty;
                var _team = string.Empty;
                if (teamMember != "All")
                {
                    using (MySqlCommand cmd = new MySqlCommand($"SELECT `TicketNumber` FROM {App.Schema}.`it_ticket_assignment` WHERE `TeamMember`=@p1", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", teamMember);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            _where = "`Status`='Assigned' AND `Type`<>'Project' AND `TicketNumber` IN (";
                            var ticketNumbers = new StringBuilder();
                            ticketNumbers.Append(_where);
                            while (await reader.ReadAsync())
                            {
                                ticketNumbers.Append($"{reader.GetString(0)}, ");
                            }
                            _where = $"{ticketNumbers.Remove(ticketNumbers.Length - 2, 2)})";
                        }
                    }
                }
                else
                {
                    _where = "`Status`='Assigned' AND `Type`<>'Project'";
                }
                if (!CurrentUser.ITTeam)
                {
                    _where += $" AND (`Confidential`=0 OR (`Confidential`=1 AND `Submitter`='{CurrentUser.FullName}'))";
                }
                using (DataTable dt = new DataTable())
                {
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter($"SELECT * FROM `{App.Schema}`.`it_ticket_master` WHERE {_where}", App.ConAsync))
                    {
                        await adapter.FillAsync(dt).ConfigureAwait(false);
                        return dt;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// IT Ticket Notes DataTable
        /// </summary>
        /// <returns>NotesTable</returns>
        public async static Task<DataTable> GetNotesDataTableAsync()
        {
            using (DataTable dt = new DataTable())
            {
                try
                {
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter($"SELECT * FROM `{App.Schema}`.`it_ticket_notes`", App.ConAsync))
                    {
                        await adapter.FillAsync(dt).ConfigureAwait(false);
                        return dt;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Load current IT Ticket note's
        /// </summary>
        /// <param name="ticketNumber">IT Ticket Number</param>
        /// <returns>NotesTable as DataTable. Handle Null return on ticketNumber == 0</returns>
        public async static Task<DataTable> GetNotesDataTableAsync(int ticketNumber)
        {
            if (ticketNumber == 0)
            {
                return null;
            }
            using (MySqlDataAdapter adapter = new MySqlDataAdapter($"SELECT * FROM `{App.Schema}`.`it_ticket_notes` WHERE `IDNumber`=@p1", App.ConAsync))
            {
                adapter.SelectCommand.Parameters.AddWithValue("p1", ticketNumber);
                using (DataTable dt = new DataTable())
                {
                    await adapter.FillAsync(dt).ConfigureAwait(false);
                    return dt;
                }
            }
        }

        /// <summary>
        /// Load current IT Ticket assignment's
        /// </summary>
        /// <param name="ticketNumber">IT Ticket Number</param>
        /// <returns>AssignmentTable as DataTable.  Handle Null return on ticketNumber == 0</returns>
        public async static Task<DataTable> GetAssignmentDataTableAsync(int ticketNumber)
        {
            if (ticketNumber == 0)
            {
                return null;
            }
            using (MySqlDataAdapter adapter = new MySqlDataAdapter($"SELECT * FROM `{App.Schema}`.`it_ticket_assignment` WHERE `TicketNumber`=@p1", App.ConAsync))
            {
                adapter.SelectCommand.Parameters.AddWithValue("p1", ticketNumber);
                using (DataTable dt = new DataTable())
                {
                    await adapter.FillAsync(dt).ConfigureAwait(false);
                    return dt;
                }
            }
        }

        /// <summary>
        /// Load current IT projects
        /// </summary>
        /// <returns>Projects DataTable</returns>
        public async static Task<DataTable> GetProjectDataTableAsync()
        {
            try
            {
                using (MySqlDataAdapter adapter = new MySqlDataAdapter($"SELECT * FROM `{App.Schema}`.`it_projects` AS p, `{App.Schema}`.`it_ticket_master` AS m WHERE p.`TicketNumber` = m.`TicketNumber`", App.ConAsync))
                {
                    using (DataTable dt = new DataTable())
                    {
                        await adapter.FillAsync(dt);
                        return dt;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Load current IT project tasks
        /// </summary>
        /// <param name="projectID">Project ID Number to sort by</param>
        /// <returns>Project Tasks DataTable</returns>
        public async static Task<DataTable> GetProjectTaskDataTableAsync(int projectID)
        {
            try
            {
                using (MySqlDataAdapter adapter = new MySqlDataAdapter($"SELECT * FROM `{App.Schema}`.`it_project_tasks` AS t, `{App.Schema}`.`it_ticket_master` AS m WHERE t.`TicketNumber` = m.`TicketNumber` AND t.`ProjectID`={projectID}", App.ConAsync))
                {
                    using (DataTable dt = new DataTable())
                    {
                        await adapter.FillAsync(dt).ConfigureAwait(false);
                        return dt;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get YTD IT Ticket metrics
        /// </summary>
        /// <param name="mType">Type of metric to calculate</param>
        /// <returns>Count value as int based on metric type</returns>
        public async static Task<int> GetMetricsAsync(MetricType mType)
        {
            try
            {
                var cmdText = string.Empty;
                switch (mType)
                {
                    case MetricType.Completed:
                        cmdText = $"SELECT COUNT(*) FROM {App.Schema}.`it_ticket_master` WHERE (`DateCompleted` BETWEEN '{DateTime.Now.Year}-01-01' AND '{DateTime.Now.AddYears(1).Year}-01-01') AND `Type` IN ('Ticket', 'Task')";
                        break;
                    case MetricType.Submission:
                        cmdText = $"SELECT COUNT(*) FROM {App.Schema}.`it_ticket_master` WHERE (`SubmitDate` BETWEEN '{DateTime.Now.Year}-01-01' AND '{DateTime.Now.AddYears(1).Year}-01-01') AND `Type` IN ('Ticket', 'Task')";
                        break;
                    case MetricType.ResponseTime:
                        cmdText = $"SELECT AVG(`ResponseTime`) FROM {App.Schema}.`it_ticket_master` WHERE (`DateCompleted` BETWEEN '{DateTime.Now.Year}-01-01' AND '{DateTime.Now.AddYears(1).Year}-01-01') AND `Type` IN ('Ticket', 'Task')";
                        break;
                }
                using (MySqlCommand cmd = new MySqlCommand(cmdText, App.ConAsync))
                {
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Get MTD IT Ticket metrics
        /// </summary>
        /// <param name="mType">Type of metric to calculate</param>
        /// <param name="month">Month of metric</param>
        /// <param name="year">Year of metric</param>
        /// <returns>Count value as int based on metric type</returns>
        public async static Task<int> GetMetricsAsync(MetricType mType, int month, int year)
        {
            try
            {
                var cmdText = string.Empty;
                var nextYear = month == 12 ? year + 1 : year;
                var nextMonth = month == 12 ? 1 : month + 1;
                switch (mType)
                {
                    case MetricType.Completed:
                        cmdText = $"SELECT COUNT(*) FROM {App.Schema}.`it_ticket_master` WHERE `Status` IN ('Closed', 'Denied') AND (`SubmitDate` BETWEEN '{year}-{month}-01' AND '{nextYear}-{nextMonth}-01') AND `Type` IN ('Ticket', 'Task')";
                        break;
                    case MetricType.Submission:
                        cmdText = $"SELECT COUNT(*) FROM {App.Schema}.`it_ticket_master` WHERE (`SubmitDate` BETWEEN '{year}-{month}-01' AND '{nextYear}-{nextMonth}-01') AND `Type` IN ('Ticket', 'Task')";
                        break;
                    case MetricType.ResponseTime:
                        cmdText = $"SELECT AVG(`ResponseTime`) FROM {App.Schema}.`it_ticket_master` WHERE (`DateCompleted` BETWEEN '{year}-{month}-01' AND '{nextYear}-{nextMonth}-01') AND (`SubmitDate` BETWEEN '{year}-{month}-01' AND '{nextYear}-{nextMonth}-01') AND `Type` IN ('Ticket', 'Task')";
                        break;
                }
                using (MySqlCommand cmd = new MySqlCommand(cmdText, App.ConAsync))
                {
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public void AcceptChanges()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// IT Ticket Type
    /// </summary>
    public class TicketSubject
    {
        #region Properties

        public string Title { get; set; }

        #endregion

        public static List<TicketSubject> GetList()
        {
            try
            {
                var _list = new List<TicketSubject>();
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`it_ticket_type`", App.ConAsync))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _list.Add(new TicketSubject { Title = reader.GetString(nameof(Title)) });
                        }
                    }
                }
                return _list;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async static Task<List<TicketSubject>> GetListAsync()
        {
            try
            {
                var _list = new List<TicketSubject>();
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`it_ticket_type`", App.ConAsync))
                {
                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            _list.Add(new TicketSubject { Title = reader.GetString(nameof(Title)) });
                        }
                    }
                }
                return _list;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// IT Ticket Status
    /// </summary>
    public class TicketStatus
    {
        #region Properties

        public string Title { get; set; }

        #endregion

        public static TicketStatus Create(string title)
        {
            return new TicketStatus { Title = title };
        }

        public static List<TicketStatus> GetList()
        {
            try
            {
                var _list = new List<TicketStatus>();
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`it_ticket_status`", App.ConAsync))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _list.Add(Create(reader.GetString(nameof(Title))));
                        }
                    }
                }
                return _list;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async static Task<List<TicketStatus>> GetListAsync()
        {
            try
            {
                var _list = new List<TicketStatus>();
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`it_ticket_status`", App.ConAsync))
                {
                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            _list.Add(Create(reader.GetString(nameof(Title))));
                        }
                    }
                }
                return _list;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// IT Team Member Object
    /// </summary>
    public class ITTeamMember : INotifyPropertyChanged
    {
        #region Properties

        public string Name { get; set; }
        private DateTime assignDate;
        public DateTime AssignDate
        {
            get { return assignDate; }
            set { if (value == DateTime.MinValue && Assigned) { value = DateTime.Today; } assignDate = value; OnPropertyChanged(nameof(AssignDate)); }
        }
        private bool assigned;
        public bool Assigned
        {
            get { return assigned; }
            set { assigned = value; OnPropertyChanged(nameof(Assigned)); }
        }

        #endregion

        /// <summary>
        /// Get a list of all IT Team Members
        /// </summary>
        /// <param name="addAll">Add a team member named 'All'</param>
        /// <returns>List of ITTeamMember</ITTeamMember></returns>
        public async static Task<List<ITTeamMember>> GetListAsync(bool addAll)
        {
            var _tempList = new List<ITTeamMember>();
            if (addAll)
            {
                _tempList.Add(new ITTeamMember { Name = "All", AssignDate = DateTime.MinValue });
            }
            using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`users` WHERE `ITTeam`=1", App.ConAsync))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        _tempList.Add(new ITTeamMember { Name = reader.GetString("FullName"), AssignDate = DateTime.MinValue, Assigned = false });
                    }
                }
            }
            return _tempList;
        }

        /// <summary>
        /// Get a BindingList of all IT Team Members
        /// </summary>
        /// <param name="addAll">Add a team member named 'All'</param>
        /// <returns>BindingList of ITTeamMember</ITTeamMember></returns>
        public async static Task<BindingList<ITTeamMember>> GetBindingListAsync(bool addAll)
        {
            var _tempList = new BindingList<ITTeamMember>();
            if (addAll)
            {
                _tempList.Add(new ITTeamMember { Name = "All", AssignDate = DateTime.MinValue });
            }
            using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`users` WHERE `ITTeam`=1", App.ConAsync))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        _tempList.Add(new ITTeamMember { Name = reader.GetString("FullName"), AssignDate = DateTime.MinValue, Assigned = false });
                    }
                }
            }
            return _tempList;
        }

        /// <summary>
        /// IT Ticket Assignment DataBase Transaction
        /// </summary>
        /// <param name="itteam">IT Team Member as ITTeamMember object</param>
        /// <param name="ticketID">Ticket ID Number</param>
        /// <param name="transactionType">Type of transaction as a boolean.  true = Add, false = Remove</param>
        /// <returns>Success of Transaction as boolean.  true = accepted, false = denied or failed</returns>
        public async static Task<bool> AssignmentTransactionAsync(ITTeamMember itteam, int ticketID, bool transactionType)
        {
            try
            {
                var _tempQuery = transactionType
                    ? $"INSERT INTO `{App.Schema}`.`it_ticket_assignment` (`TicketNumber`, `TeamMember`, `AssignmentDate`) VALUES (@p1, @p2, @p3)"
                    : $"DELETE FROM `{App.Schema}`.`it_ticket_assignment` WHERE `TeamMember`=@p1 AND `TicketNumber`=@p2";
                using (MySqlCommand cmd = new MySqlCommand(_tempQuery, App.ConAsync))
                {
                    if (transactionType)
                    {
                        cmd.Parameters.AddWithValue("p1", ticketID);
                        cmd.Parameters.AddWithValue("p2", itteam.Name);
                        cmd.Parameters.AddWithValue("p3", itteam.AssignDate);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("p1", itteam.Name);
                        cmd.Parameters.AddWithValue("p2", ticketID);
                    }
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region INotifyPropertyChanged Implementation

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

    }

    /// <summary>
    /// IT Document Object
    /// </summary>
    public class ITDocument : INotifyPropertyChanged
    {
        #region Properties

        public string FileName { get; set; }
        public string FilePath { get; set; }
        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        #endregion

        /// <summary>
        /// Get a BindingList of all IT Attached Documents
        /// </summary>
        /// <returns>BindingList of ITDocument</returns>
        public async static Task<BindingList<ITDocument>> GetBindingListAsync()
        {
            var _tempList = new BindingList<ITDocument>();
            using (MySqlCommand cmd = new MySqlCommand($"SELECT `FilePath` FROM `{App.Schema}`.`it_ticket_documents` WHERE `TicketNumber`=1", App.ConAsync))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        _tempList.Add(new ITDocument { FileName = reader.GetString(nameof(FilePath)), FilePath = $"{Properties.Settings.Default.HDTDocumentLocation}{reader.GetString(nameof(FilePath))}", IsSelected = true });
                    }
                }
            }
            return _tempList;
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                try
                {
                    handler(this, e);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// IT Project Object
    /// </summary>
    public class ITProject
    {
        #region Properties

        public int ID { get; set; }
        public string Title { get; set; }
        public DateTime PromiseDate { get; set; }
        public DateTime DDD { get; set; }

        #endregion

        /// <summary>
        /// Load current IT projects
        /// </summary>
        /// <returns>active IT projects as List<string></returns>
        public async static Task<List<ITProject>> GetListAsync()
        {
            try
            {
                var _list = new List<ITProject>();
                using (MySqlCommand cmd = new MySqlCommand($"SELECT DISTINCT(p.`TicketNumber`), p.`Title` FROM `{App.Schema}`.`it_ticket_master` AS m, `{App.Schema}`.`it_projects` as p WHERE m.`Status`<>'Closed' AND m.`Status`<>'Denied'", App.ConAsync))
                {
                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            _list.Add(new ITProject { ID = reader.GetInt32("TicketNumber"), Title = reader.GetString(nameof(Title)) } );
                        }
                    }
                }
                return _list;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// ITTicket Object Extensions
    /// </summary>
    public static class ITTIcketExtensions
    {
        /// <summary>
        /// Submit ITTicket Object to the database
        /// </summary>
        /// <param name="ticket">ITTicket Object</param>
        /// <returns>Transaction Success as bool.  true = accepted, false = failed</returns>
        public async static Task<bool> SubmitAsync(this ITTicket ticket)
        {
            ticket.Priority = new Priority { Description = "--Unassigned--", Level = 6 };
            try
            {
                var Command = $"INSERT INTO `{App.Schema}`.`it_ticket_master`";
                const string Columns = "(SubmitDate, Submitter, Location, Subject, Type, RequestCompletionDate, RequestCompletionReason, Description, IAR, Status, Priority, Confidential, DateCompleted, POC)";
                const string Values = "Values(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14)";

                using (MySqlCommand cmd = new MySqlCommand(Command + Columns + Values, App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", ticket.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("p2", ticket.Submitter);
                    cmd.Parameters.AddWithValue("p3", ticket.Location);
                    cmd.Parameters.AddWithValue("p4", ticket.Subject.Title);
                    cmd.Parameters.AddWithValue("p5", ticket.Type.ToString());
                    cmd.Parameters.AddWithValue("p6", ticket.RequestDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("p7", ticket.RequestReason);
                    cmd.Parameters.AddWithValue("p8", ticket.Description);
                    cmd.Parameters.AddWithValue("p9", ticket.IAR);
                    cmd.Parameters.AddWithValue("p10", ticket.Status.Title);
                    cmd.Parameters.AddWithValue("p11", ticket.Priority.Description);
                    cmd.Parameters.AddWithValue("p12", ticket.Confidential);
                    cmd.Parameters.AddWithValue("p13", ticket.CompletionDate);
                    cmd.Parameters.AddWithValue("p14", ticket.POC);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    ticket.IDNumber = Convert.ToInt32(cmd.LastInsertedId);
                    if (ticket.DocumentList.Count > 0)
                    {
                        await ticket.AddAttachmentAsync(true).ConfigureAwait(false);
                    }
                }
                if (ticket.Type == TicketType.Project)
                {
                    using (MySqlCommand cmd = new MySqlCommand($"INSERT INTO `{App.Schema}`.`it_projects` (TicketNumber, PromiseDate, DropDeadDate, Title) Values(@p1, @p2, @p3, @p4)", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", ticket.IDNumber);
                        cmd.Parameters.AddWithValue("p2", ticket.Project.PromiseDate);
                        cmd.Parameters.AddWithValue("p3", ticket.Project.DDD);
                        cmd.Parameters.AddWithValue("p4", ticket.Project.Title);
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Update the IT Ticket in the it_ticket_master DataBase with the ITTIcket Object
        /// </summary>
        /// <param name="ticket">ITTicket Object</param>
        /// <returns>Transaction Success as bool.  true = accepted, false = failed</returns>
        public async static Task<bool> UpdateAsync(this ITTicket ticket)
        {
            try
            {
                if (ticket.Type == TicketType.Project)
                {
                    using (MySqlCommand cmd = new MySqlCommand($"UPDATE `{App.Schema}`.`it_projects` SET `PromiseDate`=@p1, `DropDeadDate`=@p2, `Title`=@p3 WHERE `TicketNumber`=@p4", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", ticket.Project.PromiseDate);
                        cmd.Parameters.AddWithValue("p2", ticket.Project.DDD);
                        cmd.Parameters.AddWithValue("p3", ticket.Project.Title);
                        cmd.Parameters.AddWithValue("p4", ticket.IDNumber);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                using (MySqlCommand cmd = new MySqlCommand($@"UPDATE `{App.Schema}`.`it_ticket_master` SET `Location`=@p1, `Subject`=@p2, `Type`=@p3, `RequestCompletionDate`=@p4, `RequestCompletionReason`=@p5, `Description`=@p6, `IAR`=@p7, `Status`=@p8, `Priority`=@p9, `Confidential`=@p10, `DateCompleted`=@p11, `POC`=@p12 WHERE `TicketNumber`=@p13", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", ticket.Location);
                    cmd.Parameters.AddWithValue("p2", ticket.Subject.Title);
                    cmd.Parameters.AddWithValue("p3", ticket.Type.ToString());
                    cmd.Parameters.AddWithValue("p4", ticket.RequestDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("p5", ticket.RequestReason);
                    cmd.Parameters.AddWithValue("p6", ticket.Description);
                    cmd.Parameters.AddWithValue("p7", ticket.IAR);
                    cmd.Parameters.AddWithValue("p8", ticket.Status.Title);
                    cmd.Parameters.AddWithValue("p9", ticket.Priority.Description);
                    cmd.Parameters.AddWithValue("p10", ticket.Confidential);
                    cmd.Parameters.AddWithValue("p11", ticket.CompletionDate);
                    cmd.Parameters.AddWithValue("p12", ticket.POC);
                    cmd.Parameters.AddWithValue("p13", ticket.IDNumber);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Add Notes to the current IT Ticket
        /// </summary>
        /// <param name="ticket">ITTicket object</param>
        /// <returns>Transaction Success as bool.  true = accepted, false = failed</returns>
        public async static Task<bool> AddNotesAsync(this ITTicket ticket)
        {
            try
            {
                var _note = await OMNIDataBase.AddNoteAsync("it_ticket", ticket.IDNumber).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(_note))
                {
                    if (ticket.NotesTable == null)
                    {
                        ticket.NotesTable = ITTicket.GetNotesDataTableAsync().Result;
                    }
                    ticket.NotesTable.Rows.Add(ticket.IDNumber, DateTime.Now, _note, CurrentUser.FullName);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Remove an attachment from a IT Ticket
        /// </summary>
        /// <param name="ticket">ITTicket object</param>
        /// <param name="documentListIndex">ITTicket object DocumentList Index to remove</param>
        /// <returns>Transaction Success as bool.  true = accepted, false = failed</returns>
        public async static Task<bool> RemoveAttachmentAsync(this ITTicket ticket, int documentListIndex)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($@"DELETE FROM `{App.Schema}`.`it_ticket_documents` WHERE `TicketNumber`=@p1 AND `FilePath`=@p2", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", ticket.IDNumber);
                    cmd.Parameters.AddWithValue("p2", ticket.DocumentList[documentListIndex].FileName);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
                if (File.Exists($"{Properties.Settings.Default.HDTDocumentLocation}{ticket.DocumentList[documentListIndex].FileName}"))
                {
                    File.Delete($"{Properties.Settings.Default.HDTDocumentLocation}{ticket.DocumentList[documentListIndex].FileName}");
                }
                ticket.DocumentList.RemoveAt(documentListIndex);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Add an attachment to an IT Ticket
        /// </summary>
        /// <param name="ticket">ITTicket object</param>
        /// <param name="isSubmit">Is this during form submission</param>
        /// <returns>Transaction Success as bool.  true = accepted, false = failed</returns>
        public async static Task<bool> AddAttachmentAsync(this ITTicket ticket, bool isSubmit)
        {
            try
            {
                if (isSubmit)
                {
                    foreach (var document in ticket.DocumentList)
                    {
                        File.Copy(document.FilePath, $"{Properties.Settings.Default.HDTDocumentLocation}{document.FileName}", true);
                        document.FilePath = $"{Properties.Settings.Default.HDTDocumentLocation}{document.FileName}";
                        using (MySqlCommand cmd = new MySqlCommand($@"INSERT INTO `{App.Schema}`.`it_ticket_documents` (`TicketNumber`, `FilePath`) VALUES (@p1, @p2)", App.ConAsync))
                        {
                            cmd.Parameters.AddWithValue("p1", ticket.IDNumber);
                            cmd.Parameters.AddWithValue("p2", Path.GetFileName(document.FileName));
                            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    var ofd = new OpenFileDialog { Title = "Select Document(s) to attach.", Multiselect = true };
                    ofd.ShowDialog();
                    if (ofd.FileNames.Length > 0 && ticket.IDNumber != null)
                    {
                        foreach (string fileName in ofd.FileNames)
                        {
                            File.Copy(fileName, $"{Properties.Settings.Default.HDTDocumentLocation}{Path.GetFileName(fileName)}", true);
                            ticket.DocumentList.Add(new ITDocument { FileName = Path.GetFileName(fileName), FilePath = $"{Properties.Settings.Default.HDTDocumentLocation}{Path.GetFileName(fileName)}", IsSelected = true });
                            using (MySqlCommand cmd = new MySqlCommand($@"INSERT INTO `{App.Schema}`.`it_ticket_documents` (`TicketNumber`, `FilePath`) VALUES (@p1, @p2)", App.ConAsync))
                            {
                                cmd.Parameters.AddWithValue("p1", ticket.IDNumber);
                                cmd.Parameters.AddWithValue("p2", Path.GetFileName(fileName));
                                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                            }
                        }
                    }
                    else
                    {
                        foreach (string fileName in ofd.FileNames)
                        {
                            ticket.DocumentList.Add(new ITDocument { FileName = Path.GetFileName(fileName), FilePath = fileName, IsSelected = true });
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Add an attachment by dragging and dropping it on the IT Ticket Form UI
        /// </summary>
        /// <param name="ticket">ITTicket object</param>
        /// <param name="fileDrop">File drop string from the IDataObject GetData method</param>
        /// <returns>Transaction Success as bool.  true = accepted, false = failed</returns>
        public async static Task<bool> DragAndDropAttachmentAsync(this ITTicket ticket, object fileDrop)
        {
            var _tempFile = (string[])fileDrop;
            try
            {
                foreach (var _file in _tempFile)
                {
                    if (ticket.IDNumber == 0 || ticket.IDNumber == null)
                    {
                        ticket.DocumentList.Add(new ITDocument { FileName = Path.GetFileName(_file), FilePath = _file, IsSelected = true });
                    }
                    else
                    {
                        File.Copy(_file, $"{Properties.Settings.Default.HDTDocumentLocation}{Path.GetFileName(_file)}", true);
                        ticket.DocumentList.Add(new ITDocument { FileName = Path.GetFileName(_file), FilePath = $"{Properties.Settings.Default.HDTDocumentLocation}{Path.GetFileName(_file)}", IsSelected = true });
                        using (MySqlCommand cmd = new MySqlCommand($@"INSERT INTO `{App.Schema}`.`it_ticket_documents` (`TicketNumber`, `FilePath`) VALUES (@p1, @p2)", App.ConAsync))
                        {
                            cmd.Parameters.AddWithValue("p1", ticket.IDNumber);
                            cmd.Parameters.AddWithValue("p2", Path.GetFileName(_file));
                            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Turn a ticket into a new project
        /// </summary>
        /// <param name="ticket">ITTicket object</param>
        /// <returns>Transaction Success as bool.  true = accepted, false = failed</returns>
        public async static Task<bool> NewProjectAsync(this ITTicket ticket)
        {
            var _tempType = ticket.Type;
            try
            {
                ticket.Type = TicketType.Project;
                ticket.Project = new ITProject
                {
                    PromiseDate = DateTime.Today.AddDays(14),
                    DDD = DateTime.Today.AddDays(30),
                    Title = "New Project"
                };
                if (ticket.IDNumber != 0)
                {
                    using (MySqlCommand cmd = new MySqlCommand($"INSERT INTO `{App.Schema}`.`it_projects` (TicketNumber, PromiseDate, DropDeadDate, Title) VALUES (@p1, @p2, @p3, @p4)", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", ticket.IDNumber);
                        cmd.Parameters.AddWithValue("p2", ticket.Project.PromiseDate);
                        cmd.Parameters.AddWithValue("p3", ticket.Project.DDD);
                        cmd.Parameters.AddWithValue("p4", ticket.Project.Title);
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                    if (!await ticket.UpdateAsync())
                    {
                        ticket.Type = _tempType;
                        return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                ticket.Type = _tempType;
                return false;
            }
        }

        /// <summary>
        /// Revert a project or task back to a ticket
        /// </summary>
        /// <param name="ticket">ITTicket object</param>
        /// <returns>Transaction Success as bool.  true = accepted, false = failed</returns>
        public async static Task<bool> RevertToTicketAsync(this ITTicket ticket)
        {
            var _tempType = ticket.Type;
            try
            {
                var _type = ticket.Type == TicketType.Project ? "`it_projects`" : "`it_project_tasks`";
                var _id = ticket.Type == TicketType.Project ? "`ProjectID`" : "`TaskID`";
                ticket.Type = TicketType.Ticket;
                if (await ticket.UpdateAsync())
                {
                    using (MySqlCommand cmd = new MySqlCommand($"DELETE FROM `{App.Schema}`.{_type} WHERE {_id}=@p1", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", ticket.IDNumber);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    return true;
                }
                else
                {
                    ticket.Type = _tempType;
                    return false;
                }
            }
            catch (Exception)
            {
                ticket.Type = _tempType;
                return false;
            }
        }

        /// <summary>
        /// Turn a ticket into a new task
        /// </summary>
        /// <param name="ticket">ITTicket object</param>
        /// <param name="projectID">Project IDNumber for task assignment</param>
        /// <returns>Transaction Success as bool.  true = accepted, false = failed</returns>
        public async static Task<bool> SaveAsTaskAsync(this ITTicket ticket, int projectID)
        {
            var _tempType = ticket.Type;
            try
            {
                ticket.Type = TicketType.Task;
                var _id = 0;
                using (MySqlCommand cmd = new MySqlCommand($"SELECT `ProjectID` FROM `{App.Schema}`.`it_projects` WHERE `TicketNumber`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", projectID);
                    _id = Convert.ToInt32(await cmd.ExecuteScalarAsync().ConfigureAwait(false));
                }
                using (MySqlCommand cmd = new MySqlCommand($"INSERT INTO `{App.Schema}`.`it_project_tasks` (TicketNumber, ProjectID) VALUES (@p1, @p2)", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", ticket.IDNumber);
                    cmd.Parameters.AddWithValue("p2", _id);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
                if (!await ticket.UpdateAsync())
                {
                    ticket.Type = _tempType;
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                ticket.Type = _tempType;
                return false;
            }
        }
    }
}
