using IBMU2.UODOTNET;
using OMNI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace OMNI.Helpers
{
    /// <summary>
    /// Manage 2000 COM Class
    /// </summary>
    public sealed class M2k
    {
        /// <summary>
        /// Retrieve the master print number attached to a part number in Manage2000
        /// </summary>
        /// <param name="partNumber">Part Number</param>
        /// <param name="manageAccount">Manage account to use</param>
        /// <returns>Part number or master print part number</returns>
        public static string Master(string partNumber, string manageAccount)
        {
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(Properties.Settings.Default.ManageHostName, Properties.Settings.Default.ManageAccount, Properties.Settings.Default.ManageAccount, manageAccount, "udcs"))
                {
                    using (UniFile uFile = uSession.CreateUniFile("IM"))
                    {
                        using (UniDynArray udArray = uFile.Read(partNumber))
                        {
                            return string.IsNullOrEmpty(udArray.Extract(157).StringValue) ? partNumber : udArray.Extract(157).StringValue;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return "Invalid";
            }
        }

        /// <summary>
        /// Inquire a Part Numbers Status in Manage2000
        /// </summary>
        /// <param name="partNumber">Part Number</param>
        /// <param name="manageAccount">Manage account to use</param>
        /// <returns>Engineering status of a given part number</returns>
        public static string EngineeringStatus(string partNumber, string manageAccount)
        {
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(Properties.Settings.Default.ManageHostName, Properties.Settings.Default.ManageAccount, Properties.Settings.Default.ManageAccount, manageAccount, "udcs"))
                {
                    using (UniFile uFile = uSession.CreateUniFile("IPL"))
                    {
                        using (UniDynArray udArray = uFile.Read(partNumber))
                        {
                            return string.IsNullOrEmpty(udArray.Extract(40).StringValue) ? "NotOnFile" : udArray.Extract(40).StringValue;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return "O";
            }
        }

        /// <summary>
        /// Modify records in manage
        /// !!WARNING!! DO NOT USE this method to modify business logic transactions like wip reciepts
        /// The intent of this method is to modify single records that are stand alone in the M2k database i.e. N location reason
        /// </summary>
        /// <param name="fileName">Name of the file to modify</param>
        /// <param name="attribute">Name of the attribute to modidy</param>
        /// <param name="newValue">The new value to input into manage</param>
        /// <param name="recordID">Record ID</param>
        public static void ModifyRecord(string fileName, int attribute, string newValue, string recordID)
        {
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(Properties.Settings.Default.ManageHostName, Properties.Settings.Default.ManageAccount, Properties.Settings.Default.ManageAccount, $"E:/roi/{CurrentUser.Site}.MAIN", "udcs"))
                {
                    using (UniFile uFile = uSession.CreateUniFile(fileName))
                    {
                        using (UniDynArray udArray = uFile.Read(recordID))
                        {
                            udArray.Insert(attribute, newValue);
                            uFile.Write(recordID, udArray);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Delete records in manage
        /// !!WARNING!! DO NOT USE this method to modify business logic transactions like wip reciepts
        /// The intent of this method is to delete single records that are stand alone in the M2k database i.e. N location reason
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="attribute">Name of the attribute to modidy</param>
        /// <param name="recordID">Record ID</param>
        public static void DeleteRecord(string fileName, int attribute, string recordID)
        {
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(Properties.Settings.Default.ManageHostName, Properties.Settings.Default.ManageAccount, Properties.Settings.Default.ManageAccount, $"E:/roi/{CurrentUser.Site}.MAIN", "udcs"))
                {
                    using (UniFile uFile = uSession.CreateUniFile(fileName))
                    {
                        using (UniDynArray udArray = uFile.Read(recordID))
                        {
                            udArray.Delete(attribute);
                            uFile.Write(recordID, udArray);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        //********************************************************************
        //Everything Below was written for the SQL implementation for M2k data
        //********************************************************************

        /// <summary>
        /// Grab current sales from M2k
        /// </summary>
        /// <param name="startDate">Date to start the query</param>
        /// <param name="endDate">Date to end the query</param>
        /// <returns>current sales as int</returns>
        public static int GetLiveSales(string startDate, string endDate)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Properties.Settings.Default.omniMSSQLConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand($"SELECT SUM([Sales]) FROM [dbo].[SA-INIT] WHERE [Inv_So_Date]>='{startDate}' AND [Inv_So_Date]<='{endDate}' AND [Record_Type]='SL'", con))
                    {
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Search manage for a part number
        /// </summary>
        /// <param name="partNumber">Part Number to search for</param>
        /// <param name="company">Company M2k system to search through</param>
        /// <returns>File path to controlled print as string</returns>
        public async static Task<string> SQLPartSearchAsync(string partNumber, string company)
        {
            var _results = new string[4];
            if (company == "Wcco")
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(Properties.Settings.Default.omniMSSQLConnectionString))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand($"SELECT a.[Drawing_Nbrs], b.[Engineering_Status] FROM [dbo].[IM-INIT] a RIGHT JOIN [dbo].[IPL-INIT] b ON a.[Part_Number] = b.[Part_Nbr] WHERE a.[Part_Number] = @p1;", con))
                        {
                            cmd.Parameters.AddWithValue("p1", partNumber);
                            using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                if (reader.HasRows)
                                {
                                    while (await reader.ReadAsync().ConfigureAwait(false))
                                    {
                                        _results[0] = await reader.IsDBNullAsync(0) ? partNumber : reader.GetString(0);
                                        _results[1] = await reader.IsDBNullAsync(1) ? "O" : reader.GetString(1).Trim();
                                    }
                                }
                                else
                                {
                                    return "Invalid";
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    
                }
                return _results[1] == "O" ? _results[1] : _results[0];
            }
            else if (company == "Csi")
            {
                _results[0] = "Invalid";
                _results[1] = "Invalid";
                _results[2] = await Task.Run(() => Master(partNumber, Properties.Settings.Default.CSIManageAccount)).ConfigureAwait(false);
                _results[3] = await Task.Run(() => EngineeringStatus(partNumber, Properties.Settings.Default.CSIManageAccount)).ConfigureAwait(false);
                return _results[2] == "Invalid"
                    ? _results[2]
                    : _results[3] == "O"
                        ? _results[3]
                        : _results[2];
            }
            else
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(Properties.Settings.Default.omniMSSQLConnectionString))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand($"SELECT a.[Drawing_Nbrs], b.[Engineering_Status] FROM [dbo].[IM-INIT] a RIGHT JOIN [dbo].[IPL-INIT] b ON a.[Part_Number] = b.[Part_Nbr] WHERE a.[Part_Number] = @p1;", con))
                        {
                            cmd.Parameters.AddWithValue("p1", partNumber);
                            using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                if (reader.HasRows)
                                {
                                    while (await reader.ReadAsync().ConfigureAwait(false))
                                    {
                                        _results[0] = await reader.IsDBNullAsync(0) ? "Invalid" : reader.GetString(0);
                                        _results[1] = await reader.IsDBNullAsync(1) ? "O" : reader.GetString(1).Trim();
                                    }
                                }
                                else
                                {
                                    _results[0] = "Invalid";
                                    _results[1] = "O";
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
                _results[2] = await Task.Run(() => Master(partNumber, Properties.Settings.Default.CSIManageAccount)).ConfigureAwait(false);
                _results[3] = await Task.Run(() => EngineeringStatus(partNumber, Properties.Settings.Default.CSIManageAccount)).ConfigureAwait(false);
                return _results[0] == "Invalid" && _results[2] == "Invalid"
                    ? _results[0]
                    : _results[1] == "O" && _results[3] == "O"
                        ? _results[1]
                        : _results[0] != "Invalid" && _results[1] != "O"
                            ? _results[0]
                            : _results[2] != "Invalid" && _results[3] != "O"
                                ? _results[2]
                                : _results[0] != "Invalid" && _results[1] == "O"
                                    ? _results[1]
                                    : _results[2] != "Invalid" && _results[3] == "O"
                                        ? _results[3]
                                        : "Invalid";
            }
        }

        /// <summary>
        /// Get a part numbers engineering status
        /// </summary>
        /// <param partNumber>Part Number</param>
        /// <returns>engineering status as string</returns>
        public static string GetEngineeringStatus(string partNumber)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Properties.Settings.Default.omniMSSQLConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand($"SELECT [Engineering_Status] FROM [dbo].[IPL-INIT] WHERE [Part_Nbr] = @p1;", con))
                    {
                        cmd.Parameters.AddWithValue("p1", partNumber);
                        return cmd.ExecuteScalar().ToString();
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get a list of work instructions based on a part number
        /// </summary>
        /// <param partNumber>Part Number</param>
        /// <returns>Work instruction url's as list of strings</returns>
        public static List<string> GetWorkInstructions(string partNumber)
        {
            try
            {
                var _tempList = new List<string>();
                using (SqlConnection con = new SqlConnection(Properties.Settings.Default.omniMSSQLConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand($"SELECT [Url] FROM [dbo].[IM-INIT_Url_Codes] WHERE [ID1] = @p1;", con))
                    {
                        cmd.Parameters.AddWithValue("p1", partNumber);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _tempList.Add(reader.GetString(0));
                                }
                            }
                            return _tempList;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public static class M2kResponseCleaner
    {
        /// <summary>
        /// Cleans and Prepares the M2k string response for use in application parsing
        /// </summary>
        /// <param name="_tempList">Command Response as List<string></param>
        public static void Clean(this List<string> _tempList)
        {
            foreach (string _temp in _tempList.ToArray())
            {
                if (_temp == " " || _temp.Contains("record") || _temp.Contains("LIST"))
                {
                    _tempList.Remove(_temp);
                }
                else
                {
                    _tempList[_tempList.IndexOf(_temp)].Trim();
                }
            }
        }

        /// <summary>
        /// Cleans and Prepares the M2k string response for use in application parsing
        /// </summary>
        /// <param name="_tempList">Command Response as List<string></param>
        /// <param name="arg">Extra argument to use for cleaning</param>
        public static void Clean(this List<string> _tempList, string arg)
        {
            foreach (string _temp in _tempList.ToArray())
            {
                if (_temp == " " || _temp.Contains("record") || _temp.Contains("LIST") || _temp.Contains(arg))
                {
                    _tempList.Remove(_temp);
                }
                else
                {
                    _tempList[_tempList.IndexOf(_temp)].Trim();
                }
            }
        }

        /// <summary>
        /// Cleans and Prepares the M2k string response for use in application parsing
        /// </summary>
        /// <param name="_tempList">Command Response as List<string></param>
        /// <param name="arg">Extra arguments to use for cleaning</param>
        public static void Clean(this List<string> _tempList, params string[] arg)
        {
            foreach (string _temp in _tempList.ToArray())
            {
                foreach (string s in arg)
                {
                    if (_temp.Contains(s))
                    {
                        _tempList.Remove(s);
                    }
                }
                if (_temp == " " || _temp.Contains("record") || _temp.Contains("LIST"))
                {
                    _tempList.Remove(_temp);
                }
                else
                {
                    _tempList[_tempList.IndexOf(_temp)].Trim();
                }
            }
        }
    }
}
