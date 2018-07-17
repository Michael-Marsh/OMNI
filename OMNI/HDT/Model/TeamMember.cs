using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using OMNI.Extensions;

namespace OMNI.HDT.Model
{
    public class TeamMember : INotifyPropertyChanged
    {
        #region Properties

        public string Name { get; set; }
        private DateTime assignDate;
        public DateTime AssignDate
        {
            get { return assignDate; }
            set { if (value == DateTime.MinValue && Assigned) { value = DateTime.Now; } assignDate = value; OnPropertyChanged(nameof(AssignDate)); }
        }
        private bool assigned;
        public bool Assigned
        {
            get { return assigned; }
            set { assigned = value; OnPropertyChanged(nameof(Assigned)); }
        }

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
        /// Get a list of all IT Team Members
        /// </summary>
        /// <param name="addAll">Add a team member named 'All'</param>
        /// <returns>List of Team Members</returns>
        public static List<TeamMember> GetList(bool addAll)
        {
            var _tempList = new List<TeamMember>();
            if (addAll)
            {
                _tempList.Add(new TeamMember { Name = "All", AssignDate = DateTime.MinValue });
            }
            using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                      SELECT [FullName] FROM [users] WHERE [ITTeam] = 1; ", App.SqlConAsync))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _tempList.Add(new TeamMember { Name = reader.SafeGetString("FullName"), AssignDate = DateTime.MinValue, Assigned = false });
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
        public static BindingList<TeamMember> GetBindingList(bool addAll)
        {
            var _tempList = new BindingList<TeamMember>();
            if (addAll)
            {
                _tempList.Add(new TeamMember { Name = "All", AssignDate = DateTime.MinValue });
            }
            using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                      SELECT [FullName] FROM [users] WHERE [ITTeam] = 1; ", App.SqlConAsync))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _tempList.Add(new TeamMember { Name = reader.SafeGetString("FullName"), AssignDate = DateTime.MinValue, Assigned = false });
                    }
                }
            }
            return _tempList;
        }
    }
}
