using OMNI.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace OMNI.Models
{
    public class CMMSPartRevision
    {
        #region Properties

        public string RevisionID { get; set; }
        public string RevisedBy { get; set; }
        public DateTime RevisionDate { get; set; }
        public string Description { get; set; }
        public int SafetyStock { get; set; }
        public string DefualtLocation { get; set; }

        #endregion

        /// <summary>
        /// Set the currently selected revision ID
        /// </summary>
        /// <param name="partNumber">CMMS Part number</param>
        /// <param name="currentRevision">Optional: CMMS Part current revision</param>
        /// <returns>Loaded CMMSPartRevision Object</returns>
        public async static Task<CMMSPartRevision> GetCurrentRevisionAsync(int partNumber, string currentRevision = null)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase}; SELECT * FROM [cmms_parts_revision] WHERE [PartNumber]=@p1 AND [revision_id]=@p2", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", partNumber);
                    cmd.Parameters.AddWithValue("p2", currentRevision);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            return new CMMSPartRevision
                            {
                                RevisionID = currentRevision,
                                RevisedBy = reader.SafeGetString(nameof(RevisedBy)),
                                RevisionDate = reader.SafeGetDateTime(nameof(RevisionDate)),
                                Description = reader.SafeGetString(nameof(Description)),
                                SafetyStock = reader.SafeGetInt32("SafetyStockQuantity"),
                                DefualtLocation = reader.SafeGetString(nameof(DefualtLocation))
                            };
                        }
                    }
                    return new CMMSPartRevision();
                }
            }
            catch (Exception)
            {
                return new CMMSPartRevision();
            }
        }

        /// <summary>
        /// Get a list of revisions made to a CMMS Part
        /// </summary>
        /// <param name="partNumber">CMMS part number</param>
        /// <returns>List of the specified part number revision's as BindingList<CMMSPartRevision></returns>
        public static async Task<IList<CMMSPartRevision>> GetRevisionListAsync(int partNumber)
        {
            var _revisionList = new List<CMMSPartRevision>();
            try
            {
                using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; SELECT * FROM [cmms_parts_revision] WHERE [PartNumber]=@p1 ORDER BY [RevisionDate] DESC", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", partNumber);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            _revisionList.Add(new CMMSPartRevision
                            {
                                RevisionID = reader.SafeGetString("revision_id"),
                                RevisedBy = reader.SafeGetString(nameof(RevisedBy)),
                                RevisionDate = reader.SafeGetDateTime(nameof(RevisionDate)),
                                Description = reader.SafeGetString(nameof(Description)),
                                SafetyStock = reader.SafeGetInt32("SafetyStockQuantity"),
                                DefualtLocation = reader.SafeGetString(nameof(DefualtLocation))
                            });
                        }
                    }
                }
                return _revisionList;
            }
            catch (Exception)
            {
                return new List<CMMSPartRevision>();
            }
        }
    }
}
