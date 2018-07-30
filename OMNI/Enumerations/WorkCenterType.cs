using System.ComponentModel;

namespace OMNI.Enumerations
{
    public enum WorkCenterType
    {
        [Description("QMSWorkCenter")]
        QMS = 0,
        [Description("CalWorkCenter")]
        QMSCal = 1,
        [Description("HDTWorkCenter")]
        HDT = 2
    }
}
