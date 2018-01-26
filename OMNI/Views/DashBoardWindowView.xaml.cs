<<<<<<< HEAD
﻿using OMNI.CustomControls;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace OMNI.Views
{
    /// <summary>
    /// Interaction logic for DashBoardWindowView.xaml
    /// </summary>
    public partial class DashBoardWindowView : Window
    {
        public static DashBoardWindowView DashBoardView { get; set; }

        public DashBoardWindowView()
        {
            InitializeComponent();
            if (DashBoardView == null)
            {
                DashBoardView = this;
            }
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((ViewModels.DashBoardActionSpaceViewModel)ActionSV.DataContext).Dispose();
            DashBoardView = null;
            if (CMMSPartManagementUCView.PartUserControl != null && CMMSPartManagementUCView.PartUserControl.Children.Count > 0 && CMMSPartManagementUCView.PartUserControl.Children[0].GetType() == typeof(CMMSPartUCView))
            {
                ((ViewModels.CMMSPartUCViewModel)((CMMSPartUCView)CMMSPartManagementUCView.PartUserControl.Children[0]).DataContext).Dispose();
            }
            DashBoardTabControl.WorkSpace = null;
            MainWindowView.MainWindow.WindowState = WindowState.Maximized;
            Helpers.UpdateTimer.UpdateTimerTick += ViewModels.MainWindowViewModel.MainWindowUpdateTick;
        }
    }
}
=======
﻿using OMNI.CustomControls;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace OMNI.Views
{
    /// <summary>
    /// Interaction logic for DashBoardWindowView.xaml
    /// </summary>
    public partial class DashBoardWindowView : Window
    {
        public static DashBoardWindowView DashBoardView { get; set; }

        public DashBoardWindowView()
        {
            InitializeComponent();
            if (DashBoardView == null)
            {
                DashBoardView = this;
            }
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((ViewModels.DashBoardActionSpaceViewModel)ActionSV.DataContext).Dispose();
            DashBoardView = null;
            if (CMMSPartManagementUCView.PartUserControl != null && CMMSPartManagementUCView.PartUserControl.Children.Count > 0 && CMMSPartManagementUCView.PartUserControl.Children[0].GetType() == typeof(CMMSPartUCView))
            {
                ((ViewModels.CMMSPartUCViewModel)((CMMSPartUCView)CMMSPartManagementUCView.PartUserControl.Children[0]).DataContext).Dispose();
            }
            DashBoardTabControl.WorkSpace = null;
            MainWindowView.MainWindow.WindowState = WindowState.Maximized;
            Helpers.UpdateTimer.UpdateTimerTick += ViewModels.MainWindowViewModel.MainWindowUpdateTick;
        }
    }
}
>>>>>>> origin/master
