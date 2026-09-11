namespace CodeAppsDataMigration
{
    partial class PgToPgMigrationForm
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
            lblSubTitle = new Label();

            tlpConfig = new TableLayoutPanel();
            grpSource = new GroupBox();
            grpTarget = new GroupBox();

            lblSrcHost = new Label();  txtSrcHost = new TextBox();
            lblSrcPort = new Label();  txtSrcPort = new TextBox();
            lblSrcDb = new Label();    cmbSrcDatabase = new ComboBox();  btnLoadSrcDbs = new Button();
            lblSrcUser = new Label();  txtSrcUser = new TextBox();
            lblSrcPwd = new Label();   txtSrcPassword = new TextBox();
            btnTestSrc = new Button();

            lblTgtHost = new Label();  txtTgtHost = new TextBox();
            lblTgtPort = new Label();  txtTgtPort = new TextBox();
            lblTgtDb = new Label();    cmbTgtDatabase = new ComboBox();  btnLoadTgtDbs = new Button();
            lblTgtUser = new Label();  txtTgtUser = new TextBox();
            lblTgtPwd = new Label();   txtTgtPassword = new TextBox();
            btnTestTgt = new Button();

            pnlMode = new TableLayoutPanel();
            rdoBranchMode = new RadioButton();
            rdoFullMode = new RadioButton();
            lblModeHelp = new Label();

            pnlOptions = new TableLayoutPanel();
            flowChecks = new FlowLayoutPanel();
            chkCopyProfile = new CheckBox();
            chkMergeSettings = new CheckBox();
            chkTruncate = new CheckBox();
            chkDisableTriggers = new CheckBox();
            chkResetSequences = new CheckBox();
            btnSaveConnections = new Button();

            pnlToolbar = new TableLayoutPanel();
            flowLeftButtons = new FlowLayoutPanel();
            flowRightButtons = new FlowLayoutPanel();
            btnCreateBranch = new Button();
            btnLoadBranches = new Button();
            btnLoadTables = new Button();
            btnSelectAll = new Button();
            btnSelectNone = new Button();
            btnStart = new Button();
            btnCancel = new Button();

            pnlBranch = new TableLayoutPanel();
            flowTgtMain = new FlowLayoutPanel();
            lblTgtMain = new Label();
            cmbTgtMainBranch = new ComboBox();
            lblMapHint = new Label();
            grdBranchMap = new DataGridView();
            colSrcBranch = new DataGridViewTextBoxColumn();
            colTgtBranch = new DataGridViewComboBoxColumn();
            txtLog = new TextBox();

            grdTables = new DataGridView();
            colSelect = new DataGridViewCheckBoxColumn();
            colTable = new DataGridViewTextBoxColumn();
            colSrcRows = new DataGridViewTextBoxColumn();
            colTgtRows = new DataGridViewTextBoxColumn();
            colCopied = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();

            pnlFooter = new TableLayoutPanel();
            lblSummary = new Label();
            progressBar = new ProgressBar();
            lblStatus = new Label();

            pnlHeader.SuspendLayout();
            tlpConfig.SuspendLayout();
            grpSource.SuspendLayout();
            grpTarget.SuspendLayout();
            pnlMode.SuspendLayout();
            pnlOptions.SuspendLayout();
            flowChecks.SuspendLayout();
            pnlToolbar.SuspendLayout();
            flowLeftButtons.SuspendLayout();
            flowRightButtons.SuspendLayout();
            pnlBranch.SuspendLayout();
            flowTgtMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grdBranchMap).BeginInit();
            ((System.ComponentModel.ISupportInitialize)grdTables).BeginInit();
            pnlFooter.SuspendLayout();
            SuspendLayout();

            // ========== Header ==========
            pnlHeader.BackColor = Color.FromArgb(45, 55, 72);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 64;
            pnlHeader.Padding = new Padding(20, 8, 20, 8);
            pnlHeader.Controls.Add(lblSubTitle);
            pnlHeader.Controls.Add(lblTitle);

            lblTitle.Text = "Offline  →  Online PostgreSQL Migration";
            lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = false;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 26;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            lblSubTitle.Text = "Moves a client's data from the offline PostgreSQL server into the online PostgreSQL database, branch by branch, like the SQL Server data migration.";
            lblSubTitle.Font = new Font("Segoe UI", 9F);
            lblSubTitle.ForeColor = Color.FromArgb(203, 213, 224);
            lblSubTitle.AutoSize = false;
            lblSubTitle.Dock = DockStyle.Fill;
            lblSubTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblSubTitle.AutoEllipsis = true;

            // ========== Connection groups ==========
            tlpConfig.BackColor = Color.FromArgb(247, 250, 252);
            tlpConfig.Dock = DockStyle.Top;
            tlpConfig.Height = 190;
            tlpConfig.Padding = new Padding(10, 8, 10, 0);
            tlpConfig.ColumnCount = 2;
            tlpConfig.RowCount = 1;
            tlpConfig.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpConfig.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpConfig.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpConfig.Controls.Add(grpSource, 0, 0);
            tlpConfig.Controls.Add(grpTarget, 1, 0);

            grpSource.Text = "  Source  -  OFFLINE PostgreSQL (client's local server)  ";
            grpSource.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grpSource.ForeColor = Color.FromArgb(197, 48, 48);
            grpSource.Dock = DockStyle.Fill;
            grpSource.Margin = new Padding(5, 0, 10, 4);
            grpSource.Padding = new Padding(10, 6, 10, 6);
            grpSource.Controls.Add(BuildConnectionTable(
                lblSrcHost, txtSrcHost, lblSrcPort, txtSrcPort,
                lblSrcDb, cmbSrcDatabase, btnLoadSrcDbs,
                lblSrcUser, txtSrcUser, lblSrcPwd, txtSrcPassword, btnTestSrc));
            btnLoadSrcDbs.Click += btnLoadSrcDbs_Click;
            btnTestSrc.Click += btnTestSrc_Click;

            grpTarget.Text = "  Target  -  ONLINE PostgreSQL (new cloud / hosted server)  ";
            grpTarget.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grpTarget.ForeColor = Color.FromArgb(47, 133, 90);
            grpTarget.Dock = DockStyle.Fill;
            grpTarget.Margin = new Padding(10, 0, 5, 4);
            grpTarget.Padding = new Padding(10, 6, 10, 6);
            grpTarget.Controls.Add(BuildConnectionTable(
                lblTgtHost, txtTgtHost, lblTgtPort, txtTgtPort,
                lblTgtDb, cmbTgtDatabase, btnLoadTgtDbs,
                lblTgtUser, txtTgtUser, lblTgtPwd, txtTgtPassword, btnTestTgt));
            btnLoadTgtDbs.Click += btnLoadTgtDbs_Click;
            btnTestTgt.Click += btnTestTgt_Click;

            // ========== Mode row ==========
            pnlMode.BackColor = Color.FromArgb(247, 250, 252);
            pnlMode.Dock = DockStyle.Top;
            pnlMode.Height = 36;
            pnlMode.Padding = new Padding(15, 2, 15, 2);
            pnlMode.ColumnCount = 3;
            pnlMode.RowCount = 1;
            pnlMode.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pnlMode.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pnlMode.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlMode.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlMode.Controls.Add(rdoBranchMode, 0, 0);
            pnlMode.Controls.Add(rdoFullMode, 1, 0);
            pnlMode.Controls.Add(lblModeHelp, 2, 0);

            rdoBranchMode.Text = "Branch migration  (map offline branch → online branch, like SQL Server → PostgreSQL)";
            rdoBranchMode.Checked = true;
            rdoBranchMode.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            rdoBranchMode.ForeColor = Color.FromArgb(45, 55, 72);
            rdoBranchMode.AutoSize = true;
            rdoBranchMode.Margin = new Padding(0, 6, 25, 0);
            rdoBranchMode.CheckedChanged += rdoMode_CheckedChanged;

            rdoFullMode.Text = "Full database copy  (empty online database, same schema)";
            rdoFullMode.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            rdoFullMode.ForeColor = Color.FromArgb(45, 55, 72);
            rdoFullMode.AutoSize = true;
            rdoFullMode.Margin = new Padding(0, 6, 25, 0);
            rdoFullMode.CheckedChanged += rdoMode_CheckedChanged;

            lblModeHelp.Text = "";
            lblModeHelp.Font = new Font("Segoe UI", 8.5F);
            lblModeHelp.ForeColor = Color.FromArgb(113, 128, 150);
            lblModeHelp.Dock = DockStyle.Fill;
            lblModeHelp.TextAlign = ContentAlignment.MiddleLeft;
            lblModeHelp.AutoEllipsis = true;

            // ========== Options row ==========
            pnlOptions.BackColor = Color.FromArgb(247, 250, 252);
            pnlOptions.Dock = DockStyle.Top;
            pnlOptions.Height = 44;
            pnlOptions.Padding = new Padding(15, 4, 15, 4);
            pnlOptions.ColumnCount = 2;
            pnlOptions.RowCount = 1;
            pnlOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlOptions.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pnlOptions.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlOptions.Controls.Add(flowChecks, 0, 0);
            pnlOptions.Controls.Add(btnSaveConnections, 1, 0);

            flowChecks.Dock = DockStyle.Fill;
            flowChecks.FlowDirection = FlowDirection.LeftToRight;
            flowChecks.WrapContents = false;
            flowChecks.Margin = new Padding(0);
            flowChecks.Controls.Add(chkCopyProfile);
            flowChecks.Controls.Add(chkMergeSettings);
            flowChecks.Controls.Add(chkTruncate);
            flowChecks.Controls.Add(chkDisableTriggers);
            flowChecks.Controls.Add(chkResetSequences);

            StyleCheck(chkCopyProfile, "Copy branch profile (name, address, GST, bank, print)");
            StyleCheck(chkMergeSettings, "Copy settings by name (branch / main / voucher / control order)");
            StyleCheck(chkTruncate, "Empty online tables before copy");
            StyleCheck(chkDisableTriggers, "Disable FK / trigger checks");
            StyleCheck(chkResetSequences, "Reset ID sequences after copy");

            StyleButton(btnSaveConnections, "Save Connections", Color.FromArgb(113, 128, 150), 140);
            btnSaveConnections.Anchor = AnchorStyles.Right;
            btnSaveConnections.Margin = new Padding(0);
            btnSaveConnections.Click += btnSaveConnections_Click;

            // ========== Toolbar row ==========
            pnlToolbar.BackColor = Color.FromArgb(237, 242, 247);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Height = 48;
            pnlToolbar.Padding = new Padding(10, 6, 10, 6);
            pnlToolbar.ColumnCount = 2;
            pnlToolbar.RowCount = 1;
            pnlToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlToolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlToolbar.Controls.Add(flowLeftButtons, 0, 0);
            pnlToolbar.Controls.Add(flowRightButtons, 1, 0);

            flowLeftButtons.Dock = DockStyle.Fill;
            flowLeftButtons.FlowDirection = FlowDirection.LeftToRight;
            flowLeftButtons.WrapContents = false;
            flowLeftButtons.Margin = new Padding(0);
            flowLeftButtons.Controls.Add(btnCreateBranch);
            flowLeftButtons.Controls.Add(btnLoadBranches);
            flowLeftButtons.Controls.Add(btnLoadTables);
            flowLeftButtons.Controls.Add(btnSelectAll);
            flowLeftButtons.Controls.Add(btnSelectNone);

            flowRightButtons.Dock = DockStyle.Fill;
            flowRightButtons.FlowDirection = FlowDirection.RightToLeft;
            flowRightButtons.WrapContents = false;
            flowRightButtons.Margin = new Padding(0);
            flowRightButtons.Controls.Add(btnCancel);
            flowRightButtons.Controls.Add(btnStart);

            StyleButton(btnCreateBranch, "1. Create Main Branch", Color.FromArgb(237, 137, 54), 165);
            btnCreateBranch.Margin = new Padding(5, 0, 5, 0);
            btnCreateBranch.Click += btnCreateBranch_Click;

            StyleButton(btnLoadBranches, "2. Load Branches", Color.FromArgb(66, 153, 225), 140);
            btnLoadBranches.Margin = new Padding(5, 0, 5, 0);
            btnLoadBranches.Click += btnLoadBranches_Click;

            StyleButton(btnLoadTables, "Load Tables", Color.FromArgb(66, 153, 225), 120);
            btnLoadTables.Margin = new Padding(5, 0, 5, 0);
            btnLoadTables.Click += btnLoadTables_Click;

            StyleButton(btnSelectAll, "Select All", Color.FromArgb(159, 122, 234), 100);
            btnSelectAll.Margin = new Padding(5, 0, 5, 0);
            btnSelectAll.Click += btnSelectAll_Click;

            StyleButton(btnSelectNone, "Select None", Color.FromArgb(159, 122, 234), 100);
            btnSelectNone.Margin = new Padding(5, 0, 5, 0);
            btnSelectNone.Click += btnSelectNone_Click;

            StyleButton(btnStart, "▶  3. Start Migration", Color.FromArgb(72, 187, 120), 165);
            btnStart.Margin = new Padding(5, 0, 5, 0);
            btnStart.Enabled = false;
            btnStart.Click += btnStart_Click;

            StyleButton(btnCancel, "Cancel", Color.FromArgb(229, 62, 62), 100);
            btnCancel.Margin = new Padding(5, 0, 5, 0);
            btnCancel.Enabled = false;
            btnCancel.Click += btnCancel_Click;

            // ========== Branch panel ==========
            pnlBranch.Dock = DockStyle.Fill;
            pnlBranch.BackColor = Color.White;
            pnlBranch.Padding = new Padding(10, 6, 10, 6);
            pnlBranch.ColumnCount = 1;
            pnlBranch.RowCount = 3;
            pnlBranch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlBranch.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            pnlBranch.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            pnlBranch.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            pnlBranch.Controls.Add(flowTgtMain, 0, 0);
            pnlBranch.Controls.Add(grdBranchMap, 0, 1);
            pnlBranch.Controls.Add(txtLog, 0, 2);

            flowTgtMain.Dock = DockStyle.Fill;
            flowTgtMain.FlowDirection = FlowDirection.LeftToRight;
            flowTgtMain.WrapContents = false;
            flowTgtMain.Margin = new Padding(0);
            flowTgtMain.Controls.Add(lblTgtMain);
            flowTgtMain.Controls.Add(cmbTgtMainBranch);
            flowTgtMain.Controls.Add(lblMapHint);

            lblTgtMain.Text = "Online Main Branch (To):";
            lblTgtMain.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTgtMain.ForeColor = Color.FromArgb(47, 133, 90);
            lblTgtMain.AutoSize = true;
            lblTgtMain.Margin = new Padding(0, 9, 8, 0);

            cmbTgtMainBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTgtMainBranch.Font = new Font("Segoe UI", 10F);
            cmbTgtMainBranch.Width = 360;
            cmbTgtMainBranch.Margin = new Padding(0, 4, 20, 0);
            cmbTgtMainBranch.SelectedIndexChanged += cmbTgtMainBranch_SelectedIndexChanged;

            lblMapHint.Text = "For each offline branch below, choose the online branch that was created for it (leave blank to skip).";
            lblMapHint.Font = new Font("Segoe UI", 8.5F);
            lblMapHint.ForeColor = Color.FromArgb(113, 128, 150);
            lblMapHint.AutoSize = true;
            lblMapHint.Margin = new Padding(0, 10, 0, 0);

            grdBranchMap.Dock = DockStyle.Fill;
            grdBranchMap.Margin = new Padding(0, 0, 0, 6);
            grdBranchMap.BackgroundColor = Color.White;
            grdBranchMap.BorderStyle = BorderStyle.FixedSingle;
            grdBranchMap.AllowUserToAddRows = false;
            grdBranchMap.AllowUserToDeleteRows = false;
            grdBranchMap.AllowUserToResizeRows = false;
            grdBranchMap.RowHeadersVisible = false;
            grdBranchMap.MultiSelect = false;
            grdBranchMap.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grdBranchMap.Font = new Font("Segoe UI", 9.5F);
            grdBranchMap.RowTemplate.Height = 28;
            grdBranchMap.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(237, 242, 247);
            grdBranchMap.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grdBranchMap.ColumnHeadersHeight = 32;
            grdBranchMap.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grdBranchMap.EnableHeadersVisualStyles = false;
            grdBranchMap.Columns.AddRange(new DataGridViewColumn[] { colSrcBranch, colTgtBranch });
            grdBranchMap.DataError += grdBranchMap_DataError;

            colSrcBranch.HeaderText = "Offline Branch (From)";
            colSrcBranch.Name = "colSrcBranch";
            colSrcBranch.ReadOnly = true;
            colSrcBranch.FillWeight = 50;

            colTgtBranch.HeaderText = "Online Branch (To)";
            colTgtBranch.Name = "colTgtBranch";
            colTgtBranch.FillWeight = 50;
            colTgtBranch.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            colTgtBranch.FlatStyle = FlatStyle.Flat;

            txtLog.Dock = DockStyle.Fill;
            txtLog.Margin = new Padding(0);
            txtLog.Multiline = true;
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Both;
            txtLog.WordWrap = false;
            txtLog.Font = new Font("Consolas", 9F);
            txtLog.BackColor = Color.FromArgb(250, 250, 250);
            txtLog.ForeColor = Color.FromArgb(45, 55, 72);

            // ========== Table grid (full copy mode) ==========
            grdTables.Dock = DockStyle.Fill;
            grdTables.BackgroundColor = Color.White;
            grdTables.BorderStyle = BorderStyle.None;
            grdTables.AllowUserToAddRows = false;
            grdTables.AllowUserToDeleteRows = false;
            grdTables.AllowUserToResizeRows = false;
            grdTables.RowHeadersVisible = false;
            grdTables.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grdTables.MultiSelect = false;
            grdTables.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grdTables.Font = new Font("Segoe UI", 9.5F);
            grdTables.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(237, 242, 247);
            grdTables.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grdTables.ColumnHeadersHeight = 32;
            grdTables.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grdTables.EnableHeadersVisualStyles = false;
            grdTables.Columns.AddRange(new DataGridViewColumn[] { colSelect, colTable, colSrcRows, colTgtRows, colCopied, colStatus });
            grdTables.CellValueChanged += grdTables_CellValueChanged;
            grdTables.CurrentCellDirtyStateChanged += grdTables_CurrentCellDirtyStateChanged;
            grdTables.Visible = false;

            colSelect.HeaderText = "Copy"; colSelect.Name = "colSelect"; colSelect.FillWeight = 7; colSelect.ReadOnly = false;
            colTable.HeaderText = "Table"; colTable.Name = "colTable"; colTable.FillWeight = 30; colTable.ReadOnly = true;
            colSrcRows.HeaderText = "Offline Rows"; colSrcRows.Name = "colSrcRows"; colSrcRows.FillWeight = 13; colSrcRows.ReadOnly = true;
            colSrcRows.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colTgtRows.HeaderText = "Online Rows (before)"; colTgtRows.Name = "colTgtRows"; colTgtRows.FillWeight = 15; colTgtRows.ReadOnly = true;
            colTgtRows.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colCopied.HeaderText = "Copied"; colCopied.Name = "colCopied"; colCopied.FillWeight = 13; colCopied.ReadOnly = true;
            colCopied.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colStatus.HeaderText = "Status"; colStatus.Name = "colStatus"; colStatus.FillWeight = 22; colStatus.ReadOnly = true;

            // ========== Footer ==========
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Height = 64;
            pnlFooter.BackColor = Color.FromArgb(247, 250, 252);
            pnlFooter.Padding = new Padding(15, 6, 15, 6);
            pnlFooter.ColumnCount = 2;
            pnlFooter.RowCount = 2;
            pnlFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            pnlFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            pnlFooter.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            pnlFooter.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            pnlFooter.Controls.Add(lblSummary, 0, 0);
            pnlFooter.SetColumnSpan(lblSummary, 2);
            pnlFooter.Controls.Add(progressBar, 0, 1);
            pnlFooter.Controls.Add(lblStatus, 1, 1);

            lblSummary.Text = "";
            lblSummary.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblSummary.ForeColor = Color.FromArgb(45, 55, 72);
            lblSummary.Dock = DockStyle.Fill;
            lblSummary.TextAlign = ContentAlignment.MiddleLeft;
            lblSummary.Margin = new Padding(0);
            lblSummary.AutoEllipsis = true;

            progressBar.Dock = DockStyle.Fill;
            progressBar.Margin = new Padding(0, 4, 10, 2);

            lblStatus.Dock = DockStyle.Fill;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(74, 85, 104);
            lblStatus.Margin = new Padding(0);
            lblStatus.AutoEllipsis = true;
            lblStatus.Text = "";

            // ========== Form ==========
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 760);
            // Dock order: last added = outermost.
            Controls.Add(pnlBranch);
            Controls.Add(grdTables);
            Controls.Add(pnlFooter);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlOptions);
            Controls.Add(pnlMode);
            Controls.Add(tlpConfig);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(960, 640);
            Name = "PgToPgMigrationForm";
            Text = "Offline to Online PostgreSQL Migration";
            Load += PgToPgMigrationForm_Load;
            FormClosing += PgToPgMigrationForm_FormClosing;

            pnlHeader.ResumeLayout(false);
            grpSource.ResumeLayout(false);
            grpTarget.ResumeLayout(false);
            tlpConfig.ResumeLayout(false);
            pnlMode.ResumeLayout(false);
            pnlMode.PerformLayout();
            flowChecks.ResumeLayout(false);
            flowChecks.PerformLayout();
            pnlOptions.ResumeLayout(false);
            pnlOptions.PerformLayout();
            flowLeftButtons.ResumeLayout(false);
            flowRightButtons.ResumeLayout(false);
            pnlToolbar.ResumeLayout(false);
            flowTgtMain.ResumeLayout(false);
            flowTgtMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grdBranchMap).EndInit();
            pnlBranch.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)grdTables).EndInit();
            pnlFooter.ResumeLayout(false);
            ResumeLayout(false);
        }

        /// <summary>One Host/Port/Database/User/Password block as a TableLayoutPanel.</summary>
        private static TableLayoutPanel BuildConnectionTable(
            Label lblHost, TextBox txtHost, Label lblPort, TextBox txtPort,
            Label lblDb, ComboBox cmbDb, Button btnLoadDbs,
            Label lblUser, TextBox txtUser, Label lblPwd, TextBox txtPwd, Button btnTest)
        {
            var normal = new Font("Segoe UI", 9F);
            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, Font = normal, ColumnCount = 5, RowCount = 4, Margin = new Padding(0) };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125F));
            for (int i = 0; i < 4; i++) tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

            tlp.Controls.Add(StyleLabel(lblHost, "Host", normal), 0, 0);
            tlp.Controls.Add(StyleField(txtHost, normal), 1, 0);
            tlp.Controls.Add(StyleLabel(lblPort, "Port", normal), 2, 0);
            tlp.Controls.Add(StyleField(txtPort, normal), 3, 0);

            tlp.Controls.Add(StyleLabel(lblDb, "Database", normal), 0, 1);
            cmbDb.DropDownStyle = ComboBoxStyle.DropDown;
            tlp.Controls.Add(StyleField(cmbDb, normal), 1, 1);
            tlp.SetColumnSpan(cmbDb, 3);
            StyleButton(btnLoadDbs, "Load DBs", Color.FromArgb(113, 128, 150), 115, 27);
            btnLoadDbs.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnLoadDbs.Margin = new Padding(8, 2, 0, 2);
            tlp.Controls.Add(btnLoadDbs, 4, 1);

            tlp.Controls.Add(StyleLabel(lblUser, "Username", normal), 0, 2);
            tlp.Controls.Add(StyleField(txtUser, normal), 1, 2);
            tlp.SetColumnSpan(txtUser, 3);

            tlp.Controls.Add(StyleLabel(lblPwd, "Password", normal), 0, 3);
            txtPwd.UseSystemPasswordChar = true;
            tlp.Controls.Add(StyleField(txtPwd, normal), 1, 3);
            tlp.SetColumnSpan(txtPwd, 3);
            StyleButton(btnTest, "Test Connection", Color.FromArgb(66, 153, 225), 115, 27);
            btnTest.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnTest.Margin = new Padding(8, 2, 0, 2);
            tlp.Controls.Add(btnTest, 4, 3);
            return tlp;
        }

        private static Label StyleLabel(Label l, string text, Font font)
        {
            l.Text = text; l.Font = font; l.ForeColor = Color.FromArgb(74, 85, 104);
            l.AutoSize = false; l.Dock = DockStyle.Fill; l.TextAlign = ContentAlignment.MiddleLeft; l.Margin = new Padding(0);
            return l;
        }

        private static Control StyleField(Control c, Font font)
        {
            c.Font = font; c.Anchor = AnchorStyles.Left | AnchorStyles.Right; c.Margin = new Padding(0, 2, 0, 2);
            return c;
        }

        private static void StyleCheck(CheckBox c, string text)
        {
            c.Text = text; c.Font = new Font("Segoe UI", 9F); c.ForeColor = Color.FromArgb(45, 55, 72);
            c.AutoSize = true; c.Checked = true; c.Margin = new Padding(0, 8, 25, 0);
        }

        private static void StyleButton(Button b, string text, Color back, int width, int height = 32)
        {
            b.Text = text; b.Size = new Size(width, height); b.MinimumSize = new Size(width, height);
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
            b.BackColor = back; b.ForeColor = Color.White;
            b.Font = new Font("Segoe UI", 9F, FontStyle.Bold); b.Cursor = Cursors.Hand; b.UseVisualStyleBackColor = false;
        }

        #endregion

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubTitle;

        private TableLayoutPanel tlpConfig;
        private GroupBox grpSource;
        private GroupBox grpTarget;

        private Label lblSrcHost;  private TextBox txtSrcHost;
        private Label lblSrcPort;  private TextBox txtSrcPort;
        private Label lblSrcDb;    private ComboBox cmbSrcDatabase;  private Button btnLoadSrcDbs;
        private Label lblSrcUser;  private TextBox txtSrcUser;
        private Label lblSrcPwd;   private TextBox txtSrcPassword;
        private Button btnTestSrc;

        private Label lblTgtHost;  private TextBox txtTgtHost;
        private Label lblTgtPort;  private TextBox txtTgtPort;
        private Label lblTgtDb;    private ComboBox cmbTgtDatabase;  private Button btnLoadTgtDbs;
        private Label lblTgtUser;  private TextBox txtTgtUser;
        private Label lblTgtPwd;   private TextBox txtTgtPassword;
        private Button btnTestTgt;

        private TableLayoutPanel pnlMode;
        private RadioButton rdoBranchMode;
        private RadioButton rdoFullMode;
        private Label lblModeHelp;

        private TableLayoutPanel pnlOptions;
        private FlowLayoutPanel flowChecks;
        private CheckBox chkCopyProfile;
        private CheckBox chkMergeSettings;
        private CheckBox chkTruncate;
        private CheckBox chkDisableTriggers;
        private CheckBox chkResetSequences;
        private Button btnSaveConnections;

        private TableLayoutPanel pnlToolbar;
        private FlowLayoutPanel flowLeftButtons;
        private FlowLayoutPanel flowRightButtons;
        private Button btnCreateBranch;
        private Button btnLoadBranches;
        private Button btnLoadTables;
        private Button btnSelectAll;
        private Button btnSelectNone;
        private Button btnStart;
        private Button btnCancel;

        private TableLayoutPanel pnlBranch;
        private FlowLayoutPanel flowTgtMain;
        private Label lblTgtMain;
        private ComboBox cmbTgtMainBranch;
        private Label lblMapHint;
        private DataGridView grdBranchMap;
        private DataGridViewTextBoxColumn colSrcBranch;
        private DataGridViewComboBoxColumn colTgtBranch;
        private TextBox txtLog;

        private DataGridView grdTables;
        private DataGridViewCheckBoxColumn colSelect;
        private DataGridViewTextBoxColumn colTable;
        private DataGridViewTextBoxColumn colSrcRows;
        private DataGridViewTextBoxColumn colTgtRows;
        private DataGridViewTextBoxColumn colCopied;
        private DataGridViewTextBoxColumn colStatus;

        private TableLayoutPanel pnlFooter;
        private Label lblSummary;
        private ProgressBar progressBar;
        private Label lblStatus;
    }
}
