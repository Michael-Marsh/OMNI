using iTextSharp.text.pdf;
using Microsoft.Win32;
using System;

namespace OMNI.Helpers
{
    /// <summary>
    /// Map any electronic form to write to later.
    /// </summary>
    public class MapForm
    {
        /// <summary>
        /// Map a pdf from and save the results to a word document.
        /// </summary>
        public static void TypePDF()
        {
            try
            {
                var fields = string.Empty;
                var ofd = new OpenFileDialog { Title = "Map pdf Form", Filter = "Adobe PDF (*pdf)|*.pdf", DefaultExt = "*.pdf", Multiselect = true };
                ofd.ShowDialog();
                foreach (string file in ofd.FileNames)
                {
                    using (PdfReader reader = new PdfReader(file))
                    {
                        var pdfField = reader.AcroFields;
                        var builder = new System.Text.StringBuilder();
                        builder.Append(fields);
                        foreach (var field in pdfField.Fields)
                        {
                            builder.Append($"{field} FIELD VALUE: {pdfField.GetField(field.Key)} \n");
                        }
                        fields = builder.ToString();
                    }
                    //TODO: rebuild the field generator
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
            }
        }
    }
}
