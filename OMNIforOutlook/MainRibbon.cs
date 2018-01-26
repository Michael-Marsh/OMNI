using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;

namespace OMNIforOutlook
{
    public partial class MainRibbon
    {
        private void MainRibbon_Load(object sender, RibbonUIEventArgs e)
        {

        }

        private void HDTCreate_btn_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(Environment.GetFolderPath(Environment.SpecialFolder.Programs));
                sb.Append("\\Michael Marsh\\WCCO Belting\\OMNI.appref-ms");
                System.Diagnostics.Process.Start(sb.ToString(), "/hdtc");
            }
            catch (Exception)
            {
                return;
            }
            //Use this sub part for during debug and development testing
            /*try
            {
                System.Diagnostics.Process.Start(Properties.Settings.Default.DebugAppLocation, "/hdtc");
            }
            catch (Exception)
            {
                return;
            }*/
        }
    }
}
