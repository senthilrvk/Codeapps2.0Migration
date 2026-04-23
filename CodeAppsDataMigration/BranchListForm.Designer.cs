namespace CodeAppsDataMigration
{
    partial class BranchListForm
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
            dgvBranches = new DataGridView();
            pnlBottom = new Panel();
            lblStatus = new Label();

            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBranches).BeginInit();
            pnlBottom.SuspendLayout();
            SuspendLayout();

            //
            // pnlTop
            //
            pnlTop.BackColor = Color.FromArgb(45, 55, 72);
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(btnRefresh);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Size = new Size(750, 50);

            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(15, 12);
            lblTitle.Text = "SQL Server - Branch List";

            //
            // btnRefresh
            //
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.BackColor = Color.FromArgb(66, 153, 225);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.Location = new Point(640, 10);
            btnRefresh.Size = new Size(95, 30);
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.Click += btnRefresh_Click;

            //
            // dgvBranches
            //
            dgvBranches.AllowUserToAddRows = false;
            dgvBranches.AllowUserToDeleteRows = false;
            dgvBranches.ReadOnly = true;
            dgvBranches.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBranches.BackgroundColor = Color.White;
            dgvBranches.BorderStyle = BorderStyle.None;
            dgvBranches.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvBranches.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(237, 242, 247);
            dgvBranches.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvBranches.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(45, 55, 72);
            dgvBranches.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(237, 242, 247);
            dgvBranches.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(45, 55, 72);
            dgvBranches.ColumnHeadersHeight = 35;
            dgvBranches.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvBranches.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            dgvBranches.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 248, 255);
            dgvBranches.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvBranches.Dock = DockStyle.Fill;
            dgvBranches.EnableHeadersVisualStyles = false;
            dgvBranches.GridColor = Color.FromArgb(226, 232, 240);
            dgvBranches.Name = "dgvBranches";
            dgvBranches.RowHeadersVisible = false;
            dgvBranches.RowTemplate.Height = 32;
            dgvBranches.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBranches.CellContentClick += dgvBranches_CellContentClick;

            //
            // pnlBottom
            //
            pnlBottom.BackColor = Color.FromArgb(237, 242, 247);
            pnlBottom.Controls.Add(lblStatus);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Size = new Size(750, 30);

            //
            // lblStatus
            //
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(113, 128, 150);
            lblStatus.Location = new Point(10, 7);
            lblStatus.Text = "Ready";

            //
            // BranchListForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(750, 480);
            Controls.Add(dgvBranches);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);
            Name = "BranchListForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Branch List - SQL Server";
            Load += BranchListForm_Load;

            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBranches).EndInit();
            pnlBottom.ResumeLayout(false);
            pnlBottom.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTop;
        private Label lblTitle;
        private Button btnRefresh;
        private DataGridView dgvBranches;
        private Panel pnlBottom;
        private Label lblStatus;
    }
}
