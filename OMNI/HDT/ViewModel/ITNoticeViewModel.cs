using OMNI.ViewModels;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OMNI.Views;
using OMNI.HDT.View;
using OMNI.HDT.Model;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using OMNI.Converters;

namespace OMNI.HDT.ViewModel
{
    public class ITNoticeViewModel : ViewModelBase
    {
        public DataTable NoticeDataTable { get; set; }
        private DataRowView selectedRow;
        public DataRowView SelectedRow
        {
            get { return selectedRow; }
            set
            {
                if (value != selectedRow)
                {
                    if (NoticeView.FormGrid.Children.Count > 0)
                    {
                        ((ViewModelBase)((TicketFormView)NoticeView.FormGrid.Children[0]).DataContext).Dispose();
                        NoticeView.FormGrid.Children.Clear();
                    }
                    try
                    {
                        NoticeView.FormGrid.Children.Add(new TicketFormView
                        {
                            DataContext = new TicketFormViewModel { ITTicket = new Ticket(Convert.ToInt32(selectedRow.Row.ItemArray[0])) }
                        } as UserControl);
                    }
                    catch (NullReferenceException)
                    {
                        selectedRow = value = null;
                    }
                }
                selectedRow = value;
            }
        }
        public ICollectionView NoticeCollection { get; set; }

        /// <summary>
        /// ITNoticeViewModel
        /// </summary>
        public ITNoticeViewModel()
        {
            if (NoticeDataTable == null)
            {
                NoticeDataTable = Ticket.LoadNotice();
                if (NoticeDataTable != null)
                {
                    NoticeDataTable.DefaultView.Sort = "SubmitDate DESC";
                    if (NoticeCollection == null)
                    {
                        NoticeCollection = CollectionViewSource.GetDefaultView(NoticeDataTable);
                        NoticeCollection.GroupDescriptions.Add(new PropertyGroupDescription("SubmitDate", new DateGroupConverter()));
                        SelectedRow = (DataRowView)NoticeCollection.CurrentItem;
                    }
                }
            }
        }
    }
}
