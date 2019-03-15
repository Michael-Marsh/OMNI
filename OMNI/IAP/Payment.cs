using OMNI.Extensions;
using OMNI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace OMNI.IAP
{
    public class Payment
    {
        #region Properties

        public string RecordType { get { return "P"; } }
        public string PaymentType { get { return "CHK"; } }
        public string PaymentNumber { get { return string.Empty; } }
        public double PaymentAmount { get; set; } //need to total this when creating the list
        public string PayeeNumber { get; set; }
        public string PayeeName { get; set; }
        public string PayeeName2 { get { return string.Empty; } }
        public string PayeeAddress { get; set; }
        public string PayeeAddress2 { get; set; }
        public string PayeeAddress3 { get { return string.Empty; } }
        public string PayeeCity { get; set; }
        public string PayeeState { get; set; }
        public string PayeePostalCode { get; set; }
        public string PayeeCountryCode { get; set; }
        public string PayeeRouting { get { return string.Empty; } }
        public string PayeeAccount { get { return string.Empty; } }
        public string Memo { get { return string.Empty; } }
        public string DeliveryMethod { get { return "D"; } }
        public List<Remittance> Remittances { get; set; }

        public static string WCCOQuery
        {
            get
            {
                return $@"USE [WCCO_MAIN];
                            SELECT
	                            DISTINCT(SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))) as 'Payee_Nbr',
	                            a.[Payee_Name],
	                            CASE WHEN(SELECT COUNT([F_Addr_1]) FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID2] = 2 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))) > 0
                                    THEN(SELECT[F_Addr_1] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID2] = 2 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0)))
                                    ELSE(SELECT[F_Addr_1] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID2] = 1 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))) END as 'Payee_Addr',
	                            CASE WHEN(SELECT COUNT([F_Addr_2]) FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID2] = 2 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))) > 0
                                    THEN ISNULL((SELECT[F_Addr_2] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID2] = 2 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))), ' ')
		                            ELSE ISNULL((SELECT[F_Addr_2] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID2] = 1 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))), ' ') END as 'Payee_Addr2',
	                            CASE WHEN(SELECT COUNT([F_City]) FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID2] = 2 AND[ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))) > 0
                                    THEN(SELECT[F_City] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 2 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0)))
                                    ELSE(SELECT[F_City] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 1 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))) END as 'Payee_City',
	                            CASE WHEN(SELECT COUNT([F_State]) FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID2] = 2 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))) > 0
                                    THEN ISNULL((SELECT[F_State] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID2] = 2 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))), ' ')
		                            ELSE ISNULL((SELECT[F_State] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID2] = 1 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))), ' ') END as 'Payee_State',
	                            CASE WHEN(SELECT COUNT([F_Zip]) FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID2] = 2 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))) > 0
                                    THEN(SELECT[F_Zip] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 2 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0)))
                                    ELSE(SELECT[F_Zip] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 1 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))) END as 'Payee_Zip',
	                            CASE WHEN(SELECT COUNT([F_Country]) FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID2] = 2 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))) > 0
                                    THEN ISNULL((SELECT[F_Country] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID2] = 2 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))), 'USA')
		                            ELSE ISNULL((SELECT[F_Country] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID2] = 1 AND [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))), 'USA') END as 'Payee_Country'
                            FROM
	                            [dbo].[SI-INIT] a
                            ORDER BY a.[Payee_Name] ASC;";
            }
        }
        public static string CSIQuery
        {
            get
            {
                return $@"USE [CSI_TRAIN];
                            SELECT
                                DISTINCT([Ven_Nbr]) as 'Payee_Nbr',
                                a.[Payee_Name],
                                (SELECT [F_Addr_1] FROM [CSI_MAIN].[dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))) as 'Payee_Addr',
                                ISNULL((SELECT [F_Addr_2] FROM [CSI_MAIN].[dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))), '') as 'Payee_Addr2',
                                (SELECT [F_City] FROM [CSI_MAIN].[dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))) as 'Payee_City',
                                ISNULL((SELECT [F_State] FROM [CSI_MAIN].[dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))), '') as 'Payee_State',
                                (SELECT [F_Zip] FROM [CSI_MAIN].[dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))) as 'Payee_Zip',
                                ISNULL((SELECT [F_Country] FROM [CSI_MAIN].[dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID1] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))), 'USA') as 'Payee_Country'
                            FROM
                                [dbo].[SI-INIT] a
                            ORDER BY a.[Payee_Name] ASC;";
            }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Payment()
        { }

        /// <summary>
        /// Populates a list of payments in conjuction with selected invoices for a time period that have been preselected and approved to pay
        /// </summary>
        /// <param name="companyNbr">Company Number</param>
        /// <returns>List of payment objects that will relate with a single company summary</returns>
        public static List<Payment> GetPaymentList(int companyNbr)
        {
            var _tempList = new List<Payment>();
            var _cmdStr = string.Empty;
            switch (companyNbr)
            {
                case 1:
                    _cmdStr = WCCOQuery;
                    break;
                case 2:
                    _cmdStr = CSIQuery;
                    break;
            }
            if (App.SqlConAsync != null || App.SqlConAsync.State != ConnectionState.Closed || App.SqlConAsync.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(_cmdStr, App.SqlConAsync))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _tempList.Add(new Payment
                                    {
                                        PayeeNumber = reader.SafeGetString("Payee_Nbr"),
                                        PayeeName = reader.SafeGetString("Payee_Name"),
                                        PayeeAddress = reader.SafeGetString("Payee_Addr"),
                                        PayeeAddress2 = reader.SafeGetString("Payee_Addr2"),
                                        PayeeCity = reader.SafeGetString("Payee_City"),
                                        PayeeState = reader.SafeGetString("Payee_State"),
                                        PayeePostalCode = reader.SafeGetString("Payee_Zip"),
                                        PayeeCountryCode = reader.SafeGetString("Payee_Country"),
                                        Remittances = Remittance.GetRemittanceList(companyNbr, reader.SafeGetString("Payee_Nbr"))
                                    });
                                    _tempList[_tempList.Count - 1].PaymentAmount = _tempList[_tempList.Count - 1].Remittances.Sum(o => o.AmountPaid);
                                }
                            }
                        }
                    }

                }
                catch (SqlException sqlEx)
                {
                    ExceptionWindow.Show("SQL Exception", sqlEx.Message);
                }
                catch (Exception ex)
                {
                    ExceptionWindow.Show("Unhandled Exception", ex.Message);
                }
            }
            else
            {
                ExceptionWindow.Show("Connection Error", "A connection could not be made to pull accurate data, please contact your administrator");
            }
            return _tempList;
        }
    }
}
