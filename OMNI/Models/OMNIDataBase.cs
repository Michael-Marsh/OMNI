using OMNI.Extensions;
using OMNI.Helpers;
using OMNI.ViewModels;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
            using (SqlCommand cmd = new SqlCommand($"SELECT TABLE_NAME as 'Table' FROM {App.DataBase}.INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';", App.SqlConAsync))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _tempList.Add(new OMNIDataBase { TableName = reader.SafeGetString("Table") });
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
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        UPDATE [qir_notice] SET [{CurrentUser.IdNumber}]={updateValue} WHERE [{_col}]=@p1", App.SqlConAsync))
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
        public static int RowCount(string tableName)
        {
            var count = 0;
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT COUNT(*) FROM [{tableName}]", App.SqlConAsync))
                {
                    count = Convert.ToInt16(cmd.ExecuteScalar());
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
        public static int? CountWithValues(string tableName, string columnName, params string[] whereClaus)
        {
            try
            {
                Count = 0;
                foreach (string value in whereClaus)
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                            SELECT COUNT(*) FROM [{tableName}] WHERE [{columnName}]=@p1", App.SqlConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", value);
                        Count += Convert.ToInt16(cmd.ExecuteScalar());
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
                return null;
            }
        }

        /// <summary>
        /// Count the null values in a table column
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="columnName">Column Name</param>
        /// <returns>Count of null values</returns>
        public static int? CountNullValues(string tableName, string columnName)
        {
            try
            {
                Count = 0;
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT COUNT(*) FROM [{tableName}] WHERE [{columnName}] IS NULL", App.SqlConAsync))
                {
                    Count += Convert.ToInt16(cmd.ExecuteScalar());
                }
                if (Count == -1 || Count == 0)
                {
                    return null;
                }
                return Count;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Count the items in a table column based on a comparison in values
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="whereClaus">Filter Option</param>
        /// <returns>Count of items</returns>
        public static int? CountWithComparison(string tableName, string whereClaus)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT COUNT(*) FROM [{tableName}] WHERE {whereClaus}", App.SqlConAsync))
                {
                    Count = Convert.ToInt16(cmd.ExecuteScalar());
                }
                if (Count == -1 || Count == 0)
                {
                    return null;
                }
                return Count;
            }
            catch (Exception)
            {
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
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT [TotalCost] FROM [{tableName}] WHERE {whereClaus}", App.SqlConAsync))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
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
                while (App.SqlConAsync.State == ConnectionState.Closed)
                {
                    await App.SqlConAsync.OpenAsync();
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
        public static string AddNote(string table, int? formNumber)
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
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                            INSERT [{table}_notes] (IDNumber, Note, Submitter)
                                                            VALUES(@p1, @p2, @p3)", App.SqlConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", formNumber);
                        cmd.Parameters.AddWithValue("p2", _note);
                        cmd.Parameters.AddWithValue("p3", CurrentUser.FullName);
                        cmd.ExecuteNonQuery();
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
        public static bool Exists(string tableName, string columnName, string whereClaus)
        {
            var _exists = 0;
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT COUNT({columnName}) FROM [{tableName}] WHERE [{columnName}]=@p1", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", whereClaus);
                    _exists = Convert.ToInt32(cmd.ExecuteScalar());
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
            using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                    SELECT [Sales], [Firm] FROM [monthlysales] WHERE [Year]=@p1 AND Month=@p2", App.SqlConAsync))
            {
                cmd.Parameters.AddWithValue("p1", lookupYear);
                cmd.Parameters.AddWithValue("p2", lookupMonth);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        values[0] = reader.SafeGetInt32("Sales");
                        values[1] = reader.SafeGetInt32("Firm");
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
            using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                    SELECT [Sales] FROM [monthlysales] WHERE [Year]=@p1", App.SqlConAsync))
            {
                cmd.Parameters.AddWithValue("p1", lookupYear);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        values += reader.SafeGetInt32("Sales");
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
        public static void UpdateSales(string month, int year, int sales, bool firm = false)
        {
            var valueCheck = 0;
            using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                    SELECT COUNT(*) FROM [monthlysales] WHERE [Month]=@p1 AND [Year]=@p2", App.SqlConAsync))
            {
                cmd.Parameters.AddWithValue("p1", month);
                cmd.Parameters.AddWithValue("p2", year);
                valueCheck = Convert.ToInt32(cmd.ExecuteScalar());
            }
            var _cmd = string.Empty;
            _cmd = valueCheck >= 1 
                ? $@"USE {App.DataBase};
                    UPDATE [monthlysales] SET [Sales]=@p1, [Firm]=@p2 WHERE [Month]=@p3 AND [Year]=@p4"
                : $@"USE {App.DataBase};
                    INSERT INTO [monthlysales] ([Sales], [Firm], [Month], [Year]) VALUES (@p1, @p2, @p3, @p4)";
            using (SqlCommand cmd = new SqlCommand(_cmd, App.SqlConAsync))
            {
                cmd.Parameters.AddWithValue("p1", sales);
                cmd.Parameters.AddWithValue("p2", firm);
                cmd.Parameters.AddWithValue("p3", month);
                cmd.Parameters.AddWithValue("p4", year);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Retrieve the user domain name using the user ID number
        /// </summary>
        /// <param name="userID">User ID</param>
        /// <returns>DomainName as string</returns>
        public static string UserDomainNameFromID(int userID)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT [DomainName] FROM [users] WHERE [EmployeeNumber]={userID}", App.SqlConAsync))
                {
                    return (cmd.ExecuteScalar()).ToString();
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
        public static DataTable GetExtruderPlateTable()
        {
            var sqlRowCount = 0;
            var tempFile = $"{Properties.Settings.Default.omnitemp}datamine.xlsm";
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT COUNT(*) FROM [plate_index];", App.SqlConAsync))
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
                        using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                                INSERT INTO [plate_index] (Part_No, Ext_No, Die_ID, Top_Bar, Plate, Date) VALUES(@p1, @p2, @p3, @p4, @p5, @p6)", App.SqlConAsync))
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
                using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {App.DataBase};
                                                                    SELECT DISTINCT [Date], [Part_No], [Ext_No], [Die_ID], [Top_Bar], [Plate] FROM [plate_index]", App.SqlConAsync))
                {
                    using (DataTable table = new DataTable())
                    {
                        adapter.Fill(table);
                        return table;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex, nameof(GetExtruderPlateTable));
                return null;
            }
        }

        /// <summary>
        /// Get a list of sites
        /// </summary>
        /// <returns>populated list of sites</returns>
        public static List<string> GetSiteList()
        {
            try
            {
                var _temp = new List<string>();
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                    SELECT [companyName] AS 'Site' FROM [company]", App.SqlConAsync))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                _temp.Add(reader.SafeGetString("Site"));
                            }
                        }
                    }
                }
                return _temp;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get the file location of the document help guide by site
        /// </summary>
        /// <returns></returns>
        public static string GetDocIndexHelpLocation()
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                    SELECT [DocIndexGuide] FROM [dbo].[company] WHERE [companyName]=@p1", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", CurrentUser.Site);
                    return cmd.ExecuteScalar().ToString();
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Developer Methods
        /// </summary>
        public static void UpdateUsers()
        {
            var users = new Users();
            using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                    SELECT * FROM [users]", App.SqlConAsync))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.EmployeeNumber = reader.SafeGetInt32("EmployeeNumber");
                        users.DomainName = reader.SafeGetString("DomainName");
                        users.FullName = reader.SafeGetString("FullName");
                        users.EmployeeNumber = reader.SafeGetInt32("EmployeeNumber");
                        users.Email = reader.SafeGetString("eMail");
                        users.Quality = reader.SafeGetBoolean("Quality");
                        users.QualityNotice = reader.SafeGetBoolean("QualityNotice");
                        users.Kaizen = reader.SafeGetBoolean("Kaizen");
                        users.CMMS = reader.SafeGetBoolean("CMMS");
                        users.CMMSCrew = reader.SafeGetBoolean("CMMSCrew");
                        users.IT = reader.SafeGetBoolean("IT");
                        users.Engineering = reader.SafeGetBoolean("Engineering");
                        users.Admin = reader.SafeGetBoolean("OMNIAdministrator");
                        users.Developer = reader.SafeGetBoolean("Developer");
                        SubmitUser(users);
                    }
                }
            }
        }
        public static void SubmitUser(Users users)
        {
            using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                    INSERT INTO [users]([EmployeeNumber], [DomainName], [AccountName], [FullName], [eMail], [Quality], [QualityNotice], [Kaizen], [CMMS], [CMMSCrew], [IT], [Engineering], [OMNIAdministrator], [Developer])
                                                    VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14)", App.SqlConAsync))
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
