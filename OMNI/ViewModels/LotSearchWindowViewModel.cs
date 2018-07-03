using OMNI.Commands;
using OMNI.Helpers;
using OMNI.Models;
using OMNI.QMS.View;
using OMNI.QMS.ViewModel;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// Lot Search ViewModel
    /// </summary>
    public class LotSearchWindowViewModel : ViewModelBase
    {
        #region Properties

        public InventorySkew Skew { get; set; }

        char prevLot;
        string lot;
        public string LotNbr
        {
            get { return lot; }
            set
            {
                if (value.Length > 5)
                {
                    Skew = new InventorySkew(value);
                    PartNbr = Skew?.PartNumber;
                    OnPropertyChanged(nameof(Skew));
                    OnPropertyChanged(nameof(ValidLot));
                    OnPropertyChanged(nameof(ValidQIR));
                    OnPropertyChanged(nameof(FromVisibility));
                }
                if (value.Length == 11)
                {
                    value = lot = $"{prevLot}{value.Last()}";
                    Quantity = null;
                    ToLoc = FromLoc = NonReason = null;
                    OnPropertyChanged(nameof(LotNbr));
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(ToLoc));
                    OnPropertyChanged(nameof(FromLoc));
                    OnPropertyChanged(nameof(FromVisibility));
                }
                if (!string.IsNullOrEmpty(value))
                {
                    prevLot = value.Last();
                }
                lot = value.ToUpper();
                OnPropertyChanged(nameof(LotNbr));
            }
        }
        private string partNbr;
        public string PartNbr
        {
            get { return partNbr; }
            set
            {
                if (!string.IsNullOrEmpty(value) && value.Length > 5 && LotType)
                {
                    Skew = InventorySkew.GetSkewFromPartNrb(value);
                    OnPropertyChanged(nameof(Skew));
                    OnPropertyChanged(nameof(ValidLot));
                    OnPropertyChanged(nameof(ValidQIR));
                    OnPropertyChanged(nameof(FromVisibility));
                }
                partNbr = value;
                OnPropertyChanged(nameof(PartNbr));
            }
        }
        public int? Quantity { get; set; }
        string toLoc;
        public string ToLoc
        {
            get { return toLoc; }
            set { toLoc = value?.ToUpper(); NonReason = null; OnPropertyChanged(nameof(ToLoc)); }
        }
        string fromLoc;
        public string FromLoc
        {
            get { return fromLoc; }
            set { fromLoc = value?.ToUpper(); OnPropertyChanged(nameof(FromLoc)); }
        }
        public bool FromVisibility { get { return Skew?.OnHand?.Count > 1 || (LotType && ValidLot); } }

        public bool ValidLot { get { return Skew?.PartNumber != null; } }
        public bool ValidQIR { get { return Skew?.QIRList?.Count > 0; } }
        bool movePro;
        public bool MoveProcess
        {
            get { return movePro; }
            set { movePro = value; OnPropertyChanged(nameof(MoveProcess)); }
        }
        private bool type;
        public bool LotType
        {
            get { return type; }
            set
            {
                type = value;
                Skew = null;
                PartNbr = LotNbr = string.Empty;
                OnPropertyChanged(nameof(Skew));
                OnPropertyChanged(nameof(ValidLot));
                OnPropertyChanged(nameof(ValidQIR));
                OnPropertyChanged(nameof(LotType));
                Quantity = null;
                ToLoc = FromLoc = NonReason = null;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(ToLoc));
                OnPropertyChanged(nameof(FromLoc));
                OnPropertyChanged(nameof(FromVisibility));
            }
        }

        private string nonR;
        public string NonReason
        {
            get { return nonR; }
            set { nonR = value; OnPropertyChanged(nameof(NonReason)); }
        }

        public BindingList<InventorySkew> MoveHistory { get; set; }

        public Random uID;

        RelayCommand _openQIR;
        RelayCommand _print;
        RelayCommand _move;
        RelayCommand _refresh;

        #endregion

        /// <summary>
        /// Lot Search Window ViewModel Constructor
        /// </summary>
        public LotSearchWindowViewModel()
        {
            Skew = new InventorySkew();
            LotNbr = string.Empty;
            FromLoc = string.Empty;
            uID = new Random();
            MoveHistory = new BindingList<InventorySkew>();
            LotType = false;
        }

        #region Open QIR ICommand

        /// <summary>
        /// Open QIR Command
        /// </summary>
        public ICommand OpenQIRICommand
        {
            get
            {
                if (_openQIR == null)
                {
                    _openQIR = new RelayCommand(OpenQIRExecute, OpenQIRCanExecute);
                }
                return _openQIR;
            }
        }

        /// <summary>
        /// Open QIR Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private void OpenQIRExecute(object parameter)
        {
            var qirWindow = new QIRFormWindowView();
            if (parameter != null)
            {
                qirWindow.QIRFormGrid.Children.Add(new QIRFormView { DataContext = new QIRFormViewModel(Skew.QIRList.FirstOrDefault(o => o.IDNumber == Convert.ToInt32(parameter))) });
                qirWindow.Show();
            }
        }
        private bool OpenQIRCanExecute(object parameter) => ValidQIR;

        #endregion

        #region Print ICommand

        /// <summary>
        /// Print Travel Command
        /// </summary>
        public ICommand PrintICommand
        {
            get
            {
                if (_print == null)
                {
                    _print = new RelayCommand(PrintExecute, PrintCanExecute);
                }
                return _print;
            }
        }

        /// <summary>
        /// Print Travel Command Execution
        /// </summary>
        /// <param name="parameter">Travel Card Type as String</param>
        private void PrintExecute(object parameter)
        {
            switch(parameter.ToString())
            {
                case "T":
                    Skew.PrintTravelCard(Quantity.ToString());
                    break;
                case "R":
                    Skew.CreateReferenceCard(Quantity.ToString());
                    break;
            }
        }
        private bool PrintCanExecute(object parameter) => Quantity != null && Quantity > 0;

        #endregion

        #region Move ICommand

        /// <summary>
        /// Unplanned Move Command
        /// </summary>
        public ICommand MoveICommand
        {
            get
            {
                if (_move == null)
                {
                    _move = new RelayCommand(MoveExecute, MoveCanExecute);
                }
                return _move;
            }
        }

        /// <summary>
        /// Unplanned Move Command Execution
        /// </summary>
        /// <param name="parameter"></param>
        private void MoveExecute(object parameter)
        {
            var _suf = Skew.ErpMove(FromLoc, ToLoc, Convert.ToInt32(Quantity), LotType);
            Skew.MoveQuantity = Quantity;
            Skew.MoveFrom = string.IsNullOrEmpty(FromLoc) ? Skew.OnHand.First().Key : FromLoc;
            Skew.MoveTo = ToLoc.ToUpper();
            Skew.NonConfReason = NonReason;
            MoveHistory.Add(Skew);
            Task.Run(() => ProcessingMove(_suf, MoveHistory.Count - 1));
            Quantity = null;
            ToLoc = FromLoc = NonReason = null;
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(ToLoc));
            OnPropertyChanged(nameof(FromLoc));
        }
        private bool MoveCanExecute(object parameter)
        {
            if (ValidLot && Quantity > 0 && !string.IsNullOrEmpty(ToLoc))
            {
                if (Skew.OnHand.Count > 1 || LotType)
                {
                    return !string.IsNullOrEmpty(FromLoc) && !string.IsNullOrEmpty(NonReason);
                }
                else if (ToLoc.ToUpper()[ToLoc.Length - 1] == 'N')
                {
                    return !string.IsNullOrEmpty(NonReason) && NonReason?.Length > 5;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Refresh ICommand

        /// <summary>
        /// Refresh Command
        /// </summary>
        public ICommand RefreshICommand
        {
            get
            {
                if (_refresh == null)
                {
                    _refresh = new RelayCommand(RefreshExecute, RefreshCanExecute);
                }
                return _refresh;
            }
        }

        /// <summary>
        /// Refresh Command Execution
        /// </summary>
        /// <param name="parameter">Travel Card Type as String</param>
        private void RefreshExecute(object parameter)
        {
            LotNbr = Skew.LotNumber;
        }
        private bool RefreshCanExecute(object parameter) => ValidLot;

        #endregion

        /// <summary>
        /// View validation that the unplanned move function is working
        /// **Only use as a task delegation**
        /// </summary>
        /// <param name="uID">Unique suffix to track</param>
        /// <param name="arrayNumber">The array location of the skew to process in the MoveHistory</param>
        /// <param name="nonReason">Non-conforming reason</param>
        private void ProcessingMove(int uID, int arrayNumber)
        {
            MoveHistory[arrayNumber].MoveStatus = "In Que";
            OnPropertyChanged(nameof(MoveHistory));
            while (System.IO.File.Exists($"{Properties.Settings.Default.MoveFileLocation}LOCXFERC2K.DAT{uID}"))
            {
                MoveHistory[arrayNumber].MoveStatus = "Processing";
                OnPropertyChanged(nameof(MoveHistory));
            }
            MoveHistory[arrayNumber].MoveStatus = "Verifing Record";
            OnPropertyChanged(nameof(MoveHistory));
            System.Threading.Thread.Sleep(3000);
            if (Skew.MoveFrom[Skew.MoveFrom.Length - 1] == 'N' && Skew.MoveTo[Skew.MoveTo.Length - 1] != 'N')
            {
                MoveHistory[arrayNumber].MoveStatus = "Removing N-Loc Reason";
                OnPropertyChanged(nameof(MoveHistory));
                M2k.DeleteRecord("LOT.MASTER", 42, $"{LotNbr}|P");
            }
            else if (Skew.MoveFrom[Skew.MoveFrom.Length - 1] != 'N' && Skew.MoveTo[Skew.MoveTo.Length - 1] == 'N')
            {
                MoveHistory[arrayNumber].MoveStatus = "Adding N-Loc Reason";
                OnPropertyChanged(nameof(MoveHistory));
                M2k.ModifyRecord("LOT.MASTER", 42, Skew.NonConfReason, $"{LotNbr}|P");
            }
            MoveHistory[arrayNumber].MoveStatus = "Writing Record";
            OnPropertyChanged(nameof(MoveHistory));
            if (MoveHistory[arrayNumber].LotNumber == LotNbr)
            {
                System.Threading.Thread.Sleep(5000);
                if (MoveHistory[arrayNumber].LotNumber == LotNbr)
                {
                    LotNbr = MoveHistory[arrayNumber].LotNumber;
                }
            }
            MoveHistory[arrayNumber].MoveStatus = "Complete";
            OnPropertyChanged(nameof(MoveHistory));
        }
    }
}
