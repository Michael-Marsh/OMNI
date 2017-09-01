using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

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
        /// <returns>List of ITTeamMember</ITTeamMember></returns>
        public async static Task<List<TeamMember>> GetListAsync(bool addAll)
        {
            var _tempList = new List<TeamMember>();
            if (addAll)
            {
                _tempList.Add(new TeamMember { Name = "All", AssignDate = DateTime.MinValue });
            }
            using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`users` WHERE `ITTeam`=1", App.ConAsync))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        _tempList.Add(new TeamMember { Name = reader.GetString("FullName"), AssignDate = DateTime.MinValue, Assigned = false });
                    }
                }
            }
            return _tempList;
        }
    }
}
