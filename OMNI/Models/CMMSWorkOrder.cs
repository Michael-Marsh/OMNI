using iTextSharp.text.pdf;
using Microsoft.Win32;
using OMNI.Enumerations;
using OMNI.Extensions;
using OMNI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace OMNI.Models
{
    public class CMMSWorkOrder : FormBase
    {
        #region Properties

        public CMMSStatus Status { get; set; }
        public string Priority { get; set; }
        public string Workcenter { get; set; }
        public string Description { get; set; }
        public bool Safety { get; set; }
        public bool Quality { get; set; }
        public bool Production { get; set; }
        public bool MachineDown { get; set; }
        public string CrewAssigned { get; set; }
        public DateTime RequestDate { get; set; }
        public string RequestedDateReason { get; set; }
        public DateTime DateAssigned { get; set; }
        public DateTime DateComplete { get; set; }
        public bool AttachedNotes { get; set; }
        public bool LockOut { get; set; }
        public bool Rush { get; set; }
        public bool ProcessChange { get; set; }

        #endregion

        public CMMSWorkOrder()
        {
            FormModule = Module.CMMS;
        }

        /// <summary>
        /// Load a CMMS Work Order
        /// </summary>
        /// <param name="cmmsWorkOrderNumber">Work Order Number to load.</param>
        /// <returns>A loaded CMMSWorkOrder Object</returns>
        public async static Task<CMMSWorkOrder> LoadAsync(int? cmmsWorkOrderNumber)
        {
            try
            {
                var _wo = new CMMSWorkOrder();
                using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; SELECT COUNT(*) FROM [cmmsworkorder] WHERE [WorkOrderNumber]=@p1", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", cmmsWorkOrderNumber);
                    var i = Convert.ToInt32(cmd.ExecuteScalar());
                    if (i == 0)
                    {
                        ExceptionWindow.Show("Invalid Work Order Number", $"{cmmsWorkOrderNumber} is invalid.\nPlease double check your entry and try again.");
                        return null;
                    }
                }
                using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; SELECT * FROM [cmmsworkorder] WHERE [WorkOrderNumber]=@p1", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", cmmsWorkOrderNumber);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            _wo.IDNumber = cmmsWorkOrderNumber;
                            _wo.Status = (CMMSStatus)Enum.Parse(typeof(CMMSStatus), reader.SafeGetString(nameof(Status)));
                            _wo.Priority = reader.SafeGetString(nameof(Priority));
                            _wo.Date = reader.SafeGetDateTime(nameof(Date));
                            _wo.Submitter = reader.SafeGetString(nameof(Submitter));
                            _wo.Workcenter = reader.SafeGetString(nameof(Workcenter));
                            _wo.Description = reader.SafeGetString(nameof(Description));
                            _wo.Safety = reader.SafeGetBoolean(nameof(Safety));
                            _wo.Quality = reader.SafeGetBoolean(nameof(Quality));
                            _wo.Production = reader.SafeGetBoolean(nameof(Production));
                            _wo.CrewAssigned = reader.SafeGetString("CrewMembersAssigned");
                            _wo.RequestDate = reader.SafeGetDateTime("RequestedByDate");
                            _wo.RequestedDateReason = reader.SafeGetString("RequestDateReason");
                            _wo.DateAssigned = reader.SafeGetDateTime(nameof(DateAssigned));
                            _wo.DateComplete = reader.SafeGetDateTime("DateCompleted");
                            _wo.MachineDown = reader.SafeGetBoolean(nameof(MachineDown));
                            _wo.AttachedNotes = reader.SafeGetBoolean(nameof(AttachedNotes));
                            _wo.LockOut = reader.SafeGetBoolean("Lockout");
                            _wo.Rush = reader.SafeGetBoolean("Rush");
                            _wo.ProcessChange = reader.SafeGetBoolean("ProcessChange");
                        }
                    }
                }
                return _wo;
            }
            catch (Exception e)
            {
                ExceptionWindow.Show("Unhandled Exception", e.Message, e);
                return null;
            }
        }

        /// <summary>
        /// Load the Selected Notice module into a DataTable
        /// </summary>
        /// <param name="module">Module to load</param>
        /// <param name="crewMemberFullName">Crew member full name or All for a general filter</param>
        /// <param name="site">Current Work Site</param>
        /// <returns>Notice Module DataTable</returns>
        public static DataTable LoadNotice(int module, string crewMemberFullName, string site)
        {
            var _tempTable = new DataTable();
            if (crewMemberFullName == null)
            {
                crewMemberFullName = CurrentUser.FullName;
            }
            using (var cmd = new SqlCommand($"{App.DataBase}.[dbo].[cmms_notice_load]", App.SqlConAsync) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@fullName", crewMemberFullName);
                cmd.Parameters.AddWithValue("@module", module);
                cmd.Parameters.AddWithValue("@site", site);
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(_tempTable);
                    return _tempTable;
                }
            }
        }

        /// <summary>
        /// Load the Work Order notes into a DataTable
        /// </summary>
        /// <param name="workOrderId">Work Order Number to select from the notes database</param>
        /// <returns>Notes DataTable</returns>
        public static DataTable LoadNotes(int workOrderId)
        {
            var _tempTable = new DataTable();
            using (var cmd = new SqlCommand($"{App.DataBase}.[dbo].cmms_notes_load", App.SqlConAsync) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@workOrderID", workOrderId);
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(_tempTable);
                    return _tempTable;
                }
            }
        }

        /// <summary>
        /// Get YTD Work Order metrics
        /// </summary>
        /// <param name="mType">Type of metric to calculate</param>
        /// <returns>Count value as int based on metric type</returns>
        public static int GetMetrics(MetricType mType)
        {
            try
            {
                var cmdText = string.Empty;
                switch (mType)
                {
                    case MetricType.Completed:
                        cmdText = $"USE {App.DataBase}; SELECT COUNT(*) FROM [cmmsworkorder] WHERE ([DateCompleted] BETWEEN '{DateTime.Now.Year}-01-01' AND '{DateTime.Now.AddYears(1).Year}-01-01') AND [Site]='{CurrentUser.Site}'";
                        break;
                    case MetricType.Submission:
                        cmdText = $"USE {App.DataBase}; SELECT COUNT(*) FROM [cmmsworkorder] WHERE ([Date] BETWEEN '{DateTime.Now.Year}-01-01' AND '{DateTime.Now.AddYears(1).Year}-01-01') AND [Site]='{CurrentUser.Site}'";
                        break;
                    case MetricType.ResponseTime:
                        cmdText = $"USE {App.DataBase}; SELECT AVG([ResponseTime]) FROM [cmmsworkorder] WHERE ([DateCompleted] BETWEEN '{DateTime.Now.Year}-01-01' AND '{DateTime.Now.AddYears(1).Year}-01-01') AND [Site]='{CurrentUser.Site}'";
                        break;
                }
                using (SqlCommand cmd = new SqlCommand(cmdText, App.SqlConAsync))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static DataTable GetUserMetrics()
        {
            var _tempTable = new DataTable();
            using (var cmd = new SqlCommand($@"USE {App.DataBase};
                                                SELECT
	                                                [WorkOrderNumber] as 'WO Nbr',
	                                                [Date] as 'Submitted',
	                                                [Submitter],
	                                                [DateAssigned] as 'Assigned',
	                                                [DateCompleted] as 'Completed',
                                                    [CrewMembersAssigned] as 'ActiveCrew'
                                                FROM
	                                                [dbo].[cmmsworkorder]
                                                WHERE
	                                                [Status] = 'Completed' AND [Site] = @p1;", App.SqlConAsync))
            {
                cmd.SafeAddParameters("p1", CurrentUser.Site);
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(_tempTable);
                    return _tempTable;
                }
            }
        }

        /// <summary>
        /// Get MTD Work Order metrics
        /// </summary>
        /// <param name="mType">Type of metric to calculate</param>
        /// <param name="month">Month of metric</param>
        /// <param name="year">Year of metric</param>
        /// <returns>Count value as int based on metric type</returns>
        public static int GetMetrics(MetricType mType, int month, int year)
        {
            try
            {
                var cmdText = string.Empty;
                var nextYear = month == 12 ? year + 1 : year;
                var nextMonth = month == 12 ? 1 : month + 1;
                switch (mType)
                {
                    case MetricType.Completed:
                        cmdText = $"USE {App.DataBase}; SELECT COUNT(*) FROM [cmmsworkorder] WHERE [Status] IN ('Completed', 'Denied') AND ([Date] BETWEEN '{year}-{month}-01' AND '{nextYear}-{nextMonth}-01') AND [Site]='{CurrentUser.Site}'";
                        break;
                    case MetricType.Submission:
                        cmdText = $"USE {App.DataBase}; SELECT COUNT(*) FROM [cmmsworkorder] WHERE ([Date] BETWEEN '{year}-{month}-01' AND '{nextYear}-{nextMonth}-01') AND [Site]='{CurrentUser.Site}'";
                        break;
                    case MetricType.ResponseTime:
                        cmdText = $"USE {App.DataBase}; SELECT AVG([ResponseTime]) FROM [cmmsworkorder] WHERE ([DateCompleted] BETWEEN '{year}-{month}-01' AND '{nextYear}-{nextMonth}-01') AND ([Date] BETWEEN '{year}-{month}-01' AND '{nextYear}-{nextMonth}-01') AND [Site]='{CurrentUser.Site}'";
                        break;
                }
                using (SqlCommand cmd = new SqlCommand(cmdText, App.SqlConAsync))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }

    public class CMMSGLAccount
    {
        #region Properties

        public string GLAccount { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }

        #endregion

        /// <summary>
        /// CMMS GL Account Object Creation
        /// </summary>
        /// <param name="glNumber">GL account number</param>
        /// <param name="description">GL Description</param>
        /// <param name="category">GL Category</param>
        /// <returns>CMMS GL account Object</returns>
        public static CMMSGLAccount Create(string glNumber, string description, string category) => new CMMSGLAccount { GLAccount = glNumber, Description = description, Category = category };

        /// <summary>
        /// List of CMMS GL Accounts
        /// </summary>
        /// <returns>Generated List of QIR Causes</returns>
        public async static Task<List<CMMSGLAccount>> CMMSGLAccountListAsync()
        {
            var _glList = new List<CMMSGLAccount>();

            try
            {
                using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; SELECT * FROM [cmms_glaccounts] WHERE [Site]='{CurrentUser.Site}'", App.SqlConAsync))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            _glList.Add(Create(reader.SafeGetString(nameof(GLAccount)), reader.SafeGetString(nameof(Description)), reader.SafeGetString(nameof(Category))));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
            }

            return _glList;
        }

        /// <summary>
        /// Find a GL Account number by a work center name
        /// </summary>
        /// <param name="workCenter">Name of the work center</param>
        /// <returns>GL Account number</returns>
        public static string FindbyWorkCenter(string workCenter)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; SELECT [GLAccount] FROM [dbo].[cmms_glaccounts] WHERE [Description] = @p1;", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("@p1", workCenter);
                    return (cmd.ExecuteScalar()).ToString();
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                return string.Empty;
            }
        }
    }

    public class AttachedDocuments : INotifyPropertyChanged
    {
        #region Properties

        public string FileName { get; set; }
        public string FilePath { get; set; }
        private bool attached;
        public bool Attached
        {
            get { return attached; }
            set { attached = value; OnPropertyChanged(nameof(Attached)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                try
                {
                    handler(this, e);
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        #endregion

        /// <summary>
        /// Create a list of Attached Documents
        /// </summary>
        /// <param name="workOrderID">Work Order Number</param>
        /// <returns>New list of AttachedDocuments object</returns>
        public async static Task<List<AttachedDocuments>> CreateDocListAsync(int workOrderID)
        {
            var _docList = new List<AttachedDocuments>();
            try
            {
                using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; SELECT * FROM [cmms_work_order_documents] WHERE [WorkOrderNumber]=@p1", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", workOrderID);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            _docList.Add(new AttachedDocuments { FileName = reader.SafeGetString(nameof(FilePath)), FilePath = $"{Properties.Settings.Default.CMMSDocumentLocation}{reader.SafeGetString(nameof(FilePath))}", Attached = true });
                        }
                    }
                }
                return _docList;
            }
            catch (Exception e)
            {
                ExceptionWindow.Show("Unhandled Exception", e.Message, e);
                return null;
            }
        }
    }

    public static class CMMSWorkOrderExtensions
    {
        /// <summary>
        /// Submit a CMMS work order to the OMNI database
        /// </summary>
        /// <param name="wo">Current Work Order</param>
        /// <returns>CMMS Work Order Number</returns>
        public static int? Submit(this CMMSWorkOrder wo)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        INSERT INTO
                                                        [cmmsworkorder]([Status], [Priority], [Date], [Submitter], [WorkCenter], [Description], [Safety], [Quality], [Production], [CrewMembersAssigned], [RequestedByDate],
                                                        [RequestDateReason], [DateAssigned], [DateCompleted], [MachineDown], [PartsUsed], [AttachedNotes], [Lockout], [Site], [Rush], [ProcessChange])
                                                        Values(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16, @p17, @p18, @p19, @p20, @p21);
                                                        SELECT [WorkOrderNumber] FROM [cmmsworkorder] WHERE [WorkOrderNumber] = @@IDENTITY;", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", wo.Status.ToString());
                    cmd.Parameters.AddWithValue("p2", wo.Priority);
                    cmd.Parameters.AddWithValue("p3", wo.Date);
                    cmd.Parameters.AddWithValue("p4", wo.Submitter);
                    cmd.Parameters.AddWithValue("p5", wo.Workcenter);
                    cmd.Parameters.AddWithValue("p6", wo.Description);
                    cmd.Parameters.AddWithValue("p7", wo.Safety);
                    cmd.Parameters.AddWithValue("p8", wo.Quality);
                    cmd.Parameters.AddWithValue("p9", wo.Production);
                    cmd.Parameters.AddWithValue("p10", wo.CrewAssigned);
                    cmd.SafeAddParameters("p11", wo.RequestDate);
                    cmd.SafeAddParameters("p12", wo.RequestedDateReason);
                    cmd.SafeAddParameters("p13", wo.DateAssigned);
                    cmd.SafeAddParameters("p14", wo.DateComplete);
                    cmd.Parameters.AddWithValue("p15", wo.MachineDown);
                    cmd.Parameters.AddWithValue("p16", DBNull.Value);
                    cmd.Parameters.AddWithValue("p17", wo.AttachedNotes);
                    cmd.Parameters.AddWithValue("p18", wo.LockOut);
                    cmd.Parameters.AddWithValue("p19", CurrentUser.Site);
                    cmd.Parameters.AddWithValue("p20", wo.Rush);
                    cmd.Parameters.AddWithValue("p21", wo.ProcessChange);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception e)
            {
                ExceptionWindow.Show("Unhandled Exception", e.Message, e);
                return null;
            }
        }

        /// <summary>
        /// Update a CMMS work order in the OMNI database
        /// </summary>
        /// <param name="wo">Current Work Order</param>
        public static void Update(this CMMSWorkOrder wo)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        UPDATE [cmmsworkorder] SET 
                                                            [Status]=@p1,
                                                            [Priority]=@p2,
                                                            [WorkCenter]=@p3,
                                                            [Description]=@p4,
                                                            [Safety]=@p5,
                                                            [Quality]=@p6,
                                                            [Production]=@p7,
                                                            [CrewMembersAssigned]=@p8,
                                                            [RequestedByDate]=@p9,
                                                            [RequestDateReason]=@p10,
                                                            [DateAssigned]=@p11,
                                                            [DateCompleted]=@p12,
                                                            [MachineDown]=@p13,
                                                            [PartsUsed]=@p14,
                                                            [AttachedNotes]=@p15,
                                                            [Lockout]=@p16,
                                                            [Rush]=@p17,
                                                            [ProcessChange]=@p18 WHERE [WorkOrderNumber]=@p19", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", wo.Status.ToString());
                    cmd.Parameters.AddWithValue("p2", wo.Priority);
                    cmd.Parameters.AddWithValue("p3", wo.Workcenter);
                    cmd.Parameters.AddWithValue("p4", wo.Description);
                    cmd.Parameters.AddWithValue("p5", wo.Safety);
                    cmd.Parameters.AddWithValue("p6", wo.Quality);
                    cmd.Parameters.AddWithValue("p7", wo.Production);
                    cmd.Parameters.AddWithValue("p8", wo.CrewAssigned);
                    cmd.SafeAddParameters("p9", wo.RequestDate);
                    cmd.SafeAddParameters("p10", wo.RequestedDateReason);
                    cmd.SafeAddParameters("p11", wo.DateAssigned);
                    cmd.SafeAddParameters("p12", wo.DateComplete);
                    cmd.Parameters.AddWithValue("p13", wo.MachineDown);
                    cmd.Parameters.AddWithValue("p14", "");
                    cmd.Parameters.AddWithValue("p15", wo.AttachedNotes);
                    cmd.Parameters.AddWithValue("p16", wo.LockOut);
                    cmd.Parameters.AddWithValue("p17", wo.Rush);
                    cmd.Parameters.AddWithValue("p18", wo.ProcessChange);
                    cmd.Parameters.AddWithValue("p19", wo.IDNumber);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                ExceptionWindow.Show("Unhandled Exception", e.Message, e);
            }
        }

        /// <summary>
        /// Submit attached documents directly to the database
        /// </summary>
        /// <param name="wo">Current Work Order</param>
        /// <param name="fileName">FileName</param>
        public static void SubmitAttachDocument(this CMMSWorkOrder wo, string fileName)
        {
            if (wo.IDNumber > 0)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                            INSERT [cmms_work_order_documents] ([WorkOrderNumber], [FilePath]) VALUES(@p1, @p2)", App.SqlConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", wo.IDNumber);
                        cmd.Parameters.AddWithValue("p2", fileName);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    ExceptionWindow.Show("Unhandled Exception", e.Message, e);
                }
            }
        }

        /// <summary>
        /// Delete attached documents directly from the database
        /// </summary>
        /// <param name="wo">Current Work Order</param>
        /// <param name="fileName">FileName</param>
        public static void RemoveDocument(this CMMSWorkOrder wo, string fileName)
        {
            if (wo.IDNumber > 0)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; DELETE FROM [cmms_work_order_documents] WHERE [WorkOrderNumber]=@p1 AND [FilePath]=@p2", App.SqlConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", wo.IDNumber);
                        cmd.Parameters.AddWithValue("p2", fileName);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    ExceptionWindow.Show("Unhandled Exception", e.Message, e);
                }
            }
        }

        /// <summary>
        /// Export to FORM5006 rev E pdf. This will open the created form up right after if not autosaved.
        /// </summary>
        /// <param name="woObject">Current CMMS Work Order Object</param>
        /// <param name="autoSave">optional: Autosave in the OMNI temp file</param>
        public static void ExportToPDF(this CMMSWorkOrder woObject, bool autoSave = false)
        {
            if (woObject != null)
            {
                try
                {
                    using (PdfReader reader = new PdfReader(Properties.Settings.Default.CMMSWorkOrderDocument))
                    {
                        var _fileName = string.Empty;
                        _fileName = $"{Properties.Settings.Default.omnitemp}{woObject.IDNumber}.pdf";
                        using (PdfStamper stamp = new PdfStamper(reader, new FileStream(_fileName, FileMode.Create)))
                        {
                            var pdfField = stamp.AcroFields;
                            pdfField.SetField("RadioButtonList[0]", Convert.ToInt32(woObject.MachineDown).ToString());
                            pdfField.SetField("TextField1[0]", woObject.CrewAssigned);
                            pdfField.SetField("TextField1[1]", woObject.Workcenter);
                            pdfField.SetField("TextField1[2]", $"01-00-{CMMSGLAccount.FindbyWorkCenter(woObject.Workcenter)}");
                            pdfField.SetField("TextField1[3]", woObject.Description);
                            pdfField.SetField("TextField1[4]", woObject.Submitter);
                            pdfField.SetField("CheckBox1[0]", Convert.ToInt32(woObject.Safety).ToString());
                            pdfField.SetField("CheckBox1[1]", Convert.ToInt32(woObject.Quality).ToString());
                            pdfField.SetField("CheckBox1[2]", Convert.ToInt32(woObject.Production).ToString());
                            pdfField.SetField("DateTimeField1[0]", woObject.Date.ToShortDateString());
                            pdfField.SetField("TextField1[5]", woObject.RequestedDateReason);
                            var _rDate = woObject.RequestDate == DateTime.MinValue ? "" : woObject.RequestDate.ToShortDateString();
                            pdfField.SetField("DateTimeField1[1]", _rDate);
                            pdfField.SetField("TextField1[6]", woObject.Status.ToString());
                            var _denied = woObject.Status == CMMSStatus.Denied ? "1" : "0";
                            pdfField.SetField("CheckBox1[3]", _denied);
                            stamp.FormFlattening = false;
                        }
                    }
                    if (!autoSave)
                    {
                        Process.Start($"{Properties.Settings.Default.omnitemp}{woObject.IDNumber}.pdf");
                    }
                }
                catch (Exception ex)
                {
                    ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Attach documents to a CMMS work order
        /// </summary>
        /// <param name="wo">CMMS Work Order object</param>
        /// <returns>Binding List of all the attached documents</returns>
        public static BindingList<AttachedDocuments> AttachDocument(this CMMSWorkOrder wo)
        {
            var _tempList = new BindingList<AttachedDocuments>();
            var ofd = new OpenFileDialog { Title = "Select Document(s) to attach.", Multiselect = true };
            ofd.ShowDialog();
            if (ofd.FileNames.Length > 0)
            {
                foreach (string fileName in ofd.FileNames)
                {
                    if (wo.IDNumber == null)
                    {
                        _tempList.Add(new AttachedDocuments { FileName = Path.GetFileName(fileName), FilePath = fileName, Attached = true });
                    }
                    else
                    {
                        File.Copy(fileName, $"{Properties.Settings.Default.CMMSDocumentLocation}{Path.GetFileName(fileName)}", true);
                        wo.SubmitAttachDocument(Path.GetFileName(fileName));
                        _tempList.Add( new AttachedDocuments { FileName = Path.GetFileName(fileName), FilePath = $"{Properties.Settings.Default.CMMSDocumentLocation}{Path.GetFileName(fileName)}", Attached = true });
                    }
                }
                return _tempList;
            }
            return null;
        }

        /// <summary>
        /// Attach documents to a CMMS Work order by drag and drop
        /// </summary>
        /// <param name="wo">CMMS Work Order Object</param>
        /// <param name="filePath">File path</param>
        public static BindingList<AttachedDocuments> DragAndDropAttach(this CMMSWorkOrder wo, object filePath)
        {
            try
            {
                var _tempList = new BindingList<AttachedDocuments>();
                var _tempFile = (string[])filePath;
                foreach (string _file in _tempFile)
                {
                    if (wo.IDNumber == null)
                    {
                        _tempList.Add(new AttachedDocuments { FileName = Path.GetFileName(_file), FilePath = _file, Attached = true });
                    }
                    else
                    {
                        File.Copy(_file, $"{Properties.Settings.Default.CMMSDocumentLocation}{Path.GetFileName(_file)}", true);
                        wo.SubmitAttachDocument(Path.GetFileName(_file));
                        _tempList.Add(new AttachedDocuments { FileName = Path.GetFileName(_file), FilePath = $"{Properties.Settings.Default.CMMSDocumentLocation}{Path.GetFileName(_file)}", Attached = true });
                    }
                }
                return _tempList;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
