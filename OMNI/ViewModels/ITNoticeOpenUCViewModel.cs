using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Windows.Data;

namespace OMNI.ViewModels
{
    /// <summary>
    /// IT Notice Open UserControl ViewModel
    /// </summary>
    public class ITNoticeOpenUCViewModel : ITNoticeUCViewModelBase
    {
        #region Properties

        public override string SelectedTeamMember
        {
            get
            {
                return base.SelectedTeamMember;
            }

            set
            {
                base.SelectedTeamMember = value;
                ITNoticeOpenTick();
            }
        }

        #endregion

        /// <summary>
        /// IT Notice Open UserControl ViewModel Constructor
        /// </summary>
        public ITNoticeOpenUCViewModel()
        {
            Module = ITNotice.Open;
            UpdateTimer.Add(ITNoticeOpenTick);
            ITNoticeOpenTick();
        }

        public void ITNoticeOpenTick()
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
                    if (SelectedTeamMember != "All")
                    {
                        NoticeTable = ITTicket.GetNoticeDataTable(ITNotice.Open, false);
                        NoticeTable.DefaultView.RowFilter = $"TeamMember='{SelectedTeamMember}'";
                    }
                    else
                    {
                        NoticeTable = ITTicket.GetNoticeDataTable(ITNotice.Open, true);
                    }
                    OpenTicketsView = CollectionViewSource.GetDefaultView(NoticeTable);
                    OpenTicketsView.GroupDescriptions.Add(new PropertyGroupDescription(CurrentGroup));
                    OpenTicketsView.MoveCurrentToPosition(_position);
                }
                catch (ArgumentOutOfRangeException)
                {
                    OpenTicketsView.MoveCurrentToFirst();
                }
                catch (NullReferenceException)
                {
                    return;
                }
                OnPropertyChanged(nameof(OpenTicketsView));
            }
        }
        public override void GroupExecute(object parameter)
        {
            base.GroupExecute(parameter);
            UpdateTimer.Remove(ITNoticeOpenTick);
        }
        public override void RefreshExecute(object parameter)
        {
            base.RefreshExecute(parameter);
            ITNoticeOpenTick();
            UpdateTimer.Add(ITNoticeOpenTick);
        }
        public override void OnDispose(bool disposing)
        {
            UpdateTimer.Remove(ITNoticeOpenTick);
            base.OnDispose(disposing);
        }
    }
}
