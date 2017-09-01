using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace OMNI.Models
{
    /// <summary>
    /// Priority Object Interaction Logic
    /// </summary>
    public class Priority
    {
        #region Properties

        public int Level { get; set; }
        public string Description { get; set; }

        #endregion

        /// <summary>
        /// Priority Object Creation
        /// </summary>
        /// <param name="level">Priority Level</param>
        /// <param name="description">Priority Description</param>
        /// <returns>New Priority Object</returns>
        public static Priority Create(int level, string description)
        {
            return new Priority { Level = level, Description = description };
        }

        /// <summary>
        /// Priority Object Creation
        /// </summary>
        /// <param name="description">Priority Description</param>
        /// <returns>New Priority Object</returns>
        public static Priority Create(string description)
        {
            using (MySqlCommand cmd = new MySqlCommand($"SELECT `PriorityLevel` FROM `{App.Schema}`.`priority` WHERE `Priority`='{description}'", App.ConAsync))
            {
                return new Priority { Level = Convert.ToInt32(cmd.ExecuteScalar()), Description = description };
            }
        }

        /// <summary>
        /// List of priority descriptions and levels
        /// </summary>
        /// <param name="unassigned">Add --Unassigned-- to the list</param>
        /// <returns>Priority Object List</returns>
        public async static Task<List<Priority>> GetListAsync(bool unassigned)
        {
            var _priority = new List<Priority>();
            using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`priority`", App.ConAsync))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        if (unassigned && reader.GetInt32("PriorityLevel") != 6)
                        {
                            _priority.Add(Create(reader.GetInt32("PriorityLevel"), reader.GetString(nameof(Priority))));
                        }
                    }
                }
            }
            return _priority;
        }

    }
}
