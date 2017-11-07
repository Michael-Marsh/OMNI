using System.Windows;

namespace OMNI.Views
{
    /// <summary>
    /// Interaction logic for DataExportFilter.xaml
    /// </summary>
    public partial class DataExportFilter : Window
    {
        public DataExportFilter()
        {
            InitializeComponent();
            Loaded += delegate { StartDatePicker.Focus(); };
        }

        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
