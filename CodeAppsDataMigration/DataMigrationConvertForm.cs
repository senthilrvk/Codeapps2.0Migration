using CodeAppsDataMigration.Data;
using CodeAppsDataMigration.Migration;
using Microsoft.Data.SqlClient;
using Npgsql;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeAppsDataMigration
{
    public partial class DataMigrationConvertForm : Form
    {
        int nFromBranchId = 0;
        int nBranchId = 0, nMainBranchId = 0;
        private DataTable _pgBranchAll = new DataTable();

        public DataMigrationConvertForm()
        {
            InitializeComponent();
        }

        private void DataMigrationConvertForm_Load(object sender, EventArgs e)
        {
            LoadSqlBranches();
            LoadPgMainBranches();
            LoadPgBranchesAll();
        }

        private void LoadSqlBranches()
        {
            try
            {
                using var conn = SqlServerConnection.Create();
                conn.Open();
                using var cmd = new SqlCommand("SELECT * FROM branch", conn);
                using var reader = cmd.ExecuteReader();

                var dt = new DataTable();
                dt.Load(reader);

                dt.Columns.Add("DisplayText", typeof(string), "BranchName + ' [' + BranchId + ']'");
                cmbSqlBranch.DataSource = dt;
                cmbSqlBranch.DisplayMember = "DisplayText";
                cmbSqlBranch.ValueMember = "BranchId";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load SQL Server branches: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPgMainBranches()
        {
            try
            {
                using var conn = PostgresConnection.Create();
                conn.Open();
                using var cmd = new NpgsqlCommand("SELECT * FROM mainbranch", conn);
                using var reader = cmd.ExecuteReader();

                var dt = new DataTable();
                dt.Load(reader);

                dt.Columns.Add("DisplayText", typeof(string), "mainbranchname + ' [' + mainbranchid + ']'");
                cmbPgMainBranch.DataSource = dt;
                cmbPgMainBranch.DisplayMember = "DisplayText";
                cmbPgMainBranch.ValueMember = "mainbranchid";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load PostgreSQL main branches: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPgBranchesAll()
        {
            try
            {
                using var conn = PostgresConnection.Create();
                conn.Open();
                using var cmd = new NpgsqlCommand("SELECT * FROM branch", conn);
                using var reader = cmd.ExecuteReader();

                _pgBranchAll = new DataTable();
                _pgBranchAll.Load(reader);
                _pgBranchAll.Columns.Add("DisplayText", typeof(string), "branchname + ' [' + branchid + ']'");

                FilterPgBranches();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load PostgreSQL branches: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbPgMainBranch_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterPgBranches();
        }

        private void FilterPgBranches()
        {
            if (cmbPgMainBranch.SelectedValue == null || _pgBranchAll.Rows.Count == 0)
                return;

            int mainBranchId = Convert.ToInt32(cmbPgMainBranch.SelectedValue);

            var filtered = _pgBranchAll.Clone();
            foreach (DataRow row in _pgBranchAll.Rows)
            {
                if (Convert.ToInt32(row["mainbranchid"]) == mainBranchId)
                {
                    filtered.ImportRow(row);
                }
            }

            cmbPgBranch.DataSource = filtered;
            cmbPgBranch.DisplayMember = "DisplayText";
            cmbPgBranch.ValueMember = "branchid";
        }

        private async void btnDataTransfer_Click(object sender, EventArgs e)
        {
            if (cmbSqlBranch.SelectedValue == null || cmbPgMainBranch.SelectedValue == null || cmbPgBranch.SelectedValue == null)
            {
                MessageBox.Show("Please select all branches.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = SqlServerConnection.GetConnectionString();
            string pg = PostgresConnection.GetConnectionString();

            nFromBranchId = Convert.ToInt32(cmbSqlBranch.SelectedValue);
            nMainBranchId = Convert.ToInt32(cmbPgMainBranch.SelectedValue);
            nBranchId = Convert.ToInt32(cmbPgBranch.SelectedValue);

            // Disable controls during migration
            btnDataTransfer.Enabled = false;
            cmbSqlBranch.Enabled = false;
            cmbPgMainBranch.Enabled = false;
            cmbPgBranch.Enabled = false;
            progressBar.Value = 0;
            lblStatus.Text = "Starting migration...";

            var progress = new Progress<(string message, int percent)>(p =>
            {
                progressBar.Value = Math.Min(p.percent, 100);
                lblStatus.Text = p.message;
            });

            try
            {
                await Task.Run(() =>
                {
                    var runner = new MigrationRunner(sql, pg);
                    runner.SetProgressCallback((msg, pct) =>
                    {
                        ((IProgress<(string, int)>)progress).Report((msg, pct));
                    });

                    runner.FromDbTaxUpdate();
                    runner.RunAll(nMainBranchId, nBranchId, nFromBranchId);
                    runner.UpdatePrimaryKeyColumns(nMainBranchId, nBranchId);
                });

                progressBar.Value = 100;
                lblStatus.Text = "Migration completed successfully!";
                MessageBox.Show("Migration completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Migration failed!";
                MessageBox.Show("Migration failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable controls
                btnDataTransfer.Enabled = true;
                cmbSqlBranch.Enabled = true;
                cmbPgMainBranch.Enabled = true;
                cmbPgBranch.Enabled = true;
            }
        }
    }
}
