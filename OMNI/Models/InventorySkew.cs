using OMNI.Extensions;
using OMNI.Helpers;
using OMNI.QMS.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;

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
        public string NonConfReason { get; set; }

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
                using (SqlCommand cmd = new SqlCommand($@"USE [CONTI_MAIN];
                                                            SELECT 
	                                                            a.[Part_Nbr], b.[Description], b.[Um], c.[Locations], c.[Oh_Qtys]
                                                            FROM 
	                                                            [dbo].[LOT-INIT] a 
                                                            LEFT JOIN 
	                                                            [dbo].[IM-INIT] b ON b.[Part_Number] = a.[Part_Nbr]
                                                            LEFT JOIN
	                                                            [dbo].[LOT-INIT_Lot_Loc_Qtys] c ON c.[ID1] = a.[Lot_Number]
                                                            WHERE 
	                                                            a.[Lot_Number] = @p1;", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", $"{LotNumber}|P|01");
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
                using (SqlCommand cmd = new SqlCommand($@"USE [CONTI_MAIN];
                                                            SELECT
	                                                            b.[ID], b.[Work_Center]
                                                            FROM
	                                                            [dbo].[WP-INIT_Lot_Entered] a
                                                            LEFT JOIN
	                                                            [dbo].[WPO-INIT] b ON b.[ID] LIKE CONCAT(a.[Wp_Nbr], '%')
                                                            WHERE
	                                                            a.[Lot_Entered] = @p1;", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", $"{LotNumber}|P|01");
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
                //Diamond Number population
                //Requires a Lot structure crawl
                var _found = false;
                var _lot = $"a.[Parent_Lot] = '{lotNbr}|P|01'";
                while (!_found)
                {
                    _lot += ";";
                    using (SqlCommand cmd = new SqlCommand($@"USE [CONTI_MAIN];
                                                            SELECT
                                                                SUBSTRING(a.[Component_Lot],0,LEN(a.[Component_Lot]) - 1) as 'Comp_Lot', b.[Inventory_Type] as 'Type'
                                                            FROM
	                                                            [dbo].[Lot Structure] a
                                                            RIGHT OUTER JOIN
	                                                            [dbo].[IM-INIT] b ON b.[Part_Number] = a.[Comp_Pn]
                                                            WHERE
	                                                            {_lot}", App.SqlConAsync))
                    {
                        _lot = string.Empty;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (reader.SafeGetString("Comp_Lot").Contains("|P|02"))
                                    {
                                        DiamondNumber = reader.SafeGetString("Comp_Lot");
                                        _found = true;
                                    }
                                    else if (reader.SafeGetString("Type") == "RR")
                                    {
                                        if (string.IsNullOrEmpty(DiamondNumber))
                                        {
                                            DiamondNumber = reader.SafeGetString("Comp_Lot");
                                        }
                                        else
                                        {
                                            DiamondNumber += $"/{reader.SafeGetString("Comp_Lot")}";
                                        }
                                        _found = true;
                                    }
                                    else if (reader.SafeGetString("Type") != "HM")
                                    {
                                        if (string.IsNullOrEmpty(_lot))
                                        {
                                            _lot = $"a.[Parent_Lot] = '{reader.SafeGetString("Comp_Lot")}|P'";
                                        }
                                        else
                                        {
                                            _lot += $"AND a.[Parent_Lot] = '{reader.SafeGetString("Comp_Lot")}|P'";
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(_lot))
                                        {
                                            _lot = $"a.[Parent_Lot] = '{reader.SafeGetString("Comp_Lot")}'";
                                        }
                                        else
                                        {
                                            _lot += $"AND a.[Parent_Lot] = '{reader.SafeGetString("Comp_Lot")}|P'";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _found = true;
                                DiamondNumber = string.Empty;
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
                    using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE [CONTI_MAIN];
                                                                            SELECT 
                                                                                SUBSTRING(l.[Lot_Number],0,LEN(l.[Lot_Number]) - 1) as 'Lot_Number', o.[Oh_Qtys], o.[Loc] 
                                                                            FROM 
                                                                                [dbo].[LOT-INIT] l 
                                                                            RIGHT JOIN 
                                                                                [dbo].[LOT-INIT_Lot_Loc_Qtys] o ON o.[ID1] = l.[Lot_Number] 
                                                                            WHERE 
                                                                                l.[Part_Nbr] = '{PartNumber}|01' AND [Stores_Oh] != 0;", App.SqlConAsync))
                    {
                        adapter.Fill(ItemsLot);
                    }
                }
                if (QIRList == null)
                {
                    QIRList = new List<QIR>();
                }
                QIRList.Load(LotNumber);
                //IT.LOT.HIST
                LotHistory = GetLotHistoryTable(LotNumber, "LotNumber");
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
            if (!partNbr.Contains("|"))
            {
                partNbr = $"{partNbr}|01";
            }
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE CONTI_MAIN;
                                                            SELECT SUM(ISNULL([True_Inc_Std_Costs], 0.0) + ISNULL([True_Ru_Std_Costs], 0.0)) as 'PartCost'
                                                              FROM [CONTI_MAIN].[dbo].[IM-INIT_Std_Costs]
                                                            WHERE [ID1] = @p1
                                                            GROUP BY [ID1]", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", partNbr);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                return double.TryParse(reader.GetValue(0).ToString(), out double d) ? d : 0.00;
                            }
                        }
                        else
                        {
                            return 0.00;
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
                partNbr = partNbr.Contains("|") ? partNbr : $"{partNbr}|01";
                using (SqlCommand cmd = new SqlCommand($@"USE CONTI_MAIN; SELECT [Um] FROM [dbo].[IM-INIT] WHERE [Part_Number] = @p1;", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", partNbr);
                    return cmd.ExecuteScalar().ToString();
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
                using (SqlCommand cmd = new SqlCommand($@"USE CONTI_MAIN;
                                                            SELECT 
	                                                            a.[Part_Wo_Desc] as 'Desc', b.[Work_Center] as 'Machine' 
                                                            FROM 
	                                                            [dbo].[WP-INIT] a 
                                                            RIGHT JOIN
	                                                            [dbo].[WPO-INIT] b ON b.ID LIKE CONCAT(a.[Wp_Nbr], '%') 
                                                            WHERE 
	                                                            a.[Wp_Nbr] = @p1;", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", workOrderNbr);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                _temp[0] = reader.SafeGetString("Desc");
                                _temp[1] = reader.SafeGetString("Machine");
                                return _temp;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                return null;
            }
            catch (Exception)
            { return null; }
        }

        /// <summary>
        /// Gets the skew information from a part number for non-lot tracable sku's
        /// </summary>
        /// <param name="partNrb">Inputed part number</param>
        /// <returns>new InventorySkew object</returns>
        public static InventorySkew GetSkewFromPartNrb(string partNrb)
        {
            var _temp = new InventorySkew { OnHand = new Dictionary<string, int>() };
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"USE CONTI_MAIN;
                                                            SELECT
                                                                a.[Description] as 'Desc', a.[Um] 
                                                            FROM
                                                                [dbo].[IM-INIT] a
                                                            WHERE
                                                                a.[Part_Number] = @p1;", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", partNrb);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                _temp.PartNumber = partNrb;
                                _temp.Description = reader.SafeGetString("Desc");
                                _temp.UOM = reader.SafeGetString("Um");
                            }
                        }
                    }
                }
                using (SqlCommand cmd = new SqlCommand($@"USE CONTI_MAIN;
                                                            SELECT
                                                                a.[Location], a.[Oh_Qty_By_Loc] as 'OnHand' 
                                                            FROM
                                                                [dbo].[IPL-INIT_Location_Data] a
                                                            WHERE
                                                                a.[ID1] = @p1;", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", partNrb);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                _temp.OnHand.Add(reader.SafeGetString("Location"), reader.SafeGetInt32("OnHand"));
                            }
                        }
                    }
                }
                _temp.LotHistory = GetLotHistoryTable(partNrb, "PartNbr");
                return _temp;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get a DataTable of historical transactions of lots based on part or lot number
        /// </summary>
        /// <param name="searchNbr">Either Part Number or Lot Number</param>
        /// <param name="searchColumn">Column to run the search on</param>
        /// <returns>DataTable of historical lot transactions</returns>
        public static DataTable GetLotHistoryTable(string searchNbr, string searchColumn)
        {
            try
            {
                using (DataTable dt = new DataTable())
                {
                    if (!string.IsNullOrEmpty(searchNbr))
                    {

                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE [CONTI_MAIN];
                                                                            SELECT 
                                                                                *
                                                                            FROM
                                                                                [dbo].[LotHistory]
                                                                            WHERE
                                                                                [{searchColumn}] = @p1 AND (CAST([TranDateTime] as DATE) > DATEADD(YEAR, -3, GETDATE()))
                                                                            ORDER BY
                                                                                [TranDateTime] DESC;", App.SqlConAsync))
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("p1", searchNbr);
                            adapter.Fill(dt);
                        }
                    }
                    return dt;
                }
            }
            catch (Exception)
            {
                return new DataTable();
            }
        }
    }
}
