using OMNI.Enumerations;
using System.Windows.Controls;

namespace OMNI.Views
{
    /// <summary>
    /// Interaction logic for ECRFormUCView.xaml
    /// </summary>
    public partial class ECRFormUCView : UserControl
    {
        public ECRFormUCView(FormCommand module)
        {
            InitializeComponent();
            if (module == FormCommand.Search)
                Loaded += delegate { SearchECR.Focus(); };
            else
                Loaded += delegate { SearchECR.Focus(); };
        }
    }
}
