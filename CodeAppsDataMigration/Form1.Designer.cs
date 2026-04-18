namespace CodeAppsDataMigration
{
    partial class Form1
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
            lblSqlBranch = new Label();
            cmbSqlBranch = new ComboBox();
            lblPgMainBranch = new Label();
            cmbPgMainBranch = new ComboBox();
            lblPgBranch = new Label();
            cmbPgBranch = new ComboBox();
            btnDataTransfer = new Button();
            progressBar = new ProgressBar();
            lblStatus = new Label();
            SuspendLayout();
            //
            // lblSqlBranch
            //
            lblSqlBranch.AutoSize = true;
            lblSqlBranch.Location = new Point(30, 25);
            lblSqlBranch.Name = "lblSqlBranch";
            lblSqlBranch.Size = new Size(120, 15);
            lblSqlBranch.TabIndex = 1;
            lblSqlBranch.Text = "SQL Server Branch (From):";
            //
            // cmbSqlBranch
            //
            cmbSqlBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSqlBranch.FormattingEnabled = true;
            cmbSqlBranch.Location = new Point(220, 22);
            cmbSqlBranch.Name = "cmbSqlBranch";
            cmbSqlBranch.Size = new Size(250, 23);
            cmbSqlBranch.TabIndex = 2;
            //
            // lblPgMainBranch
            //
            lblPgMainBranch.AutoSize = true;
            lblPgMainBranch.Location = new Point(30, 65);
            lblPgMainBranch.Name = "lblPgMainBranch";
            lblPgMainBranch.Size = new Size(150, 15);
            lblPgMainBranch.TabIndex = 3;
            lblPgMainBranch.Text = "PostgreSQL Main Branch:";
            //
            // cmbPgMainBranch
            //
            cmbPgMainBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPgMainBranch.FormattingEnabled = true;
            cmbPgMainBranch.Location = new Point(220, 62);
            cmbPgMainBranch.Name = "cmbPgMainBranch";
            cmbPgMainBranch.Size = new Size(250, 23);
            cmbPgMainBranch.TabIndex = 4;
            cmbPgMainBranch.SelectedIndexChanged += cmbPgMainBranch_SelectedIndexChanged;
            //
            // lblPgBranch
            //
            lblPgBranch.AutoSize = true;
            lblPgBranch.Location = new Point(30, 105);
            lblPgBranch.Name = "lblPgBranch";
            lblPgBranch.Size = new Size(120, 15);
            lblPgBranch.TabIndex = 5;
            lblPgBranch.Text = "PostgreSQL Branch (To):";
            //
            // cmbPgBranch
            //
            cmbPgBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPgBranch.FormattingEnabled = true;
            cmbPgBranch.Location = new Point(220, 102);
            cmbPgBranch.Name = "cmbPgBranch";
            cmbPgBranch.Size = new Size(250, 23);
            cmbPgBranch.TabIndex = 6;
            //
            // btnDataTransfer
            //
            btnDataTransfer.Location = new Point(180, 150);
            btnDataTransfer.Name = "btnDataTransfer";
            btnDataTransfer.Size = new Size(179, 30);
            btnDataTransfer.TabIndex = 0;
            btnDataTransfer.Text = "Start Migration";
            btnDataTransfer.UseVisualStyleBackColor = true;
            btnDataTransfer.Click += btnDataTransfer_Click;
            //
            // progressBar
            //
            progressBar.Location = new Point(30, 200);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(460, 25);
            progressBar.TabIndex = 7;
            //
            // lblStatus
            //
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(30, 232);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(50, 15);
            lblStatus.TabIndex = 8;
            lblStatus.Text = "Ready";
            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(520, 260);
            Controls.Add(lblSqlBranch);
            Controls.Add(cmbSqlBranch);
            Controls.Add(lblPgMainBranch);
            Controls.Add(cmbPgMainBranch);
            Controls.Add(lblPgBranch);
            Controls.Add(cmbPgBranch);
            Controls.Add(btnDataTransfer);
            Controls.Add(progressBar);
            Controls.Add(lblStatus);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Data Migration";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnDataTransfer;
        private Label lblSqlBranch;
        private ComboBox cmbSqlBranch;
        private Label lblPgMainBranch;
        private ComboBox cmbPgMainBranch;
        private Label lblPgBranch;
        private ComboBox cmbPgBranch;
        private ProgressBar progressBar;
        private Label lblStatus;
    }
}
