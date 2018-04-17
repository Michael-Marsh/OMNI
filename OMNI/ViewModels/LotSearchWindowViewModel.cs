using OMNI.Commands;
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
                    OnPropertyChanged(nameof(Skew));
                    OnPropertyChanged(nameof(ValidLot));
                    OnPropertyChanged(nameof(ValidQIR));
                }
                if (value.Length == 11)
                {
                    value = lot = $"{prevLot}{value.Last()}";
                    Quantity = null;
                    ToLoc = FromLoc = null;
                    OnPropertyChanged(nameof(LotNbr));
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(ToLoc));
                    OnPropertyChanged(nameof(FromLoc));
                }
                if (!string.IsNullOrEmpty(value))
                {
                    prevLot = value.Last();
                }
                lot = value.ToUpper();
            }
        }
        public int? Quantity { get; set; }
        string toLoc;
        public string ToLoc
        {
            get { return toLoc; }
            set { toLoc = value?.ToUpper(); OnPropertyChanged(nameof(ToLoc)); }
        }
        string fromLoc;
        public string FromLoc
        {
            get { return fromLoc; }
            set { fromLoc = value?.ToUpper(); OnPropertyChanged(nameof(FromLoc)); }
        }

        public bool ValidLot { get { return Skew?.PartNumber != null; } }
        public bool ValidQIR { get { return Skew?.QIRList?.Count > 0; } }
        bool movePro;
        public bool MoveProcess
        {
            get { return movePro; }
            set { movePro = value; OnPropertyChanged(nameof(MoveProcess)); }
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
        /// <param name="parameter">Travel Card Type as String</param>
        private void MoveExecute(object parameter)
        {
            //String Format
            //1~Transaction type~2~Station ID~3~Transaction time~4~Transaction date~5~Facility code~6~Partnumber~7~From location~8~To location~9~Quantity #1~10~Lot #1~9~Quantity #2~10~Lot #2~~99~COMPLETE
            //Must meet this format in order to work with M2k

            FromLoc = Skew.OnHand.Count > 1 ? FromLoc.ToUpper() : Skew.OnHand.First().Key.ToUpper();
            var moveText = $"1~LOCXFER~2~{CurrentUser.DomainName}~3~{DateTime.Now.ToString("HH:mm")}~4~{DateTime.Today.ToString("MM-dd-yyyy")}~5~01~6~{Skew.PartNumber}~7~{FromLoc.ToUpper()}~8~{ToLoc}~9~{Quantity}~10~{Skew.LotNumber.ToUpper()}|P~99~COMPLETE";
            var suffix = uID.Next(128, 512);
            System.IO.File.WriteAllText($"{Properties.Settings.Default.MoveFileLocation}LOCXFERC2K.DAT{suffix}", moveText);
            Skew.MoveQuantity = Quantity;
            Skew.MoveFrom = FromLoc.ToUpper();
            Skew.MoveTo = ToLoc.ToUpper(); ;
            MoveHistory.Add(Skew);
            Task.Run(() => ProcessingMove(suffix, MoveHistory.Count - 1));
            Quantity = null;
            ToLoc = FromLoc = null;
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(ToLoc));
            OnPropertyChanged(nameof(FromLoc));
        }
        private bool MoveCanExecute(object parameter) =>
            Quantity > 0 && !string.IsNullOrEmpty(ToLoc)
                ? Skew.OnHand.Count > 1 && string.IsNullOrEmpty(FromLoc) 
                    ? false
                    : true
                : false;

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
        private void ProcessingMove(int uID, int arrayNumber)
        {
            MoveHistory[arrayNumber].MoveStatus = "Processing";
            OnPropertyChanged(nameof(MoveHistory));
            while (System.IO.File.Exists($"{Properties.Settings.Default.MoveFileLocation}LOCXFERC2K.DAT{uID}"))
            {
            }
            MoveHistory[arrayNumber].MoveStatus = "Complete";
            OnPropertyChanged(nameof(MoveHistory));
            if (MoveHistory[arrayNumber].LotNumber == LotNbr)
            {
                System.Threading.Thread.Sleep(5000);
                if (MoveHistory[arrayNumber].LotNumber == LotNbr)
                {
                    LotNbr = MoveHistory[arrayNumber].LotNumber;
                }
            }
        }
    }
}
