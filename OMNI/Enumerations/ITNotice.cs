using System.ComponentModel;

namespace OMNI.Enumerations
{
    public enum ITNotice
    {
        [Description("Pending")]
        Unassigned = 0,
        [Description("Assigned")]
        Open = 1,
        [Description(nameof(Closed))]
        Closed = 2,
        Projects = 3
    }
}
