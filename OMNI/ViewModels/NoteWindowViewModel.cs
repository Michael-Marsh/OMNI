using OMNI.Commands;
using OMNI.Views;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// Note Window ViewModel Interaction Logic
    /// </summary>
    public class NoteWindowViewModel : ViewModelBase
    {
        #region Properties

        private static string note { get; set; }
        public string Note
        {
            get { return note; }
            set { note = value; OnPropertyChanged(nameof(Note)); }
        }
        private static bool SaveNote { get; set; }

        RelayCommand _close;

        #endregion

        /// <summary>
        /// Note Window ViewModel Constructor
        /// </summary>
        public NoteWindowViewModel()
        {
            SaveNote = false;
        }

        /// <summary>
        /// Display a window to enter notes.
        /// </summary>
        /// <returns>Note</returns>
        public static string Show()
        {
            note = null;
            new NoteWindowView().ShowDialog();
            return note;
        }

        /// <summary>
        /// Sets the notes value to null if the window is closed.
        /// </summary>
        public static void Close()
        {
            if (!SaveNote)
            {
                note = null;
            }
        }

        /// <summary>
        /// Close Command
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                if (_close == null)
                {
                    _close = new RelayCommand(CloseExecute);
                }
                return _close;
            }
        }

        /// <summary>
        /// Close Command Execution
        /// </summary>
        /// <param name="parameter">Command function to execute</param>
        private void CloseExecute(object parameter)
        {
            if (parameter.ToString() == "Save")
            {
                SaveNote = true;
            }
            NoteWindowView.NoteWindow.Close();
        }

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing) { }
        }
    }
}
