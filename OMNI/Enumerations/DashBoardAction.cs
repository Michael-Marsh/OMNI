using System.ComponentModel;

namespace OMNI.Enumerations
{
    public enum DashBoardAction
    {
        SubmitQIR = 0,
        UpdateQIR = 1,
        Exception = 2,
        MapForm = 3,
        [Description("Quality Notice")]
        QIRNotice = 4,
        CreateWO = 7,
        UpdateWO = 8,
        [Description("Open Work Orders")]
        CMMSOpen = 9,
        [Description("Assigned Work Orders")]
        CMMSAssigned = 10,
        [Description("Pending Work Orders")]
        CMMSPending = 11,
        [Description("Closed Work Orders")]
        CMMSClosed = 12,
        ViewPart = 13,
        UserAccount = 14,
        ImportrM = 15,
        UpdateInfo = 16,
        UpdateOMNI = 17,
        UserSubmission = 18,
        DevTesting = 19,
        SubmitECR = 20,
        UpdateECR = 21,
        CreateTicket = 22,
        SearchTicket = 23,
        [Description("Pending Tickets")]
        PendingTicket = 24,
        [Description("Open Tickets")]
        OpenTicket = 25,
        [Description("Closed Tickets")]
        ClosedTicket = 26,
        [Description("IT Projects")]
        ITProject = 27,
        [Description("Review Import")]
        ReviewImport = 28,
        [Description("Slitter Calibration")]
        SlitCal = 29,
        [Description("Tape Measure Calibration")]
        TMCal = 30,
        [Description("IT Notice")]
        ITNotice = 31,
        [Description("Instrument Calibration")]
        InstCal = 32,
        [Description("Logged Call Report")]
        LCM = 33
    }
}
