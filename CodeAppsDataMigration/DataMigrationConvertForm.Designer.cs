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
            grpSqlServer = new GroupBox();
            lblSqlBranch = new Label();
            cmbSqlBranch = new ComboBox();
            grpPostgres = new GroupBox();
            lblPgMainBranch = new Label();
            cmbPgMainBranch = new ComboBox();
            lblPgBranch = new Label();
            cmbPgBranch = new ComboBox();
            btnDataTransfer = new Button();
            progressBar = new ProgressBar();
            lblStatus = new Label();
            grpSqlServer.SuspendLayout();
            grpPostgres.SuspendLayout();
            SuspendLayout();
            //
            // grpSqlServer
            //
            grpSqlServer.Controls.Add(lblSqlBranch);
            grpSqlServer.Controls.Add(cmbSqlBranch);
            grpSqlServer.Location = new Point(15, 12);
            grpSqlServer.Name = "grpSqlServer";
            grpSqlServer.Size = new Size(320, 100);
            grpSqlServer.TabIndex = 0;
            grpSqlServer.TabStop = false;
            grpSqlServer.Text = "SQL Server (From)";
            //
            // lblSqlBranch
            //
            lblSqlBranch.AutoSize = true;
            lblSqlBranch.Location = new Point(15, 30);
            lblSqlBranch.Name = "lblSqlBranch";
            lblSqlBranch.Size = new Size(50, 15);
            lblSqlBranch.TabIndex = 0;
            lblSqlBranch.Text = "Branch:";
            //
            // cmbSqlBranch
            //
            cmbSqlBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSqlBranch.FormattingEnabled = true;
            cmbSqlBranch.Location = new Point(15, 55);
            cmbSqlBranch.Name = "cmbSqlBranch";
            cmbSqlBranch.Size = new Size(290, 23);
            cmbSqlBranch.TabIndex = 1;
            //
            // grpPostgres
            //
            grpPostgres.Controls.Add(lblPgMainBranch);
            grpPostgres.Controls.Add(cmbPgMainBranch);
            grpPostgres.Controls.Add(lblPgBranch);
            grpPostgres.Controls.Add(cmbPgBranch);
            grpPostgres.Location = new Point(350, 12);
            grpPostgres.Name = "grpPostgres";
            grpPostgres.Size = new Size(320, 100);
            grpPostgres.TabIndex = 1;
            grpPostgres.TabStop = false;
            grpPostgres.Text = "PostgreSQL (To)";
            //
            // lblPgMainBranch
            //
            lblPgMainBranch.AutoSize = true;
            lblPgMainBranch.Location = new Point(15, 30);
            lblPgMainBranch.Name = "lblPgMainBranch";
            lblPgMainBranch.Size = new Size(80, 15);
            lblPgMainBranch.TabIndex = 0;
            lblPgMainBranch.Text = "Main Branch:";
            //
            // cmbPgMainBranch
            //
            cmbPgMainBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPgMainBranch.FormattingEnabled = true;
            cmbPgMainBranch.Location = new Point(110, 27);
            cmbPgMainBranch.Name = "cmbPgMainBranch";
            cmbPgMainBranch.Size = new Size(195, 23);
            cmbPgMainBranch.TabIndex = 1;
            cmbPgMainBranch.SelectedIndexChanged += cmbPgMainBranch_SelectedIndexChanged;
            //
            // lblPgBranch
            //
            lblPgBranch.AutoSize = true;
            lblPgBranch.Location = new Point(15, 65);
            lblPgBranch.Name = "lblPgBranch";
            lblPgBranch.Size = new Size(75, 15);
            lblPgBranch.TabIndex = 2;
            lblPgBranch.Text = "Sub Branch:";
            //
            // cmbPgBranch
            //
            cmbPgBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPgBranch.FormattingEnabled = true;
            cmbPgBranch.Location = new Point(110, 62);
            cmbPgBranch.Name = "cmbPgBranch";
            cmbPgBranch.Size = new Size(195, 23);
            cmbPgBranch.TabIndex = 3;
            //
            // btnDataTransfer
            //
            btnDataTransfer.Location = new Point(260, 125);
            btnDataTransfer.Name = "btnDataTransfer";
            btnDataTransfer.Size = new Size(179, 30);
            btnDataTransfer.TabIndex = 2;
            btnDataTransfer.Text = "Start Migration";
            btnDataTransfer.UseVisualStyleBackColor = true;
            btnDataTransfer.Click += btnDataTransfer_Click;
            //
            // progressBar
            //
            progressBar.Location = new Point(15, 170);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(655, 25);
            progressBar.TabIndex = 3;
            //
            // lblStatus
            //
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(15, 202);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(50, 15);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "Ready";
            //
            // DataMigrationConvertForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(685, 230);
            Controls.Add(grpSqlServer);
            Controls.Add(grpPostgres);
            Controls.Add(btnDataTransfer);
            Controls.Add(progressBar);
            Controls.Add(lblStatus);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "DataMigrationConvertForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Data Migration";
            Load += DataMigrationConvertForm_Load;
            grpSqlServer.ResumeLayout(false);
            grpSqlServer.PerformLayout();
            grpPostgres.ResumeLayout(false);
            grpPostgres.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox grpSqlServer;
        private Label lblSqlBranch;
        private ComboBox cmbSqlBranch;
        private GroupBox grpPostgres;
        private Label lblPgMainBranch;
        private ComboBox cmbPgMainBranch;
        private Label lblPgBranch;
        private ComboBox cmbPgBranch;
        private Button btnDataTransfer;
        private ProgressBar progressBar;
        private Label lblStatus;
    }
}
