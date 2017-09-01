using OMNI.Commands;
using OMNI.Enumerations;
using OMNI.Views;
using OMNI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OMNI.Helpers;

namespace OMNI.ViewModels
{
    public class CMMSPartManagementUCViewModel : ViewModelBase
    {
        #region Properties

        public string CurrentAction { get; set; }
        public static EventHandler PartSave { get; set; }
        public static bool WCCOPartCanSave { get; set; }

        RelayCommand primary;

        #endregion

        /// <summary>
        /// CMMS Part Management UserControl ViewModel Constructor
        /// </summary>
        public CMMSPartManagementUCViewModel()
        {
            CurrentAction = "None";
            WCCOPartCanSave = false;
        }

        /// <summary>
        /// Primary Interface Command
        /// </summary>
        public ICommand PrimaryCommand
        {
            get
            {
                if (primary == null)
                {
                    primary = new RelayCommand(PrimaryExecute, PrimaryCanExecute);
                }
                return primary;
            }
        }

        /// <summary>
        /// Primary Command Execution
        /// </summary>
        /// <param name="parameter"></param>
        private void PrimaryExecute(object parameter)
        {
            CurrentAction = parameter.ToString();
            if(!parameter.ToString().Contains("Save"))
            {
                CMMSPartManagementUCView.PartUserControl.Children.Clear();
            }
            switch (parameter.ToString())
            {
                case "WCCO_New":
                    CMMSPartManagementUCView.PartUserControl.Children.Add(new CMMSPartUCView { DataContext = new CMMSPartUCViewModel(CMMSPartAction.New) });
                    break;
                case "WCCO_Open":
                    CMMSPartManagementUCView.PartUserControl.Children.Add(new CMMSPartUCView { DataContext = new CMMSPartUCViewModel(CMMSPartAction.Open) });
                    break;
                case "WCCO_Save":
                    PartSave?.Invoke(null, null);
                    break;
                case "VP_New":
                    CMMSPartManagementUCView.PartUserControl.Children.Add(new CMMSVendorPartUCView());
                    break;
            }
        }
        public virtual bool PrimaryCanExecute(object parameter)
        {
            switch (parameter.ToString())
            {
                case "WCCO_Save":
                    return WCCOPartCanSave;
                case "VP_Save":
                    return false;
                case "Vendor_Save":
                    return false;
                default:
                    return true;
            }
        }

    }
}
