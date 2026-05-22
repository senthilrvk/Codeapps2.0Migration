using CodeAppsDataMigration.Helpers;

namespace CodeAppsDataMigration
{
    public partial class AesEncryptionForm : Form
    {
        public AesEncryptionForm()
        {
            InitializeComponent();
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
            {
                MessageBox.Show("Please enter text to encrypt.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                txtOutput.Text = AesEncryption.Encrypt(txtInput.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Encryption failed:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
            {
                MessageBox.Show("Please enter text to decrypt.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                txtOutput.Text = AesEncryption.Decrypt(txtInput.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Decryption failed:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
