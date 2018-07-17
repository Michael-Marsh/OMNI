using OMNI.Enumerations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace OMNI.Models
{
    public class Calibration
    {
        #region Properties

        public int IDNumber { get; set; }
        public DateTime CalibrationDate { get; set; }
        public string Notes { get; set; }
        public string Submitter { get; set; }

        #endregion
    }

    public class Slitter : Calibration
    {
        #region Properties

        public int MachineID { get; set; }
        public string Direction { get; set; }
        public List<double> Actual { get; set; }
        public List<double> Measured { get; set; }
        public CalType Type { get; set; }
        public List<string> MachineNameList { get; set; }
        public DataTable CalibrationData { get; set; }

        #endregion

        /// <summary>
        /// Slitter Constructor
        /// </summary>
        public Slitter()
        {
            Submitter = CurrentUser.FullName;
            CalibrationDate = DateTime.Today;
            if (MachineNameList == null)
            {
                MachineNameList = new List<string>();
                MachineNameList.Add("Slitter 1");
                MachineNameList.Add("Slitter 2");
            }
            if (CalibrationData == null)
            {
                CalibrationData = new DataTable();
                CalibrationData.Columns.Add("40 FT", typeof(double));
                CalibrationData.Columns.Add("80 FT", typeof(double));
                CalibrationData.Columns.Add("120 FT", typeof(double));
                CalibrationData.Columns.Add("160 FT", typeof(double));
                CalibrationData.Columns.Add("200 FT", typeof(double));
            }
        }
    }

    public class TapeMeasure : Calibration
    {
        #region Properties



        #endregion
    }

    public static class CalibrationExtension
    {
        /// <summary>
        /// Submit a Slitter calcheck or calibration to the database
        /// </summary>
        /// <param name="slitter">Slitter Object</param>
        /// <returns>Transaction Success as bool.  true = accepted / false = rejected</returns>
        public static bool Submit(this Slitter slitter)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        INSERT INTO [calibration_slitter](SlitterID, CalibrationDate, SlitterDirection, Submitter, Notes)
                                                        OUTPUT INSERTED.ID
                                                        Values(@p1, @p2, @p3, @p4, @p5)", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", slitter.MachineID);
                    cmd.Parameters.AddWithValue("p2", slitter.CalibrationDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("p3", slitter.Direction);
                    cmd.Parameters.AddWithValue("p4", slitter.Submitter);
                    cmd.Parameters.AddWithValue("p5", slitter.Notes);
                    cmd.ExecuteNonQuery();
                    var slitterCalID = Convert.ToInt32(cmd.ExecuteScalar());
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
