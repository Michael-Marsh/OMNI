using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Windows.Data;

namespace OMNI.ViewModels
{
    /// <summary>
    /// CMMS Notice Closed UserControl ViewModel Interaction Logic
    /// </summary>
    public class CMMSNoticeClosedUCViewModel : CMMSNoticeUCViewModelBase
    {
        #region Properties

        public override string SelectedCrewMember
        {
            get
            {
                return base.SelectedCrewMember;
            }

            set
            {
                base.SelectedCrewMember = value;
                CMMSNoticeClosedTick();
            }
        }

        #endregion

        /// <summary>
        /// CMMS Notice Closed User Control ViewModel Constructor
        /// </summary>
        public CMMSNoticeClosedUCViewModel()
        {
            Module = CMMSActionGridView.Closed;
            UpdateTimer.Add(CMMSNoticeClosedTick);
            CMMSNoticeClosedTick();
        }

        public override void CMMSNoticeClosedTick()
        {
            try
            {
                var _tempPosition = 0;
                if (OpenOrdersView != null)
                {
                    _tempPosition = OpenOrdersView.CurrentPosition;
                }
                NoticeTable = CMMSWorkOrder.LoadNoticeAsync(Convert.ToInt32(Module), SelectedCrewMember).Result;
                OpenOrdersView = CollectionViewSource.GetDefaultView(NoticeTable);
                OpenOrdersView.GroupDescriptions.Add(new PropertyGroupDescription(CurrentGroup));
                OpenOrdersView.MoveCurrentToPosition(_tempPosition);
            }
            catch (ArgumentOutOfRangeException)
            {
                OpenOrdersView.MoveCurrentToFirst();
            }
            OnPropertyChanged(nameof(OpenOrdersView));
        }
        public override void GroupExecute(object parameter)
        {
            base.GroupExecute(parameter);
            UpdateTimer.Remove(CMMSNoticeClosedTick);
        }
        public override void RefreshExecute(object parameter)
        {
            base.RefreshExecute(parameter);
            UpdateTimer.Add(CMMSNoticeClosedTick);
        }
        public override void CompleteExecute(object parameter)
        {
            base.CompleteExecute(parameter);
            CMMSNoticeClosedTick();
        }
        public override void DenyExecute(object parameter)
        {
            base.DenyExecute(parameter);
            CMMSNoticeClosedTick();
        }
    }
}
