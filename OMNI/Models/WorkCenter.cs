using OMNI.Enumerations;
using OMNI.Extensions;
using OMNI.Helpers;
using OMNI.QMS.Enumeration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        public WorkCenter()
        { }

        public WorkCenter(int idNbr)
        {
            IDNumber = idNbr;
            using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; SELECT [WorkCenterName] FROM [workcenter] WHERE [WorkCenterNumber] = @p1", App.SqlConAsync))
            {
                cmd.Parameters.AddWithValue("p1", idNbr);
                Name = cmd.ExecuteScalar().ToString();
            }
        }

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
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase}; SELECT [WorkCenterNumber], [WorkCenterName] FROM [workcenter] WHERE [{listType.GetDescription()}]=1", App.SqlConAsync))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                _workCenterList.Add(new WorkCenter { IDNumber = reader.SafeGetInt32("WorkCenterNumber"), Name = reader.SafeGetString("WorkCenterName") });
                            }
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
        public static string GetEZType(int? workCenterNumber)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase}; SELECT [WorkCenterType] FROM [workcenter] WHERE [WorkCenterNumber]=@p1", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", workCenterNumber);
                    return cmd.ExecuteScalar().ToString();
                    //Enum.TryParse((cmd.ExecuteScalar()).ToString(), out NCMType _type);
                    //return _type;
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                return "None";
            }
        }
    }
}