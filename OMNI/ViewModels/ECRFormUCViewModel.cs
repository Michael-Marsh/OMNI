using OMNI.Enumerations;
using OMNI.Models;

namespace OMNI.ViewModels
{
    /// <summary>
    /// Engineering Change Request Form UserControl ViewModel Interaction Logic
    /// </summary>
    public class ECRFormUCViewModel : ViewModelBase
    {
        #region Properties

        public ECR ecr { get; set; }
        private FormCommand commandType;
        public FormCommand CommandType
        {
            get
            { return commandType; }
            set
            { commandType = value; OnPropertyChanged(nameof(CommandType)); }
        }

        #endregion

        /// <summary>
        /// Engineering Change Request Form UserControl ViewModel Constructor
        /// </summary>
        public ECRFormUCViewModel()
        {

        }

        #region View Command Interface

        #endregion
    }
}
