using OMNI.Commands;
using OMNI.QMS.View;
using OMNI.ViewModels;
using System.Windows.Input;

namespace OMNI.QMS.ViewModel
{
    /// <summary>
    /// QIR Search ViewModel Interaction Logic
    /// </summary>
    public class QIRSearchViewModel : ViewModelBase
    {
        #region Properties

        public int? QIRNumber { get; set; }

        RelayCommand _qirSearch;

        #endregion

        /// <summary>
        /// Search Command
        /// </summary>
        public ICommand SearchCommand
        {
            get
            {
                if (_qirSearch == null)
                {
                    _qirSearch = new RelayCommand(SearchExecute, SearchCanExecute);
                }
                return _qirSearch;
            }
        }

        /// <summary>
        /// Search Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private void SearchExecute(object parameter)
        {
            var qirWindow = new QIRFormWindowView();
            var qir = new QMS.Model.QIR(QIRNumber, true);
            if (qir.Found != 0)
            {
                qirWindow.QIRFormGrid.Children.Add(new QIRFormView { DataContext = new QMS.ViewModel.QIRFormViewModel(qir) });
                qirWindow.Show();
            }
        }
        private bool SearchCanExecute(object parameter) => string.IsNullOrWhiteSpace(QIRNumber.ToString())
                ? false
                : true;

        /// <summary>
        /// QIR Search ViewModel Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                _qirSearch = null;
            }
        }
    }
}
