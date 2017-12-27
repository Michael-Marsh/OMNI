namespace OMNIforOutlook
{
    partial class MainRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public MainRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainRibbon));
            this.MainTab = this.Factory.CreateRibbonTab();
            this.HDTGroup = this.Factory.CreateRibbonGroup();
            this.HDTCreate_btn = this.Factory.CreateRibbonButton();
            this.HDTViewLog_btn = this.Factory.CreateRibbonButton();
            this.MainTab.SuspendLayout();
            this.HDTGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTab
            // 
            this.MainTab.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.MainTab.Groups.Add(this.HDTGroup);
            this.MainTab.Label = "OMNI";
            this.MainTab.Name = "MainTab";
            // 
            // HDTGroup
            // 
            this.HDTGroup.Items.Add(this.HDTCreate_btn);
            this.HDTGroup.Items.Add(this.HDTViewLog_btn);
            this.HDTGroup.Label = "Help Desk";
            this.HDTGroup.Name = "HDTGroup";
            // 
            // HDTCreate_btn
            // 
            this.HDTCreate_btn.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.HDTCreate_btn.Image = global::OMNIforOutlook.Properties.Resources.Add;
            this.HDTCreate_btn.Label = "Create Ticket";
            this.HDTCreate_btn.Name = "HDTCreate_btn";
            this.HDTCreate_btn.ShowImage = true;
            this.HDTCreate_btn.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.HDTCreate_btn_Click);
            // 
            // HDTViewLog_btn
            // 
            this.HDTViewLog_btn.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.HDTViewLog_btn.Image = global::OMNIforOutlook.Properties.Resources.IT;
            this.HDTViewLog_btn.Label = "View Log";
            this.HDTViewLog_btn.Name = "HDTViewLog_btn";
            this.HDTViewLog_btn.ShowImage = true;
            // 
            // MainRibbon
            // 
            this.Name = "MainRibbon";
            this.RibbonType = resources.GetString("$this.RibbonType");
            this.Tabs.Add(this.MainTab);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.MainRibbon_Load);
            this.MainTab.ResumeLayout(false);
            this.MainTab.PerformLayout();
            this.HDTGroup.ResumeLayout(false);
            this.HDTGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab MainTab;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup HDTGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton HDTCreate_btn;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton HDTViewLog_btn;
    }

    partial class ThisRibbonCollection
    {
        internal MainRibbon MainRibbon
        {
            get { return this.GetRibbon<MainRibbon>(); }
        }
    }
}
