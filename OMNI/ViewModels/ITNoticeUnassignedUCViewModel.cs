using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Windows.Data;

namespace OMNI.ViewModels
{
    /// <summary>
    /// IT Notice unassigned UserControl ViewModel
    /// </summary>
    public class ITNoticeUnassignedUCViewModel : ITNoticeUCViewModelBase
    {
        /// <summary>
        /// IT Notice Unassigned UserControl ViewModel
        /// </summary>
        public ITNoticeUnassignedUCViewModel()
        {
            Module = ITNotice.Unassigned;
            UpdateTimer.Add(ITNoticeUnassignedTick);
            ITNoticeUnassignedTick();
        }

        public void ITNoticeUnassignedTick()
        {
            try
            {
                var _position = 0;
                if (OpenTicketsView != null)
                {
                    _position = OpenTicketsView.CurrentPosition;
                }
                NoticeTable = ITTicket.GetNoticeDataTable(ITNotice.Unassigned, true);
                OpenTicketsView = CollectionViewSource.GetDefaultView(NoticeTable);
                OpenTicketsView.GroupDescriptions.Add(new PropertyGroupDescription(CurrentGroup));
                OpenTicketsView.MoveCurrentToPosition(_position);
            }
            catch (ArgumentOutOfRangeException)
            {
                OpenTicketsView.MoveCurrentToFirst();
            }
            OnPropertyChanged(nameof(OpenTicketsView));
        }
        public override void GroupExecute(object parameter)
        {
            base.GroupExecute(parameter);
            UpdateTimer.Remove(ITNoticeUnassignedTick);
        }
        public override void RefreshExecute(object parameter)
        {
            base.RefreshExecute(parameter);
            ITNoticeUnassignedTick();
            UpdateTimer.Add(ITNoticeUnassignedTick);
        }

        public override void OnDispose(bool disposing)
        {
            UpdateTimer.Remove(ITNoticeUnassignedTick);
            base.OnDispose(disposing);
        }
    }
}
