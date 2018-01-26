using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Windows.Data;

namespace OMNI.ViewModels
{
    /// <summary>
    /// CMMS Notice Open Orders UserControl ViewModel
    /// </summary>
    public class CMMSNoticeOpenOrdersUCViewModel : CMMSNoticeUCViewModelBase
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
                CMMSNoticeOpenOrdersTick();
            }
        }

        #endregion

        /// <summary>
        /// CMMS Notice Open Orders User Control ViewModel Constructor
        /// </summary>
        public CMMSNoticeOpenOrdersUCViewModel()
        {
            Module = CMMSActionGridView.OpenOrders;
            UpdateTimer.Add(CMMSNoticeOpenOrdersTick);
            CMMSNoticeOpenOrdersTick();
        }

        public override void CMMSNoticeOpenOrdersTick()
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
            UpdateTimer.Remove(CMMSNoticeOpenOrdersTick);
        }
        public override void RefreshExecute(object parameter)
        {
            base.RefreshExecute(parameter);
            UpdateTimer.Add(CMMSNoticeOpenOrdersTick);
        }
        public override void CompleteExecute(object parameter)
        {
            base.CompleteExecute(parameter);
            CMMSNoticeOpenOrdersTick();
        }
        public override void DenyExecute(object parameter)
        {
            base.DenyExecute(parameter);
            CMMSNoticeOpenOrdersTick();
        }
    }
}
