using OMNI.Commands;
using OMNI.ViewModels;
using System.Windows.Input;

namespace OMNI.IAP
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public Summary SummaryFile { get; set; }

        private RelayCommand _createFile;

        #endregion

        /// <summary>
        /// IAP ViewModel Constructor
        /// </summary>
        /// <param name="payerName">Name of the Payer</param>
        public ViewModel(string payerName)
        {
            switch (payerName)
            {
                case "WCCO":
                    SummaryFile = new Summary(1, "WCCO Belting Inc.", "P.O. Box 1205", "Wahpeton", "ND", "58074", "USA", "1000", "10000");
                    break;
                case "CSI":
                    SummaryFile = new Summary(2, "CSI Calendering Inc.", "P.O. Box 1206", "Wahpeton", "ND", "58074", "USA", "2000", "20000");
                    break;
            }
        }

        #region Create File ICommand

        public ICommand CreateFileICommand
        {
            get
            {
                if (_createFile == null)
                {
                    _createFile = new RelayCommand(CreateFileExecute);
                }
                return _createFile;
            }
        }

        private void CreateFileExecute(object parameter)
        {
            SummaryFile.CreateFile();
        }

        #endregion
    }
}
