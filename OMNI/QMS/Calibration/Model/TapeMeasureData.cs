using OMNI.Extensions;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace OMNI.QMS.Calibration.Model
{
    public class TapeMeasureData
    {
        #region Properties

        public int Workcenter { get; set; }
        public DateTime CalDate { get; set; }
        public DateTime CalDueDate { get; set; }
        public CalStatus Status { get; set; }
        public string Owned { get; set; }
        public string Comments { get; set; }
        public string Calibrator { get; set; }
        public int InstrumentID { get; set; }

        #endregion

        /// <summary>
        /// Tape measure data object constructor
        /// </summary>
        public TapeMeasureData()
        {
            if(CalDate == DateTime.MinValue)
            {
                CalDate = DateTime.Now;
            }
            Status = CalStatus.Pending;
            Calibrator = CurrentUser.FullName;
        }

        /// <summary>
        /// Get the description list for the tape measures
        /// </summary>
        /// <returns>list of description strings</returns>
        public static List<string> GetDescriptionList()
        {
            var _temp = new List<string>();
            if (App.SqlConAsync != null || App.SqlConAsync.State != ConnectionState.Closed || App.SqlConAsync.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.SqlConAsync.Database};
                                                                SELECT
	                                                                [Description]
                                                                FROM
	                                                                [dbo].[TM_Desc];", App.SqlConAsync))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _temp.Add(reader.SafeGetString("Description").Replace("\r\n",""));
                                }
                            }
                        }
                    }
                    return _temp;
                }
                catch (SqlException sqlEx)
                {
                    ExceptionWindow.Show("Database Error", sqlEx.Message);
                    return _temp;
                }
                catch (Exception ex)
                {
                    ExceptionWindow.Show("Unhandled Exception", ex.Message, ex.Source, "Tapemeasure Constructor");
                    return _temp;
                }
            }
            else
            {
                ExceptionWindow.Show("Connection Error", "A connection could not be made to pull accurate data, please contact your administrator");
                return _temp;
            }
        }

        /// <summary>
        /// Add a revision to the database
        /// </summary>
        /// <param name="revision">The tape measure data object to pass to the database</param>
        /// <param name="idNumber">The tape ID number</param>
        /// <returns>Pass\Fail as False\True</returns>
        public static bool AddRevision(TapeMeasureData revision, int idNumber)
        {
            if (App.SqlConAsync != null || App.SqlConAsync.State != ConnectionState.Closed || App.SqlConAsync.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.SqlConAsync.Database};
                                                                INSERT INTO
                                                                [TMD] ([Tape_ID], [Workcenter], [Cal_Date], [Cal_DueDate], [Status], [Owned], [Comments], [Calibrator], [Inst_ID])
                                                                Values(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9);", App.SqlConAsync))
                    {
                        cmd.SafeAddParameters("p1", idNumber);
                        cmd.SafeAddParameters("p2", revision.Workcenter);
                        cmd.SafeAddParameters("p3", revision.CalDate);
                        cmd.SafeAddParameters("p4", revision.CalDate.AddMonths(6));
                        cmd.SafeAddParameters("p5", revision.Status);
                        cmd.SafeAddParameters("p6", revision.Owned);
                        cmd.SafeAddParameters("p7", revision.Comments);
                        cmd.SafeAddParameters("p8", revision.Calibrator);
                        cmd.SafeAddParameters("p9", revision.InstrumentID);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (SqlException)
                {
                    return false;
                }
                catch (Exception )
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
