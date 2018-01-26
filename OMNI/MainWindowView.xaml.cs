using OMNI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace OMNI
{
    /// <summary>
    /// MainWindow View
    /// </summary>
    public partial class MainWindowView : Window
    {
        #region Properties

        public static Window MainWindow { get; set; }

        #endregion

        public MainWindowView()
        {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            InitializeComponent();
            Loaded += delegate { UserName_tbx.Focus(); };
            MainWindow = this;
        }

        private void RelayPassword(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
                ((MainWindowViewModel)DataContext).Password = ((PasswordBox)sender).Password;
        }

        private void App_Exit(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
