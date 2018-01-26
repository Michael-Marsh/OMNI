using OMNI.Commands;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// Slitter Calibration UserControl ViewModel Interaction Logic
    /// </summary>
    public class SlitterCalibrationUCViewModel : ViewModelBase
    {
        #region Properties

        public Slitter SlitterCal { get; set; }
        private string selectedMachine;
        public string SelectedMachine
        {
            get { return selectedMachine; }
            set { if (value != null) { SlitterCal.MachineID = SlitterCal.MachineNameList.IndexOf(value) + 1; } selectedMachine = value; OnPropertyChanged(nameof(SelectedMachine)); }
        }
        public bool Submitted { get; set; }

        RelayCommand _submit;

        #endregion

        /// <summary>
        /// Slitter Calibration UserControl ViewModel Constructor
        /// </summary>
        public SlitterCalibrationUCViewModel()
        {
            if (SlitterCal == null)
            {
                SlitterCal = new Slitter();
            }
            Submitted = false;
        }

        #region View Interface Commands

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
            if (!SlitterCal.SubmitAsync().Result)
            {
                ExceptionWindow.Show("Submission Interference", "OMNI is currently unavailabe to submit this calcheck to the database\nPlease contact IT for further assistance");
            }
            else
            {
                Submitted = true;
            }
        }
        private bool SubmitCanExecute(object parameter)
        {
            return !Submitted && SlitterCal.MachineID > 0 ? true : false;
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

            }
        }
    }
}
