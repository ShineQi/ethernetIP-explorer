namespace EnIPExplorer
{
    partial class DecoderEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DecoderEditor));
            this.AttGridView = new System.Windows.Forms.DataGridView();
            this.btSave = new System.Windows.Forms.Button();
            this.btAbort = new System.Windows.Forms.Button();
            this.TyName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.AttGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // AttGridView
            // 
            this.AttGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.AttGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TyName,
            this.AName,
            this.AType});
            this.AttGridView.Location = new System.Drawing.Point(36, 21);
            this.AttGridView.Name = "AttGridView";
            this.AttGridView.Size = new System.Drawing.Size(443, 319);
            this.AttGridView.TabIndex = 1;
            this.AttGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.AttGridView_CellValidating);
            this.AttGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AttGridView_KeyDown);
            // 
            // btSave
            // 
            this.btSave.Location = new System.Drawing.Point(146, 372);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(97, 23);
            this.btSave.TabIndex = 4;
            this.btSave.Text = "Save && Close";
            this.btSave.UseVisualStyleBackColor = true;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // btAbort
            // 
            this.btAbort.Location = new System.Drawing.Point(269, 372);
            this.btAbort.Name = "btAbort";
            this.btAbort.Size = new System.Drawing.Size(96, 23);
            this.btAbort.TabIndex = 5;
            this.btAbort.Text = "Abort && Close";
            this.btAbort.UseVisualStyleBackColor = true;
            this.btAbort.Click += new System.EventHandler(this.btAbort_Click);
            // 
            // TyName
            // 
            this.TyName.HeaderText = "Type Name";
            this.TyName.Name = "TyName";
            this.TyName.Width = 150;
            // 
            // AName
            // 
            this.AName.HeaderText = "Field Name";
            this.AName.Name = "AName";
            this.AName.Width = 150;
            // 
            // AType
            // 
            this.AType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AType.HeaderText = "CIPType";
            this.AType.Name = "AType";
            // 
            // DecoderEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(517, 422);
            this.Controls.Add(this.btAbort);
            this.Controls.Add(this.btSave);
            this.Controls.Add(this.AttGridView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DecoderEditor";
            this.Text = "Simple User Decoder Editor";
            ((System.ComponentModel.ISupportInitialize)(this.AttGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView AttGridView;
        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.Button btAbort;
        private System.Windows.Forms.DataGridViewTextBoxColumn TyName;
        private System.Windows.Forms.DataGridViewTextBoxColumn AName;
        private System.Windows.Forms.DataGridViewComboBoxColumn AType;

    }
}