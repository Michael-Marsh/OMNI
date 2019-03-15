using OMNI.Extensions;
using OMNI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace OMNI.IAP
{
    public class Remittance
    {
        #region Properties

        public string RecordType { get { return "I"; } }
        public string InvoiceNumber { get; set; }
        public string InvoiceDate { get; set; }
        public string Description { get; set; }
        public string InvoiceAmount { get; set; }
        public string DiscountAmount { get; set; }
        public double AmountPaid { get; set; }
        public string PayeeId { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Remittance()
        {  }

        /// <summary>
        /// Populates a list of remittance objects for use in conjunction with payment records
        /// </summary>
        /// <param name="companyNbr">Company Number</param>
        /// <param name="payeeId">Payee ID Number</param>
        /// <returns>Returns a remittance list based on the payment vendor</returns>
        public static List<Remittance> GetRemittanceList(int companyNbr, string payeeId)
        {
            var _tempList = new List<Remittance>();
            var _db = string.Empty;
            switch (companyNbr)
            {
                case 1:
                    _db = "WCCO_MAIN";
                    break;
                case 2:
                    _db = "CSI_TRAIN";
                    break;
            }
            if (App.SqlConAsync != null || App.SqlConAsync.State != ConnectionState.Closed || App.SqlConAsync.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE [{_db}];
                                                                SELECT
	                                                                b.[Invoice_Nbr] as 'InvoiceNbr',
	                                                                a.[Invoice_Date],
	                                                                a.[D_esc],
	                                                                a.[Invoice_Amt],
	                                                                CASE WHEN a.[Discount_Amt1] IS NOT NULL
		                                                                THEN
			                                                                a.[Invoice_Amt] - a.[Discount_Amt1]
		                                                                ELSE
			                                                                0.00 END as 'DiscountAmt',
	                                                                CASE WHEN a.[Discount_Amt1] IS NOT NULL
		                                                                THEN
			                                                                a.[Discount_Amt1]
		                                                                ELSE
			                                                                a.[Pay_Amt] END as 'AmountPaid'
                                                                FROM
	                                                                [dbo].[AP-INIT] a
                                                                RIGHT JOIN
	                                                                [dbo].[SI-INIT] b ON b.[ID] = a.[ID]
                                                                WHERE
	                                                                a.[Payment_Vendor] = @p1;", App.SqlConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", payeeId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _tempList.Add(new Remittance
                                    {
                                        InvoiceNumber = reader.SafeGetString("InvoiceNbr"),
                                        InvoiceDate = reader.SafeGetDateTime("Invoice_Date").ToString("MM/dd/yyyy"),
                                        Description = reader.SafeGetString("D_esc"),
                                        InvoiceAmount = reader.SafeGetDouble("Invoice_Amt").ToString(),
                                        DiscountAmount = reader.SafeGetDouble("DiscountAmt").ToString(),
                                        AmountPaid = reader.SafeGetDouble("AmountPaid")
                                    });
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
