using OMNI.QMS.Model;
using OMNI.QMS.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace OMNI.QMS.View
{
    /// <summary>
    /// Interaction logic for QIRForm.xaml
    /// </summary>
    public partial class QIRFormView : UserControl
    {
        public QIRFormView()
        {
            InitializeComponent();
        }

        private void Photo_Drop(object sender, DragEventArgs e)
        {
            if (PrimaryCommand.Content.ToString() == "Update")
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var _validateFile = true;
                foreach (var file in files)
                {
                    var ext = System.IO.Path.GetExtension(file);
                    _validateFile = !ext.Equals(".jpg", System.StringComparison.OrdinalIgnoreCase) || !ext.Equals(".jpeg", System.StringComparison.OrdinalIgnoreCase);
                }
                if (_validateFile)
                {
                    ((QIRFormViewModel)DataContext).Qir.AttachPhoto(e.Data.GetData("FileDrop"));
                }
            }
        }
    }
}
