using MySql.Data.MySqlClient;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace OMNI.QMS.Model
{
    public class QIRRevision : INotifyPropertyChanged
    {
        #region Properties

        public string RevNumber { get; set; }
        public DateTime RevDate { get; set; }
        public string RevSubmitter { get; set; }
        public int Shift { get; set; }
        public string LotNumber { get; set; }
        private string diamondNumber;
        public string DiamondNumber { get { return diamondNumber; } set { diamondNumber = value; OnPropertyChanged(nameof(DiamondNumber)); } }
        public int NCMCode { get; set; }
        private int origin;
        public int Origin { get { return origin; } set { origin = value; OnPropertyChanged(nameof(Origin)); } }
        public int MaterialLost { get; set; }
        public string Cause { get; set; }
        public int SupplierID { get; set; }
        private QIRDisposition dispo;
        public QIRDisposition Disposition { get { return dispo; } set { dispo = value; OnPropertyChanged(nameof(Disposition)); } }
        public string Problem { get; set; }
        public string CauseReason { get; set; }
        public string DispositionReason { get; set; }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
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
        /// QIRRevision Object Constructor
        /// </summary>
        public QIRRevision(bool qirEZ)
        {
            RevSubmitter = CurrentUser.FullName;
            RevDate = DateTime.Now;
            Shift = RevDate.Hour >= 7 && RevDate.Hour < 15 ? 1 : RevDate.Hour >= 15 && RevDate.Hour < 23 ? 2 : 3;
            if (qirEZ)
            {
                Cause = "Method";
                Disposition = new QIRDisposition { Description = "Review at Next Operation", Status = Enumeration.QIRStatus.Open };
                DispositionReason = "Call Quality once reviewed to receive a disposition of materials.";
            }
        }

        /// <summary>
        /// List of QIR Revisions
        /// </summary>
        /// <param name="qirNumber">QIR Number</param>
        /// <param name="dispoList">List of QIR Dispositions</param>
        /// <returns>Generated List of QIR Revisions</returns>
        public static List<QIRRevision> GetQIRRevisionList(int? qirNumber, List<QIRDisposition> dispoList)
        {
            var _qirRevList = new List<QIRRevision>();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`qir_revisions` WHERE `QIRNumber`=@p1 ORDER BY `revision_date` DESC", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", qirNumber);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var test = !reader.IsDBNull(7) ? reader.GetString(nameof(DiamondNumber)) : string.Empty;
                            _qirRevList.Add(new QIRRevision(false)
                            {
                                RevNumber = reader.GetString("revision_id"),
                                RevDate = reader.GetDateTime("revision_date"),
                                RevSubmitter = reader.GetString("revision_submitter"),
                                Shift = reader.GetInt32(nameof(Shift)),
                                LotNumber = !reader.IsDBNull(6) ? reader.GetString(nameof(LotNumber)) : string.Empty,
                                DiamondNumber = !reader.IsDBNull(7) ? reader.GetString(nameof(DiamondNumber)) : string.Empty,
                                NCMCode = reader.GetInt32(nameof(NCMCode)),
                                Origin = reader.GetInt32(nameof(Origin)),
                                MaterialLost = reader.GetInt32(nameof(MaterialLost)),
                                Cause = reader.GetString(nameof(Cause)),
                                SupplierID = !reader.IsDBNull(12) ? reader.GetInt32(nameof(SupplierID)) : 0,
                                Disposition = new QIRDisposition { Description = reader.GetString(nameof(Disposition)), Status = dispoList[dispoList.FindIndex(o => o.Description == reader.GetString(nameof(Disposition)))].Status },
                                Problem = !reader.IsDBNull(13) ? reader.GetString(nameof(Problem)) : string.Empty,
                                CauseReason = !reader.IsDBNull(15) ? reader.GetString(nameof(CauseReason)) : string.Empty,
                                DispositionReason = !reader.IsDBNull(16) ? reader.GetString(nameof(DispositionReason)) : string.Empty
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
            }
            return _qirRevList;
        }

        /// <summary>
        /// Get a QIRRevision object from the OMNI database
        /// </summary>
        /// <param name="qirNumber">QIR Number</param>
        /// <param name="revID">QIRRevision ID Number</param>
        /// <param name="dispoList">List of QIR Dispositions</param>
        /// <returns>Populated QIRRevision object</returns>
        public static QIRRevision GetQIRRevision(int? qirNumber, string revID, List<QIRDisposition> dispoList)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`qir_revisions` WHERE `QIRNumber`=@p1 AND `revision_id`=@p2", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", qirNumber);
                    cmd.Parameters.AddWithValue("p2", revID);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return new QIRRevision(false)
                            {
                                RevNumber = revID,
                                RevDate = reader.GetDateTime("revision_date"),
                                RevSubmitter = reader.GetString("revision_submitter"),
                                Shift = reader.GetInt32(nameof(Shift)),
                                LotNumber = !reader.IsDBNull(6) ? reader.GetString(nameof(LotNumber)) : string.Empty,
                                DiamondNumber = !reader.IsDBNull(7) ? reader.GetString(nameof(DiamondNumber)) : string.Empty,
                                NCMCode = reader.GetInt32(nameof(NCMCode)),
                                Origin = reader.GetInt32(nameof(Origin)),
                                MaterialLost = reader.GetInt32(nameof(MaterialLost)),
                                Cause = reader.GetString(nameof(Cause)),
                                SupplierID = reader.IsDBNull(12) ? reader.GetInt32(nameof(SupplierID)) : 0,
                                Disposition = new QIRDisposition { Description = reader.GetString(nameof(Disposition)), Status = dispoList[dispoList.FindIndex(o => o.Description == reader.GetString(nameof(Disposition)))].Status },
                                Problem = !reader.IsDBNull(14) ? reader.GetString(nameof(Problem)) : string.Empty,
                                CauseReason = !reader.IsDBNull(15) ? reader.GetString(nameof(CauseReason)) : string.Empty,
                                DispositionReason = !reader.IsDBNull(16) ? reader.GetString(nameof(DispositionReason)) : string.Empty
                            };
                        }
                    }
                }
                return new QIRRevision(false);
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                return new QIRRevision(false);
            }
        }
    }

    public static class QIRRevisionExtension
    {
        /// <summary>
        /// Submit a QIR Revision to the OMNI database
        /// </summary>
        /// <param name="rev">Current QIRRevision Object</param>
        /// <param name="idNumber">QIR Object IDNumber</param>
        public static void Submit(this QIRRevision rev, int idNumber)
        {
            try
            {
                var _exists = false;
                var _increment = 1;
                rev.RevSubmitter = CurrentUser.FullName;
                while (!_exists)
                {
                    using (MySqlCommand cmd = new MySqlCommand($"SELECT EXISTS(SELECT * FROM `{App.Schema}`.`qir_revisions` WHERE `revision_id`=@p1 AND `QIRNumber`=@p2)", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", rev.RevDate.ToString($"yyyyMMMdd-{_increment}"));
                        cmd.Parameters.AddWithValue("p2", idNumber);
                        if (Convert.ToBoolean(cmd.ExecuteScalar()))
                        {
                            _increment++;
                        }
                        else
                        {
                            _exists = true;
                        }
                    }
                }
                using (MySqlCommand cmd = new MySqlCommand($@"INSERT INTO `{App.Schema}`.`qir_revisions`
                                                               (`revision_id`, `revision_date`, `revision_submitter`, `QIRNumber`, `Shift`, `LotNumber`, `DiamondNumber`, `NCMCode`, `Origin`, `MaterialLost`, `Cause`, `SupplierID`, `Disposition`, `Problem`, `CauseReason`, `DispositionReason`)
                                                               Values(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16)", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", rev.RevDate.ToString($"yyyyMMMdd-{_increment}"));
                    cmd.Parameters.AddWithValue("p2", rev.RevDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("p3", rev.RevSubmitter);
                    cmd.Parameters.AddWithValue("p4", idNumber);
                    cmd.Parameters.AddWithValue("p5", rev.Shift);
                    cmd.Parameters.AddWithValue("p6", rev.LotNumber);
                    cmd.Parameters.AddWithValue("p7", rev.DiamondNumber);
                    cmd.Parameters.AddWithValue("p8", rev.NCMCode);
                    cmd.Parameters.AddWithValue("p9", rev.Origin);
                    cmd.Parameters.AddWithValue("p10", rev.MaterialLost);
                    cmd.Parameters.AddWithValue("p11", rev.Cause);
                    cmd.Parameters.AddWithValue("p12", rev.SupplierID);
                    cmd.Parameters.AddWithValue("p13", rev.Disposition.Description);
                    cmd.Parameters.AddWithValue("p14", rev.Problem);
                    cmd.Parameters.AddWithValue("p15", rev.CauseReason);
                    cmd.Parameters.AddWithValue("p16", rev.DispositionReason);
                    cmd.ExecuteNonQuery();
                }
                rev.RevNumber = rev.RevDate.ToString($"yyyyMMMdd-{_increment}");
            }
            catch (Exception e)
            {
                ExceptionWindow.Show("Unhandled Exception", e.Message, e);
            }
        }
    }
}
