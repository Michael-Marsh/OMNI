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
                System.Diagnostics.Process.Start(Properties.Settings.Default.ProductionAppLocation, "/hdtc");
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
