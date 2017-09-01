using System.ComponentModel;

namespace OMNI.Enumerations
{
    public enum WorkCenterType
    {
        [Description("QMSWorkCenter")]
        QMS = 0,
        [Description("CalWorkCenter")]
        QMSCalibration = 1,
        [Description("HDTWorkCenter")]
        HDT = 2
    }
}
