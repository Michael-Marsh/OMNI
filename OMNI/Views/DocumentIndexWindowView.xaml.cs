using System.Windows;

namespace OMNI.Views
{
    /// <summary>
    /// Interaction logic for DocumentIndexWindowView.xaml
    /// </summary>
    public partial class DocumentIndexWindowView : Window
    {
        public DocumentIndexWindowView()
        {
            InitializeComponent();
            Loaded += delegate { Search.Focus(); };
        }
    }
}
