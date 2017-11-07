using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using OMNI.Models;
using System.ComponentModel;
using OMNI.HDT.Enumeration;

namespace OMNI.HDT.Model
{
    public class Ticket : FormBase, INotifyPropertyChanged
    {
        #region Properties

        public string Location { get; set; }
        public List<string> SubjectList { get; set; }
        public string Subject { get; set; }
        public TicketType Type { get; set; }
        public DateTime RequestDate { get; set; }
        public string RequestReason { get; set; }
        public string Description { get; set; }
        public bool IAR { get; set; }
        public List<string> StatusList { get; set; }
        public string Status { get; set; }
        public List<Priority> PriorityList { get; set; }
        public Priority Priority { get; set; }
        public bool Confidential { get; set; }
        public DateTime CompletionDate { get; set; }
        public BindingList<TeamMember> AssignedTo { get; set; }
        public bool CanAssign { get { return CurrentUser.ITTeam && CompletionDate == DateTime.MinValue; } }
        public bool CanEdit { get { return (CurrentUser.ITTeam || CurrentUser.FullName == Submitter) && CompletionDate == DateTime.MinValue; } }
        public DataTable NotesTable { get; set; }
        public string POC { get; set; }
        public BindingList<ITDocument> DocumentList { get; set; }
        public ITProject Project { get; set; }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
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
        /// Ticket Object constructor
        /// </summary>
        public Ticket()
        {
            StatusList = GetStatusList();
            SubjectList = GetSubjectList();
        }

        /// <summary>
        /// Ticket object constructor
        /// </summary>
        /// <param name="idNumber">Ticket ID Number</param>
        public Ticket(int idNumber)
        {
            StatusList = GetStatusList();
            SubjectList = GetSubjectList();
            try
            {
                var _tempPriority = string.Empty;
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`it_ticket_master` WHERE `TicketNumber`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", idNumber);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            IDNumber = idNumber;
                            Submitter = reader.GetString(nameof(Submitter));
                            Date = reader.GetDateTime("SubmitDate");
                            Location = reader.GetString(nameof(Location));
                            Subject = reader.GetString(nameof(Subject));
                            Type = (TicketType)Enum.Parse(typeof(TicketType), reader.GetString(nameof(Type)));
                            RequestDate = reader.GetDateTime("RequestCompletionDate");
                            RequestReason = reader.IsDBNull(7) ? string.Empty : reader.GetString("RequestCompletionReason");
                            Description = reader.GetString(nameof(Description));
                            IAR = reader.GetBoolean(nameof(IAR));
                            Status = reader.GetString(nameof(Status));
                            _tempPriority = reader.GetString(nameof(Priority));
                            Confidential = reader.GetBoolean(nameof(Confidential));
                            CompletionDate = reader.GetDateTime("DateCompleted");
                            POC = reader.IsDBNull(14) ? string.Empty : reader.GetString(nameof(POC));
                        }
                    }
                }
                Priority = _tempPriority == "--Unassigned--" ? Priority.Create(6, "--Unassigned--") : Priority.Create(_tempPriority);
                AssignedTo = TeamMember.GetBindingListAsync(false).Result;
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Get a list of Ticket status states
        /// </summary>
        /// <returns>List of statuses as List<string></returns>
        public static List<string> GetStatusList()
        {
            try
            {
                var _list = new List<string>();
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`it_ticket_status`", App.ConAsync))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _list.Add(reader.GetString("Title"));
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

        /// <summary>
        /// Get a list of Ticket subjects
        /// </summary>
        /// <returns>List of subjects as List<string></returns>
        public static List<string> GetSubjectList()
        {
            try
            {
                var _list = new List<string>();
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`it_ticket_type`", App.ConAsync))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _list.Add(reader.GetString("Title"));
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

        /// <summary>
        /// Load a DataTable with the currently submited tickets and all open tickets
        /// </summary>
        /// <returns>Ticket Notice Data as DataTable</returns>
        public async static Task<DataTable> LoadNoticeAsync()
        {
            var cmdString = $"SELECT `TicketNumber`, `Submitter`, `SubmitDate`, `Subject`, `Status`, `Type`, `Confidential` FROM `{App.Schema}`.it_ticket_master WHERE `DateCompleted`>'{DateTime.Now.AddDays(-CurrentUser.NoticeHistory).ToString("yyyy-MM-dd HH:mm:ss")}' OR `Status` NOT IN('Denied', 'Closed')";
            if (!CurrentUser.ITTeam)
            {
                cmdString += $" AND (`Confidential`=0 OR (`Confidential`=1 AND `Submitter`='{CurrentUser.FullName}'))";
            }
            using (DataTable dt = new DataTable())
            {
                try
                {
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmdString, App.ConAsync))
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
    }
}
