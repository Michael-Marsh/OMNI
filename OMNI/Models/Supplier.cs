using MySql.Data.MySqlClient;
using OMNI.Helpers;
using System;
using System.Collections.Generic;
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
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`supplier`", App.ConAsync))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            _supplierList.Add(new Supplier { ID = reader.GetInt32("SupplierNumber"), Name = reader.GetString("SupplierName") });
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
        public async static Task<int> GetSupplierNumberAsync(string supplierName)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT `SupplierNumber` FROM `{App.Schema}`.`supplier` WHERE `SupplierName` LIKE %{supplierName}%", App.ConAsync))
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
}
