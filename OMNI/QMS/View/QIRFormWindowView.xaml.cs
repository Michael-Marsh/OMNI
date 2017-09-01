using System.Windows;
using System.Windows.Controls;

namespace OMNI.QMS.View
{
    /// <summary>
    /// Interaction logic for QIRFormWindowView.xaml
    /// </summary>
    public partial class QIRFormWindowView : Window
    {
        public Grid QIRFormGrid { get; set; }

        public QIRFormWindowView()
        {
            InitializeComponent();
            QIRFormGrid = QIRFormWindowGrid;
        }
    }
}
