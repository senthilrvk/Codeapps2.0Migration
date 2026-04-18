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
            using var form = new Form1();
            form.ShowDialog(this);
        }

        private void btnConnectionSettings_Click(object sender, EventArgs e)
        {
            using var form = new ConnectionSettingsForm();
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
