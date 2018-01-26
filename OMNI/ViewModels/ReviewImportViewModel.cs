using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using OMNI.Models;
using OMNI.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace OMNI.ViewModels
{
    /// <summary>
    /// Employee Review Import ViewModel Logic
    /// </summary>
    public class ReviewImportViewModel : ViewModelBase
    {
        #region Properties

        private string current;
        public string CurrentImport
        {
            get { return current; }
            set { current = value; OnPropertyChanged(nameof(CurrentImport)); }
        }
        public string ExcelFilePath { get; set; }
        public int Progress { get; set; }

        public static ReviewImportWindowView RIWindow { get; set; }
        public BackgroundWorker BgWorker { get; set; }

        #endregion

        /// <summary>
        /// Employee Review Import ViewModel Constructor
        /// </summary>
        public ReviewImportViewModel()
        {
            try
            {
                var ofd = new OpenFileDialog { Filter = "Excel (*.xlsx)|*.xlsx", Title = "Select the Excel Document", Multiselect = false };
                ofd.ShowDialog();
                if (ofd.FileName.Length != 0)
                {
                    ExcelFilePath = ofd.FileName;
                    BgWorker = new BackgroundWorker { WorkerReportsProgress = true };
                    BgWorker.ProgressChanged += ImportProgressChanged;
                    BgWorker.RunWorkerCompleted += ImportCompleted;
                    BgWorker.DoWork += ImportWorking;
                    BgWorker.RunWorkerAsync();
                }
                Progress = 0;
            }
            catch (Exception)
            {
                RIWindow?.Close();
            }
        }

        #region BgWorker EventArgs

        public void ImportProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
            OnPropertyChanged(nameof(Progress));
        }

        public void ImportCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Progress = 0;
            RIWindow.Close();
        }

        public void ImportWorking(object sender, DoWorkEventArgs e)
        {
            CurrentImport = "Reading File...";
            using (SpreadsheetDocument ExcelDoc = SpreadsheetDocument.Open(ExcelFilePath, false))
            {
                var wbPart = ExcelDoc.WorkbookPart;
                var ExcelSheet = wbPart.Workbook.Descendants<Sheet>().FirstOrDefault();
                var wsPart = (WorksheetPart)wbPart.GetPartById(ExcelSheet.Id);
                var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                var empList = new List<Employee>();
                var rowCount = wsPart.Worksheet.Descendants<Row>().Count();
                BgWorker.ReportProgress(10);
                for (int i = 2; i <= rowCount; i++)
                {
                    empList.Add(new Employee
                    {
                        Name = stringTable.SharedStringTable.ElementAt(int.Parse(wsPart.Worksheet.Descendants<Cell>().First(c => c.CellReference == $"A{i}").InnerText)).InnerText,
                        ID = Convert.ToInt32(wsPart.Worksheet.Descendants<Cell>().First(c => c.CellReference == $"C{i}").InnerText),
                        StartDate = DateTime.FromOADate(double.Parse(wsPart.Worksheet.Descendants<Cell>().First(c => c.CellReference == $"D{i}").InnerText)),
                        Title = stringTable.SharedStringTable.ElementAt(int.Parse(wsPart.Worksheet.Descendants<Cell>().First(c => c.CellReference == $"K{i}").InnerText)).InnerText,
                        Department = stringTable.SharedStringTable.ElementAt(int.Parse(wsPart.Worksheet.Descendants<Cell>().First(c => c.CellReference == $"R{i}").InnerText)).InnerText
                    });
                }
                BgWorker.ReportProgress(20);
                Directory.CreateDirectory($"C:\\Users\\{CurrentUser.DomainName}\\Desktop\\Employee Review Forms\\");
                var newFilePath = $"C:\\Users\\{CurrentUser.DomainName}\\Desktop\\Employee Review Forms\\";
                foreach (Employee emp in empList)
                {
                    CurrentImport = $"Writing {emp.Name}.pdf";
                    using (PdfReader reader = new PdfReader(Properties.Settings.Default.ERDFilePath))
                    {
                        using (PdfStamper stamp = new PdfStamper(reader, new FileStream($"{newFilePath}{emp.Name}.pdf", FileMode.Create)))
                        {
                            emp.Tenure = DateTime.Today.Year - emp.StartDate.Year;
                            var pdfField = stamp.AcroFields;
                            pdfField.SetField("TextField1[0]", emp.Name);
                            pdfField.SetField("DateTimeField1[0]", emp.StartDate.ToShortDateString());
                            if (emp.Tenure == 1)
                            {
                                pdfField.SetField("TextField1[1]", $"{emp.Tenure} yr");
                            }
                            else
                            {
                                pdfField.SetField("TextField1[1]", $"{emp.Tenure} yrs");
                            }
                            pdfField.SetField("TextField1[2]", emp.Title);
                            pdfField.SetField("TextField1[3]", emp.Department);
                            pdfField.SetField("TextField1[4]", emp.ID.ToString());
                            pdfField.SetField("TextField1[5]", emp.Name);
                            pdfField.SetField("TextField1[6]", emp.Title);
                            pdfField.SetField("DateTimeField1[3]", DateTime.Today.ToShortDateString());
                            stamp.FormFlattening = false;
                        }
                    }
                    BgWorker.ReportProgress(80 / rowCount + Progress);
                    rowCount--;
                }
            }
        }

        #endregion

        /// <summary>
        /// Open Employee Review Import Window
        /// </summary>
        public static void Open()
        {
            RIWindow = new ReviewImportWindowView();
            RIWindow.Show();
        }
    }

    /// <summary>
    /// Employee Object
    /// </summary>
    public class Employee
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public int Tenure { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public int ID { get; set; }
    }
}
