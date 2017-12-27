using System;

namespace OMNIforOutlook
{
    public partial class OMNIAddIn
    {
        private void OMNIAddIn_Startup(object sender, EventArgs e)
        {

        }

        private void OMNIAddIn_Shutdown(object sender, EventArgs e)
        {
            // Note: Outlook no longer raises this event. If you have code that
            //    must run when Outlook shuts down, see https://go.microsoft.com/fwlink/?LinkId=506785
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            Startup += new EventHandler(OMNIAddIn_Startup);
            Shutdown += new EventHandler(OMNIAddIn_Shutdown);
        }

        #endregion
    }
}
