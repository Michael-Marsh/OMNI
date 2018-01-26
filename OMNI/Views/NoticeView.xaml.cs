using System.Windows.Controls;

namespace OMNI.Views
{
    /// <summary>
    /// Interaction logic for NoticeView.xaml
    /// </summary>
    public partial class NoticeView : UserControl
    {
        #region Properties

        public static Grid FormGrid { get; private set; }
        public static Grid FilterGrid { get; private set; }
        public static Grid NoticeGrid { get; private set; }

        #endregion

        public NoticeView()
        {
            InitializeComponent();
            FormGrid = FormGridView;
            FilterGrid = FilterGridView;
            NoticeGrid = NoticeGridView;
        }
    }
}
