using IBMU2.UODOTNET;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace OMNI.Helpers
{
    /// <summary>
    /// Manage 2000 COM Class
    /// </summary>
    public sealed class M2k
    {
        /// <summary>
        /// Retrieve a part numbers cost from Manage2000.
        /// </summary>
        /// <param name="partNumber">Part Number</param>
        public static double Cost(string partNumber)
        {
            var i = 1;
            var cost = 0.0;
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(Properties.Settings.Default.ManageHostName, "query", "query", Properties.Settings.Default.WCCOManageAccount, "udcs"))
                {
                    using (UniFile uFile = uSession.CreateUniFile("IM"))
                    {
                        using (UniDynArray udArray = uFile.Read(partNumber))
                        {
                            while (i < 8)
                            {
                                if (!string.IsNullOrWhiteSpace(udArray.Extract(23, i).StringValue))
                                {
                                    cost += double.Parse(udArray.Extract(23, i).StringValue);
                                }
                                if (!string.IsNullOrWhiteSpace(udArray.Extract(29, i).StringValue))
                                {
                                    cost += double.Parse(udArray.Extract(29, i).StringValue);
                                }
                                i++;
                            }
                        }
                    }
                    UniObjects.CloseSession(uSession);
                }
                return cost / 100000;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Retrieve a part number's unit of measure (UOM) from Manage2000.
        /// </summary>
        /// <param name="partNumber">Part Number</param>
        public static string UOM(string partNumber)
        {
            var UOM = "None";
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(Properties.Settings.Default.ManageHostName, "query", "query", Properties.Settings.Default.WCCOManageAccount, "udcs"))
                {
                    using (UniFile uFile = uSession.CreateUniFile("IM"))
                    {
                        using (UniDynArray udArray = uFile.Read(partNumber))
                        {
                            if (!string.IsNullOrWhiteSpace(udArray.Extract(3).StringValue))
                                UOM = udArray.Extract(3).StringValue;
                        }
                    }
                    UniObjects.CloseSession(uSession);
                }
                return UOM;
            }
            catch (Exception)
            {
                return "None";
            }
        }

        /// <summary>
        /// Retrieve a part number that has been dedicated to a work order from Manage2000.
        /// </summary>
        /// <param name="workOrder">Work Order Number</param>
        public static string PartNumber(string workOrder)
        {
            var partNumber = string.Empty;
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(Properties.Settings.Default.ManageHostName, "query", "query", Properties.Settings.Default.WCCOManageAccount, "udcs"))
                {
                    using (UniFile uFile = uSession.CreateUniFile("WP"))
                    {
                        using (UniDynArray udArray = uFile.Read(workOrder))
                        {
                            if (!string.IsNullOrWhiteSpace(udArray.Extract(1).StringValue))
                                partNumber = udArray.Extract(1).StringValue;
                        }
                    }
                    UniObjects.CloseSession(uSession);
                }
                return partNumber;
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// Retrieve a work center number that has been dedicated to a work order from Manage2000.
        /// </summary>
        /// <param name="workOrder"></param>
        public static int WorkCenter(string workOrder)
        {
            var workCenter = string.Empty;
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(Properties.Settings.Default.ManageHostName, "query", "query", Properties.Settings.Default.WCCOManageAccount, "udcs"))
                {
                    using (UniFile uFile = uSession.CreateUniFile("WP"))
                    {
                        using (UniDynArray udArray = uFile.Read(workOrder))
                        {
                            if (!string.IsNullOrWhiteSpace(udArray.Extract(166).StringValue))
                                workCenter = udArray.Extract(166).StringValue;
                        }
                    }
                    UniObjects.CloseSession(uSession);
                }
                return Convert.ToInt32(workCenter.Substring(0, 5));
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
        public async static Task<string> PartSearchAsync(string partNumber, string company)
        {
            /*try
            {
                var _test = new Dictionary<string, string>();
                using (var odbcCon = new OdbcConnection("DSN=M2k-64;Uid=query;Pwd=query;"))
                {
                    odbcCon.Open();
                    using (var odbcCmd = new OdbcCommand($"SELECT * FROM IM_1_NF", odbcCon))
                    {
                        odbcCmd.ExecuteScalar();
                        using (var odbcReader = odbcCmd.ExecuteReader())
                        {
                            while (odbcReader.Read())
                            {
                                _test.Add(odbcReader.GetString(0), odbcReader.GetString(1));
                            }
                        }
                    }
                    odbcCon.Close();
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Testing", ex.Message);
                return "Invalid";
            }*/


            var _results = new string[4];
            if (company == "Wcco")
            {
                _results[0] = await Task.Run(() => Master(partNumber, Properties.Settings.Default.WCCOManageAccount)).ConfigureAwait(false);
                _results[1] = await Task.Run(() => EngineeringStatus(partNumber, Properties.Settings.Default.WCCOManageAccount)).ConfigureAwait(false);
                _results[2] = "Invalid";
                _results[3] = "Invalid";
                return _results[0] == "Invalid" 
                    ? _results[0] 
                    : _results[1] == "O" 
                        ? _results[1] 
                        : _results[0];
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
                _results[0] = await Task.Run(() => Master(partNumber, Properties.Settings.Default.WCCOManageAccount)).ConfigureAwait(false);
                _results[1] = await Task.Run(() => EngineeringStatus(partNumber, Properties.Settings.Default.WCCOManageAccount)).ConfigureAwait(false);
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
        /// Retrieve the master print number attached to a part number in Manage2000
        /// </summary>
        /// <param name="partNumber">Part Number</param>
        /// <param name="manageAccount">Manage account to use</param>
        /// <returns>Part number or master print part number</returns>
        public static string Master(string partNumber, string manageAccount)
        {
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(Properties.Settings.Default.ManageHostName, "query", "query", manageAccount, "udcs"))
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
                using (UniSession uSession = UniObjects.OpenSession(Properties.Settings.Default.ManageHostName, "query", "query", manageAccount, "udcs"))
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
        /// Get a Part Numbers attached work instructions from Manage2000
        /// </summary>
        /// <param name="partNumber">Part Number</param>
        public static List<string> WorkInstructions(string partNumber)
        {
            var i = 0;
            var WorkInstructionList = new List<string> { Capacity = 4 };
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(Properties.Settings.Default.ManageHostName, "query", "query", Properties.Settings.Default.WCCOManageAccount, "udcs"))
                {
                    using (UniFile uFile = uSession.CreateUniFile("IM"))
                    {
                        using (UniDynArray udArray = uFile.Read(partNumber))
                        {
                            while (i < 5)
                            {
                                if (!string.IsNullOrWhiteSpace(udArray.Extract(257, i).StringValue))
                                    WorkInstructionList.Add(udArray.Extract(257, i).StringValue);
                                i++;
                            }
                        }
                    }
                    UniObjects.CloseSession(uSession);
                }
                return WorkInstructionList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieve the BOM quantity of a parent part number for a child part number
        /// </summary>
        /// <param name="childPartNumber">Child part number</param>
        /// <param name="parentPartNumber">Parent part number</param>
        /// <returns>BOM amount as int</returns>
        public static double BOM(string childPartNumber, string parentPartNumber)
        {
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(Properties.Settings.Default.ManageHostName, "query", "query", Properties.Settings.Default.WCCOManageAccount, "udcs"))
                {
                    using (UniFile uFile = uSession.CreateUniFile("PS"))
                    {
                        using (UniDynArray udArray = uFile.Read($"{childPartNumber}*{parentPartNumber}"))
                        {
                            return string.IsNullOrEmpty(udArray.Extract(3).StringValue) ? 0 : Convert.ToDouble(udArray.Extract(3).StringValue) / 1000;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Get the description length from any part number
        /// </summary>
        /// <param name="partNumber">Part Number</param>
        /// <returns>Description Length as int</returns>
        public static double DescriptionLength(string partNumber)
        {
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(Properties.Settings.Default.ManageHostName, "query", "query", Properties.Settings.Default.WCCOManageAccount, "udcs"))
                {
                    using (UniFile uFile = uSession.CreateUniFile("IM"))
                    {
                        using (UniDynArray udArray = uFile.Read($"{partNumber}"))
                        {
                            var _temp = udArray.Extract(2).StringValue;
                            return string.IsNullOrEmpty(_temp) ? 0.00 : Convert.ToDouble(_temp.Substring(_temp.IndexOf('X') + 1, _temp.Length - _temp.IndexOf('X') - 1));
                        }
                    }
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Pull important M2k values from a lot number
        /// </summary>
        /// <param name="lotNumber">Lot Number</param>
        /// <returns>string array [part number, diamond number, material cost, UOM, work order, work center]</returns>
        public static string[] GetQIRDatafromLot(string lotNumber)
        {
            var _data = new string[6];
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(Properties.Settings.Default.ManageHostName, "query", "query", Properties.Settings.Default.WCCOManageAccount, "udcs"))
                {
                    using (UniFile uFile = uSession.CreateUniFile("LOT.MASTER"))
                    {
                        using (UniDynArray udArray = uFile.Read($"{lotNumber}|P"))
                        {
                            //Part Number
                            _data[0] = udArray.Extract(1).StringValue;
                        }
                    }
                    var i = 1;
                    var cost = 0.00;
                    using (UniFile uFile = uSession.CreateUniFile("IM"))
                    {
                        using (UniDynArray udArray = uFile.Read(_data[0]))
                        {
                            //Material Cost
                            while (i < 8)
                            {
                                if (!string.IsNullOrWhiteSpace(udArray.Extract(23, i).StringValue))
                                {
                                    cost += double.Parse(udArray.Extract(23, i).StringValue);
                                }
                                if (!string.IsNullOrWhiteSpace(udArray.Extract(29, i).StringValue))
                                {
                                    cost += double.Parse(udArray.Extract(29, i).StringValue);
                                }
                                i++;
                            }
                            _data[2] = (cost / 100000).ToString();
                            //UOM
                            if (!string.IsNullOrWhiteSpace(udArray.Extract(3).StringValue))
                            {
                                _data[3] = udArray.Extract(3).StringValue;
                            }
                        }
                    }
                    using (UniCommand udCmd = uSession.CreateUniCommand())
                    {
                        udCmd.Command = $"LIST WP WITH F168 = \"{lotNumber}|P\" F0 F166";
                        udCmd.Execute();
                        var cmdResponse = udCmd.Response.Replace("\r", "").Split('\n');
                        if (cmdResponse.Length > 0)
                        {
                            //Work Order
                            _data[4] = cmdResponse[3].Substring(0, 6);
                            //Work Center
                            _data[5] = cmdResponse[3].Substring(cmdResponse[3].Length - 6);
                        }
                        //Diamond Number
                        while (string.IsNullOrEmpty(_data[1]))
                        {
                            udCmd.Command = $"LIST LS WITH @ID LIKE '{lotNumber}...'";
                            udCmd.Execute();
                            var cmdRep = udCmd.Response.Replace("\r", "").Split('\n');
                            for (int ai = 0; ai <= cmdRep.Length; ai += 20)
                            {
                                if (cmdRep[ai].Length == 50 && string.IsNullOrEmpty(_data[1]))
                                {
                                    _data[1] = cmdRep[ai].Substring(cmdRep[ai].Length - 8, 6);
                                }
                                else if (cmdRep[ai].Length == 50 && !string.IsNullOrEmpty(_data[1]))
                                {
                                    _data[1] += $"/{cmdRep[ai].Substring(cmdRep[ai].Length - 8, 6)}";
                                }
                            }
                            if (string.IsNullOrEmpty(_data[1]))
                            {
                                if (cmdRep[0].Length == 53)
                                {
                                    lotNumber = cmdRep[0].Substring(cmdRep[0].Length - 11, 9);
                                }
                                else
                                {
                                    _data[1] = "N/A";
                                }
                            }
                        }
                    }
                }
                return _data;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Pull important M2k values from a work order number
        /// </summary>
        /// <param name="woNumber">Work Order Number</param>
        /// <returns>string array [part number, material cost, UOM, work center]</returns>
        public static string[] GetQIRDatafromWONumber(string woNumber)
        {
            var _data = new string[5];
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(Properties.Settings.Default.ManageHostName, "query", "query", Properties.Settings.Default.WCCOManageAccount, "udcs"))
                {
                    using (UniFile uFile = uSession.CreateUniFile("WP"))
                    {
                        using (UniDynArray udArray = uFile.Read(woNumber))
                        {
                            //Part Number
                            if (!string.IsNullOrWhiteSpace(udArray.Extract(1).StringValue))
                            {
                                _data[0] = udArray.Extract(1).StringValue;
                            }
                            //Work Center
                            if (!string.IsNullOrWhiteSpace(udArray.Extract(166).StringValue))
                            {
                                _data[3] = udArray.Extract(166).StringValue;
                            }
                        }
                    }
                    var i = 1;
                    var cost = 0.0;
                    using (UniFile uFile = uSession.CreateUniFile("IM"))
                    {
                        using (UniDynArray udArray = uFile.Read(_data[0]))
                        {
                            //Material Cost
                            while (i < 8)
                            {
                                if (!string.IsNullOrWhiteSpace(udArray.Extract(23, i).StringValue))
                                {
                                    cost += double.Parse(udArray.Extract(23, i).StringValue);
                                }
                                if (!string.IsNullOrWhiteSpace(udArray.Extract(29, i).StringValue))
                                {
                                    cost += double.Parse(udArray.Extract(29, i).StringValue);
                                }
                                i++;
                            }
                            _data[1] = (cost / 100000).ToString();
                            //UOM
                            if (!string.IsNullOrWhiteSpace(udArray.Extract(3).StringValue))
                            {
                                _data[2] = udArray.Extract(3).StringValue;
                            }
                        }
                    }
                }
                return _data;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Pull important M2k values from a part number
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns>string array [material cost, UOM]</returns>
        public static string[] GetQIRDatafromPartNumber(string partNumber)
        {
            var _data = new string[2];
            var i = 1;
            var cost = 0.0;
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(Properties.Settings.Default.ManageHostName, "query", "query", Properties.Settings.Default.WCCOManageAccount, "udcs"))
                {
                    using (UniFile uFile = uSession.CreateUniFile("IM"))
                    {
                        using (UniDynArray udArray = uFile.Read(partNumber))
                        {
                            //Material Cost
                            while (i < 8)
                            {
                                if (!string.IsNullOrWhiteSpace(udArray.Extract(23, i).StringValue))
                                {
                                    cost += double.Parse(udArray.Extract(23, i).StringValue);
                                }
                                if (!string.IsNullOrWhiteSpace(udArray.Extract(29, i).StringValue))
                                {
                                    cost += double.Parse(udArray.Extract(29, i).StringValue);
                                }
                                i++;
                            }
                            _data[0] = (cost / 100000).ToString();
                            //UOM
                            if (!string.IsNullOrWhiteSpace(udArray.Extract(3).StringValue))
                            {
                                _data[1] = udArray.Extract(3).StringValue;
                            }
                        }
                    }
                }
                return _data;
            }
            catch (Exception)
            {
                return null;
            }
        }

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
                using (SqlConnection con = new SqlConnection("Server=SQL-HYPERV;Integrated Security=SSPI;Database=WCCO_MAIN;Connection Timeout=10"))
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
    }
}
