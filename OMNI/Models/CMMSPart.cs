using MySql.Data.MySqlClient;
using OMNI.Enumerations;
using OMNI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`cmms_parts` WHERE `PartNumber`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", partNumber);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            _part.PartNumber = partNumber;
                            _part.Status = (CMMSPartStatus)Enum.Parse(typeof(CMMSPartStatus), reader.GetString(nameof(Status)));
                            _part.Creator = reader.GetString(nameof(Creator));
                            _part.DateCreated = reader.GetDateTime(nameof(DateCreated));
                            _part.OnHand = reader.GetInt32("OnhandQuantity");
                            _current = reader.GetString(nameof(CurrentRevision));
                            _part.RecordLockStatus = reader.GetBoolean("Record_Lock");
                            if (!await reader.IsDBNullAsync(7).ConfigureAwait(false))
                            {
                                _part.RecordLockBy = reader.GetString("Record_Lock_By");
                            }
                        }
                    }
                }
                _part.CurrentRevision = await CMMSPartRevision.GetCurrentRevisionAsync(_part.PartNumber, _current).ConfigureAwait(false);
                if (!_part.RecordLockStatus)
                {
                    LockRecordAsync(_part.PartNumber);
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
        public async static Task<bool> GetRecordLockStatusAsync(int partNumber)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT `Record_Lock` FROM `{App.Schema}`.`cmms_parts` WHERE `PartNumber`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", partNumber);
                    return Convert.ToBoolean(await cmd.ExecuteScalarAsync().ConfigureAwait(false));
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
        public async static void LockRecordAsync(int partNumber)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"UPDATE `{App.Schema}`.`cmms_parts` SET `Record_Lock`=@p1, `Record_Lock_By`=@p2 WHERE `PartNumber`=@p3", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", true);
                    cmd.Parameters.AddWithValue("p2", CurrentUser.FullName);
                    cmd.Parameters.AddWithValue("p3", partNumber);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
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
        public async static void UnlockLockRecordAsync(int partNumber, bool admin)
        {
            try
            {
                var _lockedBy = string.Empty;
                using (MySqlCommand cmd = new MySqlCommand($"SELECT `Record_Lock_By` FROM `{App.Schema}`.`cmms_parts` WHERE `PartNumber`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", partNumber);
                    _lockedBy = (await cmd.ExecuteScalarAsync().ConfigureAwait(false)).ToString();
                }
                if (admin || _lockedBy == CurrentUser.FullName)
                {
                    using (MySqlCommand cmd = new MySqlCommand($"UPDATE `{App.Schema}`.`cmms_parts` SET `Record_Lock`=@p1, `Record_Lock_By`=@p2 WHERE `PartNumber`=@p3", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", false);
                        cmd.Parameters.AddWithValue("p2", null);
                        cmd.Parameters.AddWithValue("p3", partNumber);
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
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
        public async static Task<bool> SubmitAsync(this CMMSPart cmmsPart)
        {
            cmmsPart.CurrentRevision.RevisionID = DateTime.Today.ToString("ddMMMyy" + "-1");
            cmmsPart.RecordLockStatus = false;
            cmmsPart.CurrentRevision.RevisedBy = CurrentUser.FullName;
            cmmsPart.CurrentRevision.RevisionDate = DateTime.Now;
            //TODO: Remove the below hard code
            cmmsPart.CurrentRevision.DefualtLocation = "Test";
            try
            {
                var Command = $"INSERT INTO `{App.Schema}`.`cmms_parts`";
                var Columns = "(Status, Creator, DateCreated, OnHandQuantity, CurrentRevision, Record_Lock, Record_Lock_By)";
                var Values = "Values(@p1, @p2, @p3, @p4, @p5, @p6, @p7)";
                using (MySqlCommand cmd = new MySqlCommand(Command + Columns + Values, App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", cmmsPart.Status.ToString());
                    cmd.Parameters.AddWithValue("p2", cmmsPart.Creator);
                    cmd.Parameters.AddWithValue("p3", cmmsPart.DateCreated.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("p4", cmmsPart.OnHand);
                    cmd.Parameters.AddWithValue("p5", cmmsPart.CurrentRevision.RevisionID);
                    cmd.Parameters.AddWithValue("p6", cmmsPart.RecordLockStatus);
                    cmd.Parameters.AddWithValue("p7", cmmsPart.RecordLockBy);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    cmmsPart.PartNumber = Convert.ToInt32(cmd.LastInsertedId);
                }

                Command = $"INSERT INTO `{App.Schema}`.`cmms_parts_revision`";
                Columns = "(revision_id, PartNumber, RevisedBy, Description, SafetyStockQuantity, DefualtLocation)";
                Values = "Values(@p1, @p2, @p3, @p4, @p5, @p6)";
                using (MySqlCommand cmd = new MySqlCommand(Command + Columns + Values, App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", cmmsPart.CurrentRevision.RevisionID);
                    cmd.Parameters.AddWithValue("p2", cmmsPart.PartNumber);
                    cmd.Parameters.AddWithValue("p3", cmmsPart.CurrentRevision.RevisedBy);
                    cmd.Parameters.AddWithValue("p4", cmmsPart.CurrentRevision.Description);
                    cmd.Parameters.AddWithValue("p5", cmmsPart.CurrentRevision.SafetyStock);
                    cmd.Parameters.AddWithValue("p6", cmmsPart.CurrentRevision.DefualtLocation);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
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
        public async static Task<bool?> UpdateAsync(this CMMSPart cmmsPart)
        {
            if (!cmmsPart.RecordLockStatus)
            {
                try
                {
                    var _revisionIncrement = 0;
                    using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(*) FROM `{App.Schema}`.`cmms_parts_revision` WHERE `revision_id` LIKE '{DateTime.Today.ToString("ddMMMyy")}-%'", App.ConAsync))
                    {
                        _revisionIncrement = Convert.ToInt32(await cmd.ExecuteScalarAsync().ConfigureAwait(false));
                    }
                    _revisionIncrement++;
                    cmmsPart.CurrentRevision.RevisionID = DateTime.Today.ToString("ddMMMyy" + $"-{_revisionIncrement}");
                    cmmsPart.CurrentRevision.RevisedBy = CurrentUser.FullName;
                    cmmsPart.CurrentRevision.RevisionDate = DateTime.Now;
                    using (MySqlCommand cmd = new MySqlCommand($"UPDATE `{App.Schema}`.`cmms_parts` SET `Status`=@p1, `CurrentRevision`=@p2, `OnHandQuantity`=@p3 WHERE `PartNumber`=@p4", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", cmmsPart.Status.ToString());
                        cmd.Parameters.AddWithValue("p2", cmmsPart.CurrentRevision.RevisionID);
                        cmd.Parameters.AddWithValue("p3", cmmsPart.OnHand);
                        cmd.Parameters.AddWithValue("p4", cmmsPart.PartNumber);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    var Command = $"INSERT INTO `{App.Schema}`.`cmms_parts_revision`";
                    const string Columns = "(revision_id, PartNumber, RevisedBy, Description, SafetyStockQuantity, DefualtLocation)";
                    const string Values = "Values(@p1, @p2, @p3, @p4, @p5, @p6)";
                    using (MySqlCommand cmd = new MySqlCommand(Command + Columns + Values, App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", cmmsPart.CurrentRevision.RevisionID);
                        cmd.Parameters.AddWithValue("p2", cmmsPart.PartNumber);
                        cmd.Parameters.AddWithValue("p3", cmmsPart.CurrentRevision.RevisedBy);
                        cmd.Parameters.AddWithValue("p4", cmmsPart.CurrentRevision.Description);
                        cmd.Parameters.AddWithValue("p5", cmmsPart.CurrentRevision.SafetyStock);
                        cmd.Parameters.AddWithValue("p6", cmmsPart.CurrentRevision.DefualtLocation);
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
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
