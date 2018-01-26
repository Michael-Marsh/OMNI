using MySql.Data.MySqlClient;
using OMNI.Helpers;
using OMNI.ViewModels;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace OMNI.Models
{
    /// <summary>
    /// OMNI DataBase Interaction Logic
    /// </summary>
    public class OMNIDataBase
    {
        #region Properties

        private static int? Count { get; set; }

        /// <summary>
        /// Name of omni schema tables
        /// </summary>
        public string TableName { get; set; }

        #endregion

        /// <summary>
        /// Get a list of OMNI DataBase table names
        /// </summary>
        /// <returns>List of table names as List<OMNIDataBase></returns>
        public static List<OMNIDataBase> GetTableNameList()
        {
            var _tempList = new List<OMNIDataBase>();
            using (MySqlCommand cmd = new MySqlCommand($"SHOW TABLES FROM `{App.Schema}`", App.ConAsync))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _tempList.Add(new OMNIDataBase { TableName = reader.GetString(0) });
                    }
                }
            }
            return _tempList;
        }

        /// <summary>
        /// QIR Notice Delete Function
        /// </summary>
        /// <param name="qirNumber">QIR Number to delete from an individual notice board.</param>
        /// <param name="module">optional: Module to use when deleting, best used with delete all functionality [default = null]</param>
        /// <param name="updateValue">optional: Switch value [default = 1]</param>
        public async static void QIRNoticeValueSwitchAsync(int qirNumber, string module = null, int updateValue = 1)
        {
            var _col = string.Empty;
            var _val = 0;
            if (qirNumber > 0 && string.IsNullOrEmpty(module))
            {
                _col = "QIRNumber";
                _val = qirNumber;
            }
            else
            {
                _col = CurrentUser.IdNumber.ToString();
                switch (module)
                {
                    case "Inbox":
                        _val = 0;
                        break;
                    case "WatchList":
                        _val = 2;
                        break;
                }
            }
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"UPDATE `{App.Schema}`.`qir_notice` SET `{CurrentUser.IdNumber}`={updateValue} WHERE `{_col}`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", _val);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
            }
        }

        /// <summary>
        /// Count the rows in any table
        /// </summary>
        /// <param name="tableName">Name of table</param>
        /// <returns>Row count as int</returns>
        public async static Task<int> CountAsync(string tableName)
        {
            var count = 0;
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(*) FROM `{App.Schema}`.`{tableName}`", App.ConAsync))
                {
                    count = Convert.ToInt16(await cmd.ExecuteScalarAsync());
                }
                if (count == -1 || count == 0)
                {
                    return 0;
                }
                return count;
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                return 0;
            }
        }

        /// <summary>
        /// Count the items in a table column where the value(s) are static
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="columnName">Column Name</param>
        /// <param name="whereClaus">Filter Option</param>
        /// <returns>Count of items</returns>
        public async static Task<int?> CountWithValuesAsync(string tableName, string columnName, params string[] whereClaus)
        {
            try
            {
                Count = 0;
                foreach (string value in whereClaus)
                {
                    using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(*) FROM `{App.Schema}`.`{tableName}` WHERE `{columnName}`=@p1", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", value);
                        Count += Convert.ToInt16(await cmd.ExecuteScalarAsync());
                    }
                }
                if (Count == -1 || Count == 0)
                {
                    return null;
                }
                return Count;
            }
            catch (Exception)
            {
                while (App.ConAsync.State == ConnectionState.Closed)
                {
                    await App.ConAsync.OpenAsync();
                }
                return null;
            }
        }

        /// <summary>
        /// Count the null values in a table column
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="columnName">Column Name</param>
        /// <returns>Count of null values</returns>
        public async static Task<int?> CountNullValuesAsync(string tableName, string columnName)
        {
            try
            {
                Count = 0;
                using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(*) FROM `{App.Schema}`.`{tableName}` WHERE `{columnName}` IS NULL", App.ConAsync))
                {
                    Count += Convert.ToInt16(await cmd.ExecuteScalarAsync());
                }
                if (Count == -1 || Count == 0)
                {
                    return null;
                }
                return Count;
            }
            catch (Exception)
            {
                while (App.ConAsync.State == ConnectionState.Closed)
                {
                    await App.ConAsync.OpenAsync();
                }
                return null;
            }
        }

        /// <summary>
        /// Count the items in a table column based on a comparison in values
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="whereClaus">Filter Option</param>
        /// <returns>Count of items</returns>
        public async static Task<int?> CountWithComparisonAsync(string tableName, string whereClaus)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(*) FROM `{App.Schema}`.`{tableName}` WHERE {whereClaus}", App.ConAsync))
                {
                    Count = Convert.ToInt16(await cmd.ExecuteScalarAsync());
                }
                if (Count == -1 || Count == 0)
                {
                    return null;
                }
                return Count;
            }
            catch (Exception)
            {
                while (App.ConAsync.State == ConnectionState.Closed)
                {
                    await App.ConAsync.OpenAsync();
                }
                return null;
            }
        }

        /// <summary>
        /// Get the sum from a total cost column with in a table
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="whereClaus">Filter Option</param>
        /// <returns>Count of items</returns>
        public async static Task<double> TotalCostAsync(string tableName, string whereClaus)
        {
            var totalCost = 0.0;
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT `TotalCost` FROM `{App.Schema}`.`{tableName}` WHERE {whereClaus}", App.ConAsync))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            totalCost += Convert.ToDouble(reader.GetDecimal(0));
                        }
                    }
                }
                return totalCost;
            }
            catch (Exception)
            {
                while (App.ConAsync.State == ConnectionState.Closed)
                {
                    await App.ConAsync.OpenAsync();
                }
                return totalCost;
            }
        }

        /// <summary>
        /// Add a note to any form
        /// </summary>
        /// <param name="table">table to use</param>
        /// <param name="formNumber">The form number to add the note to</param>
        /// <returns>The note subject that was entered as string</returns>
        public async static Task<string> AddNoteAsync(string table, int? formNumber)
        {
            var _note = NoteWindowViewModel.Show();
            if (string.IsNullOrWhiteSpace(_note))
            {
                ExceptionWindow.Show("Blank Note", "A blank note was entered, submission has been canceled.\nIf you feel you reached this message in error please contact IT.");
                return null;
            }
            else
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand($"INSERT `{App.Schema}`.`{table}_notes` (IDNumber, Note, Submitter) VALUES(@p1, @p2, @p3)", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", formNumber);
                        cmd.Parameters.AddWithValue("p2", _note);
                        cmd.Parameters.AddWithValue("p3", CurrentUser.FullName);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    return _note;
                }
                catch (Exception ex)
                {
                    ExceptionWindow.Show("Unhandled Excpetion", ex.Message, ex);
                    return null;
                }
            }
        }

        /// <summary>
        /// Checks to see if given parameters exist in a OMNI table and column
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="columnName">Column Name</param>
        /// <param name="whereClaus">Item to check</param>
        /// <returns>Item existence as a boolean</returns>
        public async static Task<bool> ExistsAsync(string tableName, string columnName, string whereClaus)
        {
            var _exists = 0;
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT({columnName}) FROM `{App.Schema}`.`{tableName}` WHERE `{columnName}`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", whereClaus);
                    _exists = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
                return _exists >= 1
                    ? true
                    : false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieve the Monthly Sales from the OMNI Database.
        /// </summary>
        /// <param name="lookupMonth">Month of Sales to retrieve</param>
        /// <param name="lookupYear">Year of Sales to retrieve</param>
        /// <returns>Monthly Sales as int[]</returns>
        public async static Task<int[]> MonthlySalesAsync(string lookupMonth, int lookupYear)
        {
            var values = new int[2];
            using (MySqlCommand cmd = new MySqlCommand($"SELECT `Sales`, `Firm` FROM `{App.Schema}`.`monthlysales` WHERE `Year`=@p1 AND `Month`=@p2", App.ConAsync))
            {
                cmd.Parameters.AddWithValue("p1", lookupYear);
                cmd.Parameters.AddWithValue("p2", lookupMonth);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        values[0] = reader.GetInt32(0);
                        values[1] = reader.GetInt16(1);
                    }
                }
            }
            return values;
        }

        /// <summary>
        /// Retrieve the Yearly Sales from the OMNI Database.
        /// </summary>
        /// <param name="lookupYear">Year of Sales to retrieve</param>
        /// <returns>Yearly Sales as int</returns>
        public async static Task<int> YearlySalesAsync(int lookupYear)
        {
            var values = 0;
            using (MySqlCommand cmd = new MySqlCommand($"SELECT `Sales` FROM `{App.Schema}`.`monthlysales` WHERE `Year`=@p1", App.ConAsync))
            {
                cmd.Parameters.AddWithValue("p1", lookupYear);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        values += reader.GetInt32(0);
                    }
                }
            }
            return values;
        }

        /// <summary>
        /// Update Monthly Sales in OMNI's Database.
        /// </summary>
        /// <param name="month">Update Month</param>
        /// <param name="year">Update Year</param>
        /// <param name="sales">Sales number to update/insert</param>
        /// <param name="firm">optional: Marks that the sales number has been validated by accounting</param>
        public async static void UpdateSalesAsync(string month, int year, int sales, bool firm = false)
        {
            var valueCheck = 0;
            using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(*) FROM `{App.Schema}`.`monthlysales` WHERE `Month`=@p1 AND `Year`=@p2", App.ConAsync))
            {
                cmd.Parameters.AddWithValue("p1", month);
                cmd.Parameters.AddWithValue("p2", year);
                valueCheck = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            var _cmd = string.Empty;
            _cmd = valueCheck >= 1 ? $"UPDATE `{App.Schema}`.`monthlysales` SET `Sales`=@p1, `Firm`=@p2 WHERE `Month`=@p3 AND `Year`=@p4" : $"INSERT INTO `{App.Schema}`.`monthlysales` (`Sales`, `Firm`, `Month`, `Year`) VALUES (@p1, @p2, @p3, @p4)";
            using (MySqlCommand cmd = new MySqlCommand(_cmd, App.ConAsync))
            {
                cmd.Parameters.AddWithValue("p1", sales);
                cmd.Parameters.AddWithValue("p2", firm);
                cmd.Parameters.AddWithValue("p3", month);
                cmd.Parameters.AddWithValue("p4", year);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Retrieve the user domain name using the user ID number
        /// </summary>
        /// <param name="userID">User ID</param>
        /// <returns>DomainName as string</returns>
        public async static Task<string> UserDomainNameFromIDAsync(int userID)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT `DomainName` FROM `{App.Schema}`.`users` WHERE `EmployeeNumber`={userID}", App.ConAsync))
                {
                    return (await cmd.ExecuteScalarAsync()).ToString();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieve an updated plate index table
        /// </summary>
        /// <returns>plate_index as DataTable</returns>
        public async static Task<DataTable> GetExtruderPlateTableAsync()
        {
            var sqlRowCount = 0;
            var tempFile = $"{Properties.Settings.Default.omnitemp}datamine.xlsm";
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(*) FROM `{App.Schema}`.`plate_index`;", App.ConAsync))
                {
                    sqlRowCount = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                }
                File.Copy(Properties.Settings.Default.ExtruderExcelFilePath, tempFile, true);
                using (SLDocument Excel = new SLDocument(tempFile, "Extruder Inspection"))
                {
                    var stats = Excel.GetWorksheetStatistics();
                    var ExcelRowCount = stats.NumberOfRows - 2;
                    while (ExcelRowCount > sqlRowCount)
                    {
                        using (MySqlCommand cmd = new MySqlCommand($"INSERT INTO `{App.Schema}`.`plate_index` (Part_No, Ext_No, Die_ID, Top_Bar, Plate, Date) VALUES(@p1, @p2, @p3, @p4, @p5, @p6)", App.ConAsync))
                        {
                            sqlRowCount++;
                            cmd.Parameters.AddWithValue("p1", Excel.GetCellValueAsInt32(sqlRowCount, 1));
                            cmd.Parameters.AddWithValue("p2", Excel.GetCellValueAsInt32(sqlRowCount, 4));
                            cmd.Parameters.AddWithValue("p3", Excel.GetCellValueAsString(sqlRowCount, 8));
                            cmd.Parameters.AddWithValue("p4", Excel.GetCellValueAsString(sqlRowCount, 9));
                            cmd.Parameters.AddWithValue("p5", Excel.GetCellValueAsString(sqlRowCount, 10));
                            cmd.Parameters.AddWithValue("p6", Excel.GetCellValueAsDateTime(sqlRowCount, 2));
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                using (MySqlDataAdapter adapter = new MySqlDataAdapter($"SELECT DISTINCT `Date`, `Part_No`, `Ext_No`, `Die_ID`, `Top_Bar`, `Plate` FROM `{App.Schema}`.`plate_index`", App.ConAsync))
                {
                    using (DataTable table = new DataTable())
                    {
                        await adapter.FillAsync(table);
                        return table;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex, nameof(GetExtruderPlateTableAsync));
                return null;
            }
        }

        /// <summary>
        /// Developer Methods
        /// </summary>
        public static void UpdateUsers()
        {
            var users = new Users();
            using (MySqlConnection con = new MySqlConnection("Server=172.16.0.221;UID=omni;Pwd=7009;"))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM `omni.users`", con))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.EmployeeNumber = reader.GetInt32("EmployeeNumber");
                            users.DomainName = reader.GetString("DomainName");
                            users.FullName = reader.GetString("FullName");
                            users.EmployeeNumber = reader.GetInt32("EmployeeNumber");
                            users.Email = reader.GetString("eMail");
                            users.Quality = reader.GetBoolean("Quality");
                            users.QualityNotice = reader.GetBoolean("QualityNotice");
                            users.Kaizen = reader.GetBoolean("Kaizen");
                            users.CMMS = reader.GetBoolean("CMMS");
                            users.CMMSCrew = reader.GetBoolean("CMMSCrew");
                            users.IT = reader.GetBoolean("IT");
                            users.Engineering = reader.GetBoolean("Engineering");
                            users.Admin = reader.GetBoolean("OMNIAdministrator");
                            users.Developer = reader.GetBoolean("Developer");
                            SubmitUser(users);
                        }
                    }
                }
            }
        }
        public static void SubmitUser(Users users)
        {
            using (MySqlConnection con = new MySqlConnection(Properties.Settings.Default.omniConnectionString))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("INSERT INTO omni.users (`EmployeeNumber`, `DomainName`, `AccountName`, `FullName`, `eMail`, `Quality`, `QualityNotice`, `Kaizen`, `CMMS`, `CMMSCrew`, `IT`, `Engineering`, `OMNIAdministrator`, `Developer`) VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14)", con))
                {
                    cmd.Parameters.AddWithValue("p1", users.EmployeeNumber);
                    cmd.Parameters.AddWithValue("p2", users.DomainName);
                    if (users.FullName.Contains(" "))
                    {
                        cmd.Parameters.AddWithValue("p3", users.FullName.Substring(0, users.FullName.IndexOf(" ")));
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("p3", users.FullName);
                    }
                    cmd.Parameters.AddWithValue("p4", users.FullName);
                    cmd.Parameters.AddWithValue("p5", users.Email);
                    cmd.Parameters.AddWithValue("p6", users.Quality);
                    cmd.Parameters.AddWithValue("p7", users.QualityNotice);
                    cmd.Parameters.AddWithValue("p8", users.Kaizen);
                    cmd.Parameters.AddWithValue("p9", users.CMMS);
                    cmd.Parameters.AddWithValue("p10", users.CMMSCrew);
                    cmd.Parameters.AddWithValue("p11", users.IT);
                    cmd.Parameters.AddWithValue("p12", users.Engineering);
                    cmd.Parameters.AddWithValue("p13", users.Admin);
                    cmd.Parameters.AddWithValue("p14", users.Developer);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
