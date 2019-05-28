using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OMNI.IAP
{
    public class Summary
    {
        #region Properties

        public string RecordType { get { return "S"; } }
        public string PaymentDate { get {return DateTime.Today.ToString("MM/dd/yyyy"); } }
        public string PayerName { get; set; }
        public string PayerName2 { get { return string.Empty; } }
        public string PayerAddress { get; set; }
        public string PayerAddress2 { get { return string.Empty; } }
        public string PayerCity { get; set; }
        public string PayerState { get; set; }
        public string PayerPostalCode { get; set; }
        public string PayerCounterCode { get; set; }
        public string PayerRouting { get; set; }
        public string PayerAccount { get; set; }
        public string PromoLine1 { get { return string.Empty; } }
        public string PromoLine2 { get { return string.Empty; } }
        public string PromoLine3 { get { return string.Empty; } }
        public List<Payment> Payments { get; set; }

        #endregion

        /// <summary>
        /// Summary object constructor
        /// </summary>
        /// <param name="companyNbr">Company Number</param>
        /// <param name="name">Payee Name</param>
        /// <param name="address">Payee Address</param>
        /// <param name="city">Payee City</param>
        /// <param name="state">Payee State</param>
        /// <param name="zip">Payee Zipcode</param>
        /// <param name="country">Payee Country</param>
        /// <param name="routing">Payee Routing</param>
        /// <param name="account">Payee Bank Account</param>
        public Summary(int companyNbr, string name, string address, string city, string state, string zip, string country, string routing, string account)
        {
            PayerName = name;
            PayerAddress = address;
            PayerCity = city;
            PayerState = state;
            PayerPostalCode = zip;
            PayerCounterCode = country;
            PayerRouting = routing;
            PayerAccount = account;
            Payments = Payment.GetPaymentList(companyNbr);
        }
    }

    public static class SummaryExtension
    {
        private const string CurrancyFormat = "\"{0:n}\"";
        private const string LiteralString = "\"{0}\"";

        /// <summary>
        /// Create the CSV file to upload to the specified vendor
        /// Please refer to the AP Intergration project for the excel template on what this file should include
        /// Each object that is included in the file writing process 
        /// </summary>
        /// <param name="summary">Summary object to write</param>
        public static void CreateFile(this Summary summary)
        {
            var _csvItems = new List<string>();
            foreach (var p in summary.Payments.Where(o => o.PaymentAmount > 0))
            {
                //This will add in the summary header to the CSV file
                _csvItems.Add($"\"{summary.RecordType}\",\"{summary.PaymentDate}\",\"{summary.PayerName}\",\"{summary.PayerName2}\" ,\"{summary.PayerAddress}\",\"{summary.PayerAddress2}\",\"{summary.PayerCity}\",\"{summary.PayerState}\",\"{summary.PayerPostalCode}\",\"{summary.PayerCounterCode}\",\"{summary.PayerRouting}\",\"{summary.PayerAccount}\",\"{summary.PromoLine1}\",\"{summary.PromoLine2}\",\"{summary.PromoLine3}\"");
                //This will add in the payment header to the CSV file
                _csvItems.Add($"{string.Format(LiteralString, p.RecordType)}," +
                                $"{string.Format(LiteralString, p.PaymentType)}," +
                                $"{string.Format(LiteralString, p.PaymentNumber)}," +
                                $"{string.Format(CurrancyFormat, p.PaymentAmount)}," +
                                $"{string.Format(LiteralString, p.PayeeNumber)}," +
                                $"{string.Format(LiteralString, p.PayeeName)}," +
                                $"{string.Format(LiteralString, p.PayeeName2)}," +
                                $"{string.Format(LiteralString, p.PayeeAddress)}," +
                                $"{string.Format(LiteralString, p.PayeeAddress2)}," +
                                $"{string.Format(LiteralString, p.PayeeAddress3)}," +
                                $"{string.Format(LiteralString, p.PayeeCity)}," +
                                $"{string.Format(LiteralString, p.PayeeState)}," +
                                $"{string.Format(LiteralString, p.PayeePostalCode)}," +
                                $"{string.Format(LiteralString, p.PayeeCountryCode)}," +
                                $"{string.Format(LiteralString, p.PayeeRouting)}," +
                                $"{string.Format(LiteralString, p.PayeeAccount)}," +
                                $"{string.Format(LiteralString, p.Memo)}," +
                                $"{string.Format(LiteralString, p.DeliveryMethod)}");
                //To add the remittance records to the CSV file they need to be added individually to each payment record
                foreach (var r in p.Remittances)
                {
                    _csvItems.Add($"{string.Format(LiteralString, r.RecordType)}," +
                                    $"{string.Format(LiteralString, r.InvoiceNumber)}," +
                                    $"{string.Format(LiteralString, r.InvoiceDate)}," +
                                    $"{string.Format(LiteralString, r.Description)}," +
                                    $"{string.Format(CurrancyFormat, r.InvoiceAmount)}," +
                                    $"{string.Format(CurrancyFormat, r.DiscountAmount)}," +
                                    $"{string.Format(CurrancyFormat, r.AmountPaid)}");
                }
            }
            var compName = summary.PayerName.Contains("WCCO") ? "WCCO" : "CSI";
            File.WriteAllLines($"\\\\manage2\\server\\Technology\\Projects\\Ongoing\\Account Payable Intergration\\IAP_{compName}_{DateTime.Today.ToString("ddMMMyyyy")}.csv", _csvItems);
        }
    }
}
