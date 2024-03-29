﻿using System;
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
using System.Threading.Tasks;
using OMNI.Interfaces;

namespace OMNI.QMS.ViewModel
{
    public class QIRNoticeViewModel : ViewModelBase, INotice
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
                    try
                    {
                        if (NoticeView.FormGrid.Children.Count > 0)
                        {
                            ((ViewModelBase)((QIRFormView)NoticeView.FormGrid.Children[0]).DataContext).Dispose();
                            NoticeView.FormGrid.Children.Clear();
                        }
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
                    catch (Exception)
                    {
                        selectedRow = value;
                    }
                }
                selectedRow = value;
            }
        }
        public ICollectionView NoticeCollection { get; set; }
        public QIRNoticeModule CurrentFilter { get; set; }
        public DispatcherTimer ReadTimer { get; private set; }
        public DispatcherTimer UpdateTimer { get; private set; }
        public static bool RefreshNotice { get; set; }
        public bool SiteFilter { get { return CurrentUser.Site == "WCCO"; } }
        public bool Loading { get; set; }

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
                Loading = true;
                RefreshNoticeView();
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
            if (RefreshNotice)
            {
                RefreshNoticeView();
            }
            else
            {
                NoticeDataTable.UpdateNoticeTable();
            }
        }

        /// <summary>
        /// Full refresh on the NoticeDataTable and the NoticeCollection
        /// </summary>
        private void RefreshNoticeView()
        {
            NoticeDataTable = null;
            NoticeCollection = null;
            NoticeDataTable = QIR.LoadNotice();
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
            OnPropertyChanged(nameof(NoticeCollection));
            RefreshNotice = false;
            Loading = false;
            OnPropertyChanged(nameof(Loading));
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
                case QIRNoticeModule.CSI:
                    NoticeDataTable.DefaultView.RowFilter = "SupplierID = 1015";
                    UpdateTimer.Stop();
                    break;
                case QIRNoticeModule.Default:
                    NoticeDataTable.DefaultView.RowFilter = "";
                    UpdateTimer.Start();
                    break;
            }
        }
        public virtual bool FilterCanExecute(object parameter) => NoticeCollection != null && App.SqlConAsync.State == ConnectionState.Open;

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
                NoticeDataTable = null;
            }
        }
    }
}
