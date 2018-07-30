using OMNI.Models;
using System;
using System.Collections.Generic;

namespace OMNI.QMS.Calibration.Model
{
    public class Counter : CalBase
    {
        #region Properties

        public List<WorkCenter> MachineList { get; set; }
        public int Machine { get; set; }
        public List<double> ReverseCalData { get; set; }
        public List<double> ForwardCalData { get; set; }

        #endregion

        /// <summary>
        /// Counter Constructor
        /// </summary>
        public Counter()
        {
            Submitter = CurrentUser.FullName;
            CalDate = DateTime.Now;
            IDNumber = 0;
            ValidCal = false;
            if (MachineList == null)
            {
                MachineList = WorkCenter.GetListAsync(Enumerations.WorkCenterType.QMSCal).Result;
            }
            Machine = MachineList[0].IDNumber;
            if (ReverseCalData == null)
            {
                ReverseCalData = new List<double>
                {
                    Capacity = 5
                };
            }
            if (ForwardCalData == null)
            {
                ForwardCalData = new List<double>
                {
                    Capacity = 5
                };
            }
        }

        /// <summary>
        /// Counter Constructor
        /// </summary>
        /// <param name="id">ID Number to load</param>
        public Counter(int id)
        {

        }

        #region Overriden Methods

        public override int Submit(CalBase o)
        {
            return base.Submit(o);
        }

        public override bool ValidateCal(CalBase o)
        {
            return base.ValidateCal(o);
        }

        #endregion
    }
}
