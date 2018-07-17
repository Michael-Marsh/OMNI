using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
                    CrewList = new ObservableCollection<Users>(Users.CMMSUserListAsync(true, addNone: false, site: value).Result);
                    CrewList.Insert(0, Users.CreateCMMSUser("All", false));
                    OnPropertyChanged(nameof(CrewList));
                    SelectedCrewMember = CrewList.Any(o => o.FullName == CurrentUser.FullName) ? CurrentUser.FullName : "All";
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
            SelectedSite = CurrentUser.Site;
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
                NoticeTable = CMMSWorkOrder.LoadNotice(Convert.ToInt32(Module), SelectedCrewMember, SelectedSite);
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

        public override void OnDispose(bool disposing)
        {
            UpdateTimer.Remove(CMMSNoticeAssignedTick);
            base.OnDispose(disposing);
        }
    }
}
