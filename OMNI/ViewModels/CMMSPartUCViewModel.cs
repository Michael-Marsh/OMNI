using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace OMNI.ViewModels
{
    public class CMMSPartUCViewModel : ViewModelBase
    {
        #region Properties

        public CMMSPart WccoPart { get; set; }
        public CMMSPartAction PartAction { get; set; }
        private int? searchID;
        public int? SearchID
        {
            get { return searchID; }
            set
            {
                if (value != null)
                {
                    CMMSPart.UnlockLockRecordAsync(Convert.ToInt32(searchID), false);
                    WccoPart = CMMSPart.GetCMMSPartAsync(Convert.ToInt32(value)).Result;
                    OnPropertyChanged(nameof(WccoPart));
                }
                PartAction = WccoPart.PartNumber == 0 ? CMMSPartAction.Open : CMMSPartAction.Save;
                searchID = value;
                OnPropertyChanged(nameof(PartAction));
                OnPropertyChanged(nameof(SearchID));
            }
        }

        #endregion

        /// <summary>
        /// CMMS Part UserControl ViewModel
        /// </summary>
        /// <param name="action"></param>
        public CMMSPartUCViewModel(CMMSPartAction action)
        {
            PartAction = action;
            if (WccoPart == null)
            {
                WccoPart = new CMMSPart();
            }
            CMMSPartManagementUCViewModel.PartSave += WCCOPartSave;
            CMMSPartManagementUCViewModel.WCCOPartCanSave = Validate();

        }

        /// <summary>
        /// **Overrode Method**
        /// Save a WCCO Part to the database
        /// </summary>
        /// <param name="sender">todo: describe sender parameter on WCCOPartSave</param>
        /// <param name="e">todo: describe e parameter on WCCOPartSave</param>
        public void WCCOPartSave(object sender, EventArgs e)
        {
            bool? saveValidation = false;
            switch(PartAction)
            {
                case CMMSPartAction.New:
                    saveValidation = WccoPart.SubmitAsync().Result;
                    break;
                case CMMSPartAction.Save:
                    saveValidation = WccoPart.UpdateAsync().Result;
                    break;
                case CMMSPartAction.Open:
                    ExceptionWindow.Show("Invalid Part", "No part has been selected.");
                    return;
            }
            switch(saveValidation)
            {
                case true:
                    SearchID = WccoPart.PartNumber;
                    break;
                case false:
                    ExceptionWindow.Show("DataBase Connection Failure", "OMNI is currently unable to connect to the database.\nPlease contact IT for further assistance.");
                    break;
                case null:
                    ExceptionWindow.Show("Locked Record", "The part you are trying to update is currently locked.\nPlease contact the owner of this record to change it.");
                    break;
            }
        }

        public bool Validate()
        {
            //TODO: Write in validation along with triggers.
            return true;
        }

        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                CMMSPart.UnlockLockRecordAsync(Convert.ToInt32(SearchID), false);
                CMMSPartManagementUCViewModel.PartSave -= WCCOPartSave;
                CMMSPartManagementUCViewModel.WCCOPartCanSave = false;
            }
        }
    }
}
