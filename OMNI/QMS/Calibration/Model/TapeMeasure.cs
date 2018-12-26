using OMNI.Extensions;
using OMNI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace OMNI.QMS.Calibration.Model
{
    public class TapeMeasure : CalBase
    {
        #region Properties

        public string Description { get; set; }
        public IList<TapeMeasureData> TapeDataList { get; set; }
        public TapeMeasureData CurrentRevision { get; set; }

        #endregion

        /// <summary>
        /// Tape measure object default constructor
        /// </summary>
        public TapeMeasure()
        {
            if (TapeDataList == null)
            {
                TapeDataList = new List<TapeMeasureData>();
            }
            if (CurrentRevision == null)
            {
                CurrentRevision = new TapeMeasureData();
            }
        }

        /// <summary>
        /// Tape measure object contructor overloaded to accept the ID to load
        /// </summary>
        /// <param name="id"></param>
        public TapeMeasure(int id)
        {
            if (App.SqlConAsync != null || App.SqlConAsync.State != ConnectionState.Closed || App.SqlConAsync.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.SqlConAsync.Database};
                                                                SELECT
	                                                                a.[Tape_ID],
	                                                                a.[Description],
	                                                                b.[Workcenter],
	                                                                b.[Cal_Date],
	                                                                b.[Cal_DueDate],
	                                                                b.[Status],
	                                                                b.[Owned],
	                                                                b.[Comments],
	                                                                b.[Calibrator],
	                                                                b.[Inst_ID]
                                                                FROM
	                                                                [dbo].[TMM] a
                                                                RIGHT JOIN
	                                                                [dbo].[TMD] b ON b.[Tape_ID] = a.[Tape_ID]
                                                                WHERE a.[Tape_ID] = @p1;", App.SqlConAsync))
                    {
                        cmd.SafeAddParameters("p1", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    IDNumber = id;
                                    Description = string.IsNullOrEmpty(Description) ? reader.SafeGetString("Description") : Description;
                                    if (TapeDataList == null)
                                    {
                                        TapeDataList = new List<TapeMeasureData>();
                                    }
                                    TapeDataList.Add(
                                        new TapeMeasureData
                                        {
                                            Workcenter = reader.SafeGetInt32("Workcenter"),
                                            CalDate = reader.SafeGetDateTime("Cal_Date"),
                                            CalDueDate = reader.SafeGetDateTime("Cal_DueDate"),
                                            Status = (CalStatus)Enum.Parse(typeof(CalStatus), reader.SafeGetString("Status")),
                                            Owned = reader.SafeGetString("Owned"),
                                            Comments = reader.SafeGetString("Comments"),
                                            Calibrator = reader.SafeGetString("Calibrator"),
                                            InstrumentID = reader.SafeGetInt32("Inst_ID")
                                        });
                                }
                                CurrentRevision = TapeDataList[0];
                            }
                            else
                            {
                                IDNumber = null;
                                Description = string.Empty;
                                TapeDataList = new List<TapeMeasureData>();
                                CurrentRevision = new TapeMeasureData();
                            }
                        }
                    }
                }
                catch (SqlException sqlEx)
                {
                    ExceptionWindow.Show("Database Error", sqlEx.Message);
                }
                catch (Exception ex)
                {
                    ExceptionWindow.Show("Unhandled Exception", ex.Message, ex.Source, "Tapemeasure Constructor");
                }
            }
            else
            {
                ExceptionWindow.Show("Connection Error", "A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Overridden Submit method from the base object
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public override int? Submit(object o)
        {
            var _id = 0;
            var _tape = (TapeMeasure)o;
            if (App.SqlConAsync != null || App.SqlConAsync.State != ConnectionState.Closed || App.SqlConAsync.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.SqlConAsync.Database};
                                                                INSERT INTO
                                                                    [TMM] ([Description])
                                                                    Values(@p1);
                                                                SELECT [Tape_ID] FROM [TMM] WHERE [Tape_ID] = @@IDENTITY;", App.SqlConAsync))
                    {
                        cmd.SafeAddParameters("p1", _tape.Description);
                        _id = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    if (!TapeMeasureData.AddRevision(_tape.CurrentRevision, _id))
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {App.SqlConAsync.Database};
                                                                DELETE FROM [TMM] WHERE [Tape_ID] = @p1", App.SqlConAsync))
                        {
                            cmd.SafeAddParameters("p1", _id);
                            cmd.ExecuteNonQuery();
                        }
                        return null;
                    }
                    return _id;
                }
                catch (SqlException sqlEx)
                {
                    ExceptionWindow.Show("Database Error", sqlEx.Message);
                    return null;
                }
                catch (Exception ex)
                {
                    ExceptionWindow.Show("Unhandled Exception", ex.Message, ex.Source, "Tapemeasure Submit");
                    return null;
                }
            }
            else
            {
                ExceptionWindow.Show("Connection Error", "A connection could not be made to pull accurate data, please contact your administrator");
                return null;
            }
        }

        /// <summary>
        /// Update the tape measure object in the database
        /// This will create a new revision by default, inserting it into the database and adding it to the list
        /// </summary>
        /// <param name="tape">Tapemeasure object to update</param>
        public void Update(TapeMeasure tape)
        {
            if (App.SqlConAsync != null || App.SqlConAsync.State != ConnectionState.Closed || App.SqlConAsync.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.SqlConAsync.Database};
                                                                UPDATE [TMM] SET [Description] = @p1 WHERE [Tape_ID] = @p2;", App.SqlConAsync))
                    {
                        cmd.SafeAddParameters("p1", tape.Description);
                        cmd.SafeAddParameters("p2", tape.IDNumber);
                        cmd.ExecuteNonQuery();
                    }
                    TapeMeasureData.AddRevision(tape.CurrentRevision, Convert.ToInt32(tape.IDNumber));
                    tape.TapeDataList.Add(tape.CurrentRevision);
                }
                catch (SqlException sqlEx)
                {
                    ExceptionWindow.Show("Database Error", sqlEx.Message);
                }
                catch (Exception ex)
                {
                    ExceptionWindow.Show("Unhandled Exception", ex.Message, ex.Source, "Tapemeasure Submit");
                }
            }
            else
            {
                ExceptionWindow.Show("Connection Error", "A connection could not be made to pull accurate data, please contact your administrator");
            }
        }
    }
}