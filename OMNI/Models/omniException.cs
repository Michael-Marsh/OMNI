using MySql.Data.MySqlClient;
using OMNI.Helpers;
using System;
using System.Data;
using System.Threading.Tasks;

namespace OMNI.Models
{
    /// <summary>
    /// OMNI Exception Tracking Object Interaction Logic
    /// </summary>
    public sealed class OMNIException
    {
        #region Properties

        private static int? count { get; set; }

        #endregion

        /// <summary>
        /// Send an Exception to the log for tracking
        /// </summary>
        /// <param name="source">Exception Source</param>
        /// <param name="stackTrace">Exception StackTrace</param>
        /// <param name="message">Exception Message</param>
        /// <param name="methodName">Method name that threw the exception</param>
        public async static void SendtoLogAsync(string source, string stackTrace, string message, string methodName)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand("INSERT INTO omni.exceptionlog (source, stacktrace, User, Date, Message, MethodName) VALUES (@p1, @p2, @p3, @p4, @p5, @p6)", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", source);
                    cmd.Parameters.AddWithValue("p2", stackTrace);
                    var _addName = !string.IsNullOrWhiteSpace(CurrentUser.FullName) ? CurrentUser.FullName : "N/A";
                    cmd.Parameters.AddWithValue("p3", _addName);
                    cmd.Parameters.AddWithValue("p4", DateTime.Now);
                    cmd.Parameters.AddWithValue("p5", message);
                    cmd.Parameters.AddWithValue("p6", methodName);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// Log an unhandled exception as handled in the OMNI DataBase
        /// </summary>
        /// <param name="exceptionID">Unhandled Exception ID</param>
        public async static void HandleExceptionAsync(int exceptionID)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"`{App.Schema}`.`handle_exception`", App.ConAsync) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@exceptionID", exceptionID);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
            }
        }

        /// <summary>
        /// Retrieve a datatable of unhandled exceptions
        /// </summary>
        /// <returns>DataTable of all unhandled exceptions logged by OMNI users</returns>
        public async static Task<DataTable> UnhandledExceptionsTableAsync()
        {
            try
            {
                using (var _table = new DataTable())
                {
                    using (MySqlCommand cmd = new MySqlCommand($"`{App.Schema}`.`query_unhandled_exceptions`", App.ConAsync) { CommandType = CommandType.StoredProcedure })
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            await adapter.FillAsync(_table);
                            return _table;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ExceptionWindow.Message, ex);
                return null;
            }
        }
    }
}
