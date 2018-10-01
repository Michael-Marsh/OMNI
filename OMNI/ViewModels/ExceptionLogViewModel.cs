using OMNI.Commands;
using OMNI.Extensions;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Data;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// Exception Log ViewModel Interaction Logic
    /// </summary>
    public class ExceptionLogViewModel : ViewModelBase
    {
        #region Properties

        public DataTable ExceptionTable { get; set; }
        public ICollectionView ExceptionView { get; set; }
        public object SelectedException { get; set; }

        private RelayCommand _handled;

        #endregion

        /// <summary>
        /// Exception Log ViewModel Constructor
        /// </summary>
        public ExceptionLogViewModel()
        {
            ExceptionLogTick();
            UpdateTimer.Add(ExceptionLogTick);
        }

        /// <summary>
        /// Update the Exception Table
        /// </summary>
        public void ExceptionLogTick()
        {
            var t = ExceptionView == null ? 0 : ExceptionView.CurrentPosition;
            ExceptionTable = OMNIException.UnhandledExceptionsTable();
            ExceptionView = CollectionViewSource.GetDefaultView(ExceptionTable);
            ExceptionView?.MoveCurrentToPosition(t);
            OnPropertyChanged(nameof(ExceptionView));
        }

        /// <summary>
        /// Exception Handled Command
        /// </summary>
        public ICommand HandledCommand
        {
            get
            {
                if (_handled == null)
                {
                    _handled = new RelayCommand(HandledExecute);
                }
                return _handled;
            }
        }

        /// <summary>
        /// Exception Handled Command Execution
        /// </summary>
        /// <param name="parameter">Exception Number to mark as handled</param>
        private void HandledExecute(object parameter)
        {
            OMNIException.HandleExceptionAsync(Convert.ToInt32(parameter));
            ExceptionTable.Delete($"`exceptionID`={Convert.ToInt32(parameter)}");
            ExceptionTable.AcceptChanges();
            OnPropertyChanged(nameof(ExceptionTable));
        }

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                ExceptionTable?.Dispose();
                _handled = null;
                UpdateTimer.Remove(ExceptionLogTick);
            }
        }
    }
}
