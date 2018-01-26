using OMNI.Commands;
using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// Plate Search ViewModel Interaction Logic
    /// </summary>
    public class PlateSearchViewModel : ViewModelBase
    {
        #region Properties

        private FormCommand commandType;
        public FormCommand CommandType
        {
            get
            { return commandType; }
            set
            { commandType = value; OnPropertyChanged(nameof(CommandType)); }
        }

        private string partNumber;
        public string PartNumber
        {
            get
            { return partNumber; }
            set
            { partNumber = value; OnPropertyChanged(nameof(PartNumber)); }
        }

        public ObservableCollection<string> ExtruderList { get; set; }
        public string SelectedExtruder { get; set; }

        private bool running;
        public bool Running
        {
            get
            { return running; }
            set
            { running = value; OnPropertyChanged(nameof(Running)); }
        }
        private string loadContent;
        public string LoadContent
        {
            get
            { return loadContent; }
            set
            { loadContent = value; OnPropertyChanged(nameof(LoadContent)); }
        }
        public bool ShowResults { get { return !Running && CommandType.Equals(FormCommand.Clear); } }

        public DataTable ResultsTable { get; set; }

        RelayCommand _command;

        #endregion

        /// <summary>
        /// Plate Search Constructor
        /// </summary>
        public PlateSearchViewModel()
        {
            LoadContent = string.Empty;
            Running = false;
            CommandType = FormCommand.Search;
            Task.Run(LoadResultsTableAsync);
            if (ExtruderList == null)
            {
                ExtruderList = new ObservableCollection<string>();
                for (int i = 1; i <= 5; i++)
                {
                    ExtruderList.Add(i.ToString());
                }
            }
        }

        /// <summary>
        /// Initializes ResultsTable
        /// </summary>
        /// <returns></returns>
        public async Task LoadResultsTableAsync()
        {
            ResultsTable = await OMNIDataBase.GetExtruderPlateTableAsync().ConfigureAwait(false);
        }

        #region View Commands

        /// <summary>
        /// Plate Command
        /// </summary>
        public ICommand PlateSearchCommand
        {
            get
            {
                if (_command == null)
                {
                    _command = new RelayCommand(PlateSearchCommandExecute, PlateSearchCommandCanExecute);
                }
                return _command;
            }
        }

        /// <summary>
        /// Plate Command Execution
        /// </summary>
        /// <param name="parameter">Empty Object</param>
        private void PlateSearchCommandExecute(object parameter)
        {
            Running = true;
            OnPropertyChanged(nameof(SelectedExtruder));
            OnPropertyChanged(nameof(ShowResults));
            if (CommandType.Equals(FormCommand.Search))
            {
                CommandType = FormCommand.Clear;
                LoadContent = "Loading...";
                if (ResultsTable == null)
                {
                    while (!LoadResultsTableAsync().IsCompleted) { }
                    if (ResultsTable != null)
                    {
                        ResultsTable.DefaultView.RowFilter = $"`Part_No`={PartNumber} AND `Ext_No`='{SelectedExtruder}'";
                    }
                    else
                    {
                        ExceptionWindow.Show("Failed Initialization", "OMNI was unable to load the extruder inspection log.\nPlease contact IT for further assistance.");
                        CommandType = FormCommand.Search;
                    }
                }
                else
                {
                    ResultsTable.DefaultView.RowFilter = string.Empty;
                    OnPropertyChanged(nameof(ResultsTable));
                    ResultsTable.DefaultView.RowFilter = $"`Part_No`={PartNumber} AND `Ext_No`='{SelectedExtruder}'";
                }
                OnPropertyChanged(nameof(ResultsTable));
            }
            else
            {
                CommandType = FormCommand.Search;
            }
            Running = false;
            OnPropertyChanged(nameof(ShowResults));
            PartNumber = string.Empty;
            SelectedExtruder = string.Empty;
        }
        private bool PlateSearchCommandCanExecute(object parameter) => CommandType.Equals(FormCommand.Search) && (string.IsNullOrEmpty(PartNumber) || string.IsNullOrEmpty(SelectedExtruder))
            ? false
            : true;

        #endregion

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                ResultsTable = null;
                ExtruderList = null;
            }
        }
    }
}
