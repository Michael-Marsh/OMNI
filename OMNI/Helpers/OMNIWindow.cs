using System.Linq;
using System.Windows;

namespace OMNI.Helpers
{
    /// <summary>
    /// Window helper
    /// </summary>
    public sealed class OMNIWindow<T> where T : Window
    {
        /// <summary>
        /// Inquire if a window is currently open.
        /// </summary>
        /// <typeparam name="T">Window Object</typeparam>
        /// <param name="title">optional: Inquired Window Title</param>
        public static bool IsOpen(string title = null)
        {
            return string.IsNullOrEmpty(title)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Title.Equals(title));
        }

        /// <summary>
        /// Focuses an already opened window
        /// </summary>
        /// <typeparam name="T">Type of Window to focus</typeparam>
        public static void Focus()
        {
            if (Application.Current.Windows.OfType<T>().Any())
            {
                var _temp = Application.Current.Windows.OfType<T>().First();
                _temp.Focus();
                _temp.Topmost = true;
                _temp.Topmost = false;
            }
        }
    }
}
