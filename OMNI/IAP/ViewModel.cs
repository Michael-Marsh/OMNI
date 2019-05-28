using OMNI.Commands;
using OMNI.ViewModels;
using System.Windows.Input;
using System.Linq;

namespace OMNI.IAP
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public Summary SummaryFile { get; set; }
        public double GrandTotal { get { return SummaryFile.Payments.Where(o => o.PaymentAmount > 0).Sum(o => o.PaymentAmount); } }
        public int CheckCount { get { return SummaryFile.Payments.Count(o => o.PaymentAmount > 0); } }

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

        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
               
            }
        }
    }
}
