using OMNI.Models;
using System;

namespace OMNI.QMS.Calibration.Model
{
    public class CalBase
    {
        #region Properties

        public int IDNumber { get; set; }
        public DateTime CalDate { get; set; }
        public string Notes { get; set; }
        public string Submitter { get; set; }
        public bool ValidCal { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CalBase()
        {
            Submitter = CurrentUser.FullName;
            CalDate = DateTime.Now;
            IDNumber = 0;
        }

        /// <summary>
        /// Overridable Submit method
        /// </summary>
        /// <param name="o">CalBase object</param>
        /// <returns>ID Number</returns>
        public virtual int Submit(CalBase o)
        {
            return 0;
        }

        /// <summary>
        /// Overridable Validation of Calibration method
        /// </summary>
        /// <param name="o">CalBase object</param>
        /// <returns>Validation results as bool</returns>
        public virtual bool ValidateCal(CalBase o)
        {
            return false;
        }
    }
}
