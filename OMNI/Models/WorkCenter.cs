using MySql.Data.MySqlClient;
using OMNI.Enumerations;
using OMNI.Extensions;
using OMNI.Helpers;
using OMNI.QMS.Enumeration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMNI.Models
{
    /// <summary>
    /// WorkCenter Object Interaction Logic
    /// </summary>
    public class WorkCenter
    {
        #region Properties

        public int IDNumber { get; set; }
        public string Name { get; set; }

        #endregion

        /// <summary>
        /// List of WorkCenter Numbers and Names
        /// </summary>
        /// <param name="listType">Type of WorkCenter list to load as WorkCenterType</param>
        /// <returns>Generated List of WorkCenter Numbers and Names</returns>
        public async static Task<List<WorkCenter>> GetListAsync(WorkCenterType listType)
        {
            var _workCenterList = new List<WorkCenter>();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT `WorkCenterNumber`, `WorkCenterName` FROM `{App.Schema}`.`workcenter` WHERE `{listType.GetDescription()}`=1", App.ConAsync))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            _workCenterList.Add(new WorkCenter { IDNumber = reader.GetInt32("WorkCenterNumber"), Name = reader.GetString("WorkCenterName") });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
            }
            return _workCenterList;
        }

        /// <summary>
        /// Retrieve what type of EZ a work center has been assigned to.
        /// </summary>
        /// <param name="workCenterNumber">Work Center Number</param>
        /// <returns>Work Center EZ Type as NCMType</returns>
        public async static Task<NCMType> GetEZTypeAsync(int? workCenterNumber)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT WorkCenterType FROM `{App.Schema}`.`workcenter` WHERE WorkCenterNumber=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", workCenterNumber);
                    Enum.TryParse((await cmd.ExecuteScalarAsync().ConfigureAwait(false)).ToString(), out NCMType _type);
                    return _type;
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                return 0;
            }
        }
    }
}