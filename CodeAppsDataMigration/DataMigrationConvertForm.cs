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

                _sqlBranches.Columns.Add("DisplayText", typeof(string), "BranchName + ' [' + BranchId + '] (' + ISNULL(BranchAdr1, '') + ')'");
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

                dt.Columns.Add("DisplayText", typeof(string), "mainbranchname + ' [' + mainbranchid + '] (' + ISNULL(mainbranchadr1, '') + ')'");
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
                _pgBranchAll.Columns.Add("DisplayText", typeof(string), "branchname + ' [' + branchid + '] (' + ISNULL(branchadr1, '') + ')'");

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

                runner.fnSqlMainBranchValueUpdate();

                runner.FromDbTaxUpdate();

                foreach (var map in mappings)
                {
                    if (map.FromBranchId != 0 && map.ToBranchId != 0)
                    {
                        ((IProgress<(string, int)>)progress).Report(($"Migrating {map.Display}...", 0));

                        // Run the whole branch pipeline inside a single PostgreSQL transaction.
                        // If any step (any table, FK update, or setting update) fails, the entire
                        // branch is rolled back so no partial data is left behind.
                        runner.BeginBranchTransaction();
                        try
                        {
                                runner.fnSqlServerTableValueUpdate(map.FromBranchId);
                                runner.RunAll(nMainBranchId, map.ToBranchId, map.FromBranchId);
                                runner.UpdatePrimaryKeyColumns(nMainBranchId, map.ToBranchId, map.FromBranchId);
                                runner.fnControOrderUpdate(nMainBranchId);
                                runner.fnPrintFileNameUpdate(map.FromBranchId, nMainBranchId, map.ToBranchId);
                                runner.fnBranchSettingUpdate(nMainBranchId, map.ToBranchId, map.FromBranchId);
                                runner.fnVouchePrefixUpdate(nMainBranchId, map.ToBranchId, map.FromBranchId);
                                runner.fnBranchUpdate(nMainBranchId, map.ToBranchId, map.FromBranchId);
                                runner.fnBillSeriesInclusiveUpdate(map.FromBranchId, nMainBranchId, map.ToBranchId);
                                runner.fnBillNosUpdate(map.FromBranchId, nMainBranchId, map.ToBranchId);
                                runner.fnDefaultValueUpdate(map.FromBranchId, nMainBranchId, map.ToBranchId);
                                runner.fnHsnUpdate(nMainBranchId, map.ToBranchId);
                              //  runner.fnServiceItemInsertProductSub(nMainBranchId, map.ToBranchId);
                                runner.fnSalesRetLogInsert(map.FromBranchId, nMainBranchId, map.ToBranchId);
                                runner.fnTotalQtyUpdateTransaction(nMainBranchId, map.ToBranchId);
                                runner.fnUserPrevilegeMainUpdate(nMainBranchId, map.ToBranchId);
                                runner.fnUserPrevilegeUpdate(nMainBranchId, map.ToBranchId);
                                runner.fnHospitalSettingUpdate( nMainBranchId, map.ToBranchId, map.FromBranchId);
                                runner.fnChequeDepositFlagUpdate(map.ToBranchId, nMainBranchId);
                                runner.CommitBranchTransaction();
                            }
                            catch (Exception Exme)
                            {
                                runner.RollbackBranchTransaction();
                                // Attach which branch mapping was being processed, then surface
                                // to the outer (UI-thread) handler which shows the full detail.
                                // Also embed the failing query script (when available) directly in
                                // Message so it is visible in the debugger tooltip, not just the
                                // MessageBox / log file.
                                var innerMe = Migration.MigrationException.Find(Exme);
                                string scriptTail = string.IsNullOrEmpty(innerMe?.FailingQuery)
                                    ? ""
                                    : Environment.NewLine + Environment.NewLine +
                                      "---- Failing script ----" + Environment.NewLine +
                                      innerMe.FailingQuery;
                                throw new Exception(
                                    $"Failed while migrating branch '{map.Display}'. " + Exme.Message + scriptTail,
                                    Exme);
                            }

                        }
                    }

                    // Main-branch setting update (not branch-specific) in its own transaction.
                    runner.BeginBranchTransaction();
                    try
                    {
                        runner.fnMainSettingUpdate(nMainBranchId);
                        runner.CommitBranchTransaction();
                    }
                    catch
                    {
                        runner.RollbackBranchTransaction();
                        throw;
                    }
                });

                progressBar.Value = 100;
                lblStatus.Text = "Migration completed successfully!";
                MessageBox.Show("Migration completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Migration failed!";

                // Pull out WHICH table / column / function (structured) if available.
                var me = Migration.MigrationException.Find(ex);
                string tableName = me?.TableName ?? "(unknown)";
                string columnName = me?.ColumnName ?? "(unknown)";
                // Prefer the explicitly-tagged function; otherwise recover it from the
                // stack trace (works for the fnXxx steps that don't wrap their errors).
                string functionName = me?.FunctionName
                    ?? Migration.ExceptionFormatter.ExtractFailingFunction(ex);

                // Short MessageBox first: clearly names the failing function & table.
                string summary =
                    "Function: " + functionName + Environment.NewLine +
                    "Table   : " + tableName + Environment.NewLine +
                    "Column  : " + columnName;
                if (me?.RowNumber != null)
                    summary += Environment.NewLine + "Row     : " + me.RowNumber;
                if (!string.IsNullOrEmpty(me?.FailingQuery))
                    summary += Environment.NewLine + Environment.NewLine + "Query / Script:" + Environment.NewLine + me.FailingQuery;
                summary += Environment.NewLine + Environment.NewLine + "Error: " + GetRootMessage(ex);

                MessageBox.Show(summary,
                    "Migration failed - " + functionName + " / Table: " + tableName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Full detail: which function, which table, which column, constraint,
                // PG/SQL provider info, inner exceptions and stack trace.
                string details =
                    "Failed Function: " + functionName + Environment.NewLine +
                    "Failed Table   : " + tableName + Environment.NewLine +
                    "Failed Column  : " + columnName + Environment.NewLine +
                    (me?.RowNumber != null ? "Approx. Row    : " + me.RowNumber + Environment.NewLine : "") +
                    (me?.FailingQuery != null ? "Failing Query  : " + me.FailingQuery + Environment.NewLine : "") +
                    Environment.NewLine +
                    Migration.ExceptionFormatter.Describe(ex);

                // Persist to a log file so the operator can copy/share the full error.
                string logPath = "";
                try
                {
                    logPath = System.IO.Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        $"migration-error-{DateTime.Now:yyyyMMdd-HHmmss}.log");
                    System.IO.File.WriteAllText(logPath, details);
                }
                catch { /* logging is best-effort */ }

                string message = details;
                if (!string.IsNullOrEmpty(logPath))
                    message += Environment.NewLine + "Saved to: " + logPath;

                ShowErrorDetails("Migration failed - " + functionName + " / Table: " + tableName, message);
            }
            finally
            {
                btnDataTransfer.Enabled = true;
                cmbPgMainBranch.Enabled = true;
                grdBranchMap.Enabled = true;
            }
        }

        /// <summary>
        /// Returns the message of the innermost exception (the real database error),
        /// avoiding the longer re-wrapped messages that also embed the query.
        /// </summary>
        private static string GetRootMessage(Exception ex)
        {
            Exception cur = ex;
            while (cur.InnerException != null)
                cur = cur.InnerException;
            return cur.Message;
        }

        /// <summary>
        /// Shows a resizable, scrollable, copyable dialog with the full error text.
        /// A normal MessageBox truncates long messages and the text can't be selected.
        /// </summary>
        private void ShowErrorDetails(string title, string details)
        {
            using var dlg = new Form
            {
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                Size = new System.Drawing.Size(800, 500),
                MinimizeBox = false,
                MaximizeBox = true
            };

            var txt = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 9F),
                Text = details
            };

            var pnl = new Panel { Dock = DockStyle.Bottom, Height = 44 };

            var btnCopy = new Button
            {
                Text = "Copy",
                Width = 90,
                Anchor = AnchorStyles.Right,
                Location = new System.Drawing.Point(dlg.ClientSize.Width - 200, 8)
            };
            btnCopy.Click += (s, e) =>
            {
                try { Clipboard.SetText(details); } catch { }
            };

            var btnClose = new Button
            {
                Text = "Close",
                Width = 90,
                Anchor = AnchorStyles.Right,
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(dlg.ClientSize.Width - 100, 8)
            };

            pnl.Controls.Add(btnCopy);
            pnl.Controls.Add(btnClose);
            dlg.Controls.Add(txt);
            dlg.Controls.Add(pnl);
            dlg.AcceptButton = btnClose;

            dlg.ShowDialog(this);
        }


    }
}
