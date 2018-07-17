using OMNI.Enumerations;
using OMNI.Extensions;
using OMNI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace OMNI.Models
{
    public class CMMSPart : INotifyPropertyChanged
    {
        #region Properties

        public int PartNumber { get; set; }
        public CMMSPartStatus Status { get; set; }
        public IList<CMMSPartStatus> StatusList { get { return Enum.GetValues(typeof(CMMSPartStatus)).Cast<CMMSPartStatus>().ToList(); } }
        public string Creator { get; set; }
        public DateTime DateCreated { get; set; }
        public int OnHand { get; set; }
        private CMMSPartRevision currentRevision;
        public CMMSPartRevision CurrentRevision
        {
            get { return currentRevision; }
            set { currentRevision = value; OnPropertyChanged(nameof(CurrentRevision)); }
        }
        public IList<CMMSPartRevision> RevisionList { get { return PartNumber > 0 ? CMMSPartRevision.GetRevisionListAsync(PartNumber).Result : new List<CMMSPartRevision>(); } }
        public IList<CMMSPartLocation> LocationList  { get { return CMMSPartLocation.GetLocationListAsync().Result; } }
        public bool RecordLockStatus { get; set; }
        public string RecordLockBy { get; set; }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Reflects changes from the ViewModel properties to the View
        /// </summary>
        /// <param name="propertyName">Property Name</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        /// <summary>
        /// CMMS Part Constructor
        /// </summary>
        public CMMSPart()
        {
            Creator = CurrentUser.FullName;
            DateCreated = DateTime.Now;
            RecordLockStatus = false;
            currentRevision = new CMMSPartRevision();
        }

        /// <summary>
        /// Retreive a CMMS Part
        /// </summary>
        /// <param name="partNumber">Part Number to search for</param>
        /// <returns>CMMSPart object</returns>
        public async static Task<CMMSPart> GetCMMSPartAsync(int partNumber)
        {
            try
            {
                var _part = new CMMSPart();
                var _current = string.Empty;
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT * FROM [cmms_parts] WHERE [PartNumber]=@p1", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", partNumber);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            _part.PartNumber = partNumber;
                            _part.Status = (CMMSPartStatus)Enum.Parse(typeof(CMMSPartStatus), reader.SafeGetString(nameof(Status)));
                            _part.Creator = reader.SafeGetString(nameof(Creator));
                            _part.DateCreated = reader.SafeGetDateTime(nameof(DateCreated));
                            _part.OnHand = reader.SafeGetInt32("OnhandQuantity");
                            _current = reader.SafeGetString(nameof(CurrentRevision));
                            _part.RecordLockStatus = reader.SafeGetBoolean("Record_Lock");
                            _part.RecordLockBy = reader.SafeGetString("Record_Lock_By");
                        }
                    }
                }
                _part.CurrentRevision = await CMMSPartRevision.GetCurrentRevisionAsync(_part.PartNumber, _current).ConfigureAwait(false);
                if (!_part.RecordLockStatus)
                {
                    LockRecord(_part.PartNumber);
                }
                return _part;
            }
            catch (Exception)
            {
                return new CMMSPart();
            }
        }

        /// <summary>
        /// Get a records lock status
        /// </summary>
        /// <param name="partNumber">CMMS Part Number</param>
        /// <returns>Record lock status as bool</returns>
        public static bool GetRecordLockStatus(int partNumber)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT [Record_Lock] FROM [cmms_parts] WHERE [PartNumber]=@p1", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", partNumber);
                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
            catch (Exception)
            {
                return true;
            }
        }

        /// <summary>
        /// Lock a CMMS Part record for editing
        /// </summary>
        /// <param name="partNumber">CMMS Part Number</param>
        public static void LockRecord(int partNumber)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        UPDATE [cmms_parts] SET [Record_Lock]=@p1, [Record_Lock_By]=@p2 WHERE [PartNumber]=@p3", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", true);
                    cmd.Parameters.AddWithValue("p2", CurrentUser.FullName);
                    cmd.Parameters.AddWithValue("p3", partNumber);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex, "OMNI.CMMSPart.LockRecordAsync");
            }
        }

        /// <summary>
        /// Unlock a CMMS Part record
        /// </summary>
        /// <param name="partNumber">CMMS Part Number</param>
        /// <param name="admin">Admininstrator Override for unlocking records</param>
        public static void UnlockLockRecord(int partNumber, bool admin)
        {
            try
            {
                var _lockedBy = string.Empty;
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT [Record_Lock_By] FROM [cmms_parts] WHERE [PartNumber]=@p1", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", partNumber);
                    _lockedBy = (cmd.ExecuteScalar()).ToString();
                }
                if (admin || _lockedBy == CurrentUser.FullName)
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                            UPDATE [cmms_parts] SET [Record_Lock]=@p1, [Record_Lock_By]=@p2 WHERE [PartNumber]=@p3", App.SqlConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", false);
                        cmd.Parameters.AddWithValue("p2", null);
                        cmd.Parameters.AddWithValue("p3", partNumber);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex, "OMNI.CMMSPart.UnlockLockRecordAsync");
            }
        }
    }

    public static class CMMSPartExtension
    {
        /// <summary>
        /// Submit CMMSPart Object to the database
        /// </summary>
        /// <param name="cmmsPart">CMMSPart Object</param>
        /// <returns>Transaction Success as bool.  true = accepted, false = failed</returns>
        public static bool Submit(this CMMSPart cmmsPart)
        {
            cmmsPart.CurrentRevision.RevisionID = DateTime.Today.ToString("ddMMMyy" + "-1");
            cmmsPart.RecordLockStatus = false;
            cmmsPart.CurrentRevision.RevisedBy = CurrentUser.FullName;
            cmmsPart.CurrentRevision.RevisionDate = DateTime.Now;
            //TODO: Remove the below hard code
            cmmsPart.CurrentRevision.DefualtLocation = "Test";
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        INSERT INTO [cmms_parts](Status, Creator, DateCreated, OnHandQuantity, CurrentRevision, Record_Lock, Record_Lock_By)
                                                        OUTPUT INSERTED.ID
                                                        Values(@p1, @p2, @p3, @p4, @p5, @p6, @p7)", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", cmmsPart.Status.ToString());
                    cmd.Parameters.AddWithValue("p2", cmmsPart.Creator);
                    cmd.Parameters.AddWithValue("p3", cmmsPart.DateCreated.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("p4", cmmsPart.OnHand);
                    cmd.Parameters.AddWithValue("p5", cmmsPart.CurrentRevision.RevisionID);
                    cmd.Parameters.AddWithValue("p6", cmmsPart.RecordLockStatus);
                    cmd.Parameters.AddWithValue("p7", cmmsPart.RecordLockBy);
                    cmd.ExecuteNonQuery();
                    cmmsPart.PartNumber = Convert.ToInt32(cmd.ExecuteScalar());
                }

                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        INSERT INTO [cmms_parts_revision](revision_id, PartNumber, RevisedBy, Description, SafetyStockQuantity, DefualtLocation)
                                                        OUTPUT INSERTED.ID
                                                        Values(@p1, @p2, @p3, @p4, @p5, @p6)", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", cmmsPart.CurrentRevision.RevisionID);
                    cmd.Parameters.AddWithValue("p2", cmmsPart.PartNumber);
                    cmd.Parameters.AddWithValue("p3", cmmsPart.CurrentRevision.RevisedBy);
                    cmd.Parameters.AddWithValue("p4", cmmsPart.CurrentRevision.Description);
                    cmd.Parameters.AddWithValue("p5", cmmsPart.CurrentRevision.SafetyStock);
                    cmd.Parameters.AddWithValue("p6", cmmsPart.CurrentRevision.DefualtLocation);
                    cmd.ExecuteNonQuery();
                }
                cmmsPart.RevisionList.Add(cmmsPart.CurrentRevision);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Update the CMMSPart object in the database
        /// </summary>
        /// <param name="cmmsPart">CMMSPart Object</param>
        /// <returns>Transaction Success as nullable bool.  true = accepted, false = failed, null = locked</returns>
        public static bool? Update(this CMMSPart cmmsPart)
        {
            if (!cmmsPart.RecordLockStatus)
            {
                try
                {
                    var _revisionIncrement = 0;
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                            SELECT COUNT(*) FROM [cmms_parts_revision] WHERE [revision_id] LIKE '{DateTime.Today.ToString("ddMMMyy")}-%'", App.SqlConAsync))
                    {
                        _revisionIncrement = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    _revisionIncrement++;
                    cmmsPart.CurrentRevision.RevisionID = DateTime.Today.ToString("ddMMMyy" + $"-{_revisionIncrement}");
                    cmmsPart.CurrentRevision.RevisedBy = CurrentUser.FullName;
                    cmmsPart.CurrentRevision.RevisionDate = DateTime.Now;
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                            UPDATE [cmms_parts] SET [Status]=@p1, [CurrentRevision]=@p2, [OnHandQuantity]=@p3 WHERE [PartNumber]=@p4", App.SqlConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", cmmsPart.Status.ToString());
                        cmd.Parameters.AddWithValue("p2", cmmsPart.CurrentRevision.RevisionID);
                        cmd.Parameters.AddWithValue("p3", cmmsPart.OnHand);
                        cmd.Parameters.AddWithValue("p4", cmmsPart.PartNumber);
                        cmd.ExecuteNonQuery();
                    }
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                            INSERT INTO [cmms_parts_revision](revision_id, PartNumber, RevisedBy, Description, SafetyStockQuantity, DefualtLocation)
                                                            Values(@p1, @p2, @p3, @p4, @p5, @p6)", App.SqlConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", cmmsPart.CurrentRevision.RevisionID);
                        cmd.Parameters.AddWithValue("p2", cmmsPart.PartNumber);
                        cmd.Parameters.AddWithValue("p3", cmmsPart.CurrentRevision.RevisedBy);
                        cmd.Parameters.AddWithValue("p4", cmmsPart.CurrentRevision.Description);
                        cmd.Parameters.AddWithValue("p5", cmmsPart.CurrentRevision.SafetyStock);
                        cmd.Parameters.AddWithValue("p6", cmmsPart.CurrentRevision.DefualtLocation);
                        cmd.ExecuteNonQuery();
                    }
                    cmmsPart.RevisionList.Add(cmmsPart.CurrentRevision);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return null;
        }
    }
}
