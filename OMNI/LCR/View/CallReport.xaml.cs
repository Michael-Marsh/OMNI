using System.Windows.Controls;

namespace OMNI.LCR.View
{
    /// <summary>
    /// Interaction logic for CallReport.xaml
    /// </summary>
    public partial class CallReport : UserControl
    {
        public CallReport()
        {
            InitializeComponent();
            Loaded += delegate { CallDate.Focus(); };
        }
    }
}
