using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace OMNI
{
    /// <summary>
    /// Start up object
    /// </summary>
    public partial class AppStartup : Application
    {
        #region Properties

        [DllImport("User32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);
        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, SW nCmdShow);

        #endregion

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [STAThread()]
        public static void Main()
        {
            using (System.Threading.Mutex oneApp = new System.Threading.Mutex(true, "OMNI31973310", out bool check))
            {
                if (Environment.GetCommandLineArgs().Length == 1)
                {
                    if (!check)
                    {
                        var proc = Process.GetProcessesByName(nameof(OMNI)).FirstOrDefault();
                        var mwhandle = proc.MainWindowHandle;
                        SetForegroundWindow(mwhandle);
                        ShowWindow(mwhandle, SW.Restore);
                        ShowWindow(mwhandle, SW.Maximize);
                        return;
                    }
                    GC.KeepAlive(oneApp);
                }
                App.Main();
            }
        }
    }

    /// <summary>
    /// User32.dll WindowState const int
    /// </summary>
    public enum SW
    {
        Hide = 0,
        ShowNormal = 1,
        ShowMinimized = 2,
        Maximize = 3,
        Show = 5,
        Minimize = 6,
        Restore = 9,
        ShowDefault = 10
    }
}
