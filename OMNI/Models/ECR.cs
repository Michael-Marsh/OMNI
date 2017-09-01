using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMNI.Models
{
    /// <summary>
    /// Engineering Change Request Object Interaction Logic
    /// </summary>
    public class ECR
    {
        #region Properties

        public int? IDNumber { get; set; }
        public string Description { get; set; }
        public string Submitter { get; set; }
        public DateTime SubmitDate { get; set; }
        public string NewPart { get; set; }

        #endregion


    }
}
