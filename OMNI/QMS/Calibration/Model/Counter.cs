using OMNI.Extensions;
using OMNI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace OMNI.QMS.Calibration.Model
{
    public class Counter : CalBase
    {
        #region Properties

        public List<WorkCenter> MachineList { get; set; }
        public int Machine { get; set; }
        public List<CounterData> CalData { get; set; }
        public double CalCheckNbr { get; set; }

        #endregion

        /// <summary>
        /// Counter Constructor
        /// </summary>
        public Counter()
        {
            Submitter = CurrentUser.FullName;
            CalDate = DateTime.Now;
            IDNumber = null;
            ValidCal = true;
            if (MachineList == null)
            {
                MachineList = WorkCenter.GetListAsync(Enumerations.WorkCenterType.QMSCal).Result;
            }
            if (CalData == null)
            {
                CalData = new List<CounterData>();
            }
        }

        /// <summary>
        /// Counter Constructor
        /// </summary>
        /// <param name="id">ID Number to load</param>
        public Counter(int id)
        {
            IDNumber = id;
            if (MachineList == null)
            {
                MachineList = WorkCenter.GetListAsync(Enumerations.WorkCenterType.QMSCal).Result;
            }
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT * FROM dbo.[CCM] WHERE [Cal_ID] = @p1;", App.SqlConAsync))
                {
                    cmd.SafeAddParameters("p1", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Submitter = reader.SafeGetString("Submitter");
                                CalDate = reader.SafeGetDateTime("SubmitDate");
                                Machine = reader.SafeGetInt32("Machine");
                                Notes = reader.SafeGetString("Notes");
                                ValidCal = reader.SafeGetBoolean("Valid");
                                CalCheckNbr = reader.SafeGetDouble("Cal_Nbr");
                            }
                        }
                    }
                }
                CalData = CounterData.GetCounterDataList(Machine, id); 
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// Calculate the CalCheck number from the inputed cal data
        /// </summary>
        /// <param name="cntr">CalCheck Number</param>
        public static double GetCalNbr(Counter cntr)
        {
            return Convert.ToDouble((cntr.CalData.Sum(f => f.FDat) + cntr.CalData.Sum(r => r.RDat) - 1200.00) / 10.00 - 200.00);
        }

        #region Overriden Methods

        public override int? Submit(object o)
        {
            var _cntr = (Counter)o;
            try
            {
                _cntr.ValidCal = CounterData.ValidateCounterData(_cntr.CalData);
                _cntr.CalCheckNbr = GetCalNbr(_cntr);
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        INSERT INTO
                                                            dbo.[CCM] ([Submitter], [SubmitDate], [Machine], [Notes], [Valid], [Cal_Nbr])
                                                            VALUES(@p1, @p2, @p3, @p4, @p5, @p6);
                                                        SELECT [Cal_ID] FROM dbo.[CCM] WHERE [Cal_ID] = @@IDENTITY;", App.SqlConAsync))
                {
                    cmd.SafeAddParameters("p1", _cntr.Submitter);
                    cmd.SafeAddParameters("p2", _cntr.CalDate);
                    cmd.SafeAddParameters("p3", _cntr.Machine);
                    cmd.SafeAddParameters("p4", _cntr.Notes);
                    cmd.SafeAddParameters("p5", _cntr.ValidCal);
                    cmd.SafeAddParameters("p6", _cntr.CalCheckNbr);
                    _cntr.IDNumber = Convert.ToInt32(cmd.ExecuteScalar());
                }
                foreach(var c in CalData)
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                            BEGIN TRANSACTION
                                                            INSERT INTO
                                                                dbo.[CCD] ([Cal_ID], [Direction], [Interval], [Measurement])
                                                                VALUES(@p1, @p2, @p3, @p4);
                                                            INSERT INTO
                                                                dbo.[CCD] ([Cal_ID], [Direction], [Interval], [Measurement])
                                                                VALUES(@p1, @p5, @p3, @p6);
                                                            COMMIT", App.SqlConAsync))
                    {
                        cmd.SafeAddParameters("p1", _cntr.IDNumber);
                        cmd.SafeAddParameters("p2", "F");
                        cmd.SafeAddParameters("p3", c.Interval);
                        cmd.SafeAddParameters("p4", c.FDat);
                        cmd.SafeAddParameters("p5", "R");
                        cmd.SafeAddParameters("p6", c.RDat);
                        cmd.ExecuteNonQuery();
                    }
                }
                return _cntr.IDNumber;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override bool ValidateCal(object o)
        {
            var _data = (Counter)o;
            foreach(var c in _data.CalData)
            {
                if (c.FDat == null || c.RDat == null)
                { return false; }
            }
            return true;
        }

        #endregion
    }
}
