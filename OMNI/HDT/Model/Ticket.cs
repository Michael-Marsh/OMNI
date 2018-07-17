using OMNI.Extensions;
using OMNI.HDT.Enumeration;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;

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
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                          SELECT * FROM [it_ticket_master] WHERE [TicketNumber] = @p1;", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", idNumber);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            IDNumber = idNumber;
                            Submitter = reader.SafeGetString(nameof(Submitter));
                            Date = reader.SafeGetDateTime("SubmitDate");
                            Location = reader.SafeGetString(nameof(Location));
                            Subject = reader.SafeGetString(nameof(Subject));
                            Type = (TicketType)Enum.Parse(typeof(TicketType), reader.SafeGetString(nameof(Type)));
                            RequestDate = reader.SafeGetDateTime("RequestCompletionDate");
                            RequestReason = reader.SafeGetString("RequestCompletionReason");
                            Description = reader.SafeGetString(nameof(Description));
                            IAR = reader.SafeGetBoolean(nameof(IAR));
                            Status = reader.SafeGetString(nameof(Status));
                            _tempPriority = reader.SafeGetString(nameof(Priority));
                            Confidential = reader.SafeGetBoolean(nameof(Confidential));
                            CompletionDate = reader.SafeGetDateTime("DateCompleted");
                            POC = reader.SafeGetString(nameof(POC));
                        }
                    }
                }
                Priority = _tempPriority == "--Unassigned--" ? Priority.Create(6, "--Unassigned--") : Priority.Create(_tempPriority);
                AssignedTo = TeamMember.GetBindingList(false);
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex.Source, "OMNI.HDT.Model.Ticket(int idNumber)");
                OMNIException.SendtoLogAsync(ex.Source, ex.StackTrace, ex.Message, "OMNI.HDT.Model.Ticket(int idNumber)");
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
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                          SELECT[Title] FROM[it_ticket_status]; ", App.SqlConAsync))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _list.Add(reader.SafeGetString("Title"));
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
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                          SELECT [Title] FROM [it_ticket_type];", App.SqlConAsync))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _list.Add(reader.SafeGetString("Title"));
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
        public static DataTable LoadNotice()
        {
            var cmdString = $@"USE [OMNI];
                                SELECT
	                                [TicketNumber], [Submitter], [SubmitDate], [Subject], [Status], [Type], [Confidential]
                                FROM
	                                [it_ticket_master]
                                WHERE
	                                [DateCompleted]>'{DateTime.Now.AddDays(-CurrentUser.NoticeHistory).ToString("yyyy-MM-dd HH:mm:ss")}' OR [Status] NOT IN('Denied', 'Closed')";
            if (!CurrentUser.ITTeam)
            {
                cmdString += $" AND ([Confidential]=0 OR ([Confidential]=1 AND [Submitter]='{CurrentUser.FullName}'))";
            }
            using (DataTable dt = new DataTable())
            {
                try
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmdString, App.SqlConAsync))
                    {
                        adapter.Fill(dt);
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
