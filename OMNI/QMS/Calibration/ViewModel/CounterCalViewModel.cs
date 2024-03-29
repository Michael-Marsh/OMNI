﻿using OMNI.Commands;
using OMNI.Helpers;
using OMNI.QMS.Calibration.Model;
using OMNI.ViewModels;
using System;
using System.Windows.Input;

namespace OMNI.QMS.Calibration.ViewModel
{
    public class CounterCalViewModel : ViewModelBase
    {
        #region Properties

        private Counter counter;
        public Counter CounterCal
        {
            get { return counter; }
            set { counter = value; OnPropertyChanged(nameof(CounterCal)); }
        }
        public int SelectedMachine
        {
            get { return CounterCal != null ? CounterCal.Machine : 0; }
            set
            {
                if (CounterCal.Machine != value)
                {
                    CounterCal.CalData = CounterData.GetCounterDataList(value);
                    OnPropertyChanged(nameof(CounterCal));
                }
                CounterCal.Machine = value;
                OnPropertyChanged(nameof(SelectedMachine));
            }
        }
        public int? CalID
        {
            get { return CounterCal?.IDNumber ?? null; }
            set
            {
                if (value != null)
                {
                    CounterCal = new Counter(Convert.ToInt32(value));
                    OnPropertyChanged(nameof(SelectedMachine));
                }
                OnPropertyChanged(nameof(CalID));
            }
        }

        RelayCommand _submit;

        #endregion

        /// <summary>
        /// Counter Calibration UserControl ViewModel Constructor
        /// </summary>
        public CounterCalViewModel()
        {
            if (CounterCal == null)
            {
                CounterCal = new Counter();
            }
            CalID = CounterCal.IDNumber;
        }

        /// <summary>
        /// Counter Calibration UserControl ViewModel Constructor
        /// </summary>
        /// <param name="idNumber">ID Number of the cal log to load</param>
        public CounterCalViewModel(int idNumber)
        {
            if (CounterCal == null)
            {
                CounterCal = new Counter(idNumber);
            }
            CalID = CounterCal.IDNumber;
        }

        #region Submit ICommand

        /// <summary>
        /// Submit CalCheck or Calibration Command
        /// </summary>
        public ICommand SubmitCommand
        {
            get
            {
                if (_submit == null)
                {
                    _submit = new RelayCommand(SubmitExecute, SubmitCanExecute);
                }
                return _submit;
            }
        }

        /// <summary>
        /// Submit CalCheck or Calibration Execution
        /// </summary>
        /// <param name="parameter">typeof(FormCommand)</param>
        private void SubmitExecute(object parameter)
        {
            CalID = CounterCal.Submit(CounterCal);
            if (CalID == null)
            {
                ExceptionWindow.Show("Submission Interference", "OMNI is currently unavailabe to submit this calcheck to the database\nPlease contact IT for further assistance");
            }
        }
        private bool SubmitCanExecute(object parameter)
        {
            return CalID == 0 && SelectedMachine != 0 && CounterCal.ValidateCal(CounterCal);
        }

        #endregion

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                counter = null;
                CounterCal = null;
                _submit = null;
            }
        }
    }
}
