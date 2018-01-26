using MySql.Data.MySqlClient;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMNI.CMMS.Model
{
    public class WorkOrder : FormBase
    {
        #region Properties



        #endregion

        /// <summary>
        /// WorkOrder Object Constructor
        /// </summary>
        public WorkOrder()
        {

        }

        public override int? Submit(object formObject)
        {
            /*var _wo = (WorkOrder)formObject;
            try
            {
                var Command = $"INSERT INTO `{App.Schema}`.`cmmsworkorder`";
                var Columns = $"(Status, Priority, Date, Submitter, WorkCenter, Description, Safety, Quality, Production, CrewMembersAssigned, RequestedByDate, RequestDateReason, DateAssigned, DateCompleted, MachineDown, PartsUsed, AttachedNotes, Lockout)";
                const string Values = "Values(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16, @p17, @p18)";

                using (MySqlCommand cmd = new MySqlCommand(Command + Columns + Values, App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", _wo.Status.ToString());
                    cmd.Parameters.AddWithValue("p2", _wo.Priority);
                    cmd.Parameters.AddWithValue("p3", _wo.Date);
                    cmd.Parameters.AddWithValue("p4", _wo.Submitter);
                    cmd.Parameters.AddWithValue("p5", _wo.Workcenter);
                    cmd.Parameters.AddWithValue("p6", _wo.Description);
                    cmd.Parameters.AddWithValue("p7", _wo.Safety);
                    cmd.Parameters.AddWithValue("p8", _wo.Quality);
                    cmd.Parameters.AddWithValue("p9", _wo.Production);
                    cmd.Parameters.AddWithValue("p10", _wo.CrewAssigned);
                    cmd.Parameters.AddWithValue("p11", _wo.RequestDate);
                    cmd.Parameters.AddWithValue("p12", _wo.RequestedDateReason);
                    cmd.Parameters.AddWithValue("p13", _wo.DateAssigned);
                    cmd.Parameters.AddWithValue("p14", _wo.DateComplete);
                    cmd.Parameters.AddWithValue("p15", _wo.MachineDown);
                    cmd.Parameters.AddWithValue("p16", "");
                    cmd.Parameters.AddWithValue("p17", _wo.AttachedNotes);
                    cmd.Parameters.AddWithValue("p18", _wo.LockOut);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    return Convert.ToInt32(cmd.LastInsertedId);
                }
            }
            catch (Exception e)
            {
                ExceptionWindow.Show("Unhandled Exception", e.Message, e);
                return null;
            }*/
            return null;
        }
    }
}
