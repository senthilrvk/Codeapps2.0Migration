namespace CodeAppsDataMigration
{
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private void btnDataMigration_Click(object sender, EventArgs e)
        {
            using var form = new DataMigrationConvertForm();
            form.ShowDialog(this);
        }

        private void btnExcelImport_Click(object sender, EventArgs e)
        {
            using var form = new ExcelImportForm();
            form.ShowDialog(this);
        }

        private void btnBranchList_Click(object sender, EventArgs e)
        {
            using var form = new BranchListForm();
            form.ShowDialog(this);
        }

        private void btnConnectionSettings_Click(object sender, EventArgs e)
        {
            using var form = new ConnectionSettingsForm();
            form.ShowDialog(this);
        }

        private void btnAesEncryption_Click(object sender, EventArgs e)
        {
            using var form = new AesEncryptionForm();
            form.ShowDialog(this);
        }

        private void btnCountCompare_Click(object sender, EventArgs e)
        {
            using var form = new TableCountCompareForm();
            form.ShowDialog(this);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to exit?", "Exit",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
