using System.Windows;

namespace OMNI.Views
{
    /// <summary>
    /// Interaction logic for NewUserView.xaml
    /// </summary>
    public partial class RegistrationWindowView : Window
    {
        public RegistrationWindowView()
        {
            InitializeComponent();
            Loaded += delegate { FullName.Focus(); };
        }
    }
}
