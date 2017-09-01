using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Windows.Data;

namespace OMNI.ViewModels
{
    /// <summary>
    /// CMMS Notice Assigned UserControl ViewModel Interaction Logic
    /// </summary>
    public class CMMSNoticeAssignedUCViewModel : CMMSNoticeUCViewModelBase
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
                if (base.SelectedCrewMember == null)
                {
                    base.SelectedCrewMember = value;
                }
                else
                {
                    base.SelectedCrewMember = value;
                    CMMSNoticeAssignedTick();
                }
            }
        }

        #endregion

        /// <summary>
        /// CMMS Notice Assigned User Control ViewModel Constructor
        /// </summary>
        public CMMSNoticeAssignedUCViewModel()
        {
            Module = CMMSActionGridView.Assigned;
            UpdateTimer.Add(CMMSNoticeAssignedTick);
            CMMSNoticeAssignedTick();
        }

        public override void CMMSNoticeAssignedTick()
        {
            try
            {
                var _tempPosition = 0;
                if (OpenOrdersView != null)
                {
                    _tempPosition = OpenOrdersView.CurrentPosition;
                }
                NoticeTable = CMMSWorkOrder.LoadNoticeAsync(Convert.ToInt32(Module), SelectedCrewMember).Result;
                NoticeTable.DefaultView.Sort = nameof(Priority);
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
            UpdateTimer.Remove(CMMSNoticeAssignedTick);
        }
        public override void RefreshExecute(object parameter)
        {
            base.RefreshExecute(parameter);
            UpdateTimer.Add(CMMSNoticeAssignedTick);
        }
        public override void CompleteExecute(object parameter)
        {
            base.CompleteExecute(parameter);
            CMMSNoticeAssignedTick();
        }
        public override void DenyExecute(object parameter)
        {
            base.DenyExecute(parameter);
            CMMSNoticeAssignedTick();
        }
    }
}
