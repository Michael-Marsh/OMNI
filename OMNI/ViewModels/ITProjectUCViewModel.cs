using OMNI.Commands;
using OMNI.CustomControls;
using OMNI.Models;
using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Data;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// IT Project UserControl ViewModel Interaction Logic
    /// </summary>
    public class ITProjectUCViewModel : ViewModelBase
    {
        #region Properties

        public ICollectionView ProjectsView { get; set; }
        public ICollectionView ProjectTasksView { get; set; }
        private object selectedProjectRow;
        public object SelectedProjectRow
        {
            get { return selectedProjectRow; }
            set { if (selectedProjectRow != value) { UpdateProjectTaskDefualtView(value); } selectedProjectRow = value; }
        }

        RelayCommand _open;

        #endregion

        /// <summary>
        /// IT Project UserControl ViewModel Constructor
        /// </summary>
        public ITProjectUCViewModel()
        {
            if (ProjectsView == null)
            {
                ProjectsView = CollectionViewSource.GetDefaultView(ITTicket.GetProjectDataTableAsync().Result);
                ProjectsView.MoveCurrentToPosition(-1);
            }
            UpdateProjectTaskDefualtView(null);
        }

        /// <summary>
        /// Update the Project Task Table DefualtView based on selected project
        /// </summary>
        /// <param name="projectRow">Selected Project Row</param>
        public void UpdateProjectTaskDefualtView(object projectRow)
        {
            if (projectRow != null)
            {
                try
                {
                    ProjectTasksView = CollectionViewSource.GetDefaultView(ITTicket.GetProjectTaskDataTableAsync(Convert.ToInt32(((DataRowView)projectRow).Row[0])).Result);
                    ProjectTasksView.MoveCurrentToPosition(-1);
                }
                catch (IndexOutOfRangeException)
                {
                    ProjectTasksView = null;
                }
            }
        }

        #region View Command Interfaces

        /// <summary>
        /// Open IT Form Command
        /// </summary>
        public ICommand OpenCommand
        {
            get
            {
                if (_open == null)
                {
                    _open = new RelayCommand(OpenExecute);
                }
                return _open;
            }
        }

        private void OpenExecute(object parameter)
        {
            var itFormNumber = parameter != null ? Convert.ToInt32(parameter) : SelectedProjectRow != null ? Convert.ToInt32(((DataRowView)SelectedProjectRow).Row[0]) : 0;
            if (itFormNumber != 0)
            {
                DashBoardTabControl.WorkSpace.LoadITTicketTabItem(itFormNumber);
            }
        }

        #endregion
    }
}
