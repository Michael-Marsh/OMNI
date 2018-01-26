using OMNI.Commands;
using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows;

namespace OMNI.ViewModels
{
    /// <summary>
    /// Form Link Window ViewModel
    /// </summary>
    public class FormLinkWindowViewModel : ViewModelBase
    {
        #region Properties

        public int ChildFormNumber { get; set; }
        public object ChildFormObject { get; set; }
        private int? parentFormNumber;
        public int? ParentFormNumber
        {
            get { return parentFormNumber; }
            set { parentFormNumber = value; ValidateFormExists(value); OnPropertyChanged(nameof(ParentFormNumber)); }
        }
        public IList<string> FormType { get; set; }
        private string selectedFormType;
        public string SelectedFormType
        {
            get { return selectedFormType; }
            set { selectedFormType = value; if (ParentFormNumber != null) { ValidateFormExists(ParentFormNumber); } }
        }
        public bool FormExists { get; set; }

        RelayCommand _link;

        #endregion

        /// <summary>
        /// Form Link Window ViewModel Constructor
        /// </summary>
        public FormLinkWindowViewModel(object childFormObject)
        {
            ChildFormNumber = Convert.ToInt32(((FormBase)childFormObject).IDNumber);
            ChildFormObject = childFormObject;
            if (FormType == null)
            {
                FormType = Enum.GetNames(typeof(Module)).OfType<string>().ToList();
            }
            FormExists = true;
        }

        /// <summary>
        /// Validate that the submited parent form exists in the database
        /// </summary>
        /// <param name="idNumber"></param>
        public void ValidateFormExists(int? idNumber)
        {
            if (!string.IsNullOrEmpty(SelectedFormType))
            {
                var _count = 0;
                switch ((Module)Enum.Parse(typeof(Module), SelectedFormType))
                {
                    case Module.QIR:
                        _count = idNumber.ToString().Length >= 7 
                            ? Convert.ToInt32(OMNIDataBase.CountWithComparisonAsync("qir_master", $"`QIRNumber`={idNumber}").Result) 
                            : 0;
                        break;
                    case Module.HDT:
                        _count = idNumber.ToString().Length >= 6
                            ? Convert.ToInt32(OMNIDataBase.CountWithComparisonAsync("it_ticket_master", $"`TicketNumber`={idNumber}").Result)
                            : 0;
                        break;
                    case Module.CMMS:
                        _count = idNumber.ToString().Length >= 4
                            ? Convert.ToInt32(OMNIDataBase.CountWithComparisonAsync("cmmsworkorder", $"`WorkOrderNumber`={idNumber}").Result)
                            : 0;
                        break;
                }
                FormExists = _count > 0 ? true : false;
                OnPropertyChanged(nameof(FormExists));
            }
        }

        /// <summary>
        /// Link Submit Interface Command
        /// </summary>
        public ICommand LinkSubmit
        {
            get
            {
                if (_link == null)
                {
                    _link = new RelayCommand(LinkSubmitExecute, LinkSubmitCanExecute);
                }
                return _link;
            }
        }

        /// <summary>
        /// Link Submit Interface Command Execution
        /// </summary>
        /// <param name="parameter"></param>
        private void LinkSubmitExecute(object parameter)
        {
            if (!((FormBase)ChildFormObject).CreateLinkAsync(Convert.ToInt32(ParentFormNumber), (Module)Enum.Parse(typeof(Module), SelectedFormType)).Result)
            {
                ExceptionWindow.Show("Link Failure", "Unable to link this form at this time.\nPlease Contact IT for further assistance.");
            }
            foreach (Window w in Application.Current.Windows)
            {
                if (w.Title == "Form Link")
                {
                    w.Close();
                }
            }
        }
        private bool LinkSubmitCanExecute(object parameter)
        {
            return ChildFormNumber != 0 && !string.IsNullOrEmpty(SelectedFormType) && FormExists;
        }
    }
}
