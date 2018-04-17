using iTextSharp.text.pdf;
using OMNI.Helpers;
using OMNI.QMS.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;

namespace OMNI.Models
{
    /// <summary>
    /// Inventory Skew Object
    /// </summary>
    public class InventorySkew : INotifyPropertyChanged
    {
        #region Properties

        public string LotNumber { get; set; }
        public string PartNumber { get; set; }
        public string DiamondNumber { get; set; }
        public string UOM { get; set; }
        public string Description { get; set; }
        public string WorkOrderNumber { get; set; }
        public int WorkOrderSequence { get; set; }
        public WorkCenter WorkCenter { get; set; }
        public IList<QIR> QIRList { get; set; }
        public IDictionary<string, int> OnHand { get; set; }
        public DataTable ItemsLot { get; set; }
        public DataTable LotStructure { get; set; }
        public DataTable LotHistory { get; set; }
        private string moveStatus;
        public string MoveStatus
        {
            get { return moveStatus; }
            set { moveStatus = value; OnPropertyChanged(nameof(MoveStatus)); }
        }
        public int? MoveQuantity { get; set; }
        public string MoveFrom { get; set; }
        public string MoveTo { get; set; }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        /// <summary>
        /// Inventory Skew Constructor
        /// </summary>
        public InventorySkew()
        {

        }

        /// <summary>
        /// Inventory Skew Constructor
        /// </summary>
        /// <param name="lotNbr">Lot Number to use to load the defualt object</param>
        public InventorySkew(string lotNbr)
        {
            LotNumber = lotNbr;
            MoveStatus = string.Empty;
            try
            {
                using (SqlConnection con = new SqlConnection(Properties.Settings.Default.omniMSSQLConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand($@"SELECT 
	                                                            a.[Part_Nbr], b.[Description], b.[Um], c.[Locations], c.[Oh_Qtys]
                                                            FROM 
	                                                            [dbo].[LOT-INIT] a 
                                                            LEFT JOIN 
	                                                            [dbo].[IM-INIT] b ON b.[Part_Number] = a.[Part_Nbr]
                                                            LEFT JOIN
	                                                            [dbo].[LOT-INIT_Lot_Loc_Qtys] c ON c.[ID1] = a.[Lot_Number]
                                                            WHERE 
	                                                            a.[Lot_Number] = @p1;", con))
                    {
                        cmd.Parameters.AddWithValue("p1", $"{LotNumber}|P");
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                while (dr.Read())
                                {
                                    PartNumber = !dr.IsDBNull(0) ? dr.GetString(0) : "N/A";
                                    Description = !dr.IsDBNull(1) ? dr.GetString(1) : "N/A";
                                    UOM = !dr.IsDBNull(2) ? dr.GetString(2) : "N/A";
                                    if (OnHand == null)
                                    {
                                        OnHand = new Dictionary<string, int>();
                                    }
                                    if (!dr.IsDBNull(3) || !dr.IsDBNull(4))
                                    {
                                        OnHand.Add(dr.GetString(3), Convert.ToInt32(dr.GetValue(4)));
                                    }
                                }
                            }
                        }
                    }
                    using (SqlCommand cmd = new SqlCommand($@"SELECT
	                                                            b.[ID], b.[Work_Center]
                                                            FROM
	                                                            [dbo].[WP-INIT_Lot_Entered] a
                                                            LEFT JOIN
	                                                            [dbo].[WPO-INIT] b ON b.[ID] LIKE CONCAT(a.[Wp_Nbr], '%')
                                                            WHERE
	                                                            a.[Lot_Entered] = @p1;", con))
                    {
                        cmd.Parameters.AddWithValue("p1", $"{LotNumber}|P");
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                while (dr.Read())
                                {
                                    if (!dr.IsDBNull(0))
                                    {
                                        var _tempS = dr.GetString(0).Split('*');
                                        WorkOrderNumber = _tempS[0];
                                        WorkOrderSequence = Convert.ToInt32(_tempS[1]);
                                    }
                                    else
                                    {
                                        WorkOrderNumber = "N/A";
                                        WorkOrderSequence = 0;
                                    }
                                    WorkCenter = dr.IsDBNull(1) ? new WorkCenter { IDNumber = 0, Name = "N/A" } : new WorkCenter(Convert.ToInt32(dr.GetString(1)));
                                }
                            }
                            else
                            {
                                WorkOrderNumber = "N/A";
                                WorkOrderSequence = 0;
                                WorkCenter = new WorkCenter { IDNumber = 0, Name = "N/A" };
                            }
                        }
                    }
                    var _tempDiamond = $"'{lotNbr}|P*%';";
                    var _tempDmdList = new List<string>();
                    while (DiamondNumber == null)
                    {
                        var cmdString = "SELECT DISTINCT([Component_Lot]) FROM [dbo].[Lot Structure] WHERE [Ls_ID] LIKE ";
                        if (_tempDmdList.Count == 1)
                        {
                            cmdString += $"'{_tempDmdList[0]}|P*%'";
                        }
                        else if (_tempDmdList.Count > 1)
                        {
                            _tempDiamond = string.Empty;
                            foreach (string s in _tempDmdList)
                            {
                                _tempDiamond += _tempDiamond == string.Empty ? $"'{s}|P*%'" : $" OR [Ls_ID] LIKE '{s}|P*%'";
                            }
                            cmdString += $"{_tempDiamond};";
                        }
                        else
                        {
                            cmdString += _tempDiamond;
                        }
                        using (SqlCommand cmd = new SqlCommand(cmdString, con))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        _tempDmdList.Add(reader.GetString(0).Replace("|P", "").TrimEnd()); 
                                    }
                                }
                                else
                                {
                                    _tempDmdList.Clear();
                                    DiamondNumber = "";
                                }
                            }
                            if (_tempDmdList.Count == 1)
                            {
                                DiamondNumber = _tempDmdList[0].Contains("-") ? null : _tempDmdList[0];
                            }
                            else
                            {
                                foreach (string s in _tempDmdList)
                                {
                                    DiamondNumber = s.Contains("-") ? null : DiamondNumber == null ? s : $"{DiamondNumber} / {s}";
                                }
                            }
                        }
                    }
                    if (PartNumber != null)
                    {
                        if (ItemsLot == null)
                        {
                            ItemsLot = new DataTable();
                        }
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT 
                                                                                l.[Lot_Number], o.[Oh_Qtys], o.[Loc] 
                                                                            FROM 
                                                                                [dbo].[LOT-INIT] l 
                                                                            RIGHT JOIN 
                                                                                [dbo].[LOT-INIT_Lot_Loc_Qtys] o ON o.[ID1] = l.[Lot_Number] 
                                                                            WHERE 
                                                                                l.[Part_Nbr] = '{PartNumber}' AND [Stores_Oh] != 0;", con))
                        {
                            adapter.Fill(ItemsLot);
                        }
                    }
                }
                if (QIRList == null)
                {
                    QIRList = new List<QIR>();
                }
                QIRList.Load(lotNbr);
            }
            catch (Exception ex) { ExceptionWindow.Show("Unhandled Exception", ex.Message); }
        }

        /// <summary>
        /// Get the Material Cost of an Invetory Skew
        /// </summary>
        /// <param name="partNbr">Part Number</param>
        /// <returns>Material Cost as double</returns>
        public static double GetMaterialCost(string partNbr)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Properties.Settings.Default.omniMSSQLConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand($@"SELECT 
	                                                            SUM(DISTINCT([Inc_Std_Costs])) AS [Total_Inc], SUM([Ru_Std_Costs]) AS [Total_Ru] 
                                                            FROM 
	                                                            [dbo].[IM-INIT_Std_Costs] 
                                                            WHERE 
	                                                            [Part_Nbr] = @p1;", con))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var _std = reader.IsDBNull(0) ? 0.00 : Convert.ToDouble(reader.GetDecimal(0));
                                    var _run = reader.IsDBNull(1) ? 0.00 : Convert.ToDouble(reader.GetDecimal(1));
                                    return _std + _run;
                                }
                            }
                            else
                            {
                                return 0.00;
                            }
                        }
                    }
                }
                return 0.00;
            }
            catch (Exception)
            { return 0.00; }
        }

        /// <summary>
        /// Get the unit of measurement of an Invetory Skew
        /// </summary>
        /// <param name="partNbr">Part Number</param>
        /// <returns>UOM as string</returns>
        public static string GetUOM(string partNbr)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Properties.Settings.Default.omniMSSQLConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT [Um] FROM [dbo].[IM-INIT] WHERE [Part_Number] = @p1;", con))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        return cmd.ExecuteScalar().ToString();
                    }
                }
            }
            catch (Exception)
            { return null; }
        }

        /// <summary>
        /// Get the part number and workcenter information from a work order number
        /// </summary>
        /// <param name="workOrderNbr">Work Order Number</param>
        /// <returns>Part Number as string[0] and Work Center as string[1]</returns>
        public static string[] GetItemInformation(string workOrderNbr)
        {
            var _temp = new string[2];
            try
            {
                using (SqlConnection con = new SqlConnection(Properties.Settings.Default.omniMSSQLConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand($@"SELECT 
	                                                            a.[Part_Wo_Desc], b.[Work_Center] 
                                                            FROM 
	                                                            [dbo].[WP-INIT] a 
                                                            RIGHT JOIN
	                                                            [dbo].[WPO-INIT] b ON b.ID LIKE CONCAT(a.[Wp_Nbr], '%') 
                                                            WHERE 
	                                                            a.[Wp_Nbr] = @p1;", con))
                    {
                        cmd.Parameters.AddWithValue("p1", workOrderNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _temp[0] = !reader.IsDBNull(0) ? reader.GetString(0) : null;
                                    _temp[1] = !reader.IsDBNull(1) ? reader.GetString(1) : null;
                                    return _temp;
                                }
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception)
            { return null; }
        }
    }

    /// <summary>
    /// Inventory Skew Object Extensions
    /// </summary>
    public static class InventorySkewExtensions
    {
        /// <summary>
        /// Print a Travel Card for a Inventory Skew Object
        /// </summary>
        /// <param name="_skew">Current Inventory Skew</param>
        /// <param name="qty">Skew On Hand Quantity</param>
        public static void PrintTravelCard(this InventorySkew _skew, string qty)
        {
            if (_skew != null)
            {
                try
                {
                    using (PdfReader reader = new PdfReader(Properties.Settings.Default.TravelCardDocument))
                    {
                        using (PdfStamper stamp = new PdfStamper(reader, new FileStream($"{Properties.Settings.Default.omnitemp}{_skew.LotNumber}.pdf", FileMode.Create)))
                        {
                            var pdfField = stamp.AcroFields;
                            pdfField.SetField("Date Printed", DateTime.Today.ToString("MM/dd/yyyy"));
                            pdfField.SetField("P/N", _skew.PartNumber);
                            pdfField.SetField("Part No Bar", $"*{_skew.PartNumber}*");
                            pdfField.SetField("Lot", _skew.LotNumber);
                            pdfField.SetField("Lot Bar", $"*{_skew.LotNumber}*");
                            pdfField.SetField("Description", _skew.Description);
                            pdfField.SetField("D/N", _skew.DiamondNumber);
                            pdfField.SetField("Qty", qty);
                            pdfField.SetField("UOM", _skew.UOM);
                            if (_skew.QIRList.Count > 0)
                            {
                                pdfField.SetField("QIR", _skew.QIRList[0].IDNumber.ToString());
                                pdfField.SetField("QIR Bar", $"*{_skew.QIRList[0].IDNumber}*");
                            }
                            pdfField.SetField("Operator", CurrentUser.FullName);
                            stamp.FormFlattening = false;
                        }
                    }
                    PrintForm.FromPDF($"{Properties.Settings.Default.omnitemp}{_skew.LotNumber}.pdf");
                    File.Delete($"{Properties.Settings.Default.omnitemp}{_skew.LotNumber}.pdf");
                }
                catch (Exception)
                {
                    
                }
            }
        }

        /// <summary>
        /// Create a Reference Travel Card for a Inventory Skew Object
        /// </summary>
        /// <param name="_skew">Current Inventory Skew</param>
        /// <param name="qty">Skew On Hand Quantity</param>
        public static void CreateReferenceCard(this InventorySkew _skew, string qty)
        {
            if (_skew != null)
            {
                try
                {
                    using (PdfReader reader = new PdfReader(Properties.Settings.Default.ReferenceCardDocument))
                    {
                        using (PdfStamper stamp = new PdfStamper(reader, new FileStream($"{Properties.Settings.Default.omnitemp}{_skew.LotNumber}.pdf", FileMode.Create)))
                        {
                            var pdfField = stamp.AcroFields;
                            pdfField.SetField("P/N", _skew.PartNumber);
                            pdfField.SetField("L/N", _skew.LotNumber);
                            pdfField.SetField("D/N", _skew.DiamondNumber);
                            pdfField.SetField("Qty", qty);
                            if (_skew.QIRList.Count > 0)
                            {
                                pdfField.SetField("QIR", _skew.QIRList[0].IDNumber.ToString());
                            }
                            pdfField.SetField("Operator", CurrentUser.FullName);
                            stamp.FormFlattening = false;
                        }
                    }
                    Process.Start($"{Properties.Settings.Default.omnitemp}{_skew.LotNumber}.pdf");
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
