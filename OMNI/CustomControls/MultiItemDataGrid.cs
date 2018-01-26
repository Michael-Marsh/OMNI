using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace OMNI.CustomControls
{
    /// <summary>
    /// Multiple Item Selection DataGrid Interaction Logic
    /// </summary>
    public sealed class MultiItemDataGrid : DataGrid
    {
        #region Properties

        public IList SelectedItemsList
        {
            get { return (IList)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsListProperty = DependencyProperty.Register(nameof(SelectedItemsList), typeof(IList), typeof(MultiItemDataGrid), new PropertyMetadata(null));

        #endregion

        /// <summary>
        /// Multiple Item Selection DataGrid Constructor
        /// </summary>
        public MultiItemDataGrid()
        {
            SelectionChanged += MultiItemDataGrid_SelectionChanged;
        }

        /// <summary>
        /// Collects Items when selection changes
        /// </summary>
        /// <param name="sender">DataGrid Control</param>
        /// <param name="e">Selection Changed Events</param>
        void MultiItemDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItemsList = SelectedItems;
        }
    }
}
