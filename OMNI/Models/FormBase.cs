using MySql.Data.MySqlClient;
using OMNI.CustomControls;
using OMNI.Enumerations;
using OMNI.Helpers;
using OMNI.QMS.Model;
using OMNI.QMS.View;
using OMNI.QMS.ViewModel;
using OMNI.ViewModels;
using OMNI.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace OMNI.Models
{
    /// <summary>
    /// Form Base Object
    /// </summary>
    public class FormBase
    {
        #region Properties

        public int? IDNumber { get; set; }
        public DateTime Date { get; set; }
        public string Submitter { get; set; }
        public Module FormModule { get; set; }
        public BindingList<LinkedForms> FormLinkList { get; set; }
        public DataTable NotesTable { get; set; }
        public static bool FormChangeInProgress { get; set; }

        #endregion

        /// <summary>
        /// Form Base Constructor
        /// </summary>
        public FormBase()
        {
            if (FormLinkList == null)
            {
                FormLinkList = new BindingList<LinkedForms>();
                FormLinkList.ListChanged += FormSelectionChanged;
                FormChangeInProgress = false;
            }
        }

        /// <summary>
        /// Changes in the form selection
        /// </summary>
        /// <param name="sender">LinkedForms</param>
        /// <param name="e">BindingList Change Events</param>
        public static void FormSelectionChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged && !FormChangeInProgress)
            {
                try
                {
                    var _tempList = (BindingList<LinkedForms>)sender;
                    FormChangeInProgress = true;
                    var _oldHeader = _tempList[_tempList.IndexOf(_tempList.FirstOrDefault(l => l.LinkSelected && l.LinkIDNumber != _tempList[e.NewIndex].LinkIDNumber))].LinkIDNumber;
                    _tempList[_tempList.IndexOf(_tempList.FirstOrDefault(l => l.LinkSelected && l.LinkIDNumber != _tempList[e.NewIndex].LinkIDNumber))].LinkSelected = false;
                    _tempList[e.NewIndex].LinkSelected = true;
                    var _tabItem = DashBoardTabControl.GetTabItem(_oldHeader.ToString());
                    switch (_tempList[e.NewIndex].LinkFormType)
                    {
                        case Module.QIR:
                            ((QIRFormView)((RadioButton)Keyboard.FocusedElement).CommandParameter).DataContext = new QIRFormViewModel(new QIR(_tempList[e.NewIndex].LinkIDNumber, false));
                            break;
                        case Module.CMMS:
                            if (_tabItem.Content.GetType() == typeof(CMMSWorkOrderUCView))
                            {
                                var vm = ((CMMSWorkOrderUCView)_tabItem.Content).DataContext as CMMSWorkOrderUCViewModel;
                                vm.WorkOrder = CMMSWorkOrder.LoadAsync(Convert.ToInt32(_tempList[e.NewIndex].LinkIDNumber)).Result;
                                vm.SearchIDNumber = _tempList[e.NewIndex].LinkIDNumber;
                                vm.CommandType = FormCommand.Search;
                                vm.SubmitCommand.Execute(null);
                            }
                            else
                            {
                                DashBoardTabControl.WorkSpace.Items.Remove(_tabItem);
                                DashBoardTabControl.WorkSpace.LoadCMMSWorkOrderTabItem(_tempList[e.NewIndex].LinkIDNumber, true);
                            }
                            break;
                        case Module.HDT:
                            if (_tabItem.Content.GetType() == typeof(ITFormUCView))
                            {
                                var vm = ((ITFormUCView)_tabItem.Content).DataContext as ITFormUCViewModel;
                                vm.Ticket = ITTicket.GetITTicketAsync(Convert.ToInt32(_tempList[e.NewIndex].LinkIDNumber)).Result;
                                vm.SearchIDNumber = _tempList[e.NewIndex].LinkIDNumber;
                                vm.PrimaryCommandType = FormCommand.Search;
                                vm.SubmitCommand.Execute(null);
                            }
                            else
                            {
                                DashBoardTabControl.WorkSpace.Items.Remove(_tabItem);
                                DashBoardTabControl.WorkSpace.LoadITTicketTabItem(_tempList[e.NewIndex].LinkIDNumber, true);
                            }
                            break;
                    }
                    FormChangeInProgress = false;
                }
                catch (Exception)
                {
                    FormChangeInProgress = false;
                }
            }
        }

        /// <summary>
        /// Overrideable form submission method to be incorperated with new form models
        /// </summary>
        /// <param name="formObject">Form object argument to submit</param>
        /// <returns>Last instered ID number</returns>
        public virtual int? Submit(object formObject)
        {
            return null;
        }

        /// <summary>
        /// Overrideable form update method to be incorperated with new form models
        /// </summary>
        /// <param name="formObject">Form object argument to submit</param>
        public virtual void Update(object formObject)
        {
        }
    }

    public static class FormBaseExtensions
    {
        /// <summary>
        /// Get a list of all the linked documents, starting with the parent and children in seqential stage order
        /// </summary>
        /// <param name="formObject">Form Object</param>
        /// <returns>Transaction Success as bool.  true = accepted, false = failed</returns>
        public async static Task<bool> GetLinkListAsync(this FormBase formObject)
        {
            try
            {
                var _linkParent = new LinkedForms();
                _linkParent = await formObject.IsParentAsync().ConfigureAwait(false)
                    ? new LinkedForms { LinkIDNumber = Convert.ToInt32(formObject.IDNumber), LinkFormType = formObject.FormModule, LinkSelected = false }
                    : await formObject.GetParentAsync().ConfigureAwait(false);
                formObject.FormLinkList.Add(_linkParent);
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`child_form` WHERE `ParentFormID`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", _linkParent.LinkIDNumber);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            formObject.FormLinkList.Add(new LinkedForms { LinkIDNumber = reader.GetInt32("ChildFormNumber"), LinkFormType = (Module)Enum.Parse(typeof(Module), reader.GetString("ChildFormType")), LinkSelected = false });
                        }
                    }
                }
                FormBase.FormChangeInProgress = true;
                formObject.FormLinkList[formObject.FormLinkList.IndexOf(formObject.FormLinkList.FirstOrDefault(l => l.LinkIDNumber == Convert.ToInt32(formObject.IDNumber)))].LinkSelected = true;
                formObject.FormLinkList.Select(l => l.LinkSelected);
                FormBase.FormChangeInProgress = false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks to see if the form is a parent form
        /// </summary>
        /// <param name="formObject">Form Object</param>
        /// <returns>Form's parent status as bool</returns>
        public async static Task<bool> IsParentAsync(this FormBase formObject)
        {
            if (App.ConConnected)
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(`ParentFormNumber`) FROM `{App.Schema}`.`parent_form` WHERE `ParentFormNumber`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", formObject.IDNumber);
                    return Convert.ToBoolean(await cmd.ExecuteScalarAsync().ConfigureAwait(false));
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the parent form ID number from a child ID number
        /// </summary>
        /// <param name="formObject">Form Object</param>
        /// <returns>Parent Form ID Number as int</returns>
        public async static Task<LinkedForms> GetParentAsync(this FormBase formObject)
        {
            var _parentID = 0;
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT (`ParentFormID`) FROM `{App.Schema}`.`child_form` WHERE `ChildFormNumber`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", formObject.IDNumber);
                    _parentID = Convert.ToInt32(await cmd.ExecuteScalarAsync().ConfigureAwait(false));
                }
                using (MySqlCommand cmd = new MySqlCommand($"SELECT (`ParentFormType`) FROM `{App.Schema}`.`parent_form` WHERE `ParentFormNumber`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", _parentID);
                    return new LinkedForms { LinkIDNumber = _parentID, LinkFormType = (Module)Enum.Parse(typeof(Module), Convert.ToString(await cmd.ExecuteScalarAsync().ConfigureAwait(false))), LinkSelected = false };
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Checks to see if the form is a child form
        /// </summary>
        /// <param name="formObject">Form Object</param>
        /// <returns>Form's child status as bool</returns>
        public async static Task<bool> IsChildAsync(this FormBase formObject)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(`ChildFormNumber`) FROM `{App.Schema}`.`child_form` WHERE `ChildFormNumber`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", formObject.IDNumber);
                    return Convert.ToBoolean(await cmd.ExecuteScalarAsync().ConfigureAwait(false));
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Check to see if the form is currently linked
        /// </summary>
        /// <param name="formObject">Form Object</param>
        /// <returns>Form ID Number existence as bool</returns>
        public async static Task<bool> LinkExistsAsync(this FormBase formObject)
        {
            if (formObject.IDNumber == null)
            {
                return false;
            }
            var parent = 0;
            var child = 0;
            using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(`ParentFormID`) FROM `{App.Schema}`.`parent_form` WHERE `ParentFormNumber`=@p1", App.ConAsync))
            {
                cmd.Parameters.AddWithValue("p1", formObject.IDNumber);
                parent = Convert.ToInt32(await cmd.ExecuteScalarAsync().ConfigureAwait(false));
            }
            using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(`ChildFormID`) FROM `{App.Schema}`.`child_form` WHERE `ChildFormNumber`=@p1", App.ConAsync))
            {
                cmd.Parameters.AddWithValue("p1", formObject.IDNumber);
                child = Convert.ToInt32(await cmd.ExecuteScalarAsync().ConfigureAwait(false));
            }
            return child + parent > 0 ? true : false;
        }

        /// <summary>
        /// Delete a link from any form
        /// </summary>
        /// <param name="formObject">Form Object</param>
        /// <returns>Transaction Success as bool.  true = accepted / false = rejected</returns>
        public async static Task<bool> DeleteLinkAsync(this FormBase formObject)
        {
            try
            {
                var _parentID = await formObject.IsChildAsync().ConfigureAwait(false) ? await formObject.GetParentAsync().ConfigureAwait(false) : null;
                if (_parentID == null)
                {
                    using (MySqlCommand cmd = new MySqlCommand($"DELETE FROM `{App.Schema}`.`parent_form` WHERE `ParentFormNumber`=@p1", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", formObject.IDNumber);
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    var _childStageList = new List<int>();
                    using (MySqlCommand cmd = new MySqlCommand($"DELETE FROM `{App.Schema}`.`child_form` WHERE `ChildFormNumber`=@p1", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", formObject.IDNumber);
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                    using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{App.Schema}`.`child_form` WHERE `ParentFormID`=@p1", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", _parentID);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync().ConfigureAwait(false))
                                {
                                    _childStageList.Add(reader.GetInt32("ChildFormNumber"));
                                }
                            }
                        }
                    }
                    if (_childStageList.Count > 0)
                    {
                        var counter = 1;
                        foreach (var i in _childStageList)
                        {
                            using (MySqlCommand cmd = new MySqlCommand($"UPDATE `{App.Schema}`.`child_form` SET `ChildStage`=@p1 WHERE `ChildFormNumber`=@p2", App.ConAsync))
                            {
                                cmd.Parameters.AddWithValue("p1", counter);
                                cmd.Parameters.AddWithValue("p2", i);
                                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                                counter++;
                            }
                        }
                    }
                    var parentCount = 0;
                    using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(`ParentFormID`) FROM `{App.Schema}`.`child_form` WHERE `ParentFormID`=@p1", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", _parentID);
                        parentCount = Convert.ToInt32(await cmd.ExecuteScalarAsync().ConfigureAwait(false));
                    }
                    if (parentCount == 0)
                    {
                        using (MySqlCommand cmd = new MySqlCommand($"DELETE FROM `{App.Schema}`.`parent_form` WHERE `ParentFormNumber`=@p1", App.ConAsync))
                        {
                            cmd.Parameters.AddWithValue("p1", _parentID);
                            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Links 2 forms together in parent - child fashion
        /// </summary>
        /// <param name="childFormObject">Child Form Object</param>
        /// <param name="parentIDNumber">Parent Form ID Number</param>
        /// <param name="parentFormModule">Parent Form Module</param>
        /// <returns>Transaction Success as bool.  true = accepted / false = rejected</returns>
        public async static Task<bool> CreateLinkAsync(this FormBase childFormObject, int parentIDNumber, Module parentFormModule)

        {
            try
            {
                object parentObject = null;
                switch (parentFormModule)
                {
                    case Module.CMMS:
                        parentObject = await CMMSWorkOrder.LoadAsync(parentIDNumber);
                        break;
                    case Module.HDT:
                        parentObject = await ITTicket.GetITTicketAsync(parentIDNumber);
                        break;
                    case Module.QIR:
                        parentObject = new QIR(parentIDNumber, true);
                        break;
                }
                var _stage = 1;
                if (await ((FormBase)parentObject).IsParentAsync())
                {
                    using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(`ParentFormID`) FROM `{App.Schema}`.`child_form` WHERE `ParentFormID`=@p1", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", parentIDNumber);
                        _stage = Convert.ToInt32(await cmd.ExecuteScalarAsync().ConfigureAwait(false)) + 1;
                    }
                }
                else if (await ((FormBase)parentObject).IsChildAsync())
                {
                    parentIDNumber = (await ((FormBase)parentObject).GetParentAsync().ConfigureAwait(false)).LinkIDNumber;
                    using (MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(`ParentFormID`) FROM `{App.Schema}`.`child_form` WHERE `ParentFormID`=@p1", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", parentIDNumber);
                        _stage = Convert.ToInt32(await cmd.ExecuteScalarAsync().ConfigureAwait(false)) + 1;
                    }
                }
                else
                {
                    using (MySqlCommand cmd = new MySqlCommand($"INSERT INTO `{App.Schema}`.`parent_form` (`ParentFormType`, `ParentFormNumber`) VALUES(@p1, @p2)", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", parentFormModule.ToString());
                        cmd.Parameters.AddWithValue("p2", parentIDNumber);
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
                using (MySqlCommand cmd = new MySqlCommand($"INSERT INTO `{App.Schema}`.`child_form` (`ChildFormType`, `ChildFormNumber`, `ChildStage`, `ParentFormID`) VALUES(@p1, @p2, @p3, @p4)", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", childFormObject.FormModule.ToString());
                    cmd.Parameters.AddWithValue("p2", childFormObject.IDNumber);
                    cmd.Parameters.AddWithValue("p3", _stage);
                    cmd.Parameters.AddWithValue("p4", parentIDNumber);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
                await childFormObject.GetLinkListAsync().ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get a formated Notes DataTable
        /// </summary>
        /// <param name="form">FormBase Object</param>
        /// <returns>Formated Notes Table as DataTable</returns>
        public async static Task<DataTable> GetNotesTableAsync(this FormBase form)
        {
            try
            {
                using (DataTable _tempTable = new DataTable())
                {
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter($"SELECT `Timestamp`, `Note`, `Submitter` FROM `{App.Schema}`.`{form.FormModule.ToString().ToLower()}_notes` WHERE `IDNumber`=@p1", App.ConAsync))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("p1", form.IDNumber);
                        await adapter.FillAsync(_tempTable).ConfigureAwait(false);
                        return _tempTable;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Add a note to any form
        /// </summary>
        /// <param name="form">FormBase Object</param>
        public static void AddNote(this FormBase form)
        {
            var _note = NoteWindowViewModel.Show();
            if (string.IsNullOrWhiteSpace(_note))
            {
                ExceptionWindow.Show("Blank Note", "A blank note was entered, submission has been canceled.\nIf you feel you reached this message in error please contact IT.");
            }
            else
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand($"INSERT `{App.Schema}`.`{form.FormModule}_notes` (IDNumber, Note, Submitter) VALUES(@p1, @p2, @p3)", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", form.IDNumber);
                        cmd.Parameters.AddWithValue("p2", _note);
                        cmd.Parameters.AddWithValue("p3", CurrentUser.FullName);
                        cmd.ExecuteNonQuery();
                    }
                    form.NotesTable = form.GetNotesTableAsync().Result;
                }
                catch (Exception ex)
                {
                    ExceptionWindow.Show("Unhandled Excpetion", ex.Message, ex);
                }
            }
        }
    }

    public class LinkedForms : INotifyPropertyChanged
    {
        #region Properties

        public int LinkIDNumber { get; set; }
        public Module LinkFormType { get; set; }
        private bool linkSelected;
        public bool LinkSelected
        {
            get { return linkSelected; }
            set { linkSelected = value; OnPropertyChanged(nameof(LinkSelected)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// Reflects changes in LinkSelected property to the collection
        /// </summary>
        /// <param name="propertyName">Property Name</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
    }

}
