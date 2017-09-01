using MySql.Data.MySqlClient;
using OMNI.Enumerations;
using OMNI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

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
        public async static Task<bool> SubmitAsync(this Slitter slitter)
        {
            try
            {
                var Command = $"INSERT INTO `{App.Schema}`.`calibration_slitter`";
                const string Columns = "(SlitterID, CalibrationDate, SlitterDirection, Submitter, Notes)";
                const string Values = "Values(@p1, @p2, @p3, @p4, @p5)";

                using (MySqlCommand cmd = new MySqlCommand(Command + Columns + Values, App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", slitter.MachineID);
                    cmd.Parameters.AddWithValue("p2", slitter.CalibrationDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("p3", slitter.Direction);
                    cmd.Parameters.AddWithValue("p4", slitter.Submitter);
                    cmd.Parameters.AddWithValue("p5", slitter.Notes);
                    await cmd.ExecuteNonQueryAsync();
                    var slitterCalID = Convert.ToInt32(cmd.LastInsertedId);
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
