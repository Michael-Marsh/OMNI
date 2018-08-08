using OMNI.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace OMNI.QMS.Calibration.Model
{
    public class CounterData
    {
        #region Properties

        public int Interval { get; set; }
        public double? FDat { get; set; }
        public double? RDat { get; set; }
        public double IncMin { get; set; }
        public double IncMax { get; set; }

        #endregion

        /// <summary>
        /// Counter Data Default Constructor 
        /// </summary>
        public CounterData()
        {

        }

        /// <summary>
        /// Get a list of Counter data objects
        /// </summary>
        /// <param name="workCtrNbr">Work center number to look for</param>
        /// <returns>List of counter data objects</returns>
        public static List<CounterData> GetCounterDataList(int workCtrNbr)
        {
            var _temp = new List<CounterData>();
            var _interval = 0;
            var _max = 0;
            var _tol = 0.0;
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT
	                                                        a.[Cal_Interval], a.[Cal_Max], a.[Cal_Tolerance]
                                                        FROM
	                                                        dbo.[workcenter] a
                                                        WHERE
	                                                        a.[WorkCenterNumber] = @p1 AND a.[CalWorkCenter] = 1;", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", workCtrNbr);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                _interval = reader.SafeGetInt32("Cal_Interval");
                                _max = reader.SafeGetInt32("Cal_Max");
                                _tol = reader.SafeGetDouble("Cal_Tolerance");
                            }
                        }
                    }
                }
                var _count = _interval;
                while (_count <= _max)
                {
                    _temp.Add(new CounterData { Interval = _count, IncMin = _count - (_count * (_tol / 100)), IncMax = _count + (_count * (_tol / 100)) });
                    _count += _interval;
                }
                return _temp;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get a list of Counter data from a previous CalCheck
        /// </summary>
        /// <param name="calID">CalCheck ID</param>
        /// <returns>Populated list of cal data</returns>
        public static List<CounterData> GetCounterDataList(int workCtrNbr, int calID)
        {
            var _temp = new List<CounterData>();
            var _tol = GetMachineTolerance(workCtrNbr);
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT
	                                                        a.[Interval], a.[Measurement] AS 'FDat', b.Measurement AS 'RDat'
                                                        FROM
	                                                        dbo.[CCD] a
                                                        RIGHT JOIN
	                                                        dbo.[CCD] b ON b.[Cal_ID] = a.[Cal_ID] AND a.[Interval] = b.[Interval]
                                                        WHERE
	                                                        a.[Cal_ID] = @p1 AND a.[Direction] = 'F' AND b.[Direction] = 'R'", App.SqlConAsync))
                {
                    cmd.SafeAddParameters("p1", calID);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                _temp.Add(new CounterData {
                                    Interval = reader.SafeGetInt32("Interval"),
                                    RDat = reader.SafeGetDouble("RDat"),
                                    FDat = reader.SafeGetDouble("FDat"),
                                    IncMin = reader.SafeGetInt32("Interval") - (reader.SafeGetInt32("Interval") * (_tol / 100)),
                                    IncMax = reader.SafeGetInt32("Interval") + (reader.SafeGetInt32("Interval") * (_tol / 100))
                                });
                            }
                        }
                    }
                }
                return _temp;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Validates a list of counter FDat and RDat
        /// </summary>
        /// <returns>true = valid / false = invalid</returns>
        public static bool ValidateCounterData(List<CounterData> data)
        {
            foreach(var d in data)
            {
                if (d.FDat == null || d.FDat >= d.IncMax || d.FDat <= d.IncMin)
                { return false; }
                if (d.RDat == null || d.RDat >= d.IncMax || d.RDat <= d.IncMin)
                { return false; }
            }
            return true;
        }

        /// <summary>
        /// Get a tolernace from a work center
        /// </summary>
        /// <param name="wcNbr">Work Center number</param>
        /// <returns>measurement tolerance</returns>
        public static double GetMachineTolerance(int wcNbr)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT [Cal_Tolerance] FROM dbo.[WorkCenter] WHERE [WorkCenterNumber] = @p1", App.SqlConAsync))
                {
                    cmd.SafeAddParameters("p1", wcNbr);
                    return Convert.ToDouble(cmd.ExecuteScalar());
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
