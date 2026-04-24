namespace CodeAppsDataMigration
{
    partial class BranchDetailsLoadForm
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
            pnlTop = new Panel();
            lblTitle = new Label();
            btnRefresh = new Button();
            dgvBranchDetails = new DataGridView();
            pnlBottom = new Panel();
            lblStatus = new Label();

            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBranchDetails).BeginInit();
            pnlBottom.SuspendLayout();
            SuspendLayout();

            //
            // pnlTop
            //
            pnlTop.BackColor = Color.FromArgb(45, 55, 72);
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(btnRefresh);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Size = new Size(850, 50);

            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(15, 12);
            lblTitle.Text = "SQL Server - Branch Details";

            //
            // btnRefresh
            //
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.BackColor = Color.FromArgb(66, 153, 225);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.Location = new Point(740, 10);
            btnRefresh.Size = new Size(95, 30);
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.Click += btnRefresh_Click;

            //
            // dgvBranchDetails
            //
            dgvBranchDetails.AllowUserToAddRows = false;
            dgvBranchDetails.AllowUserToDeleteRows = false;
            dgvBranchDetails.ReadOnly = true;
            dgvBranchDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBranchDetails.BackgroundColor = Color.White;
            dgvBranchDetails.BorderStyle = BorderStyle.None;
            dgvBranchDetails.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvBranchDetails.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(237, 242, 247);
            dgvBranchDetails.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvBranchDetails.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(45, 55, 72);
            dgvBranchDetails.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(237, 242, 247);
            dgvBranchDetails.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(45, 55, 72);
            dgvBranchDetails.ColumnHeadersHeight = 35;
            dgvBranchDetails.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvBranchDetails.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            dgvBranchDetails.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 248, 255);
            dgvBranchDetails.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvBranchDetails.Dock = DockStyle.Fill;
            dgvBranchDetails.EnableHeadersVisualStyles = false;
            dgvBranchDetails.GridColor = Color.FromArgb(226, 232, 240);
            dgvBranchDetails.Name = "dgvBranchDetails";
            dgvBranchDetails.RowHeadersVisible = false;
            dgvBranchDetails.RowTemplate.Height = 32;
            dgvBranchDetails.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBranchDetails.CellContentClick += dgvBranchDetails_CellContentClick;

            //
            // pnlBottom
            //
            pnlBottom.BackColor = Color.FromArgb(237, 242, 247);
            pnlBottom.Controls.Add(lblStatus);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Size = new Size(850, 30);

            //
            // lblStatus
            //
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(113, 128, 150);
            lblStatus.Location = new Point(10, 7);
            lblStatus.Text = "Ready";

            //
            // BranchDetailsLoadForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(850, 500);
            Controls.Add(dgvBranchDetails);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);
            Name = "BranchDetailsLoadForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Branch Details - SQL Server";
            Load += BranchDetailsLoadForm_Load;

            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBranchDetails).EndInit();
            pnlBottom.ResumeLayout(false);
            pnlBottom.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTop;
        private Label lblTitle;
        private Button btnRefresh;
        private DataGridView dgvBranchDetails;
        private Panel pnlBottom;
        private Label lblStatus;
    }
}
