namespace CodeAppsDataMigration
{
    partial class DataMigrationConvertForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblPgMainBranch = new Label();
            cmbPgMainBranch = new ComboBox();
            grdBranchMap = new DataGridView();
            colSqlBranch = new DataGridViewTextBoxColumn();
            colPgBranch = new DataGridViewComboBoxColumn();
            btnDataTransfer = new Button();
            progressBar = new ProgressBar();
            lblStatus = new Label();
            ((System.ComponentModel.ISupportInitialize)grdBranchMap).BeginInit();
            SuspendLayout();
            //
            // lblPgMainBranch
            //
            lblPgMainBranch.AutoSize = true;
            lblPgMainBranch.Location = new Point(15, 18);
            lblPgMainBranch.Name = "lblPgMainBranch";
            lblPgMainBranch.Size = new Size(120, 15);
            lblPgMainBranch.TabIndex = 0;
            lblPgMainBranch.Text = "PostgreSQL Main Branch:";
            //
            // cmbPgMainBranch
            //
            cmbPgMainBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPgMainBranch.FormattingEnabled = true;
            cmbPgMainBranch.Location = new Point(150, 15);
            cmbPgMainBranch.Name = "cmbPgMainBranch";
            cmbPgMainBranch.Size = new Size(300, 23);
            cmbPgMainBranch.TabIndex = 1;
            cmbPgMainBranch.SelectedIndexChanged += cmbPgMainBranch_SelectedIndexChanged;
            //
            // grdBranchMap
            //
            grdBranchMap.AllowUserToAddRows = false;
            grdBranchMap.AllowUserToDeleteRows = false;
            grdBranchMap.AllowUserToResizeRows = false;
            grdBranchMap.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grdBranchMap.Columns.AddRange(new DataGridViewColumn[] { colSqlBranch, colPgBranch });
            grdBranchMap.Location = new Point(15, 55);
            grdBranchMap.Name = "grdBranchMap";
            grdBranchMap.RowHeadersVisible = false;
            grdBranchMap.SelectionMode = DataGridViewSelectionMode.CellSelect;
            grdBranchMap.Size = new Size(770, 350);
            grdBranchMap.TabIndex = 2;
            //
            // colSqlBranch
            //
            colSqlBranch.HeaderText = "SQL Server Branch (From)";
            colSqlBranch.Name = "colSqlBranch";
            colSqlBranch.ReadOnly = true;
            colSqlBranch.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colSqlBranch.FillWeight = 50F;
            //
            // colPgBranch
            //
            colPgBranch.HeaderText = "PostgreSQL Branch (To)";
            colPgBranch.Name = "colPgBranch";
            colPgBranch.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colPgBranch.FillWeight = 50F;
            colPgBranch.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            //
            // btnDataTransfer
            //
            btnDataTransfer.Location = new Point(610, 415);
            btnDataTransfer.Name = "btnDataTransfer";
            btnDataTransfer.Size = new Size(175, 30);
            btnDataTransfer.TabIndex = 3;
            btnDataTransfer.Text = "Start Migration";
            btnDataTransfer.UseVisualStyleBackColor = true;
            btnDataTransfer.Click += btnDataTransfer_Click;
            //
            // progressBar
            //
            progressBar.Location = new Point(15, 460);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(770, 25);
            progressBar.TabIndex = 4;
            //
            // lblStatus
            //
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(15, 495);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(39, 15);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "Ready";
            //
            // DataMigrationConvertForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 525);
            Controls.Add(lblPgMainBranch);
            Controls.Add(cmbPgMainBranch);
            Controls.Add(grdBranchMap);
            Controls.Add(btnDataTransfer);
            Controls.Add(progressBar);
            Controls.Add(lblStatus);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "DataMigrationConvertForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Data Migration";
            Load += DataMigrationConvertForm_Load;
            ((System.ComponentModel.ISupportInitialize)grdBranchMap).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblPgMainBranch;
        private ComboBox cmbPgMainBranch;
        private DataGridView grdBranchMap;
        private DataGridViewTextBoxColumn colSqlBranch;
        private DataGridViewComboBoxColumn colPgBranch;
        private Button btnDataTransfer;
        private ProgressBar progressBar;
        private Label lblStatus;
    }
}
