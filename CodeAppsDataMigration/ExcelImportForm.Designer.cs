namespace CodeAppsDataMigration
{
    partial class ExcelImportForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            lblTitle = new Label();
            pnlTop = new Panel();
            lblFile = new Label();
            txtFilePath = new TextBox();
            btnBrowse = new Button();
            lblDatabase = new Label();
            cmbDatabase = new ComboBox();
            btnLoadDbs = new Button();
            lblTableName = new Label();
            txtTableName = new TextBox();
            lblSheet = new Label();
            cmbSheet = new ComboBox();
            pnlGrid = new Panel();
            dgvData = new DataGridView();
            pnlBottom = new Panel();
            btnSave = new Button();
            btnCancel = new Button();
            lblStatus = new Label();
            progressBar = new ProgressBar();

            pnlHeader.SuspendLayout();
            pnlTop.SuspendLayout();
            pnlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvData).BeginInit();
            pnlBottom.SuspendLayout();
            SuspendLayout();

            // ========== Header Panel ==========
            pnlHeader.BackColor = Color.FromArgb(45, 55, 72);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Size = new Size(1100, 50);
            pnlHeader.Controls.Add(lblTitle);

            lblTitle.Text = "Excel Import to PostgreSQL";
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(16, 10);

            // ========== Top Panel (Controls) ==========
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Size = new Size(1100, 110);
            pnlTop.BackColor = Color.FromArgb(237, 242, 247);
            pnlTop.Padding = new Padding(12, 8, 12, 8);
            pnlTop.Controls.Add(lblFile);
            pnlTop.Controls.Add(txtFilePath);
            pnlTop.Controls.Add(btnBrowse);
            pnlTop.Controls.Add(lblDatabase);
            pnlTop.Controls.Add(cmbDatabase);
            pnlTop.Controls.Add(btnLoadDbs);
            pnlTop.Controls.Add(lblTableName);
            pnlTop.Controls.Add(txtTableName);
            pnlTop.Controls.Add(lblSheet);
            pnlTop.Controls.Add(cmbSheet);

            // Row 1: File browse
            lblFile.Text = "Excel File:";
            lblFile.Font = new Font("Segoe UI", 10F);
            lblFile.AutoSize = true;
            lblFile.Location = new Point(15, 14);

            txtFilePath.Location = new Point(110, 11);
            txtFilePath.Size = new Size(500, 27);
            txtFilePath.ReadOnly = true;
            txtFilePath.Font = new Font("Segoe UI", 10F);
            txtFilePath.BackColor = Color.White;

            btnBrowse.Text = "Browse...";
            btnBrowse.Location = new Point(620, 10);
            btnBrowse.Size = new Size(90, 30);
            btnBrowse.FlatStyle = FlatStyle.Flat;
            btnBrowse.BackColor = Color.FromArgb(66, 153, 225);
            btnBrowse.ForeColor = Color.White;
            btnBrowse.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnBrowse.Cursor = Cursors.Hand;
            btnBrowse.Click += btnBrowse_Click;

            lblSheet.Text = "Sheet:";
            lblSheet.Font = new Font("Segoe UI", 10F);
            lblSheet.AutoSize = true;
            lblSheet.Location = new Point(725, 14);

            cmbSheet.Location = new Point(780, 11);
            cmbSheet.Size = new Size(200, 27);
            cmbSheet.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSheet.Font = new Font("Segoe UI", 10F);
            cmbSheet.SelectedIndexChanged += cmbSheet_SelectedIndexChanged;

            // Row 2: Database, Table Name
            lblDatabase.Text = "Database:";
            lblDatabase.Font = new Font("Segoe UI", 10F);
            lblDatabase.AutoSize = true;
            lblDatabase.Location = new Point(15, 58);

            cmbDatabase.Location = new Point(110, 55);
            cmbDatabase.Size = new Size(250, 27);
            cmbDatabase.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDatabase.Font = new Font("Segoe UI", 10F);

            btnLoadDbs.Text = "Load DBs";
            btnLoadDbs.Location = new Point(370, 54);
            btnLoadDbs.Size = new Size(90, 30);
            btnLoadDbs.FlatStyle = FlatStyle.Flat;
            btnLoadDbs.BackColor = Color.FromArgb(72, 187, 120);
            btnLoadDbs.ForeColor = Color.White;
            btnLoadDbs.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLoadDbs.Cursor = Cursors.Hand;
            btnLoadDbs.Click += btnLoadDbs_Click;

            lblTableName.Text = "Table Name:";
            lblTableName.Font = new Font("Segoe UI", 10F);
            lblTableName.AutoSize = true;
            lblTableName.Location = new Point(480, 58);

            txtTableName.Location = new Point(585, 55);
            txtTableName.Size = new Size(250, 27);
            txtTableName.Font = new Font("Segoe UI", 10F);

            // ========== Grid Panel ==========
            pnlGrid.Dock = DockStyle.Fill;
            pnlGrid.Padding = new Padding(10);
            pnlGrid.Controls.Add(dgvData);

            dgvData.Dock = DockStyle.Fill;
            dgvData.AllowUserToAddRows = false;
            dgvData.AllowUserToDeleteRows = false;
            dgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvData.BackgroundColor = Color.White;
            dgvData.BorderStyle = BorderStyle.Fixed3D;
            dgvData.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 55, 72);
            dgvData.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvData.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvData.ColumnHeadersHeight = 35;
            dgvData.EnableHeadersVisualStyles = false;
            dgvData.RowHeadersVisible = false;
            dgvData.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvData.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);

            // ========== Bottom Panel ==========
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Size = new Size(1100, 55);
            pnlBottom.BackColor = Color.FromArgb(237, 242, 247);
            pnlBottom.Controls.Add(btnSave);
            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Controls.Add(lblStatus);
            pnlBottom.Controls.Add(progressBar);

            lblStatus.Text = "Ready";
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(15, 18);

            progressBar.Location = new Point(300, 15);
            progressBar.Size = new Size(400, 25);
            progressBar.Visible = false;

            btnSave.Text = "Save to PostgreSQL";
            btnSave.Location = new Point(830, 10);
            btnSave.Size = new Size(150, 35);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.BackColor = Color.FromArgb(66, 153, 225);
            btnSave.ForeColor = Color.White;
            btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSave.Cursor = Cursors.Hand;
            btnSave.Click += btnSave_Click;

            btnCancel.Text = "Close";
            btnCancel.Location = new Point(990, 10);
            btnCancel.Size = new Size(90, 35);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.BackColor = Color.FromArgb(229, 62, 62);
            btnCancel.ForeColor = Color.White;
            btnCancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.Click += btnCancel_Click;

            // ========== Form ==========
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 650);
            Controls.Add(pnlGrid);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(900, 500);
            Name = "ExcelImportForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Excel Import to PostgreSQL";

            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvData).EndInit();
            pnlBottom.ResumeLayout(false);
            pnlBottom.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label lblTitle;
        private Panel pnlTop;
        private Label lblFile;
        private TextBox txtFilePath;
        private Button btnBrowse;
        private Label lblDatabase;
        private ComboBox cmbDatabase;
        private Button btnLoadDbs;
        private Label lblTableName;
        private TextBox txtTableName;
        private Label lblSheet;
        private ComboBox cmbSheet;
        private Panel pnlGrid;
        private DataGridView dgvData;
        private Panel pnlBottom;
        private Button btnSave;
        private Button btnCancel;
        private Label lblStatus;
        private ProgressBar progressBar;
    }
}
