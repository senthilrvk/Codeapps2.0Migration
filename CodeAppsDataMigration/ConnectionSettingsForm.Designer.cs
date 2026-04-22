namespace CodeAppsDataMigration
{
    partial class ConnectionSettingsForm
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
            tabControl = new TabControl();
            tabSqlServer = new TabPage();
            tabPostgres = new TabPage();

            lblSqlServer = new Label();
            txtSqlServer = new TextBox();
            lblSqlDatabase = new Label();
            cmbSqlDatabase = new ComboBox();
            btnLoadSqlDbs = new Button();
            lblSqlUserId = new Label();
            txtSqlUserId = new TextBox();
            lblSqlPassword = new Label();
            txtSqlPassword = new TextBox();
            lblSqlTimeout = new Label();
            txtSqlTimeout = new TextBox();
            chkSqlTrustCert = new CheckBox();
            chkSqlMARS = new CheckBox();
            chkSqlEncrypt = new CheckBox();
            btnTestSql = new Button();

            lblPgHost = new Label();
            txtPgHost = new TextBox();
            lblPgPort = new Label();
            txtPgPort = new TextBox();
            lblPgDatabase = new Label();
            cmbPgDatabase = new ComboBox();
            btnLoadPgDbs = new Button();
            lblPgUsername = new Label();
            txtPgUsername = new TextBox();
            lblPgPassword = new Label();
            txtPgPassword = new TextBox();
            lblPgTimeout = new Label();
            txtPgTimeout = new TextBox();
            lblPgCmdTimeout = new Label();
            txtPgCmdTimeout = new TextBox();
            lblPgKeepAlive = new Label();
            txtPgKeepAlive = new TextBox();
            lblPgMaxPool = new Label();
            txtPgMaxPool = new TextBox();
            chkPgPooling = new CheckBox();
            chkPgErrorDetail = new CheckBox();
            btnTestPg = new Button();

            btnSave = new Button();
            btnCancel = new Button();

            tabControl.SuspendLayout();
            tabSqlServer.SuspendLayout();
            tabPostgres.SuspendLayout();
            SuspendLayout();

            //
            // tabControl
            //
            tabControl.Controls.Add(tabSqlServer);
            tabControl.Controls.Add(tabPostgres);
            tabControl.Location = new Point(12, 12);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(460, 380);
            tabControl.TabIndex = 0;

            //
            // tabSqlServer
            //
            tabSqlServer.Controls.Add(lblSqlServer);
            tabSqlServer.Controls.Add(txtSqlServer);
            tabSqlServer.Controls.Add(lblSqlDatabase);
            tabSqlServer.Controls.Add(cmbSqlDatabase);
            tabSqlServer.Controls.Add(btnLoadSqlDbs);
            tabSqlServer.Controls.Add(lblSqlUserId);
            tabSqlServer.Controls.Add(txtSqlUserId);
            tabSqlServer.Controls.Add(lblSqlPassword);
            tabSqlServer.Controls.Add(txtSqlPassword);
            tabSqlServer.Controls.Add(lblSqlTimeout);
            tabSqlServer.Controls.Add(txtSqlTimeout);
            tabSqlServer.Controls.Add(chkSqlTrustCert);
            tabSqlServer.Controls.Add(chkSqlMARS);
            tabSqlServer.Controls.Add(chkSqlEncrypt);
            tabSqlServer.Controls.Add(btnTestSql);
            tabSqlServer.Location = new Point(4, 24);
            tabSqlServer.Name = "tabSqlServer";
            tabSqlServer.Padding = new Padding(3);
            tabSqlServer.Size = new Size(452, 352);
            tabSqlServer.TabIndex = 0;
            tabSqlServer.Text = "SQL Server";
            tabSqlServer.UseVisualStyleBackColor = true;

            //
            // lblSqlServer
            //
            lblSqlServer.AutoSize = true;
            lblSqlServer.Location = new Point(15, 18);
            lblSqlServer.Name = "lblSqlServer";
            lblSqlServer.Text = "Server:";

            //
            // txtSqlServer
            //
            txtSqlServer.Location = new Point(150, 15);
            txtSqlServer.Name = "txtSqlServer";
            txtSqlServer.Size = new Size(270, 23);

            //
            // lblSqlDatabase
            //
            lblSqlDatabase.AutoSize = true;
            lblSqlDatabase.Location = new Point(15, 50);
            lblSqlDatabase.Name = "lblSqlDatabase";
            lblSqlDatabase.Text = "Database:";

            //
            // cmbSqlDatabase
            //
            cmbSqlDatabase.Location = new Point(150, 47);
            cmbSqlDatabase.Name = "cmbSqlDatabase";
            cmbSqlDatabase.Size = new Size(200, 23);
            cmbSqlDatabase.DropDownStyle = ComboBoxStyle.DropDown;

            //
            // btnLoadSqlDbs
            //
            btnLoadSqlDbs.Location = new Point(355, 46);
            btnLoadSqlDbs.Name = "btnLoadSqlDbs";
            btnLoadSqlDbs.Size = new Size(65, 25);
            btnLoadSqlDbs.Text = "Load";
            btnLoadSqlDbs.UseVisualStyleBackColor = true;
            btnLoadSqlDbs.Click += btnLoadSqlDbs_Click;

            //
            // lblSqlUserId
            //
            lblSqlUserId.AutoSize = true;
            lblSqlUserId.Location = new Point(15, 82);
            lblSqlUserId.Name = "lblSqlUserId";
            lblSqlUserId.Text = "User Id:";

            //
            // txtSqlUserId
            //
            txtSqlUserId.Location = new Point(150, 79);
            txtSqlUserId.Name = "txtSqlUserId";
            txtSqlUserId.Size = new Size(270, 23);

            //
            // lblSqlPassword
            //
            lblSqlPassword.AutoSize = true;
            lblSqlPassword.Location = new Point(15, 114);
            lblSqlPassword.Name = "lblSqlPassword";
            lblSqlPassword.Text = "Password:";

            //
            // txtSqlPassword
            //
            txtSqlPassword.Location = new Point(150, 111);
            txtSqlPassword.Name = "txtSqlPassword";
            txtSqlPassword.Size = new Size(270, 23);
            txtSqlPassword.UseSystemPasswordChar = true;

            //
            // lblSqlTimeout
            //
            lblSqlTimeout.AutoSize = true;
            lblSqlTimeout.Location = new Point(15, 146);
            lblSqlTimeout.Name = "lblSqlTimeout";
            lblSqlTimeout.Text = "Connection Timeout:";

            //
            // txtSqlTimeout
            //
            txtSqlTimeout.Location = new Point(150, 143);
            txtSqlTimeout.Name = "txtSqlTimeout";
            txtSqlTimeout.Size = new Size(80, 23);

            //
            // chkSqlTrustCert
            //
            chkSqlTrustCert.AutoSize = true;
            chkSqlTrustCert.Location = new Point(15, 178);
            chkSqlTrustCert.Name = "chkSqlTrustCert";
            chkSqlTrustCert.Text = "Trust Server Certificate";

            //
            // chkSqlMARS
            //
            chkSqlMARS.AutoSize = true;
            chkSqlMARS.Location = new Point(15, 204);
            chkSqlMARS.Name = "chkSqlMARS";
            chkSqlMARS.Text = "Multiple Active Result Sets";

            //
            // chkSqlEncrypt
            //
            chkSqlEncrypt.AutoSize = true;
            chkSqlEncrypt.Location = new Point(15, 230);
            chkSqlEncrypt.Name = "chkSqlEncrypt";
            chkSqlEncrypt.Text = "Encrypt";

            //
            // btnTestSql
            //
            btnTestSql.Location = new Point(15, 265);
            btnTestSql.Name = "btnTestSql";
            btnTestSql.Size = new Size(130, 30);
            btnTestSql.TabIndex = 0;
            btnTestSql.Text = "Test Connection";
            btnTestSql.UseVisualStyleBackColor = true;
            btnTestSql.Click += btnTestSql_Click;

            //
            // tabPostgres
            //
            tabPostgres.Controls.Add(lblPgHost);
            tabPostgres.Controls.Add(txtPgHost);
            tabPostgres.Controls.Add(lblPgPort);
            tabPostgres.Controls.Add(txtPgPort);
            tabPostgres.Controls.Add(lblPgDatabase);
            tabPostgres.Controls.Add(cmbPgDatabase);
            tabPostgres.Controls.Add(btnLoadPgDbs);
            tabPostgres.Controls.Add(lblPgUsername);
            tabPostgres.Controls.Add(txtPgUsername);
            tabPostgres.Controls.Add(lblPgPassword);
            tabPostgres.Controls.Add(txtPgPassword);
            tabPostgres.Controls.Add(lblPgTimeout);
            tabPostgres.Controls.Add(txtPgTimeout);
            tabPostgres.Controls.Add(lblPgCmdTimeout);
            tabPostgres.Controls.Add(txtPgCmdTimeout);
            tabPostgres.Controls.Add(lblPgKeepAlive);
            tabPostgres.Controls.Add(txtPgKeepAlive);
            tabPostgres.Controls.Add(lblPgMaxPool);
            tabPostgres.Controls.Add(txtPgMaxPool);
            tabPostgres.Controls.Add(chkPgPooling);
            tabPostgres.Controls.Add(chkPgErrorDetail);
            tabPostgres.Controls.Add(btnTestPg);
            tabPostgres.Location = new Point(4, 24);
            tabPostgres.Name = "tabPostgres";
            tabPostgres.Padding = new Padding(3);
            tabPostgres.Size = new Size(452, 352);
            tabPostgres.TabIndex = 1;
            tabPostgres.Text = "PostgreSQL";
            tabPostgres.UseVisualStyleBackColor = true;

            //
            // lblPgHost
            //
            lblPgHost.AutoSize = true;
            lblPgHost.Location = new Point(15, 18);
            lblPgHost.Name = "lblPgHost";
            lblPgHost.Text = "Host:";

            //
            // txtPgHost
            //
            txtPgHost.Location = new Point(150, 15);
            txtPgHost.Name = "txtPgHost";
            txtPgHost.Size = new Size(270, 23);

            //
            // lblPgPort
            //
            lblPgPort.AutoSize = true;
            lblPgPort.Location = new Point(15, 48);
            lblPgPort.Name = "lblPgPort";
            lblPgPort.Text = "Port:";

            //
            // txtPgPort
            //
            txtPgPort.Location = new Point(150, 45);
            txtPgPort.Name = "txtPgPort";
            txtPgPort.Size = new Size(80, 23);

            //
            // lblPgDatabase
            //
            lblPgDatabase.AutoSize = true;
            lblPgDatabase.Location = new Point(15, 78);
            lblPgDatabase.Name = "lblPgDatabase";
            lblPgDatabase.Text = "Database:";

            //
            // cmbPgDatabase
            //
            cmbPgDatabase.Location = new Point(150, 75);
            cmbPgDatabase.Name = "cmbPgDatabase";
            cmbPgDatabase.Size = new Size(200, 23);
            cmbPgDatabase.DropDownStyle = ComboBoxStyle.DropDown;

            //
            // btnLoadPgDbs
            //
            btnLoadPgDbs.Location = new Point(355, 74);
            btnLoadPgDbs.Name = "btnLoadPgDbs";
            btnLoadPgDbs.Size = new Size(65, 25);
            btnLoadPgDbs.Text = "Load";
            btnLoadPgDbs.UseVisualStyleBackColor = true;
            btnLoadPgDbs.Click += btnLoadPgDbs_Click;

            //
            // lblPgUsername
            //
            lblPgUsername.AutoSize = true;
            lblPgUsername.Location = new Point(15, 108);
            lblPgUsername.Name = "lblPgUsername";
            lblPgUsername.Text = "Username:";

            //
            // txtPgUsername
            //
            txtPgUsername.Location = new Point(150, 105);
            txtPgUsername.Name = "txtPgUsername";
            txtPgUsername.Size = new Size(270, 23);

            //
            // lblPgPassword
            //
            lblPgPassword.AutoSize = true;
            lblPgPassword.Location = new Point(15, 138);
            lblPgPassword.Name = "lblPgPassword";
            lblPgPassword.Text = "Password:";

            //
            // txtPgPassword
            //
            txtPgPassword.Location = new Point(150, 135);
            txtPgPassword.Name = "txtPgPassword";
            txtPgPassword.Size = new Size(270, 23);
            txtPgPassword.UseSystemPasswordChar = true;

            //
            // lblPgTimeout
            //
            lblPgTimeout.AutoSize = true;
            lblPgTimeout.Location = new Point(15, 168);
            lblPgTimeout.Name = "lblPgTimeout";
            lblPgTimeout.Text = "Timeout:";

            //
            // txtPgTimeout
            //
            txtPgTimeout.Location = new Point(150, 165);
            txtPgTimeout.Name = "txtPgTimeout";
            txtPgTimeout.Size = new Size(80, 23);

            //
            // lblPgCmdTimeout
            //
            lblPgCmdTimeout.AutoSize = true;
            lblPgCmdTimeout.Location = new Point(15, 198);
            lblPgCmdTimeout.Name = "lblPgCmdTimeout";
            lblPgCmdTimeout.Text = "Command Timeout:";

            //
            // txtPgCmdTimeout
            //
            txtPgCmdTimeout.Location = new Point(150, 195);
            txtPgCmdTimeout.Name = "txtPgCmdTimeout";
            txtPgCmdTimeout.Size = new Size(80, 23);

            //
            // lblPgKeepAlive
            //
            lblPgKeepAlive.AutoSize = true;
            lblPgKeepAlive.Location = new Point(15, 228);
            lblPgKeepAlive.Name = "lblPgKeepAlive";
            lblPgKeepAlive.Text = "Keep Alive (sec):";

            //
            // txtPgKeepAlive
            //
            txtPgKeepAlive.Location = new Point(150, 225);
            txtPgKeepAlive.Name = "txtPgKeepAlive";
            txtPgKeepAlive.Size = new Size(80, 23);

            //
            // lblPgMaxPool
            //
            lblPgMaxPool.AutoSize = true;
            lblPgMaxPool.Location = new Point(260, 228);
            lblPgMaxPool.Name = "lblPgMaxPool";
            lblPgMaxPool.Text = "Max Pool Size:";

            //
            // txtPgMaxPool
            //
            txtPgMaxPool.Location = new Point(360, 225);
            txtPgMaxPool.Name = "txtPgMaxPool";
            txtPgMaxPool.Size = new Size(60, 23);

            //
            // chkPgPooling
            //
            chkPgPooling.AutoSize = true;
            chkPgPooling.Location = new Point(15, 258);
            chkPgPooling.Name = "chkPgPooling";
            chkPgPooling.Text = "Pooling";

            //
            // chkPgErrorDetail
            //
            chkPgErrorDetail.AutoSize = true;
            chkPgErrorDetail.Location = new Point(150, 258);
            chkPgErrorDetail.Name = "chkPgErrorDetail";
            chkPgErrorDetail.Text = "Include Error Detail";

            //
            // btnTestPg
            //
            btnTestPg.Location = new Point(15, 290);
            btnTestPg.Name = "btnTestPg";
            btnTestPg.Size = new Size(130, 30);
            btnTestPg.TabIndex = 0;
            btnTestPg.Text = "Test Connection";
            btnTestPg.UseVisualStyleBackColor = true;
            btnTestPg.Click += btnTestPg_Click;

            //
            // btnSave
            //
            btnSave.Location = new Point(290, 400);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(90, 32);
            btnSave.TabIndex = 1;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;

            //
            // btnCancel
            //
            btnCancel.Location = new Point(386, 400);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 32);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;

            //
            // ConnectionSettingsForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(490, 445);
            Controls.Add(tabControl);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConnectionSettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Connection Settings";
            Load += ConnectionSettingsForm_Load;

            tabControl.ResumeLayout(false);
            tabSqlServer.ResumeLayout(false);
            tabSqlServer.PerformLayout();
            tabPostgres.ResumeLayout(false);
            tabPostgres.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl;
        private TabPage tabSqlServer;
        private TabPage tabPostgres;

        private Label lblSqlServer;
        private TextBox txtSqlServer;
        private Label lblSqlDatabase;
        private ComboBox cmbSqlDatabase;
        private Button btnLoadSqlDbs;
        private Label lblSqlUserId;
        private TextBox txtSqlUserId;
        private Label lblSqlPassword;
        private TextBox txtSqlPassword;
        private Label lblSqlTimeout;
        private TextBox txtSqlTimeout;
        private CheckBox chkSqlTrustCert;
        private CheckBox chkSqlMARS;
        private CheckBox chkSqlEncrypt;
        private Button btnTestSql;

        private Label lblPgHost;
        private TextBox txtPgHost;
        private Label lblPgPort;
        private TextBox txtPgPort;
        private Label lblPgDatabase;
        private ComboBox cmbPgDatabase;
        private Button btnLoadPgDbs;
        private Label lblPgUsername;
        private TextBox txtPgUsername;
        private Label lblPgPassword;
        private TextBox txtPgPassword;
        private Label lblPgTimeout;
        private TextBox txtPgTimeout;
        private Label lblPgCmdTimeout;
        private TextBox txtPgCmdTimeout;
        private Label lblPgKeepAlive;
        private TextBox txtPgKeepAlive;
        private Label lblPgMaxPool;
        private TextBox txtPgMaxPool;
        private CheckBox chkPgPooling;
        private CheckBox chkPgErrorDetail;
        private Button btnTestPg;

        private Button btnSave;
        private Button btnCancel;
    }
}
