namespace CodeAppsDataMigration
{
    partial class TableCountCompareForm
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

            pnlConfig = new Panel();
            lblPgMain = new Label();
            cmbPgMainBranch = new ComboBox();
            lblSqlBranch = new Label();
            cmbSqlBranch = new ComboBox();
            lblPgBranch = new Label();
            cmbPgBranch = new ComboBox();
            btnCompare = new Button();
            btnExport = new Button();

            grdCounts = new DataGridView();
            colSqlTable = new DataGridViewTextBoxColumn();
            colSqlCount = new DataGridViewTextBoxColumn();
            colPgTable = new DataGridViewTextBoxColumn();
            colPgCount = new DataGridViewTextBoxColumn();
            colDiff = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();

            pnlFooter = new Panel();
            lblSummary = new Label();
            progressBar = new ProgressBar();
            lblStatus = new Label();

            pnlHeader.SuspendLayout();
            pnlConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grdCounts).BeginInit();
            pnlFooter.SuspendLayout();
            SuspendLayout();

            // ========== Header ==========
            pnlHeader.BackColor = Color.FromArgb(45, 55, 72);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Size = new Size(1000, 50);
            pnlHeader.Controls.Add(lblTitle);

            lblTitle.Text = "Table Row Count Comparison  -  SQL Server vs PostgreSQL";
            lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 13);

            // ========== Config Panel ==========
            pnlConfig.BackColor = Color.FromArgb(247, 250, 252);
            pnlConfig.Dock = DockStyle.Top;
            pnlConfig.Size = new Size(1000, 80);
            pnlConfig.Padding = new Padding(15, 15, 15, 15);
            pnlConfig.Controls.Add(btnExport);
            pnlConfig.Controls.Add(btnCompare);
            pnlConfig.Controls.Add(cmbPgBranch);
            pnlConfig.Controls.Add(lblPgBranch);
            pnlConfig.Controls.Add(cmbSqlBranch);
            pnlConfig.Controls.Add(lblSqlBranch);
            pnlConfig.Controls.Add(cmbPgMainBranch);
            pnlConfig.Controls.Add(lblPgMain);

            lblPgMain.Text = "PG Main Branch";
            lblPgMain.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPgMain.Location = new Point(20, 12);
            lblPgMain.Size = new Size(120, 18);

            cmbPgMainBranch.Location = new Point(20, 32);
            cmbPgMainBranch.Size = new Size(200, 25);
            cmbPgMainBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPgMainBranch.Font = new Font("Segoe UI", 10F);
            cmbPgMainBranch.SelectedIndexChanged += cmbPgMainBranch_SelectedIndexChanged;

            lblSqlBranch.Text = "SQL Branch (From)";
            lblSqlBranch.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSqlBranch.Location = new Point(240, 12);
            lblSqlBranch.Size = new Size(150, 18);

            cmbSqlBranch.Location = new Point(240, 32);
            cmbSqlBranch.Size = new Size(220, 25);
            cmbSqlBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSqlBranch.Font = new Font("Segoe UI", 10F);

            lblPgBranch.Text = "PG Branch (To)";
            lblPgBranch.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPgBranch.Location = new Point(480, 12);
            lblPgBranch.Size = new Size(150, 18);

            cmbPgBranch.Location = new Point(480, 32);
            cmbPgBranch.Size = new Size(220, 25);
            cmbPgBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPgBranch.Font = new Font("Segoe UI", 10F);

            btnCompare.Text = "Compare Counts";
            btnCompare.Location = new Point(720, 30);
            btnCompare.Size = new Size(130, 30);
            btnCompare.FlatStyle = FlatStyle.Flat;
            btnCompare.FlatAppearance.BorderSize = 0;
            btnCompare.BackColor = Color.FromArgb(66, 153, 225);
            btnCompare.ForeColor = Color.White;
            btnCompare.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCompare.Cursor = Cursors.Hand;
            btnCompare.Click += btnCompare_Click;

            btnExport.Text = "Export CSV";
            btnExport.Location = new Point(860, 30);
            btnExport.Size = new Size(110, 30);
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.BackColor = Color.FromArgb(72, 187, 120);
            btnExport.ForeColor = Color.White;
            btnExport.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExport.Cursor = Cursors.Hand;
            btnExport.Enabled = false;
            btnExport.Click += btnExport_Click;

            // ========== Grid ==========
            grdCounts.Dock = DockStyle.Fill;
            grdCounts.BackgroundColor = Color.White;
            grdCounts.BorderStyle = BorderStyle.None;
            grdCounts.AllowUserToAddRows = false;
            grdCounts.AllowUserToDeleteRows = false;
            grdCounts.ReadOnly = true;
            grdCounts.RowHeadersVisible = false;
            grdCounts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grdCounts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grdCounts.Font = new Font("Segoe UI", 9.5F);
            grdCounts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(237, 242, 247);
            grdCounts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grdCounts.ColumnHeadersHeight = 32;
            grdCounts.EnableHeadersVisualStyles = false;
            grdCounts.Columns.AddRange(new DataGridViewColumn[]
            {
                colSqlTable, colSqlCount, colPgTable, colPgCount, colDiff, colStatus
            });

            colSqlTable.HeaderText = "SQL Table";
            colSqlTable.Name = "colSqlTable";
            colSqlTable.FillWeight = 22;

            colSqlCount.HeaderText = "SQL Count";
            colSqlCount.Name = "colSqlCount";
            colSqlCount.FillWeight = 13;
            colSqlCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            colPgTable.HeaderText = "PG Table";
            colPgTable.Name = "colPgTable";
            colPgTable.FillWeight = 25;

            colPgCount.HeaderText = "PG Count";
            colPgCount.Name = "colPgCount";
            colPgCount.FillWeight = 13;
            colPgCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            colDiff.HeaderText = "Diff (SQL-PG)";
            colDiff.Name = "colDiff";
            colDiff.FillWeight = 13;
            colDiff.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            colStatus.HeaderText = "Status";
            colStatus.Name = "colStatus";
            colStatus.FillWeight = 14;

            // ========== Footer ==========
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Size = new Size(1000, 60);
            pnlFooter.BackColor = Color.FromArgb(247, 250, 252);
            pnlFooter.Padding = new Padding(15, 8, 15, 8);
            pnlFooter.Controls.Add(lblStatus);
            pnlFooter.Controls.Add(progressBar);
            pnlFooter.Controls.Add(lblSummary);

            lblSummary.Text = "Ready.";
            lblSummary.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblSummary.ForeColor = Color.FromArgb(45, 55, 72);
            lblSummary.Location = new Point(15, 8);
            lblSummary.Size = new Size(700, 18);

            progressBar.Location = new Point(15, 30);
            progressBar.Size = new Size(700, 18);

            lblStatus.Location = new Point(725, 30);
            lblStatus.Size = new Size(260, 18);
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(74, 85, 104);
            lblStatus.Text = "";

            // ========== Form ==========
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 600);
            Controls.Add(grdCounts);
            Controls.Add(pnlFooter);
            Controls.Add(pnlConfig);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(900, 500);
            Name = "TableCountCompareForm";
            Text = "Table Row Count Comparison";
            Load += TableCountCompareForm_Load;

            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlConfig.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)grdCounts).EndInit();
            pnlFooter.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label lblTitle;

        private Panel pnlConfig;
        private Label lblPgMain;
        private ComboBox cmbPgMainBranch;
        private Label lblSqlBranch;
        private ComboBox cmbSqlBranch;
        private Label lblPgBranch;
        private ComboBox cmbPgBranch;
        private Button btnCompare;
        private Button btnExport;

        private DataGridView grdCounts;
        private DataGridViewTextBoxColumn colSqlTable;
        private DataGridViewTextBoxColumn colSqlCount;
        private DataGridViewTextBoxColumn colPgTable;
        private DataGridViewTextBoxColumn colPgCount;
        private DataGridViewTextBoxColumn colDiff;
        private DataGridViewTextBoxColumn colStatus;

        private Panel pnlFooter;
        private Label lblSummary;
        private ProgressBar progressBar;
        private Label lblStatus;
    }
}
