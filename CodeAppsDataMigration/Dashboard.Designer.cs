namespace CodeAppsDataMigration
{
    partial class Dashboard
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
            pnlMenu = new Panel();
            btnDataMigration = new Button();
            btnConnectionSettings = new Button();
            btnExit = new Button();
            pnlContent = new Panel();
            lblWelcome = new Label();
            lblDescription = new Label();

            pnlHeader.SuspendLayout();
            pnlMenu.SuspendLayout();
            pnlContent.SuspendLayout();
            SuspendLayout();

            // ========== Header Panel ==========
            pnlHeader.BackColor = Color.FromArgb(45, 55, 72);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Size = new Size(900, 60);
            pnlHeader.Controls.Add(lblTitle);

            // Title
            lblTitle.Text = "CodeApps Data Migration";
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 14);

            // ========== Menu Panel (Left Side) ==========
            pnlMenu.BackColor = Color.FromArgb(237, 242, 247);
            pnlMenu.Dock = DockStyle.Left;
            pnlMenu.Size = new Size(220, 0);
            pnlMenu.Padding = new Padding(10, 20, 10, 10);
            pnlMenu.Controls.Add(btnExit);
            pnlMenu.Controls.Add(btnConnectionSettings);
            pnlMenu.Controls.Add(btnDataMigration);

            // Data Migration button
            btnDataMigration.Text = "  Data Migration";
            btnDataMigration.Dock = DockStyle.Top;
            btnDataMigration.Size = new Size(200, 50);
            btnDataMigration.FlatStyle = FlatStyle.Flat;
            btnDataMigration.FlatAppearance.BorderSize = 0;
            btnDataMigration.BackColor = Color.FromArgb(66, 153, 225);
            btnDataMigration.ForeColor = Color.White;
            btnDataMigration.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            btnDataMigration.TextAlign = ContentAlignment.MiddleLeft;
            btnDataMigration.Cursor = Cursors.Hand;
            btnDataMigration.Margin = new Padding(0, 0, 0, 5);
            btnDataMigration.Click += btnDataMigration_Click;

            // Connection Settings button
            btnConnectionSettings.Text = "  Connection Settings";
            btnConnectionSettings.Dock = DockStyle.Top;
            btnConnectionSettings.Size = new Size(200, 50);
            btnConnectionSettings.FlatStyle = FlatStyle.Flat;
            btnConnectionSettings.FlatAppearance.BorderSize = 0;
            btnConnectionSettings.BackColor = Color.FromArgb(72, 187, 120);
            btnConnectionSettings.ForeColor = Color.White;
            btnConnectionSettings.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            btnConnectionSettings.TextAlign = ContentAlignment.MiddleLeft;
            btnConnectionSettings.Cursor = Cursors.Hand;
            btnConnectionSettings.Margin = new Padding(0, 0, 0, 5);
            btnConnectionSettings.Click += btnConnectionSettings_Click;

            // Exit button
            btnExit.Text = "  Exit";
            btnExit.Dock = DockStyle.Top;
            btnExit.Size = new Size(200, 50);
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.BackColor = Color.FromArgb(229, 62, 62);
            btnExit.ForeColor = Color.White;
            btnExit.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            btnExit.TextAlign = ContentAlignment.MiddleLeft;
            btnExit.Cursor = Cursors.Hand;
            btnExit.Click += btnExit_Click;

            // ========== Content Panel (Right Side) ==========
            pnlContent.BackColor = Color.White;
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Padding = new Padding(40, 40, 40, 40);
            pnlContent.Controls.Add(lblDescription);
            pnlContent.Controls.Add(lblWelcome);

            // Welcome label
            lblWelcome.Text = "Welcome";
            lblWelcome.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblWelcome.ForeColor = Color.FromArgb(45, 55, 72);
            lblWelcome.AutoSize = true;
            lblWelcome.Location = new Point(40, 40);

            // Description label
            lblDescription.Text = "Select an option from the menu to get started.\n\n" +
                "  Data Migration  -  Transfer data from SQL Server to PostgreSQL\n\n" +
                "  Connection Settings  -  Configure database connection strings";
            lblDescription.Font = new Font("Segoe UI", 11F);
            lblDescription.ForeColor = Color.FromArgb(113, 128, 150);
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(40, 90);

            // ========== Dashboard Form ==========
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 500);
            Controls.Add(pnlContent);
            Controls.Add(pnlMenu);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Dashboard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CodeApps Data Migration - Dashboard";

            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlMenu.ResumeLayout(false);
            pnlContent.ResumeLayout(false);
            pnlContent.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label lblTitle;
        private Panel pnlMenu;
        private Button btnDataMigration;
        private Button btnConnectionSettings;
        private Button btnExit;
        private Panel pnlContent;
        private Label lblWelcome;
        private Label lblDescription;
    }
}
