using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using OMNI.ViewModels;
using System.Windows.Controls;

namespace OMNI.Views
{
    /// <summary>
    /// Interaction logic for ITFormUCView.xaml
    /// </summary>
    public partial class ITFormUCView : UserControl
    {
        public ITFormUCView(FormCommand module)
        {
            InitializeComponent();
            if (module == FormCommand.Search)
                Loaded += delegate { SearchTicket.Focus(); };
            else
                Loaded += delegate { };
        }

        private void Grid_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (!PrimaryCommand.Content.ToString().Equals("Search") || SecondaryCommand.Content.ToString().Equals("Complete"))
            {
                if (!((ITFormUCViewModel)DataContext).Ticket.DragAndDropAttachmentAsync(e.Data.GetData("FileDrop")).Result)
                {
                    ExceptionWindow.Show("File Attachment Failed", "OMNI is currently not able to process your file attachment request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact IT directly.");
                }
            }
        }
    }
}
