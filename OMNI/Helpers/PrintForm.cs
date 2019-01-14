using Spire.Pdf;
using System;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace OMNI.Helpers
{
    /// <summary>
    /// Print Form Interaction Logic
    /// </summary>
    public sealed class PrintForm
    {
        /// <summary>
        /// Print a pdf form
        /// </summary>
        /// <param name="documentName"></param>
        public static void FromPDF(string documentName)
        {
            try
            {
                using (PdfDocument doc = new PdfDocument(documentName, "technology#1"))
                {
                    using (PrintDialog pdialog = new PrintDialog { AllowPrintToFile = true, AllowSomePages = true })
                    {
                        pdialog.PrinterSettings.MinimumPage = 1;
                        pdialog.PrinterSettings.MaximumPage = doc.Pages.Count;
                        pdialog.PrinterSettings.FromPage = 1;
                        pdialog.PrinterSettings.ToPage = doc.Pages.Count;
                        if (pdialog.ShowDialog() == DialogResult.OK)
                        {
                            doc.PrintFromPage = pdialog.PrinterSettings.FromPage;
                            doc.PrintToPage = pdialog.PrinterSettings.ToPage;
                            doc.PrinterName = pdialog.PrinterSettings.PrinterName;
                            doc.PrintDocument.PrinterSettings.Copies = pdialog.PrinterSettings.Copies;
                            using (PrintDocument pDoc = doc.PrintDocument)
                            {
                                pdialog.Document = pDoc;
                                pDoc.Print();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
            }
        }
    }
}
