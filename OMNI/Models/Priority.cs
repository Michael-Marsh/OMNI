using OMNI.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                    SELECT [PriorityLevel] FROM [priority] WHERE [Priority]=@p1", App.SqlConAsync))
            {
                cmd.Parameters.AddWithValue("p1", description);
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
            using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                    SELECT * FROM [priority]", App.SqlConAsync))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        if (unassigned && reader.SafeGetInt32("PriorityLevel") != 6)
                        {
                            _priority.Add(Create(reader.SafeGetInt32("PriorityLevel"), reader.SafeGetString(nameof(Priority))));
                        }
                    }
                }
            }
            return _priority;
        }

    }
}
