using System.Windows.Controls;

namespace OMNI.Views
{
    /// <summary>
    /// Interaction logic for PrivilegeManagementUCView.xaml
    /// </summary>
    public partial class OMNIManagementUCView : UserControl
    {
        public OMNIManagementUCView()
        {
            InitializeComponent();
            Loaded += delegate { Input_Text.Focus(); };
        }
    }
}
