using CodeAppsDataMigration.Data;
using CodeAppsDataMigration.Migration;
using Microsoft.Data.SqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeAppsDataMigration
{
    public partial class DataMigrationConvertForm : Form
    {
        private DataTable _pgBranchAll = new DataTable();
        private DataTable _sqlBranches = new DataTable();

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

                _sqlBranches = new DataTable();
                _sqlBranches.Load(reader);

                _sqlBranches.Columns.Add("DisplayText", typeof(string), "BranchName + ' [' + BranchId + ']'");
                PopulateGridSqlBranches();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load SQL Server branches: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateGridSqlBranches()
        {
            grdBranchMap.Rows.Clear();
            foreach (DataRow row in _sqlBranches.Rows)
            {
                int rowIdx = grdBranchMap.Rows.Add();
                var gridRow = grdBranchMap.Rows[rowIdx];
                gridRow.Cells[colSqlBranch.Name].Value = row["DisplayText"];
                gridRow.Tag = Convert.ToInt32(row["BranchId"]);
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

            colPgBranch.DataSource = filtered;
            colPgBranch.DisplayMember = "DisplayText";
            colPgBranch.ValueMember = "branchid";

            foreach (DataGridViewRow gridRow in grdBranchMap.Rows)
            {
                gridRow.Cells[colPgBranch.Name].Value = null;
            }
        }

        private async void btnDataTransfer_Click(object sender, EventArgs e)
        {
            if (cmbPgMainBranch.SelectedValue == null)
            {
                MessageBox.Show("Please select a PostgreSQL main branch.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var mappings = new List<(int FromBranchId, int ToBranchId, string Display)>();
            foreach (DataGridViewRow gridRow in grdBranchMap.Rows)
            {
                var pgVal = gridRow.Cells[colPgBranch.Name].Value;
                if (pgVal == null) continue;

                int fromId = (int)gridRow.Tag;
                int toId = Convert.ToInt32(pgVal);
                string display = Convert.ToString(gridRow.Cells[colSqlBranch.Name].Value);
                mappings.Add((fromId, toId, display));
            }

            if (mappings.Count == 0)
            {
                MessageBox.Show("Please map at least one SQL branch to a PostgreSQL branch.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int nMainBranchId = Convert.ToInt32(cmbPgMainBranch.SelectedValue);
            string sql = SqlServerConnection.GetConnectionString();
            string pg = PostgresConnection.GetConnectionString();

            btnDataTransfer.Enabled = false;
            cmbPgMainBranch.Enabled = false;
            grdBranchMap.Enabled = false;
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

                    foreach (var map in mappings)
                    {
                        ((IProgress<(string, int)>)progress).Report(($"Migrating {map.Display}...", 0));
                        runner.RunAll(nMainBranchId, map.ToBranchId, map.FromBranchId);
                        runner.UpdatePrimaryKeyColumns(nMainBranchId, map.ToBranchId);
                        runner.fnBranchSettingUpdate(nMainBranchId, map.ToBranchId, map.FromBranchId);
                        runner.fnVouchePrefixUpdate(nMainBranchId, map.ToBranchId, map.FromBranchId);
                    }

                    runner.fnMainSettingUpdate(nMainBranchId);
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
                btnDataTransfer.Enabled = true;
                cmbPgMainBranch.Enabled = true;
                grdBranchMap.Enabled = true;
            }
        }
    }
}
