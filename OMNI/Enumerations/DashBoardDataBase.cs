using System.ComponentModel;

namespace OMNI.Enumerations
{
    public enum DashBoardDataBase
    {
        [Description("Modify Work Centers")]
        workcenter = 0,
        [Description("Modify NCM Codes")]
        ncm = 1,
        [Description("Modify Suppliers")]
        supplier = 2,
        [Description("OMNI Management")]
        OMNIManagement = 3,
        [Description("Modify GL Accounts")]
        cmms_glaccounts = 4,
        [Description("Run AP")]
        ap = 5,
        [Description("CMMS Metrics")]
        cmms_metrics = 6
    }
}
