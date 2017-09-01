﻿using iTextSharp.text.pdf;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using OMNI.Enumerations;
using OMNI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
                using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(*) FROM `{App.Schema}`.`cmmsworkorder` WHERE `WorkOrderNumber`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", cmmsWorkOrderNumber);
                    var i = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    if (i == 0)
                    {
                        ExceptionWindow.Show("Invalid Work Order Number", $"{cmmsWorkOrderNumber} is invalid.\nPlease double check your entry and try again.");
                        return null;
                    }
                }
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`cmmsworkorder` WHERE `WorkOrderNumber`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", cmmsWorkOrderNumber);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            _wo.IDNumber = cmmsWorkOrderNumber;
                            _wo.Status = (CMMSStatus)Enum.Parse(typeof(CMMSStatus), reader.GetString(nameof(Status)));
                            _wo.Priority = reader.GetString(nameof(Priority));
                            _wo.Date = reader.GetDateTime(nameof(Date));
                            _wo.Submitter = reader.GetString(nameof(Submitter));
                            _wo.Workcenter = reader.GetString(nameof(Workcenter));
                            _wo.Description = reader.GetString(nameof(Description));
                            _wo.Safety = reader.GetBoolean(nameof(Safety));
                            _wo.Quality = reader.GetBoolean(nameof(Quality));
                            _wo.Production = reader.GetBoolean(nameof(Production));
                            _wo.CrewAssigned = reader.GetString("CrewMembersAssigned");
                            _wo.RequestDate = reader.GetDateTime("RequestedByDate");
                            if (!reader.IsDBNull(reader.GetOrdinal("RequestDateReason")))
                            {
                                _wo.RequestedDateReason = reader.GetString("RequestDateReason");
                            }
                            _wo.DateAssigned = reader.GetDateTime(nameof(DateAssigned));
                            _wo.DateComplete = reader.GetDateTime("DateCompleted");
                            _wo.MachineDown = reader.GetBoolean(nameof(MachineDown));
                            _wo.AttachedNotes = reader.GetBoolean(nameof(AttachedNotes));
                            _wo.LockOut = reader.GetBoolean("Lockout");
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
        /// <returns>Notice Module DataTable</returns>
        public async static Task<DataTable> LoadNoticeAsync(int module, string crewMemberFullName)
        {
            var _tempTable = new DataTable();
            using (var cmd = new MySqlCommand($"`{App.Schema}`.`cmms_notice_load`", App.ConAsync) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@fullName", crewMemberFullName);
                cmd.Parameters.AddWithValue("@module", module);
                using (var adapter = new MySqlDataAdapter(cmd))
                {
                    await adapter.FillAsync(_tempTable).ConfigureAwait(false);
                    return _tempTable;
                }
            }
        }

        /// <summary>
        /// Load the Work Order notes into a DataTable
        /// </summary>
        /// <param name="workOrderId">Work Order Number to select from the notes database</param>
        /// <returns>Notes DataTable</returns>
        public async static Task<DataTable> LoadNotesAsync(int workOrderId)
        {
            var _tempTable = new DataTable();
            using (var cmd = new MySqlCommand($"`{App.Schema}`.`cmms_notes_load`", App.ConAsync) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@workOrderID", workOrderId);
                using (var adapter = new MySqlDataAdapter(cmd))
                {
                    await adapter.FillAsync(_tempTable).ConfigureAwait(false);
                    return _tempTable;
                }
            }
        }

        /// <summary>
        /// Get YTD Work Order metrics
        /// </summary>
        /// <param name="mType">Type of metric to calculate</param>
        /// <returns>Count value as int based on metric type</returns>
        public async static Task<int> GetMetricsAsync(MetricType mType)
        {
            try
            {
                var cmdText = string.Empty;
                switch (mType)
                {
                    case MetricType.Completed:
                        cmdText = $"SELECT COUNT(*) FROM {App.Schema}.`cmmsworkorder` WHERE (`DateCompleted` BETWEEN '{DateTime.Now.Year}-01-01' AND '{DateTime.Now.AddYears(1).Year}-01-01')";
                        break;
                    case MetricType.Submission:
                        cmdText = $"SELECT COUNT(*) FROM {App.Schema}.`cmmsworkorder` WHERE (`Date` BETWEEN '{DateTime.Now.Year}-01-01' AND '{DateTime.Now.AddYears(1).Year}-01-01')";
                        break;
                    case MetricType.ResponseTime:
                        cmdText = $"SELECT AVG(`ResponseTime`) FROM {App.Schema}.`cmmsworkorder` WHERE (`DateCompleted` BETWEEN '{DateTime.Now.Year}-01-01' AND '{DateTime.Now.AddYears(1).Year}-01-01')";
                        break;
                }
                using (MySqlCommand cmd = new MySqlCommand(cmdText, App.ConAsync))
                {
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Get MTD Work Order metrics
        /// </summary>
        /// <param name="mType">Type of metric to calculate</param>
        /// <param name="month">Month of metric</param>
        /// <param name="year">Year of metric</param>
        /// <returns>Count value as int based on metric type</returns>
        public async static Task<int> GetMetricsAsync(MetricType mType, int month, int year)
        {
            try
            {
                var cmdText = string.Empty;
                var nextYear = month == 12 ? year + 1 : year;
                var nextMonth = month == 12 ? 1 : month + 1;
                switch (mType)
                {
                    case MetricType.Completed:
                        cmdText = $"SELECT COUNT(*) FROM {App.Schema}.`cmmsworkorder` WHERE `Status` IN ('Completed', 'Denied') AND (`Date` BETWEEN '{year}-{month}-01' AND '{nextYear}-{nextMonth}-01')";
                        break;
                    case MetricType.Submission:
                        cmdText = $"SELECT COUNT(*) FROM {App.Schema}.`cmmsworkorder` WHERE (`Date` BETWEEN '{year}-{month}-01' AND '{nextYear}-{nextMonth}-01')";
                        break;
                    case MetricType.ResponseTime:
                        cmdText = $"SELECT AVG(`ResponseTime`) FROM {App.Schema}.`cmmsworkorder` WHERE (`DateCompleted` BETWEEN '{year}-{month}-01' AND '{nextYear}-{nextMonth}-01') AND (`Date` BETWEEN '{year}-{month}-01' AND '{nextYear}-{nextMonth}-01')";
                        break;
                }
                using (MySqlCommand cmd = new MySqlCommand(cmdText, App.ConAsync))
                {
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
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
        public string Group { get; set; }

        #endregion

        /// <summary>
        /// CMMS GL Account Object Creation
        /// </summary>
        /// <param name="glNumber">GL account number</param>
        /// <param name="description">GL Description</param>
        /// <param name="group">GL Group</param>
        /// <returns>CMMS GL account Object</returns>
        public static CMMSGLAccount Create(string glNumber, string description, string group) => new CMMSGLAccount { GLAccount = glNumber, Description = description, Group = group };

        /// <summary>
        /// List of CMMS GL Accounts
        /// </summary>
        /// <returns>Generated List of QIR Causes</returns>
        public async static Task<List<CMMSGLAccount>> CMMSGLAccountListAsync()
        {
            var _glList = new List<CMMSGLAccount>();

            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`cmmsglaccounts`", App.ConAsync))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            _glList.Add(Create(reader.GetString(nameof(GLAccount)), reader.GetString(nameof(Description)), reader.GetString(nameof(Group))));
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
        public async static Task<string> FindbyWorkCenterAsync(string workCenter)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"`{App.Schema}`.`query_gl_account`", App.ConAsync) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@workCenter", workCenter);
                    return (await cmd.ExecuteScalarAsync().ConfigureAwait(false)).ToString();
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
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`cmms_work_order_documents` WHERE `WorkOrderNumber`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", workOrderID);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            _docList.Add(new AttachedDocuments { FileName = reader.GetString(nameof(FilePath)), FilePath = $"{Properties.Settings.Default.CMMSDocumentLocation}{reader.GetString(nameof(FilePath))}", Attached = true });
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
        public async static Task<int?> SubmitAsync(this CMMSWorkOrder wo)
        {
            try
            {
                var Command = $"INSERT INTO `{App.Schema}`.`cmmsworkorder`";
                var Columns = $"(Status, Priority, Date, Submitter, WorkCenter, Description, Safety, Quality, Production, CrewMembersAssigned, RequestedByDate, RequestDateReason, DateAssigned, DateCompleted, MachineDown, PartsUsed, AttachedNotes, Lockout)";
                const string Values = "Values(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16, @p17, @p18)";

                using (MySqlCommand cmd = new MySqlCommand(Command + Columns + Values, App.ConAsync))
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
                    cmd.Parameters.AddWithValue("p11", wo.RequestDate);
                    cmd.Parameters.AddWithValue("p12", wo.RequestedDateReason);
                    cmd.Parameters.AddWithValue("p13", wo.DateAssigned);
                    cmd.Parameters.AddWithValue("p14", wo.DateComplete);
                    cmd.Parameters.AddWithValue("p15", wo.MachineDown);
                    cmd.Parameters.AddWithValue("p16", "");
                    cmd.Parameters.AddWithValue("p17", wo.AttachedNotes);
                    cmd.Parameters.AddWithValue("p18", wo.LockOut);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    return Convert.ToInt32(cmd.LastInsertedId);
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
        public async static void UpdateAsync(this CMMSWorkOrder wo)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($@"UPDATE `{App.Schema}`.`cmmsworkorder` SET `Status`=@p1,
                                                                                                        `Priority`=@p2,
                                                                                                        `WorkCenter`=@p3,
                                                                                                        `Description`=@p4,
                                                                                                        `Safety`=@p5,
                                                                                                        `Quality`=@p6,
                                                                                                        `Production`=@p7,
                                                                                                        `CrewMembersAssigned`=@p8,
                                                                                                        `RequestedByDate`=@p9,
                                                                                                        `RequestDateReason`=@p10,
                                                                                                        `DateAssigned`=@p11,
                                                                                                        `DateCompleted`=@p12,
                                                                                                        `MachineDown`=@p13,
                                                                                                        `PartsUsed`=@p14,
                                                                                                        `AttachedNotes`=@p15,
                                                                                                        `Lockout`=@p16 WHERE `WorkOrderNumber`=@p17", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", wo.Status.ToString());
                    cmd.Parameters.AddWithValue("p2", wo.Priority);
                    cmd.Parameters.AddWithValue("p3", wo.Workcenter);
                    cmd.Parameters.AddWithValue("p4", wo.Description);
                    cmd.Parameters.AddWithValue("p5", wo.Safety);
                    cmd.Parameters.AddWithValue("p6", wo.Quality);
                    cmd.Parameters.AddWithValue("p7", wo.Production);
                    cmd.Parameters.AddWithValue("p8", wo.CrewAssigned);
                    cmd.Parameters.AddWithValue("p9", wo.RequestDate);
                    cmd.Parameters.AddWithValue("p10", wo.RequestedDateReason);
                    cmd.Parameters.AddWithValue("p11", wo.DateAssigned);
                    cmd.Parameters.AddWithValue("p12", wo.DateComplete);
                    cmd.Parameters.AddWithValue("p13", wo.MachineDown);
                    cmd.Parameters.AddWithValue("p14", "");
                    cmd.Parameters.AddWithValue("p15", wo.AttachedNotes);
                    cmd.Parameters.AddWithValue("p16", wo.LockOut);
                    cmd.Parameters.AddWithValue("p17", wo.IDNumber);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
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
        public async static void SubmitAttachDocumentAsync(this CMMSWorkOrder wo, string fileName)
        {
            if (wo.IDNumber > 0)
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand($"INSERT `{App.Schema}`.`cmms_work_order_documents` (WorkOrderNumber, FilePath) VALUES(@p1, @p2)", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", wo.IDNumber);
                        cmd.Parameters.AddWithValue("p2", fileName);
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
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
        public async static void RemoveDocumentAsync(this CMMSWorkOrder wo, string fileName)
        {
            if (wo.IDNumber > 0)
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand($"DELETE FROM `{App.Schema}`.`cmms_work_order_documents` WHERE `WorkOrderNumber`=@p1 AND `FilePath`=@p2", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", wo.IDNumber);
                        cmd.Parameters.AddWithValue("p2", fileName);
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
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
                            pdfField.SetField("TextField1[2]", $"01-00-{CMMSGLAccount.FindbyWorkCenterAsync(woObject.Workcenter).Result}");
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
                        wo.SubmitAttachDocumentAsync(Path.GetFileName(fileName));
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
                        wo.SubmitAttachDocumentAsync(Path.GetFileName(_file));
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
