using OMNI.Extensions;
using OMNI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace OMNI.Testing
{
    public class AP_Test
    {
        private const string CurrancyFormat = "\"{0:n}\"";
        private const string LiteralString = "\"{0}\"";

        public static void RunAP()
        {
            if (App.SqlConAsync != null || App.SqlConAsync.State != ConnectionState.Closed || App.SqlConAsync.State != ConnectionState.Broken)
            {
                var _csvItems = new List<string>();
                var _db = "CSI_MAIN";
                var _cmdString = $@"USE WCCO_MAIN;
                                    SELECT
                                        'P' as 'Rec_Type',
                                        CASE WHEN APC.[Eft_Flag] = 'Y' THEN 'ACH' ELSE 'CHK' END as 'Pay_Type',
                                        SUBSTRING(APC.[ID], CHARINDEX('*', APC.[ID], 0) + 1, LEN(APC.[ID])) as 'Payment_Nbr',
                                        APC.[Gross_Amt] as 'Payment_Amt',
                                        APC.[Vendor_Nbr] as 'Payee_Nbr',
                                        APC.[Vendor_Name] as 'Payee_Name',
                                        ' ' as 'Payee_Name2',
                                        CASE WHEN(SELECT COUNT([F_Addr_1]) FROM[dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 2 AND[ID1] = APC.[Vendor_Nbr]) > 0
                                            THEN(SELECT[F_Addr_1] FROM[dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 2 AND[ID1] = APC.[Vendor_Nbr])
                                            ELSE(SELECT[F_Addr_1] FROM[dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 1 AND[ID1] = APC.[Vendor_Nbr]) END as 'Payee_Addr',
	                                    CASE WHEN(SELECT COUNT([F_Addr_2]) FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 2 AND[ID1] = APC.[Vendor_Nbr]) > 0
                                            THEN ISNULL((SELECT[F_Addr_2] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 2 AND[ID1] = APC.[Vendor_Nbr]), ' ')
		                                    ELSE ISNULL((SELECT[F_Addr_2] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 1 AND[ID1] = APC.[Vendor_Nbr]), ' ') END as 'Payee_Addr2',
	                                    ' ' as 'Payee_Addr3',
	                                    CASE WHEN(SELECT COUNT([F_City]) FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 2 AND[ID1] = APC.[Vendor_Nbr]) > 0
                                            THEN(SELECT[F_City] FROM[dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 2 AND[ID1] = APC.[Vendor_Nbr])
                                            ELSE(SELECT[F_City] FROM[dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 1 AND[ID1] = APC.[Vendor_Nbr]) END as 'Payee_City',
	                                    CASE WHEN(SELECT COUNT([F_State]) FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 2 AND[ID1] = APC.[Vendor_Nbr]) > 0
                                            THEN ISNULL((SELECT[F_State] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 2 AND[ID1] = APC.[Vendor_Nbr]), ' ')
		                                    ELSE ISNULL((SELECT[F_State] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 1 AND[ID1] = APC.[Vendor_Nbr]), ' ') END as 'Payee_State',
	                                    CASE WHEN(SELECT COUNT([F_Zip]) FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 2 AND[ID1] = APC.[Vendor_Nbr]) > 0
                                            THEN(SELECT[F_Zip] FROM[dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 2 AND[ID1] = APC.[Vendor_Nbr])
                                            ELSE(SELECT[F_Zip] FROM[dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 1 AND[ID1] = APC.[Vendor_Nbr]) END as 'Payee_Zip',
	                                    CASE WHEN(SELECT COUNT([F_Country]) FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 2 AND[ID1] = APC.[Vendor_Nbr]) > 0
                                            THEN ISNULL((SELECT[F_Country] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 2 AND[ID1] = APC.[Vendor_Nbr]), 'USA')
		                                    ELSE ISNULL((SELECT[F_Country] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE[ID2] = 1 AND[ID1] = APC.[Vendor_Nbr]), 'USA') END as 'Payee_Country',
	                                    ' ' as 'Payee_Routing',
	                                    ' ' as 'Payee_Account',
	                                    ' ' as 'Memo',
	                                    'D' AS 'Del_Method'
                                    FROM
                                        [dbo].[AP_CHECKS-INIT] as APC
                                    WHERE
                                        APC.[Vendor_Nbr] IS NOT NULL AND APC.[Vendor_Nbr] != '9999' AND APC.[Check_Date] > '2018-01-01';";
                /*var _cmdString = $@"USE CSI_MAIN;
                                    SELECT
	                                    'P' as 'Rec_Type',
	                                    CASE WHEN APC.[Eft_Flag] = 'Y' THEN 'ACH' ELSE 'CHK' END as 'Pay_Type',
	                                    SUBSTRING(APC.[ID], CHARINDEX('*',APC.[ID],0) + 1,LEN(APC.[ID])) as 'Payment_Nbr',
	                                    '' as 'Payment_Amt',
	                                    APC.[Vendor_Nbr] as 'Payee_Nbr',
	                                    APC.[Vendor_Name] as 'Payee_Name',
	                                    '' as 'Payee_Name2',
	                                    (SELECT [F_Addr_1] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID1] = APC.[Vendor_Nbr]) as 'Payee_Addr',
	                                    ISNULL((SELECT [F_Addr_2] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID1] = APC.[Vendor_Nbr]), '') as 'Payee_Addr2',
	                                    '' as 'Payee_Addr3',
	                                    (SELECT [F_City] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID1] = APC.[Vendor_Nbr]) as 'Payee_City',
	                                    ISNULL((SELECT [F_State] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID1] = APC.[Vendor_Nbr]), '') as 'Payee_State',
	                                    (SELECT [F_Zip] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID1] = APC.[Vendor_Nbr]) as 'Payee_Zip',
	                                    ISNULL((SELECT [F_Country] FROM [dbo].[VEN-INIT_Name_Addr_Info] WHERE [ID1] = APC.[Vendor_Nbr]), 'USA') as 'Payee_Country',
	                                    '' as 'Payee_Routing',
	                                    '' as 'Payee_Account',
	                                    '' as 'Memo',
	                                    'D' AS 'Del_Method'
                                    FROM
	                                    [dbo].[AP_CHECKS-INIT] as APC
                                    WHERE
	                                    APC.[Vendor_Nbr] IS NOT NULL AND APC.[Vendor_Nbr] != '9999' AND APC.[Check_Date] > '2018-01-01';";*/
                var _summary = $"S,{DateTime.Today.ToString("MM/dd/yyyy")},WCCO Belting Inc., ,P.O. Box 1205, ,Wahpeton,ND,58074,USA, , , , , ";
                //var _summary = $"S,{DateTime.Today.ToString("MM/dd/yyyy")},CSI Calendering Inc., ,P.O. Box 1206, ,Wahpeton,ND,58074,USA, , , , , ";
                try
                {
                    using (var pay_cmd = new SqlCommand(_cmdString, App.SqlConAsync))
                    {
                        using (var pay_reader = pay_cmd.ExecuteReader())
                        {
                            if (pay_reader.HasRows)
                            {
                                while (pay_reader.Read())
                                {
                                    _csvItems.Add(_summary);
                                    _csvItems.Add
                                        (
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Rec_Type"))}," +
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Pay_Type"))}," +
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Payment_Nbr"))}," +
                                            $"{string.Format(CurrancyFormat, pay_reader.SafeGetDouble("Payment_Amt"))}," +
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Payee_Nbr"))}," +
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Payee_Name"))}," +
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Payee_Name2"))}," +
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Payee_Addr"))}," +
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Payee_Addr2"))}," +
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Payee_Addr3"))}," +
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Payee_City"))}," +
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Payee_State"))}," +
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Payee_Zip"))}," +
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Payee_Country"))}," +
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Payee_Routing"))}," +
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Payee_Account"))}," +
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Memo"))}," +
                                            $"{string.Format(LiteralString, pay_reader.SafeGetString("Del_Method"))}"
                                        );
                                    using (SqlCommand rem_cmd = new SqlCommand($@"USE {_db};
                                                                                SELECT
	                                                                                'I' as 'Rec_Type',
	                                                                                SUBSTRING(b.[Invoices_Paid], CHARINDEX('*',b.[Invoices_Paid],0) + 1,LEN(b.[Invoices_Paid])) as 'Invoice_Nbr',
	                                                                                CONVERT(varchar,a.[Invoice_Date],101) as 'Invoice_Date',
	                                                                                a.[D_esc] as 'Rem_Desc',
	                                                                                a.[Invoice_Amt],
	                                                                                a.[Discount_Taken] as 'Discount_Amt',
                                                                                    a.[Net_Amt] as 'Amt_Paid'
                                                                                FROM 
	                                                                                [dbo].[PDAP-INIT] a
                                                                                RIGHT JOIN
	                                                                                [dbo].[AP_CHECKS-INIT_Invoices_Paid] b on b.[Invoices_Paid] = a.[ID]
                                                                                WHERE
	                                                                                a.[Invoice_Amt] IS NOT NULL AND a.[Check_Nbr] = @p1;", App.SqlConAsync))
                                    {
                                        rem_cmd.SafeAddParameters("p1", pay_reader.SafeGetString("Payment_Nbr"));
                                        using (var rem_reader = rem_cmd.ExecuteReader())
                                        {
                                            if (rem_reader.HasRows)
                                            {
                                                while (rem_reader.Read())
                                                {
                                                    _csvItems.Add
                                                    (
                                                        $"{string.Format(LiteralString, rem_reader.SafeGetString("Rec_Type"))}," +
                                                        $"{string.Format(LiteralString, rem_reader.SafeGetString("Invoice_Nbr"))}," +
                                                        $"{string.Format(LiteralString, rem_reader.SafeGetString("Invoice_Date"))}," +
                                                        $"{string.Format(LiteralString, rem_reader.SafeGetString("Rem_Desc"))}," +
                                                        $"{string.Format(CurrancyFormat, rem_reader.SafeGetDouble("Invoice_Amt"))}," +
                                                        $"{string.Format(CurrancyFormat, rem_reader.SafeGetDouble("Discount_Amt"))}," +
                                                        $"{string.Format(CurrancyFormat, rem_reader.SafeGetDouble("Amt_Paid"))}"
                                                    );
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    File.WriteAllLines("\\\\manage2\\server\\Technology\\Projects\\Ongoing\\Account Payable Intergration\\OMNITestWCCOrev2.csv", _csvItems);
                }
                catch (SqlException sqlEx)
                {
                    ExceptionWindow.Show("Datebase Error", sqlEx.Message);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }
    }
}
