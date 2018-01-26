using System.Windows;

namespace OMNI.Views
{
    /// <summary>
    /// Interaction logic for PlateSearchWindowView.xaml
    /// </summary>
    public partial class PlateSearchWindowView : Window
    {
        public PlateSearchWindowView()
        {
            InitializeComponent();
            Loaded += delegate { PartNumber.Focus(); };
        }
    }
}
