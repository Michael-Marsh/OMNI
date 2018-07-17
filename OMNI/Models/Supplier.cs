using OMNI.Extensions;
using OMNI.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace OMNI.Models
{
    /// <summary>
    /// Supplier Object Interaction Logic
    /// </summary>
    public class Supplier
    {
        #region Properties

        public int ID { get; set; }
        public string Name { get; set; }

        #endregion

        /// <summary>
        /// List of Suppliers
        /// </summary>
        /// <returns>Generated List of Suppliers</returns>
        public async static Task<List<Supplier>> GetSupplierListAsync()
        {
            try
            {
                var _supplierList = new List<Supplier>();
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT * FROM [supplier]", App.SqlConAsync))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            _supplierList.Add(new Supplier { ID = reader.SafeGetInt32("SupplierNumber"), Name = reader.SafeGetString("SupplierName") });
                        }
                    }
                }
                return _supplierList;
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// Get a supplier number
        /// </summary>
        /// <param name="supplierName">Name of the supplier</param>
        /// <returns>Supplier Number as int</returns>
        public static int GetSupplierNumber(string supplierName)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT [SupplierNumber] FROM [supplier] WHERE [SupplierName] LIKE %{supplierName}%", App.SqlConAsync))
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
}
