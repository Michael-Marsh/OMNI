using OMNI.Commands;
using OMNI.QMS.Model;
using OMNI.ViewModels;
using System.Windows.Input;

namespace OMNI.QMS.ViewModel
{
    public class NoticeFilterViewModel : ViewModelBase
    {
        RelayCommand _markAll;
        /// <summary>
        /// Notice Filter ViewModel Constructor
        /// </summary>
        public NoticeFilterViewModel()
        {
        }

        #region MarkAllICommand Implementation

        public ICommand MarkAllICommand
        {
            get
            {
                if (_markAll == null)
                {
                    _markAll = new RelayCommand(MarkAllExecute);
                }
                return _markAll;
            }
        }

        private void MarkAllExecute(object parameter)
        {
            QIR.MarkAllViewed();
            QIRNoticeViewModel.RefreshNotice = true;
        }

        #endregion
    }
}
