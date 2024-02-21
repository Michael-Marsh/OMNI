using OMNI.Extensions;
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
        /// Grab current sales from M2k
        /// </summary>
        /// <param name="startDate">Date to start the query</param>
        /// <param name="endDate">Date to end the query</param>
        /// <returns>current sales as int</returns>
        public static int GetLiveSales(string startDate, string endDate)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE CONTI_MAIN;
                                                        SELECT SUM([Sales]) FROM [dbo].[SA-INIT] WHERE [Inv_So_Date]>='{startDate}' AND [Inv_So_Date]<='{endDate}' AND [Record_Type]='SL'", App.SqlConAsync))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
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
                    using (SqlCommand cmd = new SqlCommand($@"USE CONTI_MAIN;
                                                            SELECT a.[Drawing_Nbrs], b.[Engineering_Status] FROM [dbo].[IM-INIT] a RIGHT JOIN [dbo].[IPL-INIT] b ON a.[Part_Number] = b.[Part_Nbr] WHERE a.[Part_Number] = @p1;", App.SqlConAsync))
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
                catch (Exception)
                {
                    
                }
                return _results[1] == "O" ? _results[1] : _results[0];
            }
            else if (company == "Csi")
            {
                _results[0] = "Invalid";
                _results[1] = "Invalid";
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
                    using (SqlCommand cmd = new SqlCommand($@"USE CONTI_MAIN;
                                                            SELECT a.[Drawing_Nbrs], b.[Engineering_Status] FROM [dbo].[IM-INIT] a RIGHT JOIN [dbo].[IPL-INIT] b ON a.[Part_Number] = b.[Part_Nbr] WHERE a.[Part_Number] = @p1;", App.SqlConAsync))
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
                catch (Exception)
                {

                }
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
                using (SqlCommand cmd = new SqlCommand($@"USE CONTI_MAIN;
                                                        SELECT [Engineering_Status] FROM [dbo].[IPL-INIT] WHERE [Part_Nbr] = @p1;", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", partNumber);
                    return cmd.ExecuteScalar().ToString();
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
                using (SqlCommand cmd = new SqlCommand($@"USE CONTI_MAIN;
                                                        SELECT [Url] FROM [dbo].[IM-INIT_Url_Codes] WHERE [ID1] = @p1;", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", partNumber);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                _tempList.Add(reader.SafeGetString("Url"));
                            }
                        }
                        return _tempList;
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
