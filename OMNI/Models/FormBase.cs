using OMNI.CustomControls;
using OMNI.Enumerations;
using OMNI.Extensions;
using OMNI.Helpers;
using OMNI.QMS.Model;
using OMNI.QMS.View;
using OMNI.QMS.ViewModel;
using OMNI.ViewModels;
using OMNI.Views;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
            //TODO: Review this method to see if you can seperate the view objects to a converter at the top level
            var _tempList = (BindingList<LinkedForms>)sender;
            if (e.ListChangedType == ListChangedType.ItemChanged && !FormChangeInProgress)
            {
                try
                {
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
        public static bool GetLinkList(this FormBase formObject)
        {
            try
            {
                if (formObject.FormLinkList.Count > 0)
                {
                    formObject.FormLinkList.Clear();
                }
                var _linkParent = new LinkedForms();
                _linkParent = formObject.IsParent()
                    ? new LinkedForms { LinkIDNumber = Convert.ToInt32(formObject.IDNumber), LinkFormType = formObject.FormModule, LinkSelected = false }
                    : formObject.GetParent();
                formObject.FormLinkList.Add(_linkParent);
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase}; SELECT * FROM [child_form] WHERE [ParentFormID]=@p1", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", _linkParent.LinkIDNumber);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            formObject.FormLinkList.Add(new LinkedForms { LinkIDNumber = reader.SafeGetInt32("ChildFormNumber"), LinkFormType = (Module)Enum.Parse(typeof(Module), reader.SafeGetString("ChildFormType")), LinkSelected = false });
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
        public static bool IsParent(this FormBase formObject)
        {
            if (App.SqlConAsync.State == ConnectionState.Open)
            {
                using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                        SELECT COUNT([ParentFormNumber]) FROM [parent_form] WHERE [ParentFormNumber]=@p1", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", formObject.IDNumber);
                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the parent form ID number from a child ID number
        /// </summary>
        /// <param name="formObject">Form Object</param>
        /// <returns>Parent Form ID Number as int</returns>
        public static LinkedForms GetParent(this FormBase formObject)
        {
            var _parentID = 0;
            try
            {
                using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; SELECT ([ParentFormID]) FROM [child_form] WHERE [ChildFormNumber]=@p1", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", formObject.IDNumber);
                    _parentID = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; SELECT ([ParentFormType]) FROM [parent_form] WHERE [ParentFormNumber]=@p1", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", _parentID);
                    return new LinkedForms { LinkIDNumber = _parentID, LinkFormType = (Module)Enum.Parse(typeof(Module), Convert.ToString(cmd.ExecuteScalar())), LinkSelected = false };
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
        public static bool IsChild(this FormBase formObject)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; SELECT COUNT([ChildFormNumber]) FROM [child_form] WHERE [ChildFormNumber]=@p1", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", formObject.IDNumber);
                    return Convert.ToBoolean(cmd.ExecuteScalar());
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
        public static bool LinkExists(this FormBase formObject)
        {
            if (formObject.IDNumber == null)
            {
                return false;
            }
            var parent = 0;
            var child = 0;
            using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; SELECT COUNT([ParentFormID]) FROM [parent_form] WHERE [ParentFormNumber]=@p1", App.SqlConAsync))
            {
                cmd.Parameters.AddWithValue("p1", formObject.IDNumber);
                parent = Convert.ToInt32(cmd.ExecuteScalar());
            }
            using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; SELECT COUNT([ChildFormID]) FROM [child_form] WHERE [ChildFormNumber]=@p1", App.SqlConAsync))
            {
                cmd.Parameters.AddWithValue("p1", formObject.IDNumber);
                child = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return child + parent > 0 ? true : false;
        }

        /// <summary>
        /// Delete a link from any form
        /// </summary>
        /// <param name="formObject">Form Object</param>
        /// <returns>Transaction Success as bool.  true = accepted / false = rejected</returns>
        public static bool DeleteLink(this FormBase formObject)
        {
            try
            {
                var pId = 0;
                if (formObject.FormLinkList[0].LinkIDNumber == formObject.IDNumber || formObject.FormLinkList.Count == 2)
                {
                    pId = formObject.IsChild() ? formObject.GetParent().LinkIDNumber : Convert.ToInt32(formObject.IDNumber);
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                            BEGIN TRAN;
                                                            DELETE FROM [parent_form] WHERE [ParentFormNumber]=@p1
                                                            DELETE FROM [child_form] WHERE [ParentFormID]=@p1;
                                                            COMMIT;", App.SqlConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", pId);
                        cmd.ExecuteNonQuery();
                    }
                    formObject.FormLinkList = new BindingList<LinkedForms>();
                }
                else
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.DataBase};
                                                            DELETE FROM [child_form] WHERE [ChildFormNumber]=@p1;", App.SqlConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", formObject.IDNumber);
                        cmd.ExecuteNonQuery();
                    }
                    formObject.FormLinkList.Remove(formObject.FormLinkList.First(o => o.LinkIDNumber == formObject.IDNumber));
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
        public static bool CreateLink(this FormBase childFormObject, int parentIDNumber, Module parentFormModule)
        {
            try
            {
                object parentObject = null;
                switch (parentFormModule)
                {
                    case Module.CMMS:
                        parentObject = CMMSWorkOrder.LoadAsync(parentIDNumber).Result;
                        break;
                    case Module.QIR:
                        parentObject = new QIR(parentIDNumber, true);
                        break;
                }
                var _stage = 1;
                if (((FormBase)parentObject).IsParent())
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; SELECT COUNT([ParentFormID]) FROM [child_form] WHERE [ParentFormID]=@p1", App.SqlConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", parentIDNumber);
                        _stage = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                    }
                }
                else if (((FormBase)parentObject).IsChild())
                {
                    parentIDNumber = ((FormBase)parentObject).GetParent().LinkIDNumber;
                    using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; SELECT COUNT([ParentFormID]) FROM [child_form] WHERE [ParentFormID]=@p1", App.SqlConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", parentIDNumber);
                        _stage = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                    }
                }
                else
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; INSERT INTO [parent_form] ([ParentFormType], [ParentFormNumber]) VALUES(@p1, @p2)", App.SqlConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", parentFormModule.ToString());
                        cmd.Parameters.AddWithValue("p2", parentIDNumber);
                        cmd.ExecuteNonQuery();
                    }
                }
                using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; INSERT INTO [child_form] ([ChildFormType], [ChildFormNumber], [ChildStage], [ParentFormID]) VALUES(@p1, @p2, @p3, @p4)", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", childFormObject.FormModule.ToString());
                    cmd.Parameters.AddWithValue("p2", childFormObject.IDNumber);
                    cmd.Parameters.AddWithValue("p3", _stage);
                    cmd.Parameters.AddWithValue("p4", parentIDNumber);
                    cmd.ExecuteNonQuery();
                }
                childFormObject.GetLinkList();
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
        public static DataTable GetNotesTable(this FormBase form)
        {
            try
            {
                using (DataTable _tempTable = new DataTable())
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {App.DataBase};
                                                                        SELECT [Timestamp], [Note], [Submitter] FROM [{form.FormModule.ToString().ToLower()}_notes] WHERE [IDNumber]=@p1", App.SqlConAsync))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("p1", form.IDNumber);
                        adapter.Fill(_tempTable);
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
                    using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; INSERT [{form.FormModule}_notes] (IDNumber, Note, Submitter) VALUES(@p1, @p2, @p3)", App.SqlConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", form.IDNumber);
                        cmd.Parameters.AddWithValue("p2", _note);
                        cmd.Parameters.AddWithValue("p3", CurrentUser.FullName);
                        cmd.ExecuteNonQuery();
                    }
                    form.NotesTable = form.GetNotesTable();
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
