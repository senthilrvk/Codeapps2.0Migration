namespace CodeAppsDataMigration
{
    partial class AesEncryptionForm
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
            lblHeader = new Label();
            lblInput = new Label();
            txtInput = new TextBox();
            btnEncrypt = new Button();
            btnDecrypt = new Button();
            lblOutput = new Label();
            txtOutput = new TextBox();
            pnlHeader = new Panel();

            pnlHeader.SuspendLayout();
            SuspendLayout();

            // ========== Header Panel ==========
            pnlHeader.BackColor = Color.FromArgb(45, 55, 72);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Size = new Size(500, 50);
            pnlHeader.Controls.Add(lblHeader);

            // Header Label
            lblHeader.Text = "AES Encryption / Decryption";
            lblHeader.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblHeader.ForeColor = Color.White;
            lblHeader.AutoSize = true;
            lblHeader.Location = new Point(15, 12);

            // ========== Input Label ==========
            lblInput.Text = "Input Text:";
            lblInput.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblInput.ForeColor = Color.FromArgb(45, 55, 72);
            lblInput.AutoSize = true;
            lblInput.Location = new Point(20, 70);

            // ========== Input TextBox ==========
            txtInput.Font = new Font("Segoe UI", 10F);
            txtInput.Location = new Point(20, 95);
            txtInput.Size = new Size(450, 60);
            txtInput.Multiline = true;
            txtInput.ScrollBars = ScrollBars.Vertical;

            // ========== Encrypt Button ==========
            btnEncrypt.Text = "Encrypt";
            btnEncrypt.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnEncrypt.Size = new Size(215, 40);
            btnEncrypt.Location = new Point(20, 170);
            btnEncrypt.FlatStyle = FlatStyle.Flat;
            btnEncrypt.FlatAppearance.BorderSize = 0;
            btnEncrypt.BackColor = Color.FromArgb(66, 153, 225);
            btnEncrypt.ForeColor = Color.White;
            btnEncrypt.Cursor = Cursors.Hand;
            btnEncrypt.Click += btnEncrypt_Click;

            // ========== Decrypt Button ==========
            btnDecrypt.Text = "Decrypt";
            btnDecrypt.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDecrypt.Size = new Size(215, 40);
            btnDecrypt.Location = new Point(255, 170);
            btnDecrypt.FlatStyle = FlatStyle.Flat;
            btnDecrypt.FlatAppearance.BorderSize = 0;
            btnDecrypt.BackColor = Color.FromArgb(72, 187, 120);
            btnDecrypt.ForeColor = Color.White;
            btnDecrypt.Cursor = Cursors.Hand;
            btnDecrypt.Click += btnDecrypt_Click;

            // ========== Output Label ==========
            lblOutput.Text = "Output:";
            lblOutput.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblOutput.ForeColor = Color.FromArgb(45, 55, 72);
            lblOutput.AutoSize = true;
            lblOutput.Location = new Point(20, 225);

            // ========== Output TextBox ==========
            txtOutput.Font = new Font("Segoe UI", 10F);
            txtOutput.Location = new Point(20, 250);
            txtOutput.Size = new Size(450, 60);
            txtOutput.Multiline = true;
            txtOutput.ScrollBars = ScrollBars.Vertical;
            txtOutput.ReadOnly = true;
            txtOutput.BackColor = Color.FromArgb(237, 242, 247);

            // ========== Form ==========
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(500, 330);
            BackColor = Color.White;
            Controls.Add(pnlHeader);
            Controls.Add(lblInput);
            Controls.Add(txtInput);
            Controls.Add(btnEncrypt);
            Controls.Add(btnDecrypt);
            Controls.Add(lblOutput);
            Controls.Add(txtOutput);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "AesEncryptionForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "AES Encryption Tool";

            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlHeader;
        private Label lblHeader;
        private Label lblInput;
        private TextBox txtInput;
        private Button btnEncrypt;
        private Button btnDecrypt;
        private Label lblOutput;
        private TextBox txtOutput;
    }
}
