using System.Windows;

namespace OMNI.Views
{
    /// <summary>
    /// Interaction logic for NoteWindowView.xaml
    /// </summary>
    public partial class NoteWindowView : Window
    {
        public static NoteWindowView NoteWindow { get; set; }

        public NoteWindowView()
        {
            InitializeComponent();
            NoteWindow = this;
            Loaded += delegate { Note.Focus(); };
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NoteWindow = null;
            OMNI.ViewModels.NoteWindowViewModel.Close();
        }
    }
}
