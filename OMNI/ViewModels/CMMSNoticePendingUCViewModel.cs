using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Windows.Data;

namespace OMNI.ViewModels
{
    /// <summary>
    /// CMMS Notice Pending UserControl ViewModel Interaction Logic
    /// </summary>
    public class CMMSNoticePendingUCViewModel : CMMSNoticeUCViewModelBase
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
                    CMMSNoticePendingTick();
                }
            }
        }
        public override string SelectedSite
        {
            get { return base.SelectedSite; }
            set
            {
                if (base.SelectedSite == null)
                {
                    base.SelectedSite = value;
                }
                else
                {
                    base.SelectedSite = value;
                    CMMSNoticePendingTick();
                }
            }
        }

        #endregion

        /// <summary>
        /// CMMS Notice Pending User Control ViewModel Constructor
        /// </summary>
        public CMMSNoticePendingUCViewModel()
        {
            Module = CMMSActionGridView.Pending;
            SelectedSite = CurrentUser.Site;
            UpdateTimer.Add(CMMSNoticePendingTick);
            CMMSNoticePendingTick();
        }

        public override void CMMSNoticePendingTick()
        {
            try
            {
                var _tempPosition = 0;
                if (OpenOrdersView != null)
                {
                    _tempPosition = OpenOrdersView.CurrentPosition;
                }
                NoticeTable = CMMSWorkOrder.LoadNotice(Convert.ToInt32(Module), SelectedCrewMember, SelectedSite);
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
            UpdateTimer.Remove(CMMSNoticePendingTick);
        }
        public override void RefreshExecute(object parameter)
        {
            base.RefreshExecute(parameter);
            UpdateTimer.Add(CMMSNoticePendingTick);
        }
        public override void CompleteExecute(object parameter)
        {
            base.CompleteExecute(parameter);
            CMMSNoticePendingTick();
        }
        public override void DenyExecute(object parameter)
        {
            base.DenyExecute(parameter);
            CMMSNoticePendingTick();
        }

        public override void OnDispose(bool disposing)
        {
            UpdateTimer.Remove(CMMSNoticePendingTick);
            base.OnDispose(disposing);
        }
    }
}
