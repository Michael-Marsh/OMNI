using System.Windows;

namespace OMNI.Views
{
    /// <summary>
    /// Interaction logic for LotSearchWindowView.xaml
    /// </summary>
    public partial class LotSearchWindowView : Window
    {
        public LotSearchWindowView()
        {
            InitializeComponent();
            Loaded += delegate { LotTextBox.Focus(); };
        }
    }
}
