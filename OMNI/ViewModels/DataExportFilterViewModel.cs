using OMNI.Commands;
using OMNI.Helpers;
using OMNI.QMS.Model;
using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace OMNI.ViewModels
{
    public class DataExportFilterViewModel : ViewModelBase
    {
        #region Properties

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        RelayCommand _export;

        #endregion

        /// <summary>
        /// Data Export Filter ViewModel Constructor
        /// </summary>
        public DataExportFilterViewModel()
        {

        }

        #region Export ICommand Implementation

        public ICommand ExportCommand
        {
            get
            {
                if (_export == null)
                {
                    _export = new RelayCommand(ExportExecute, ExportCanExecute);
                }
                return _export;
            }
        }
        private void ExportExecute(object parameter)
        {
            var _progress = 0;
            DashBoardDataBaseSpaceViewModel.Exporting = true;
            using (DataTable table = QIR.GetTableData(StartDate, EndDate))
            {
                var totalWork = table.Columns.Count * table.Rows.Count;
                using (BackgroundWorker bw = new BackgroundWorker())
                {
                    try
                    {
                        bw.WorkerReportsProgress = true;
                        bw.ProgressChanged += new ProgressChangedEventHandler(
                            delegate (object sender, ProgressChangedEventArgs e)
                            {
                                DashBoardDataBaseSpaceViewModel.Progress = e.ProgressPercentage;
                            });
                        bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                            delegate (object sender, RunWorkerCompletedEventArgs e)
                            {
                                DashBoardDataBaseSpaceViewModel.Exporting = false;
                            });
                        bw.DoWork += new DoWorkEventHandler(
                            delegate (object sender, DoWorkEventArgs e)
                            {
                                var excelApp = new Excel.Application();
                                var workbook = excelApp.Workbooks;
                                workbook.Add();
                                Excel._Worksheet workSheet = excelApp.ActiveSheet;
                                workSheet.Name = "QIR Master";
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
                            });
                        bw.RunWorkerAsync();
                    }
                    catch (Exception ex)
                    {
                        ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                    }
                    finally
                    {

                    }
                }
            }
        }
        private bool ExportCanExecute(object parameter)
        {
            return true;
        }

        #endregion
    }
}
