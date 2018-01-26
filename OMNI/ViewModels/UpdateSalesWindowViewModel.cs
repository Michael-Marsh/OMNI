using OMNI.Commands;
using OMNI.Enumerations;
using OMNI.Models;
using OMNI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// Update the Monthly Sales ViewModel Interaction Logic
    /// </summary>
    public class UpdateSalesWindowViewModel : ViewModelBase
    {
        #region Properties

        public List<Month> MonthList { get; set; }
        private string selectedMonth;
        public string SelectedMonth
        {
            get { return selectedMonth; }
            set { selectedMonth = value; OnPropertyChanged(nameof(SelectedMonth)); }
        }
        public List<int> YearList { get; set; }
        private int selectedYear;
        public int SelectedYear
        {
            get { return selectedYear; }
            set { selectedYear = value; OnPropertyChanged(nameof(SelectedYear)); }
        }
        public int? SalesNumber { get; set; }
        public bool Validate { get; set; }

        RelayCommand _update;

        #endregion

        /// <summary>
        /// Update Sales ViewModel Constructor
        /// </summary>
        public UpdateSalesWindowViewModel()
        {
            if (MonthList == null)
            {
                MonthList = Enum.GetValues(typeof(Month)).Cast<Month>().ToList();
            }
            SelectedMonth = DateTime.Now.ToString("MMMM");
            if (YearList == null)
            {
                YearList = new List<int>();
                for (int i = 2016; i < DateTime.Now.Year + 2; i++)
                {
                    YearList.Add(i);
                }
            }
            SelectedYear = DateTime.Now.Year;
        }

        /// <summary>
        /// Update the Monthly Sales Command
        /// </summary>
        public ICommand UpdateCommand
        {
            get
            {
                if (_update == null)
                {
                    _update = new RelayCommand(UpdateExecute, UpdateCanExecute);
                }
                return _update;
            }
        }

        /// <summary>
        /// Update Sales Command Execution
        /// </summary>
        /// <param name="parameter">Update Sales Window</param>
        private void UpdateExecute(object parameter)
        {
            OMNIDataBase.UpdateSalesAsync(SelectedMonth, SelectedYear, (int)SalesNumber, Validate);
            var win = parameter as UpdateSalesWindowView;
            win.Close();
        }
        private bool UpdateCanExecute(object parameter) => SalesNumber == null || SalesNumber <= 0 ? false : true;

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                MonthList = null;
                YearList = null;
            }
        }
    }
}
