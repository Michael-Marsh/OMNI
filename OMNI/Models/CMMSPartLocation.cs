using OMNI.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT * FROM [cmms_part_locations]", App.SqlConAsync))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            _locList.Add(new CMMSPartLocation
                            {
                                LocationID = reader.SafeGetString(nameof(LocationID)),
                                Description = reader.SafeGetString("LocationDescription")
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
