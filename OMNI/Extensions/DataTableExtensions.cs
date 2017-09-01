using OMNI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;

namespace OMNI.Extensions
{
    /// <summary>
    /// DataTable Extensions Interaction Logic
    /// </summary>
    public static class DataTableExtensions
    {
        /// <summary>
        /// Search a DataTable for a value
        /// </summary>
        /// <param name="table">Source DataTable</param>
        /// <param name="query">Search string</param>
        public static void Search(this DataTable table, string query)
        {
            var columnNames = (from dc in table.Columns.Cast<DataColumn>() select dc.ColumnName).ToArray();
            var counter = 0;
            var queryBuilder = new StringBuilder();
            foreach (var name in columnNames)
            {
                if (counter == 0)
                {
                    queryBuilder.Append($"Convert(`{name}`, 'System.String') LIKE '%{query}%'");
                }
                else
                {
                    queryBuilder.Append($"OR Convert(`{name}`, 'System.String') LIKE '%{query}%'");
                }
                counter++;
            }
            table.DefaultView.RowFilter = queryBuilder.ToString();
        }

        /// <summary>
        /// Search a DataTable for a value
        /// </summary>
        /// <param name="table">Source DataTable</param>
        /// <param name="query">Search string</param>
        /// <param name="tableName">Table Name</param>
        /// <returns>Filtered DataView</returns>
        public static DataView SearchToDataView(this DataTable table, string query)
        {
            try
            {
                var columnNames = (from dc in table.Columns.Cast<DataColumn>() select dc.ColumnName).ToArray();
                var counter = 0;
                var queryBuilder = new StringBuilder();
                foreach (var name in columnNames)
                {
                    if (counter == 0)
                    {
                        queryBuilder.Append($"Convert(`{name}`, 'System.String') LIKE '%{query}%'");
                    }
                    else
                    {
                        queryBuilder.Append($"OR Convert(`{name}`, 'System.String') LIKE '%{query}%'");
                    }
                    counter++;
                }
                return new DataView(table) { RowFilter = queryBuilder.ToString() };
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// DataTable extension for deleteing rows
        /// </summary>
        /// <param name="table">DataTable</param>
        /// <param name="filter">Filter expression</param>
        /// <returns>Altered DataTable</returns>
        public static DataTable Delete(this DataTable table, string filter)
        {
            table.Select(filter).Delete();
            return table;
        }

        /// <summary>
        /// Delete function for DataTable
        /// </summary>
        /// <param name="rows">DataRows in DataTable</param>
        public static void Delete(this IEnumerable<DataRow> rows)
        {
            foreach (var row in rows)
                row.Delete();
        }

        /// <summary>
        /// Custom sorting
        /// </summary>
        /// <param name="table">DataTable</param>
        /// <param name="comp">Data row comparison</param>
        public static void Sort(this DataTable table, Comparison<DataRow> comp)
        {
            var _rowList = new List<DataRow>();
            foreach (DataRow row in table.Rows)
            {
                _rowList.Add(row);
            }
            _rowList.Sort(comp);
            table.Clear();
            table.AcceptChanges();
            foreach (DataRow row in _rowList)
            {
                table.Rows.Add(row.ItemArray);
            }
        }
    }

    /// <summary>
    /// OMNI DataTable Interaction Logic
    /// </summary>
    public class OMNIDataTable
    {
        #region Properties

        private static int _progress;
        public static int Progress
        {
            get { return _progress; }
            set { _progress = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Progress))); }
        }
        private static bool _exporting;
        public static bool Exporting
        {
            get { return _exporting; }
            set { _exporting = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Exporting))); }
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion

        /// <summary>
        /// Export a DataTable to Excel
        /// </summary>
        /// <param name="table">DataTable to export</param>
        /// <param name="worksheetName">Name of the worksheet</param>
        public static void ExportToExcel(DataTable table, string worksheetName)
        {
            var _progress = 0;
            Exporting = true;
            var totalWork = table.Columns.Count * table.Rows.Count;
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                try
                {
                    bw.WorkerReportsProgress = true;
                    bw.ProgressChanged += new ProgressChangedEventHandler(
                        delegate (object sender, ProgressChangedEventArgs e)
                        {
                            Progress = e.ProgressPercentage;
                        });
                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                        delegate (object sender, RunWorkerCompletedEventArgs e)
                        {
                            Exporting = false;
                        });
                    bw.DoWork += new DoWorkEventHandler(
                        delegate (object sender, DoWorkEventArgs e)
                        {
                            var excelApp = new Excel.Application();
                            var workbook = excelApp.Workbooks;
                            workbook.Add();
                            Excel._Worksheet workSheet = excelApp.ActiveSheet;
                            workSheet.Name = worksheetName;
                            for (int i = 0; i < table.Columns.Count; i++)
                            {
                                workSheet.Cells[1, (i + 1)] = table.Columns[i].ColumnName;
                                _progress++;
                                bw.ReportProgress((int)Math.Round((_progress / (double)totalWork) * 100, 0));
                            }
                            for (int i = 0; i < table.Rows.Count; i++)
                            {
                                for (int j = 0; j < table.Columns.Count; j++)
                                {
                                    workSheet.Cells[(i + 2), (j + 1)] = table.Rows[i][j];
                                    _progress++;
                                    bw.ReportProgress((int)Math.Round((_progress / (double)totalWork) * 100, 0));
                                }
                            }
                            excelApp.Visible = true;
                            Marshal.ReleaseComObject(workSheet);
                            Marshal.ReleaseComObject(workbook);
                            Marshal.ReleaseComObject(excelApp);
                            GC.Collect();
                        });
                    bw.RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                }
            }
        }
    }
}
