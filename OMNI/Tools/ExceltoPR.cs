using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMNI.Tools
{
    public class ExceltoPR
    {
        /*CurrentImport = "Reading File...";
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
                var newFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}Employee Review Forms\\";
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
        }*/


    }
}
