using OMNI.Helpers;
using OMNI.Models;
using OMNI.ViewModels;
using System.Windows.Controls;

namespace OMNI.Views
{
    /// <summary>
    /// Interaction logic for CMMSWorkOrderUCView.xaml
    /// </summary>
    public partial class CMMSWorkOrderUCView : UserControl
    {
        public CMMSWorkOrderUCView()
        {
            InitializeComponent();
            Loaded += delegate { SearchText.Focus(); };
        }

        private void Grid_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (PrimaryCommand.Content.ToString() != "Search")
            {
                var _temp = ((CMMSWorkOrderUCViewModel)DataContext).WorkOrder.DragAndDropAttach(e.Data.GetData("FileDrop"));
                if (_temp != null && _temp.Count > 0)
                {
                    foreach (var item in _temp)
                    {
                        ((CMMSWorkOrderUCViewModel)DataContext).DocumentList.Add(item);
                    }
                }
                else
                {
                    ExceptionWindow.Show("File Attachment Failed", "OMNI is currently not able to process your file attachment request.\nPlease check your connection, and try again.\nIf you need immediate assistance contact IT directly.");
                }
            }
        }
    }
}
