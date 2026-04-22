using System.Xml.Linq;

namespace CodeAppsDataMigration
{
    public partial class ConnectionSettingsForm : Form
    {
        private readonly string _xmlPath;

        public ConnectionSettingsForm()
        {
            InitializeComponent();
            _xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConnectionStrings.xml");
        }

        private void ConnectionSettingsForm_Load(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (!File.Exists(_xmlPath))
            {
                MessageBox.Show("ConnectionStrings.xml not found!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var doc = XDocument.Load(_xmlPath);

            // SQL Server
            var sql = doc.Root!.Element("SqlServer")!;
            txtSqlServer.Text = sql.Element("Server")?.Value ?? "";
            cmbSqlDatabase.Text = sql.Element("Database")?.Value ?? "";
            txtSqlUserId.Text = sql.Element("UserId")?.Value ?? "";
            txtSqlPassword.Text = sql.Element("Password")?.Value ?? "";
            txtSqlTimeout.Text = sql.Element("ConnectionTimeout")?.Value ?? "0";
            chkSqlTrustCert.Checked = bool.TryParse(sql.Element("TrustServerCertificate")?.Value, out var tc) && tc;
            chkSqlMARS.Checked = bool.TryParse(sql.Element("MultipleActiveResultSets")?.Value, out var mars) && mars;
            chkSqlEncrypt.Checked = bool.TryParse(sql.Element("Encrypt")?.Value, out var enc) && enc;

            // Postgres
            var pg = doc.Root!.Element("Postgres")!;
            txtPgHost.Text = pg.Element("Host")?.Value ?? "";
            txtPgPort.Text = pg.Element("Port")?.Value ?? "5432";
            cmbPgDatabase.Text = pg.Element("Database")?.Value ?? "";
            txtPgUsername.Text = pg.Element("Username")?.Value ?? "";
            txtPgPassword.Text = pg.Element("Password")?.Value ?? "";
            txtPgTimeout.Text = pg.Element("Timeout")?.Value ?? "0";
            txtPgCmdTimeout.Text = pg.Element("CommandTimeout")?.Value ?? "0";
            txtPgKeepAlive.Text = pg.Element("KeepAlive")?.Value ?? "30";
            txtPgMaxPool.Text = pg.Element("MaxPoolSize")?.Value ?? "20";
            chkPgPooling.Checked = bool.TryParse(pg.Element("Pooling")?.Value, out var pool) && pool;
            chkPgErrorDetail.Checked = bool.TryParse(pg.Element("IncludeErrorDetail")?.Value, out var err) && err;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var doc = new XDocument(
                new XElement("ConnectionStrings",
                    new XElement("SqlServer",
                        new XElement("Server", txtSqlServer.Text.Trim()),
                        new XElement("Database", cmbSqlDatabase.Text.Trim()),
                        new XElement("UserId", txtSqlUserId.Text.Trim()),
                        new XElement("Password", txtSqlPassword.Text.Trim()),
                        new XElement("TrustServerCertificate", chkSqlTrustCert.Checked),
                        new XElement("ConnectionTimeout", txtSqlTimeout.Text.Trim()),
                        new XElement("MultipleActiveResultSets", chkSqlMARS.Checked),
                        new XElement("Encrypt", chkSqlEncrypt.Checked)
                    ),
                    new XElement("Postgres",
                        new XElement("Host", txtPgHost.Text.Trim()),
                        new XElement("Port", txtPgPort.Text.Trim()),
                        new XElement("Database", cmbPgDatabase.Text.Trim()),
                        new XElement("Username", txtPgUsername.Text.Trim()),
                        new XElement("Password", txtPgPassword.Text.Trim()),
                        new XElement("Timeout", txtPgTimeout.Text.Trim()),
                        new XElement("CommandTimeout", txtPgCmdTimeout.Text.Trim()),
                        new XElement("KeepAlive", txtPgKeepAlive.Text.Trim()),
                        new XElement("Pooling", chkPgPooling.Checked),
                        new XElement("MaxPoolSize", txtPgMaxPool.Text.Trim()),
                        new XElement("IncludeErrorDetail", chkPgErrorDetail.Checked)
                    )
                )
            );

            doc.Save(_xmlPath);
            MessageBox.Show("Connection settings saved successfully!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnTestSql_Click(object sender, EventArgs e)
        {
            var connStr =
                $"Server={txtSqlServer.Text.Trim()};" +
                $"Database={cmbSqlDatabase.Text.Trim()};" +
                $"User Id={txtSqlUserId.Text.Trim()};" +
                $"Password={txtSqlPassword.Text.Trim()};" +
                $"TrustServerCertificate={chkSqlTrustCert.Checked};" +
                $"Connection Timeout=5;" +
                $"Encrypt={chkSqlEncrypt.Checked};";

            try
            {
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
                conn.Open();
                MessageBox.Show("SQL Server connection successful!", "Test",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SQL Server connection failed:\n{ex.Message}", "Test",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTestPg_Click(object sender, EventArgs e)
        {
            var connStr =
                $"Host={txtPgHost.Text.Trim()};" +
                $"Port={txtPgPort.Text.Trim()};" +
                $"Database={cmbPgDatabase.Text.Trim()};" +
                $"Username={txtPgUsername.Text.Trim()};" +
                $"Password={txtPgPassword.Text.Trim()};" +
                $"Timeout=5;";

            try
            {
                using var conn = new Npgsql.NpgsqlConnection(connStr);
                conn.Open();
                MessageBox.Show("PostgreSQL connection successful!", "Test",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PostgreSQL connection failed:\n{ex.Message}", "Test",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoadSqlDbs_Click(object sender, EventArgs e)
        {
            var connStr =
                $"Server={txtSqlServer.Text.Trim()};" +
                $"User Id={txtSqlUserId.Text.Trim()};" +
                $"Password={txtSqlPassword.Text.Trim()};" +
                $"TrustServerCertificate={chkSqlTrustCert.Checked};" +
                $"Connection Timeout=5;" +
                $"Encrypt={chkSqlEncrypt.Checked};";

            try
            {
                var currentDb = cmbSqlDatabase.Text;
                cmbSqlDatabase.Items.Clear();

                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
                conn.Open();
                using var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                    "SELECT name FROM sys.databases ORDER BY name", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    cmbSqlDatabase.Items.Add(reader.GetString(0));
                }

                if (!string.IsNullOrEmpty(currentDb))
                {
                    int idx = cmbSqlDatabase.FindStringExact(currentDb);
                    if (idx >= 0)
                        cmbSqlDatabase.SelectedIndex = idx;
                    else
                        cmbSqlDatabase.Text = currentDb;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load SQL Server databases:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoadPgDbs_Click(object sender, EventArgs e)
        {
            var connStr =
                $"Host={txtPgHost.Text.Trim()};" +
                $"Port={txtPgPort.Text.Trim()};" +
                $"Database=postgres;" +
                $"Username={txtPgUsername.Text.Trim()};" +
                $"Password={txtPgPassword.Text.Trim()};" +
                $"Timeout=5;";

            try
            {
                var currentDb = cmbPgDatabase.Text;
                cmbPgDatabase.Items.Clear();

                using var conn = new Npgsql.NpgsqlConnection(connStr);
                conn.Open();
                using var cmd = new Npgsql.NpgsqlCommand(
                    "SELECT datname FROM pg_database WHERE datistemplate = false ORDER BY datname", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    cmbPgDatabase.Items.Add(reader.GetString(0));
                }

                if (!string.IsNullOrEmpty(currentDb))
                {
                    int idx = cmbPgDatabase.FindStringExact(currentDb);
                    if (idx >= 0)
                        cmbPgDatabase.SelectedIndex = idx;
                    else
                        cmbPgDatabase.Text = currentDb;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load PostgreSQL databases:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
