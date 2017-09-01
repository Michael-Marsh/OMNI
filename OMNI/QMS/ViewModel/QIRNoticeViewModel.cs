using System;
using System.Data;
using OMNI.Views;
using OMNI.ViewModels;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using OMNI.Converters;
using OMNI.Commands;
using System.Windows.Input;
using System.Windows.Threading;
using OMNI.QMS.Enumeration;
using OMNI.QMS.Model;
using OMNI.Models;
using OMNI.QMS.View;

namespace OMNI.QMS.ViewModel
{
    public class QIRNoticeViewModel : ViewModelBase
    {
        #region Properties

        public DataTable NoticeDataTable { get; set; }
        private DataRowView selectedRow;
        public DataRowView SelectedRow
        {
            get { return selectedRow; }
            set
            {
                if (value != selectedRow)
                {
                    if (ReadTimer.IsEnabled)
                    {
                        ReadTimer.Stop();
                    }
                    if (NoticeView.FormGrid.Children.Count > 0)
                    {
                        ((ViewModelBase)((QIRFormView)NoticeView.FormGrid.Children[0]).DataContext).Dispose();
                        NoticeView.FormGrid.Children.Clear();
                    }
                    try
                    {
                        NoticeView.FormGrid.Children.Add(new QIRFormView
                        {
                            DataContext = new QIRFormViewModel(new QIR(Convert.ToInt32(value.Row.ItemArray[2]), false))
                        } as UserControl);
                        if (value.Row.ItemArray[0].GetType() == typeof(DBNull))
                        {
                            ReadTimer.Start();
                        }
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
        public QIRNoticeModule CurrentFilter { get; set; }
        public DispatcherTimer ReadTimer { get; private set; }
        public DispatcherTimer UpdateTimer { get; private set; }

        RelayCommand filter;

        #endregion

        /// <summary>
        /// QIR Notice ViewModel Constructor
        /// </summary>
        public QIRNoticeViewModel()
        {
            if (ReadTimer == null)
            {
                ReadTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(CurrentUser.NoticeTimer) };
                ReadTimer.Tick += ReadTimer_Tick;
            }
            if (UpdateTimer == null)
            {
                UpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(20) };
                UpdateTimer.Tick += UpdateTimer_Tick;
                UpdateTimer.Start();
            }
            if (NoticeDataTable == null)
            {
                NoticeDataTable = QIR.LoadNoticeAsync().Result;
                if (NoticeDataTable != null)
                {
                    NoticeDataTable.DefaultView.Sort = "QIRDate DESC";
                    NoticeDataTable.ColumnChanging += QIR.FlaggedColumnChanging;
                    if (NoticeCollection == null)
                    {
                        NoticeCollection = CollectionViewSource.GetDefaultView(NoticeDataTable);
                        NoticeCollection.GroupDescriptions.Add(new PropertyGroupDescription("QIRDate", new DateGroupConverter()));
                        SelectedRow = (DataRowView)NoticeCollection.CurrentItem;
                    }
                }
            }
            CurrentFilter = QIRNoticeModule.Default;
        }

        /// <summary>
        /// ReadTimer Tick Event
        /// </summary>
        /// <param name="sender">ReadTimer</param>
        /// <param name="e">ReadTimer Event Args</param>
        private void ReadTimer_Tick(object sender, EventArgs e)
        {
            if (CurrentFilter != QIRNoticeModule.Default)
            {
                NoticeDataTable.DefaultView.RowFilter += $" OR QIRNumber = {SelectedRow.Row[2]}";
            }
            SelectedRow.Row[0] = false;
            ReadTimer.Stop();
        }

        /// <summary>
        /// ReadTimer Tick Event
        /// </summary>
        /// <param name="sender">UpdateTimer</param>
        /// <param name="e">UpdateTimer Event Args</param>
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            NoticeDataTable.UpdateNoticeTable();
        }

        #region FilterICommand Implementation

        public ICommand FilterICommand
        {
            get
            {
                if (filter == null)
                {
                    filter = new RelayCommand(FilterExecute, FilterCanExecute);
                }
                return filter;
            }
        }
        private void FilterExecute(object parameter)
        {
            var _mod = (QIRNoticeModule)Enum.Parse(typeof(QIRNoticeModule), parameter.ToString());
            CurrentFilter = CurrentFilter == _mod ? QIRNoticeModule.Default : _mod;
            OnPropertyChanged(nameof(CurrentFilter));
            switch (CurrentFilter)
            {
                case QIRNoticeModule.Open:
                    NoticeDataTable.DefaultView.RowFilter = "Status = 'Open'";
                    UpdateTimer.Stop();
                    break;
                case QIRNoticeModule.New:
                    NoticeDataTable.DefaultView.RowFilter = "Flagged IS NULL";
                    UpdateTimer.Stop();
                    break;
                case QIRNoticeModule.Flagged:
                    NoticeDataTable.DefaultView.RowFilter = "Flagged = true";
                    UpdateTimer.Stop();
                    break;
                case QIRNoticeModule.Default:
                    NoticeDataTable.DefaultView.RowFilter = "";
                    UpdateTimer.Start();
                    break;
            }
        }
        public virtual bool FilterCanExecute(object parameter) => NoticeCollection != null && App.ConConnected;

        #endregion

        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                UpdateTimer.Stop();
                ReadTimer.Stop();
                NoticeCollection = null;
                NoticeDataTable?.Dispose();
            }
        }
    }
}
