using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMNI.Models
{
    /// <summary>
    /// CMMS Part Location Object
    /// </summary>
    public class CMMSPartLocation
    {
        #region Properties

        public string LocationID { get; set; }
        public string Description { get; set; }

        #endregion

        /// <summary>
        /// CMMS Part Location Constructor
        /// </summary>
        public CMMSPartLocation()
        {

        }

        /// <summary>
        /// Get a list of all available part lcoations
        /// </summary>
        /// <returns>List of CMMS Part locations as async task of IList<CMMSPartLocation></returns>
        public async static Task<IList<CMMSPartLocation>> GetLocationListAsync()
        {
            var _locList = new List<CMMSPartLocation>();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`cmms_part_locations`", App.ConAsync))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            _locList.Add(new CMMSPartLocation
                            {
                                LocationID = reader.GetString(nameof(LocationID)),
                                Description = reader.GetString("LocationDescription")
                            });
                        }
                    }
                }
                return _locList;
            }
            catch (Exception)
            {
                return new List<CMMSPartLocation>();
            }
        }
    }
}
