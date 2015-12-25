namespace EnIPExplorer
{
    partial class MainForm
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.devicesTreeView = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.DeviceLabel = new System.Windows.Forms.Label();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.PropertiesLabel = new System.Windows.Forms.Label();
            this.LogText = new System.Windows.Forms.TextBox();
            this.LogLabel = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.functionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openInterfaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendListIdentityDiscoverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.addClassInstanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addInstanceAttributToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.LogText);
            this.splitContainer1.Panel2.Controls.Add(this.LogLabel);
            this.splitContainer1.Size = new System.Drawing.Size(619, 589);
            this.splitContainer1.SplitterDistance = 491;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.devicesTreeView);
            this.splitContainer2.Panel1.Controls.Add(this.DeviceLabel);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.propertyGrid);
            this.splitContainer2.Panel2.Controls.Add(this.PropertiesLabel);
            this.splitContainer2.Size = new System.Drawing.Size(619, 491);
            this.splitContainer2.SplitterDistance = 298;
            this.splitContainer2.TabIndex = 0;
            // 
            // devicesTreeView
            // 
            this.devicesTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.devicesTreeView.ImageIndex = 0;
            this.devicesTreeView.ImageList = this.imageList1;
            this.devicesTreeView.Location = new System.Drawing.Point(0, 13);
            this.devicesTreeView.Name = "devicesTreeView";
            this.devicesTreeView.SelectedImageIndex = 0;
            this.devicesTreeView.Size = new System.Drawing.Size(298, 478);
            this.devicesTreeView.TabIndex = 1;
            this.devicesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.devicesTreeView_AfterSelect);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Plc.png");
            this.imageList1.Images.SetKeyName(1, "bullet_wrench.png");
            this.imageList1.Images.SetKeyName(2, "layout.png");
            this.imageList1.Images.SetKeyName(3, "information.png");
            this.imageList1.Images.SetKeyName(4, "arrow_out.png");
            this.imageList1.Images.SetKeyName(5, "computer.png");
            this.imageList1.Images.SetKeyName(6, "text_list_bullets.png");
            this.imageList1.Images.SetKeyName(7, "disconnect.png");
            this.imageList1.Images.SetKeyName(8, "bullet_purple.png");
            this.imageList1.Images.SetKeyName(9, "pencil.png");
            // 
            // DeviceLabel
            // 
            this.DeviceLabel.AutoSize = true;
            this.DeviceLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.DeviceLabel.Location = new System.Drawing.Point(0, 0);
            this.DeviceLabel.Name = "DeviceLabel";
            this.DeviceLabel.Size = new System.Drawing.Size(46, 13);
            this.DeviceLabel.TabIndex = 0;
            this.DeviceLabel.Text = "Devices";
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.HelpVisible = false;
            this.propertyGrid.Location = new System.Drawing.Point(0, 13);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(317, 478);
            this.propertyGrid.TabIndex = 1;
            this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            // 
            // PropertiesLabel
            // 
            this.PropertiesLabel.AutoSize = true;
            this.PropertiesLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.PropertiesLabel.Location = new System.Drawing.Point(0, 0);
            this.PropertiesLabel.Name = "PropertiesLabel";
            this.PropertiesLabel.Size = new System.Drawing.Size(54, 13);
            this.PropertiesLabel.TabIndex = 0;
            this.PropertiesLabel.Text = "Properties";
            // 
            // LogText
            // 
            this.LogText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogText.Location = new System.Drawing.Point(0, 13);
            this.LogText.Multiline = true;
            this.LogText.Name = "LogText";
            this.LogText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.LogText.Size = new System.Drawing.Size(619, 81);
            this.LogText.TabIndex = 2;
            // 
            // LogLabel
            // 
            this.LogLabel.AutoSize = true;
            this.LogLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.LogLabel.Location = new System.Drawing.Point(0, 0);
            this.LogLabel.Name = "LogLabel";
            this.LogLabel.Size = new System.Drawing.Size(25, 13);
            this.LogLabel.TabIndex = 1;
            this.LogLabel.Text = "Log";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.functionsToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(619, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Image = global::EnIPExplorer.Properties.Resources.cross;
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(97, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // functionsToolStripMenuItem
            // 
            this.functionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openInterfaceToolStripMenuItem,
            this.sendListIdentityDiscoverToolStripMenuItem,
            this.toolStripSeparator1,
            this.addClassInstanceToolStripMenuItem,
            this.addInstanceAttributToolStripMenuItem});
            this.functionsToolStripMenuItem.Name = "functionsToolStripMenuItem";
            this.functionsToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
            this.functionsToolStripMenuItem.Text = "Functions";
            // 
            // openInterfaceToolStripMenuItem
            // 
            this.openInterfaceToolStripMenuItem.Image = global::EnIPExplorer.Properties.Resources.add;
            this.openInterfaceToolStripMenuItem.Name = "openInterfaceToolStripMenuItem";
            this.openInterfaceToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.openInterfaceToolStripMenuItem.Text = "Open Interface";
            this.openInterfaceToolStripMenuItem.Click += new System.EventHandler(this.openInterfaceToolStripMenuItem_Click);
            // 
            // sendListIdentityDiscoverToolStripMenuItem
            // 
            this.sendListIdentityDiscoverToolStripMenuItem.Enabled = false;
            this.sendListIdentityDiscoverToolStripMenuItem.Image = global::EnIPExplorer.Properties.Resources.magnifier;
            this.sendListIdentityDiscoverToolStripMenuItem.Name = "sendListIdentityDiscoverToolStripMenuItem";
            this.sendListIdentityDiscoverToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.sendListIdentityDiscoverToolStripMenuItem.Text = "Send ListIdentity (Discover)";
            this.sendListIdentityDiscoverToolStripMenuItem.Click += new System.EventHandler(this.sendListIdentityDiscoverToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(226, 6);
            // 
            // addClassInstanceToolStripMenuItem
            // 
            this.addClassInstanceToolStripMenuItem.Image = global::EnIPExplorer.Properties.Resources.pencil;
            this.addClassInstanceToolStripMenuItem.Name = "addClassInstanceToolStripMenuItem";
            this.addClassInstanceToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.addClassInstanceToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.addClassInstanceToolStripMenuItem.Text = "Add Class Instance";
            this.addClassInstanceToolStripMenuItem.Click += new System.EventHandler(this.addClassInstanceToolStripMenuItem_Click);
            // 
            // addInstanceAttributToolStripMenuItem
            // 
            this.addInstanceAttributToolStripMenuItem.Image = global::EnIPExplorer.Properties.Resources.bullet_purple;
            this.addInstanceAttributToolStripMenuItem.Name = "addInstanceAttributToolStripMenuItem";
            this.addInstanceAttributToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.addInstanceAttributToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.addInstanceAttributToolStripMenuItem.Text = "Add Instance Attribut";
            this.addInstanceAttributToolStripMenuItem.Click += new System.EventHandler(this.addInstanceAttributToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Image = global::EnIPExplorer.Properties.Resources.application_form;
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem1,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // helpToolStripMenuItem1
            // 
            this.helpToolStripMenuItem1.Image = global::EnIPExplorer.Properties.Resources.information;
            this.helpToolStripMenuItem1.Name = "helpToolStripMenuItem1";
            this.helpToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
            this.helpToolStripMenuItem1.Text = "Help";
            this.helpToolStripMenuItem1.Click += new System.EventHandler(this.helpToolStripMenuItem1_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Image = global::EnIPExplorer.Properties.Resources.information;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(619, 613);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Ethernet/IP Explorer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Label DeviceLabel;
        internal System.Windows.Forms.TextBox LogText;
        private System.Windows.Forms.Label LogLabel;
        private System.Windows.Forms.TreeView devicesTreeView;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.Label PropertiesLabel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem functionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openInterfaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendListIdentityDiscoverToolStripMenuItem;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem addClassInstanceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addInstanceAttributToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
    }
}

