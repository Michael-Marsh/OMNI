using System.Windows;

namespace OMNI.Views
{
    /// <summary>
    /// Interaction logic for PartSearchWindowView.xaml
    /// </summary>
    public partial class PartSearchWindowView : Window
    {
        public PartSearchWindowView()
        {
            InitializeComponent();
            Loaded += delegate { PartNumber.Focus(); };
        }
    }
}
