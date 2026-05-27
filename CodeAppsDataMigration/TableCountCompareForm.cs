using CodeAppsDataMigration.Data;
using CodeAppsDataMigration.Migration;
using Microsoft.Data.SqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeAppsDataMigration
{
    public partial class TableCountCompareForm : Form
    {
        private DataTable _pgBranchAll = new DataTable();
        private DataTable _sqlBranches = new DataTable();

        public TableCountCompareForm()
        {
            InitializeComponent();
        }

        private void TableCountCompareForm_Load(object sender, EventArgs e)
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
                using var cmd = new SqlCommand("SELECT BranchId, BranchName FROM branch ORDER BY BranchName", conn);
                using var reader = cmd.ExecuteReader();

                _sqlBranches = new DataTable();
                _sqlBranches.Load(reader);
                _sqlBranches.Columns.Add("DisplayText", typeof(string), "BranchName + ' [' + BranchId + ']'");

                cmbSqlBranch.DataSource = _sqlBranches;
                cmbSqlBranch.DisplayMember = "DisplayText";
                cmbSqlBranch.ValueMember = "BranchId";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load SQL Server branches: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPgMainBranches()
        {
            try
            {
                using var conn = PostgresConnection.Create();
                conn.Open();
                using var cmd = new NpgsqlCommand("SELECT mainbranchid, mainbranchname FROM mainbranch ORDER BY mainbranchname", conn);
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
                MessageBox.Show("Failed to load PostgreSQL main branches: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPgBranchesAll()
        {
            try
            {
                using var conn = PostgresConnection.Create();
                conn.Open();
                using var cmd = new NpgsqlCommand("SELECT branchid, branchname, mainbranchid FROM branch ORDER BY branchname", conn);
                using var reader = cmd.ExecuteReader();

                _pgBranchAll = new DataTable();
                _pgBranchAll.Load(reader);
                _pgBranchAll.Columns.Add("DisplayText", typeof(string), "branchname + ' [' + branchid + ']'");

                FilterPgBranches();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load PostgreSQL branches: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    filtered.ImportRow(row);
            }

            cmbPgBranch.DataSource = filtered;
            cmbPgBranch.DisplayMember = "DisplayText";
            cmbPgBranch.ValueMember = "branchid";
        }

        private async void btnCompare_Click(object sender, EventArgs e)
        {
            if (cmbPgMainBranch.SelectedValue == null || cmbSqlBranch.SelectedValue == null || cmbPgBranch.SelectedValue == null)
            {
                MessageBox.Show("Please select PG main branch, SQL branch and PG branch.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            long mainBranchId = Convert.ToInt64(cmbPgMainBranch.SelectedValue);
            long fromBranchId = Convert.ToInt64(cmbSqlBranch.SelectedValue);
            long toBranchId = Convert.ToInt64(cmbPgBranch.SelectedValue);

            MigrationConfig.nMainBranchId = mainBranchId;
            MigrationConfig.nBranchId = toBranchId;
            MigrationConfig.nFromBranchId = fromBranchId;

            var tables = MigrationConfig.Tables
                .Select(t => new TableMap
                {
                    SqlTable = t.SqlTable,
                    PgTable = (t.PgTable ?? string.Empty).Replace("@", ""),
                    condition = t.condition
                })
                .ToList();

            grdCounts.Rows.Clear();
            foreach (var t in tables)
            {
                int idx = grdCounts.Rows.Add(t.SqlTable, "-", t.PgTable, "-", "-", "Pending");
                grdCounts.Rows[idx].DefaultCellStyle.BackColor = Color.White;
            }

            progressBar.Value = 0;
            progressBar.Maximum = tables.Count;
            btnCompare.Enabled = false;
            btnExport.Enabled = false;
            cmbPgMainBranch.Enabled = false;
            cmbSqlBranch.Enabled = false;
            cmbPgBranch.Enabled = false;
            lblSummary.Text = "Comparing...";

            string sqlConn = SqlServerConnection.GetConnectionString();
            string pgConn = PostgresConnection.GetConnectionString();

            int mismatchCount = 0;
            int errorCount = 0;
            long totalSql = 0;
            long totalPg = 0;

            try
            {
                for (int i = 0; i < tables.Count; i++)
                {
                    var map = tables[i];
                    lblStatus.Text = $"[{i + 1}/{tables.Count}] {map.SqlTable}";

                    var result = await Task.Run(() => CompareOne(sqlConn, pgConn, map, toBranchId));

                    var row = grdCounts.Rows[i];
                    row.Cells["colSqlCount"].Value = result.SqlCount.HasValue ? result.SqlCount.Value.ToString("N0") : "ERR";
                    row.Cells["colPgCount"].Value = result.PgCount.HasValue ? result.PgCount.Value.ToString("N0") : "ERR";

                    if (result.SqlCount.HasValue && result.PgCount.HasValue)
                    {
                        long diff = result.SqlCount.Value - result.PgCount.Value;
                        row.Cells["colDiff"].Value = diff.ToString("N0");
                        totalSql += result.SqlCount.Value;
                        totalPg += result.PgCount.Value;

                        if (diff == 0)
                        {
                            row.Cells["colStatus"].Value = "MATCH";
                            row.DefaultCellStyle.BackColor = Color.FromArgb(240, 255, 244);
                            row.DefaultCellStyle.ForeColor = Color.FromArgb(34, 84, 61);
                        }
                        else
                        {
                            row.Cells["colStatus"].Value = "MISMATCH";
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
                            row.DefaultCellStyle.ForeColor = Color.FromArgb(197, 48, 48);
                            row.DefaultCellStyle.Font = new Font(grdCounts.Font, FontStyle.Bold);
                            mismatchCount++;
                        }
                    }
                    else
                    {
                        string msg = result.Error ?? "Error";
                        row.Cells["colDiff"].Value = "-";
                        row.Cells["colStatus"].Value = "ERROR: " + Truncate(msg, 60);
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 215);
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(146, 64, 14);
                        errorCount++;
                    }

                    progressBar.Value = i + 1;
                }

                lblSummary.Text =
                    $"Total: {tables.Count} tables  |  Match: {tables.Count - mismatchCount - errorCount}  |  " +
                    $"Mismatch: {mismatchCount}  |  Errors: {errorCount}  |  " +
                    $"SQL rows: {totalSql:N0}  |  PG rows: {totalPg:N0}";
                lblStatus.Text = "Done.";
                btnExport.Enabled = grdCounts.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Comparison failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Failed.";
            }
            finally
            {
                btnCompare.Enabled = true;
                cmbPgMainBranch.Enabled = true;
                cmbSqlBranch.Enabled = true;
                cmbPgBranch.Enabled = true;
            }
        }

        private static CompareResult CompareOne(string sqlConn, string pgConn, TableMap map, long pgBranchId)
        {
            var result = new CompareResult();

            try
            {
                using var sql = new SqlConnection(sqlConn);
                sql.Open();
                string sqlQuery = $"SELECT COUNT_BIG(*) FROM dbo.[{map.SqlTable}] " + (map.condition ?? string.Empty);
                using var sqlCmd = new SqlCommand(sqlQuery, sql) { CommandTimeout = 120 };
                result.SqlCount = Convert.ToInt64(sqlCmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                result.Error = "SQL: " + ex.Message;
            }

            try
            {
                using var pg = new NpgsqlConnection(pgConn);
                pg.Open();
                string pgWhere = BuildPgWhere(pg, map.PgTable, pgBranchId);
                string pgQuery = $"SELECT COUNT(*) FROM public.\"{map.PgTable}\" {pgWhere}";
                using var pgCmd = new NpgsqlCommand(pgQuery, pg) { CommandTimeout = 120 };
                var obj = pgCmd.ExecuteScalar();
                result.PgCount = obj == null || obj == DBNull.Value ? 0L : Convert.ToInt64(obj);
            }
            catch (Exception ex)
            {
                result.Error = (result.Error == null ? string.Empty : result.Error + " | ") + "PG: " + ex.Message;
            }

            return result;
        }

        private static string BuildPgWhere(NpgsqlConnection pg, string pgTable, long pgBranchId)
        {
            using var cmd = new NpgsqlCommand(@"
                SELECT 1 FROM information_schema.columns
                WHERE table_schema = 'public' AND table_name = @t AND column_name = 'branchid' LIMIT 1", pg);
            cmd.Parameters.AddWithValue("t", pgTable);
            var has = cmd.ExecuteScalar();
            return has != null ? $"WHERE branchid = {pgBranchId}" : string.Empty;
        }

        private static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            s = Regex.Replace(s, @"\s+", " ").Trim();
            return s.Length <= max ? s : s.Substring(0, max) + "...";
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"TableCountCompare_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("SQL Table,SQL Count,PG Table,PG Count,Diff,Status");
                foreach (DataGridViewRow row in grdCounts.Rows)
                {
                    sb.Append(Csv(row.Cells["colSqlTable"].Value)).Append(',');
                    sb.Append(Csv(row.Cells["colSqlCount"].Value)).Append(',');
                    sb.Append(Csv(row.Cells["colPgTable"].Value)).Append(',');
                    sb.Append(Csv(row.Cells["colPgCount"].Value)).Append(',');
                    sb.Append(Csv(row.Cells["colDiff"].Value)).Append(',');
                    sb.AppendLine(Csv(row.Cells["colStatus"].Value));
                }
                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Exported to:\n" + dlg.FileName, "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string Csv(object? v)
        {
            string s = v?.ToString() ?? string.Empty;
            if (s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
                s = "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }

        private class CompareResult
        {
            public long? SqlCount { get; set; }
            public long? PgCount { get; set; }
            public string? Error { get; set; }
        }
    }
}
