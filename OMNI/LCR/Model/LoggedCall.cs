using OMNI.Helpers;
using OMNI.LCR.Enumeration;
using OMNI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace OMNI.LCR.Model
{
    public class LoggedCall
    {
        #region Properties

        public DateTime SubmitDate { get { return DateTime.Today; } }
        public string Submitter { get { return CurrentUser.FullName; } }
        public int? CallID { get; set; }
        public DateTime CallDate { get; set; }
        public string Location { get; set; }
        public string BusinessName { get; set; }
        public BusinessType BusType { get; set; }
        public IndustryType IndType { get; set; }
        public string ProductType { get; set; }
        public string TradeShow { get; set; }
        public List<Contact> ContactList { get; set; }
        public string CallAgenda { get; set; }
        public string CallSummary { get; set; }
        public string FollowUp { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public LoggedCall()
        {
            CallDate = DateTime.Today;
            ContactList = new List<Contact>();
        }
    }

    public static class LCExtensions
    {
        /// <summary>
        /// Submit the logged call object to a SQL database
        /// </summary>
        /// <param name="loggedCall">Current Logged Call Object</param>
        public static void Submit(this LoggedCall loggedCall)
        {
            if (App.SqlConAsync != null || App.SqlConAsync.State != ConnectionState.Closed || App.SqlConAsync.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase}; INSERT INTO
                                                                                    [LCM]([SubmitDate], [Submitter], [CallDate], [Location], [BusinessName], [BusinessType], [IndustryType], [ProductType], [TradeShow], [CallAgenda], [CallSummary], [FollowUp])
                                                                                    Values(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12);
                                                                                SELECT[IDNumber] FROM [LCM] WHERE [IDNumber] = @@IDENTITY;", App.SqlConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", loggedCall.SubmitDate);
                        cmd.Parameters.AddWithValue("p2", loggedCall.Submitter);
                        cmd.Parameters.AddWithValue("p3", loggedCall.CallDate);
                        cmd.Parameters.AddWithValue("p4", loggedCall.Location);
                        cmd.Parameters.AddWithValue("p5", loggedCall.BusinessName);
                        cmd.Parameters.AddWithValue("p6", loggedCall.BusType);
                        cmd.Parameters.AddWithValue("p7", loggedCall.IndType);
                        cmd.Parameters.AddWithValue("p8", loggedCall.ProductType);
                        cmd.Parameters.AddWithValue("p9", loggedCall.TradeShow);
                        cmd.Parameters.AddWithValue("p10", loggedCall.CallAgenda);
                        cmd.Parameters.AddWithValue("p11", loggedCall.CallSummary);
                        cmd.Parameters.AddWithValue("p12", loggedCall.FollowUp);
                        loggedCall.CallID = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    foreach (var c in loggedCall.ContactList)
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase}; INSERT INTO [LCC]([LCMId], [Name], [Title], [Email]) Values(@p1, @p2, @p3, @p4);", App.SqlConAsync))
                        {
                            cmd.Parameters.AddWithValue("p1", loggedCall.CallID);
                            cmd.Parameters.AddWithValue("p2", c.Name);
                            cmd.Parameters.AddWithValue("p3", c.Title);
                            cmd.Parameters.AddWithValue("p4", c.Email);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (SqlException sqlEx)
                {
                    ExceptionWindow.Show("SQL Exception", sqlEx.Message);
                }
                catch (Exception ex)
                {
                    ExceptionWindow.Show("Unhandled Exception", ex.Message);
                }
            }
            else
            {
                ExceptionWindow.Show("Connection Error","A connection could not be made to pull accurate data, please contact your administrator");
            }
        }
    }
}
