using OMNI.Commands;
using OMNI.Models;
using OMNI.QMS.Calibration.Model;
using OMNI.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace OMNI.QMS.Calibration.ViewModel
{
    public class TapeMeasureCalViewModel : ViewModelBase
    {
        #region Properties

        public TapeMeasure Tape { get; set; }

        private int? _tempId;
        public int? TempID
        {
            get
            { return _tempId; }
            set
            {
                if (value != null)
                {
                    Tape = new TapeMeasure(Convert.ToInt32(value));
                    PrimaryICommandUpdate();
                }
                _tempId = value;
                OnPropertyChanged(nameof(TempID));
            }
        }

        public ObservableCollection<string> DescriptionCollection { get; set; }
        public string SelectedDescription
        {
            get
            { return Tape.Description; }
            set
            { Tape.Description = value; OnPropertyChanged(nameof(SelectedDescription)); }
        }

        public ObservableCollection<WorkCenter> WorkCenterCollection { get; set; }
        public int SelectedWorkCenter
        {
            get
            { return Tape.CurrentRevision.Workcenter; }
            set
            { Tape.CurrentRevision.Workcenter = value; OnPropertyChanged(nameof(SelectedDescription)); }
        }

        public ObservableCollection<string> CalStatusCollection { get; set; }
        public string SelectedCalStatus
        {
            get
            { return Tape.CurrentRevision.Status.ToString(); }
            set
            { Tape.CurrentRevision.Status = (CalStatus)Enum.Parse(typeof(CalStatus), value); OnPropertyChanged(nameof(SelectedDescription)); }
        }

        public ObservableCollection<TapeMeasureData> RevisionCollection { get; set; }
        public TapeMeasureData SelectedRevision
        {
            get
            { return Tape.CurrentRevision; }
            set
            { Tape.CurrentRevision = value; OnPropertyChanged(nameof(SelectedRevision)); OnPropertyChanged(nameof(Tape)); }
        }

        private string _priContent;
        public string PrimaryContent
        {
            get
            { return _priContent; }
            set
            {
                _priContent = value;
                OnPropertyChanged(nameof(PrimaryContent));
            }
        }

        public string PrimaryToolTip { get { return PrimaryContent == "Submit" ? "Submit tape measure data to the database." : "Update this tape measure in the database."; } }

        RelayCommand _primary;
        RelayCommand _cal;

        #endregion

        /// <summary>
        /// Tape Measure Calibration ViewModel default constructor
        /// </summary>
        public TapeMeasureCalViewModel()
        {
            if (Tape == null)
            {
                Tape = new TapeMeasure();
            }
            if (DescriptionCollection == null)
            {
                DescriptionCollection = new ObservableCollection<string>(TapeMeasureData.GetDescriptionList());
            }
            //TODO: need to change the database to reflect the workcenter changes
            if (WorkCenterCollection == null)
            {
                WorkCenterCollection = new ObservableCollection<WorkCenter>(WorkCenter.GetListAsync(Enumerations.WorkCenterType.QMSCal).Result);
            }
            if (CalStatusCollection == null)
            {
                CalStatusCollection = new ObservableCollection<string>(Enum.GetNames(typeof(CalStatus)));
            }
            if (RevisionCollection == null)
            {
                RevisionCollection = new ObservableCollection<TapeMeasureData>();
            }
            PrimaryContent = "Submit";
        }

        /// <summary>
        /// Updates all the view portions of the primary command
        /// </summary>
        public void PrimaryICommandUpdate()
        {
            PrimaryContent = Tape.IDNumber == null ? "Submit" : "Update";
            OnPropertyChanged(nameof(PrimaryToolTip));
            SelectedDescription = Tape.Description;
            SelectedWorkCenter = Tape.CurrentRevision.Workcenter;
            RevisionCollection = new ObservableCollection<TapeMeasureData>(Tape.TapeDataList);
            OnPropertyChanged(nameof(RevisionCollection));
            SelectedRevision = RevisionCollection.Count > 0 ? RevisionCollection[0] : new TapeMeasureData();
            SelectedCalStatus = Tape.CurrentRevision.Status.ToString();
            OnPropertyChanged(nameof(Tape));
        }

        #region Primary ICommand

            /// <summary>
            /// Primary Form Command
            /// </summary>
        public ICommand PrimaryICommand
        {
            get
            {
                if (_primary == null)
                {
                    _primary = new RelayCommand(PrimaryExecute, PrimaryCanExecute);
                }
                return _primary;
            }
        }

        /// <summary>
        /// Primary Form Execution
        /// </summary>
        /// <param name="parameter">typeof(FormCommand)</param>
        private void PrimaryExecute(object parameter)
        {
            switch(PrimaryContent)
            {
                case "Submit":
                    Tape.IDNumber = Tape.Submit(Tape);
                    OnPropertyChanged(nameof(Tape));
                    RevisionCollection.Add(Tape.CurrentRevision);
                    OnPropertyChanged(nameof(RevisionCollection));
                    SelectedRevision = RevisionCollection[RevisionCollection.Count - 1];
                    break;
                case "Update":
                    Tape.Update(Tape);
                    OnPropertyChanged(nameof(Tape));
                    RevisionCollection.Add(Tape.CurrentRevision);
                    OnPropertyChanged(nameof(RevisionCollection));
                    SelectedRevision = RevisionCollection[RevisionCollection.Count - 1];
                    break;
            }
        }
        private bool PrimaryCanExecute(object parameter)
        {
            switch (PrimaryContent)
            {
                case "Submit":
                    return Tape.Description != null && Tape.CurrentRevision.Workcenter > 0 && Tape.CurrentRevision.InstrumentID > 0;
                case "Update":
                    return Tape.IDNumber != null && Tape.IDNumber > 0;
                default:
                    return false;
            }
        }

        #endregion

        #region Calibration ICommand

        /// <summary>
        /// Primary Form Command
        /// </summary>
        public ICommand CalICommand
        {
            get
            {
                if (_cal == null)
                {
                    _cal = new RelayCommand(CalExecute, CalCanExecute);
                }
                return _cal;
            }
        }

        /// <summary>
        /// Primary Form Execution
        /// </summary>
        /// <param name="parameter">typeof(FormCommand)</param>
        private void CalExecute(object parameter)
        {
            
        }
        private bool CalCanExecute(object parameter) => Tape.IDNumber != null && Tape.IDNumber > 0;

        #endregion
    }
}
