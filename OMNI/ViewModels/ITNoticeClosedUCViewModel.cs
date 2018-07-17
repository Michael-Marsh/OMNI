using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Windows.Data;

namespace OMNI.ViewModels
{
    /// <summary>
    /// IT Notice Closed UserControl ViewModel
    /// </summary>
    public class ITNoticeClosedUCViewModel : ITNoticeUCViewModelBase
    {
        /// <summary>
        /// IT Notice Closed UserControl ViewModel
        /// </summary>
        public ITNoticeClosedUCViewModel()
        {
            Module = ITNotice.Closed;
            UpdateTimer.Add(ITNoticeClosedTick);
            ITNoticeClosedTick();
        }

        public void ITNoticeClosedTick()
        {
            if (IsLiveUpdateOn)
            {
                try
                {
                    var _position = 0;
                    if (OpenTicketsView != null)
                    {
                        _position = OpenTicketsView.CurrentPosition;
                    }
                    NoticeTable = ITTicket.GetNoticeDataTable(ITNotice.Closed, true);
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
        }
        public override void GroupExecute(object parameter)
        {
            base.GroupExecute(parameter);
            UpdateTimer.Remove(ITNoticeClosedTick);
        }
        public override void RefreshExecute(object parameter)
        {
            base.RefreshExecute(parameter);
            ITNoticeClosedTick();
            UpdateTimer.Add(ITNoticeClosedTick);
        }

        public override void OnDispose(bool disposing)
        {
            UpdateTimer.Remove(ITNoticeClosedTick);
            base.OnDispose(disposing);
        }
    }
}
