using iTextSharp.text.pdf;
using Microsoft.Win32;
using System;
using System.Text;
using System.Windows;

namespace OMNI.Helpers
{
    public class DocumentTools
    {
        public static void ExceltoPDF()
        {
            var ExcelFilePath = string.Empty;
            var PdfFilePath = string.Empty;
            try
            {
                var ofd = new OpenFileDialog { Filter = "Excel (*.xlsx)|*.xlsx", Title = "Select the Excel Document", Multiselect = false };
                ofd.ShowDialog();
                if (ofd.FileName.Length != 0)
                {
                    ExcelFilePath = ofd.FileName;
                }
                else
                {
                    return;
                }
                ofd = new OpenFileDialog { Filter = "Adobe PDF (*.pdf)|*.pdf", Title = "Select the PDF Document", Multiselect = false };
                ofd.ShowDialog();
                if (ofd.FileName.Length != 0)
                {
                    PdfFilePath = ofd.FileName;
                }
                else
                {
                    return;
                }
                using (PdfReader reader = new PdfReader(PdfFilePath))
                {
                    var pdfField = reader.AcroFields;
                    var builder = new StringBuilder();
                    foreach (var field in pdfField.Fields)
                    {
                        MessageBox.Show($"Value = {pdfField.GetField(field.Key)}\nType = {pdfField.GetFieldType(field.Key)}\nName = {pdfField.GetTranslatedFieldName(field.Key)}");
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
