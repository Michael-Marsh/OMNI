using OMNI.LCR.Enumeration;
using OMNI.LCR.Model;
using OMNI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMNI.LCR.ViewModel
{
    public class CallReportVM : ViewModelBase
    {
        #region Properties

        public LoggedCall LogCall { get; set; }
        public ObservableCollection<string> BTypeCollection { get; set; }
        public string SelectedBType
        {
            get { return LogCall.BusType.ToString(); }
            set
            {
                if (value != LogCall.BusType.ToString() && Enum.TryParse(value, out BusinessType bType))
                {
                    LogCall.BusType = bType;
                }
                OnPropertyChanged(nameof(SelectedBType));
            }
        }
        public ObservableCollection<string> ITypeCollection { get; set; }
        public string SelectedIType
        {
            get { return LogCall.IndType.ToString(); }
            set
            {
                if (value != LogCall.IndType.ToString() && Enum.TryParse(value, out IndustryType iType))
                {
                    LogCall.IndType = iType;
                }
                OnPropertyChanged(nameof(SelectedIType));
            }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CallReportVM()
        {
            if (LogCall == null)
            {
                LogCall = new LoggedCall();
            }
            if (BTypeCollection == null)
            {
                BTypeCollection = new ObservableCollection<string>(Enum.GetNames(typeof(BusinessType)));
            }
            if (ITypeCollection == null)
            {
                ITypeCollection = new ObservableCollection<string>(Enum.GetNames(typeof(IndustryType)));
            }
        }
    }
}
