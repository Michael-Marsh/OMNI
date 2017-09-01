using System.Windows.Controls;

namespace OMNI.Views
{
    /// <summary>
    /// Interaction logic for CMMSPartManagementUCView.xaml
    /// </summary>
    public partial class CMMSPartManagementUCView : UserControl
    {
        #region Properties

        public static Grid PartUserControl { get; private set; }

        #endregion

        public CMMSPartManagementUCView()
        {
            InitializeComponent();
            PartUserControl = UserControlGrid;
        }
    }
}
