using CodeAppsDataMigration.Migration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CodeAppsDataMigration
{
    /// <summary>
    /// Offline -> Online PostgreSQL migration.
    ///
    /// Branch migration mode (default, same workflow as the SQL Server "Data Migration" page):
    ///   pick the online main branch, map each offline branch to an online branch, and the
    ///   data is copied with branch ids rewritten and shared-table keys re-linked.
    /// Full copy mode: clone every table into an EMPTY online database (same schema).
    /// </summary>
    public partial class PgToPgMigrationForm : Form
    {
        private readonly string _xmlPath;
        private List<PgTableInfo> _tables = new();
        private DataTable _tgtBranchesAll = new();
        private CancellationTokenSource? _cts;

        public PgToPgMigrationForm()
        {
            InitializeComponent();
            _xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConnectionStrings.xml");
        }

        // ------------------------------------------------------------------
        //  Load / save connection details
        // ------------------------------------------------------------------

        private void PgToPgMigrationForm_Load(object sender, EventArgs e)
        {
            LoadConnectionDefaults();
            ApplyMode();
        }

        private void LoadConnectionDefaults()
        {
            if (!File.Exists(_xmlPath)) return;
            try
            {
                var doc = XDocument.Load(_xmlPath);
                var pg = doc.Root?.Element("Postgres");
                if (pg != null)
                    Fill(pg, txtSrcHost, txtSrcPort, cmbSrcDatabase, txtSrcUser, txtSrcPassword);

                var mig = doc.Root?.Element("PgMigration");
                if (mig?.Element("Source") != null)
                    Fill(mig.Element("Source")!, txtSrcHost, txtSrcPort, cmbSrcDatabase, txtSrcUser, txtSrcPassword);
                if (mig?.Element("Target") != null)
                    Fill(mig.Element("Target")!, txtTgtHost, txtTgtPort, cmbTgtDatabase, txtTgtUser, txtTgtPassword);
                else
                {
                    txtTgtPort.Text = "5432";
                    txtTgtUser.Text = txtSrcUser.Text;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read ConnectionStrings.xml: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void Fill(XElement el, TextBox host, TextBox port, ComboBox db, TextBox user, TextBox pwd)
        {
            host.Text = el.Element("Host")?.Value ?? "";
            port.Text = el.Element("Port")?.Value ?? "5432";
            db.Text = el.Element("Database")?.Value ?? "";
            user.Text = el.Element("Username")?.Value ?? "";
            pwd.Text = el.Element("Password")?.Value ?? "";
        }

        private void btnSaveConnections_Click(object sender, EventArgs e)
        {
            try
            {
                var doc = File.Exists(_xmlPath) ? XDocument.Load(_xmlPath) : new XDocument(new XElement("ConnectionStrings"));
                var root = doc.Root!;
                root.Element("PgMigration")?.Remove();
                root.Add(new XElement("PgMigration",
                    Section("Source", txtSrcHost, txtSrcPort, cmbSrcDatabase, txtSrcUser, txtSrcPassword),
                    Section("Target", txtTgtHost, txtTgtPort, cmbTgtDatabase, txtTgtUser, txtTgtPassword)));
                doc.Save(_xmlPath);
                MessageBox.Show("Offline / online connection details saved.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static XElement Section(string name, TextBox host, TextBox port, ComboBox db, TextBox user, TextBox pwd) =>
            new XElement(name,
                new XElement("Host", host.Text.Trim()),
                new XElement("Port", port.Text.Trim()),
                new XElement("Database", db.Text.Trim()),
                new XElement("Username", user.Text.Trim()),
                new XElement("Password", pwd.Text.Trim()));

        // ------------------------------------------------------------------
        //  Connection strings / test / list databases
        // ------------------------------------------------------------------

        private static string BuildConnStr(TextBox host, TextBox port, string database, TextBox user, TextBox pwd, int timeout = 15) =>
            $"Host={host.Text.Trim()};Port={port.Text.Trim()};Database={database};" +
            $"Username={user.Text.Trim()};Password={pwd.Text.Trim()};" +
            $"Timeout={timeout};CommandTimeout=0;KeepAlive=30;Include Error Detail=true;";

        private string SourceConnStr => BuildConnStr(txtSrcHost, txtSrcPort, cmbSrcDatabase.Text.Trim(), txtSrcUser, txtSrcPassword);
        private string TargetConnStr => BuildConnStr(txtTgtHost, txtTgtPort, cmbTgtDatabase.Text.Trim(), txtTgtUser, txtTgtPassword);

        private void btnTestSrc_Click(object sender, EventArgs e) => TestConnection("Offline (source)", SourceConnStr);
        private void btnTestTgt_Click(object sender, EventArgs e) => TestConnection("Online (target)", TargetConnStr);

        private static void TestConnection(string label, string connStr)
        {
            try
            {
                using var conn = new NpgsqlConnection(connStr);
                conn.Open();
                MessageBox.Show($"{label} PostgreSQL connection successful!\nServer version: {conn.PostgreSqlVersion}",
                    "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{label} PostgreSQL connection failed:\n{ex.Message}", "Test", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoadSrcDbs_Click(object sender, EventArgs e) =>
            LoadDatabases(cmbSrcDatabase, BuildConnStr(txtSrcHost, txtSrcPort, "postgres", txtSrcUser, txtSrcPassword, 5));

        private void btnLoadTgtDbs_Click(object sender, EventArgs e) =>
            LoadDatabases(cmbTgtDatabase, BuildConnStr(txtTgtHost, txtTgtPort, "postgres", txtTgtUser, txtTgtPassword, 5));

        private static void LoadDatabases(ComboBox cmb, string connStr)
        {
            try
            {
                var current = cmb.Text;
                cmb.Items.Clear();
                using var conn = new NpgsqlConnection(connStr);
                conn.Open();
                using var cmd = new NpgsqlCommand("SELECT datname FROM pg_database WHERE datistemplate = false ORDER BY datname", conn);
                using var rd = cmd.ExecuteReader();
                while (rd.Read()) cmb.Items.Add(rd.GetString(0));

                if (!string.IsNullOrEmpty(current))
                {
                    int idx = cmb.FindStringExact(current);
                    if (idx >= 0) cmb.SelectedIndex = idx; else cmb.Text = current;
                }
                else if (cmb.Items.Count > 0)
                    cmb.DroppedDown = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load databases:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtSrcHost.Text) || string.IsNullOrWhiteSpace(cmbSrcDatabase.Text))
            {
                MessageBox.Show("Enter the offline (source) host and database.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtTgtHost.Text) || string.IsNullOrWhiteSpace(cmbTgtDatabase.Text))
            {
                MessageBox.Show("Enter the online (target) host and database.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (SourceConnStr.Equals(TargetConnStr, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Source and target point to the same database.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // ------------------------------------------------------------------
        //  Mode switch
        // ------------------------------------------------------------------

        private bool BranchMode => rdoBranchMode.Checked;

        private void rdoMode_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton r && !r.Checked) return;
            ApplyMode();
        }

        private void ApplyMode()
        {
            bool b = BranchMode;
            pnlBranch.Visible = b;
            grdTables.Visible = !b;

            btnCreateBranch.Visible = b;
            btnLoadBranches.Visible = b;
            btnLoadTables.Visible = !b;
            btnSelectAll.Visible = !b;
            btnSelectNone.Visible = !b;

            chkCopyProfile.Visible = b;
            chkMergeSettings.Visible = b;
            chkTruncate.Visible = !b;
            chkResetSequences.Visible = !b;

            btnStart.Enabled = b ? grdBranchMap.Rows.Count > 0 : _tables.Any(t => t.ExistsInTarget);
            lblSummary.Text = b
                ? "Enter both connections, click Load Branches, pick the online main branch and map each offline branch."
                : "Enter both connections and click Load Tables.";
        }

        // ==================================================================
        //  BRANCH MIGRATION MODE
        // ==================================================================

        private long _preferredTgtMain;   // main branch just created online; selected + auto-mapped after Load Branches

        /// <summary>Step 1: create the offline main branch and its branches on the online server via the app's API.</summary>
        private void btnCreateBranch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSrcHost.Text) || string.IsNullOrWhiteSpace(cmbSrcDatabase.Text))
            {
                MessageBox.Show("Enter the offline (source) host and database first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using var dlg = new PgCreateBranchForm(SourceConnStr);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.CreatedMainBranchId > 0)
            {
                _preferredTgtMain = dlg.CreatedMainBranchId;
                Log($"Main branch created online: mainbranchid {dlg.CreatedMainBranchId} (offline main {dlg.OfflineMainBranchId}).");
                if (!string.IsNullOrWhiteSpace(txtTgtHost.Text) && !string.IsNullOrWhiteSpace(cmbTgtDatabase.Text))
                    btnLoadBranches_Click(sender, e);
            }
        }

        private async void btnLoadBranches_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;
            SetBusy(true);
            txtLog.Clear();
            grdBranchMap.Rows.Clear();
            lblStatus.Text = "Reading branches...";
            try
            {
                var srcCs = SourceConnStr; var tgtCs = TargetConnStr;
                DataTable srcBranches = null!, tgtMains = null!, tgtBranches = null!;
                await Task.Run(() =>
                {
                    using (var c = new NpgsqlConnection(srcCs))
                    {
                        c.Open();
                        srcBranches = Query(c, @"SELECT b.branchid, b.mainbranchid, b.branchname, COALESCE(m.mainbranchname,'') mainbranchname
                                                 FROM branch b LEFT JOIN mainbranch m ON m.mainbranchid = b.mainbranchid
                                                 ORDER BY b.mainbranchid, b.branchid");
                    }
                    using (var c = new NpgsqlConnection(tgtCs))
                    {
                        c.Open();
                        tgtMains = Query(c, "SELECT mainbranchid, mainbranchname, COALESCE(tempid, 0) tempid FROM mainbranch ORDER BY mainbranchname");
                        tgtBranches = Query(c, "SELECT branchid, mainbranchid, branchname, COALESCE(tempid, 0) tempid FROM branch ORDER BY branchname");
                    }
                });

                foreach (DataRow r in srcBranches.Rows)
                {
                    long bid = Convert.ToInt64(r["branchid"]), mid = Convert.ToInt64(r["mainbranchid"]);
                    string text = $"{r["branchname"]} [{bid}]   -   main: {r["mainbranchname"]} [{mid}]";
                    int i = grdBranchMap.Rows.Add(text, null);
                    grdBranchMap.Rows[i].Tag = (mid, bid);
                }

                tgtMains.Columns.Add("DisplayText", typeof(string), "mainbranchname + ' [' + mainbranchid + ']'");
                cmbTgtMainBranch.DataSource = tgtMains;
                cmbTgtMainBranch.DisplayMember = "DisplayText";
                cmbTgtMainBranch.ValueMember = "mainbranchid";

                _tgtBranchesAll = tgtBranches;
                _tgtBranchesAll.Columns.Add("DisplayText", typeof(string), "branchname + ' [' + branchid + ']'");

                // Prefer the main branch just created online, else the one whose tempid points at the first offline main.
                long wantMain = _preferredTgtMain;
                if (wantMain == 0 && grdBranchMap.Rows.Count > 0)
                {
                    var (firstSrcMain, _) = ((long, long))grdBranchMap.Rows[0].Tag!;
                    foreach (DataRow r in tgtMains.Rows)
                        if (Convert.ToInt64(r["tempid"]) == firstSrcMain) { wantMain = Convert.ToInt64(r["mainbranchid"]); break; }
                }
                if (wantMain != 0)
                    foreach (DataRow r in tgtMains.Rows)
                        if (Convert.ToInt64(r["mainbranchid"]) == wantMain) { cmbTgtMainBranch.SelectedValue = wantMain; break; }
                _preferredTgtMain = 0;
                FilterTgtBranches();
                int auto = AutoMapByTempId();
                if (auto > 0) Log($"{auto} offline branch(es) mapped automatically to the online branches created from them.");

                lblSummary.Text = $"{srcBranches.Rows.Count} offline branches   |   {tgtMains.Rows.Count} online main branches   |   {tgtBranches.Rows.Count} online branches";
                lblStatus.Text = "Select the online main branch, then choose an online branch for each offline branch to migrate.";
                Log("Branches loaded. Map each offline branch to the online branch that was created for it, then click Start Migration.");
                btnStart.Enabled = grdBranchMap.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Failed to read branches.";
                MessageBox.Show("Failed to read branches:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SetBusy(false); }
        }

        private static DataTable Query(NpgsqlConnection c, string sql)
        {
            using var cmd = new NpgsqlCommand(sql, c);
            var dt = new DataTable();
            dt.Load(cmd.ExecuteReader());
            return dt;
        }

        private void cmbTgtMainBranch_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterTgtBranches();
            AutoMapByTempId();
        }

        /// <summary>
        /// Online branches created by "Create Main Branch" carry tempid = offline branchid.
        /// Pre-select those so the operator only has to confirm. Returns how many were mapped.
        /// </summary>
        private int AutoMapByTempId()
        {
            if (cmbTgtMainBranch.SelectedValue == null || _tgtBranchesAll.Rows.Count == 0) return 0;
            long mainId = Convert.ToInt64(cmbTgtMainBranch.SelectedValue);
            int n = 0;
            foreach (DataGridViewRow row in grdBranchMap.Rows)
            {
                var (_, srcBranch) = ((long, long))row.Tag!;
                foreach (DataRow r in _tgtBranchesAll.Rows)
                {
                    if (Convert.ToInt64(r["mainbranchid"]) != mainId || Convert.ToInt64(r["tempid"]) != srcBranch) continue;
                    row.Cells[colTgtBranch.Index].Value = r["branchid"];
                    n++;
                    break;
                }
            }
            return n;
        }

        private void FilterTgtBranches()
        {
            if (cmbTgtMainBranch.SelectedValue == null || _tgtBranchesAll.Rows.Count == 0) return;
            long mainId = Convert.ToInt64(cmbTgtMainBranch.SelectedValue);

            var filtered = _tgtBranchesAll.Clone();
            foreach (DataRow r in _tgtBranchesAll.Rows)
                if (Convert.ToInt64(r["mainbranchid"]) == mainId) filtered.ImportRow(r);

            colTgtBranch.DataSource = filtered;
            colTgtBranch.DisplayMember = "DisplayText";
            colTgtBranch.ValueMember = "branchid";
            foreach (DataGridViewRow row in grdBranchMap.Rows)
                row.Cells[colTgtBranch.Index].Value = null;
        }

        private void grdBranchMap_DataError(object sender, DataGridViewDataErrorEventArgs e) => e.ThrowException = false;

        private async void StartBranchMigration()
        {
            if (cmbTgtMainBranch.SelectedValue == null)
            {
                MessageBox.Show("Select the online main branch.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            long tgtMain = Convert.ToInt64(cmbTgtMainBranch.SelectedValue);

            var maps = new List<PgBranchMapping>();
            foreach (DataGridViewRow row in grdBranchMap.Rows)
            {
                var v = row.Cells[colTgtBranch.Index].Value;
                if (v == null || v == DBNull.Value) continue;
                var (srcMain, srcBranch) = ((long, long))row.Tag!;
                maps.Add(new PgBranchMapping
                {
                    SrcMainId = srcMain, SrcBranchId = srcBranch,
                    TgtMainId = tgtMain, TgtBranchId = Convert.ToInt64(v),
                    Display = Convert.ToString(row.Cells[colSrcBranch.Index].Value) ?? ""
                });
            }
            if (maps.Count == 0)
            {
                MessageBox.Show("Choose an online branch for at least one offline branch.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (maps.Select(m => m.TgtBranchId).Distinct().Count() != maps.Count)
            {
                MessageBox.Show("The same online branch is selected for more than one offline branch.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string list = string.Join("\n", maps.Select(m => $"   {m.Display}   ->   online branch [{m.TgtBranchId}]"));
            string warn =
                $"Migrate {maps.Count} branch(es) into online main branch [{tgtMain}]:\n\n{list}\n\n" +
                $"FROM  offline : {txtSrcHost.Text.Trim()} / {cmbSrcDatabase.Text.Trim()}\n" +
                $"TO    online  : {txtTgtHost.Text.Trim()} / {cmbTgtDatabase.Text.Trim()}\n\n" +
                "Existing online data of the mapped branches will be REPLACED.\nContinue?";
            if (MessageBox.Show(warn, "Confirm Branch Migration", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            var opts = new PgBranchOptions
            {
                CopyBranchProfile = chkCopyProfile.Checked,
                MergeSettings = chkMergeSettings.Checked,
                DisableTriggers = chkDisableTriggers.Checked
            };

            SetBusy(true);
            btnCancel.Enabled = true;
            _cts = new CancellationTokenSource();
            progressBar.Value = 0;
            txtLog.Clear();

            var progress = new Progress<(string message, int percent)>(p =>
            {
                progressBar.Value = Math.Clamp(p.percent, 0, 100);
                lblStatus.Text = p.message;
            });
            long totalRows = 0;
            var tableDone = new Progress<PgTableResult>(r =>
            {
                totalRows += r.RowsCopied;
                Log($"{(r.Ok ? "OK  " : "ERR ")} {r.Table,-45} {r.RowsCopied,10:N0}   {r.Status}");
            });

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var ct = _cts.Token;
                var srcCs = SourceConnStr; var tgtCs = TargetConnStr;
                await Task.Run(() =>
                {
                    var m = new PgBranchMigrator(srcCs, tgtCs);
                    m.SetProgressCallback((msg, pct) => ((IProgress<(string, int)>)progress).Report((msg, pct)));
                    m.SetTableDoneCallback(r => ((IProgress<PgTableResult>)tableDone).Report(r));
                    m.Run(maps, opts, ct);
                }, ct);

                sw.Stop();
                progressBar.Value = 100;
                lblStatus.Text = $"Branch migration completed in {sw.Elapsed.TotalSeconds:N1}s.";
                Log($"\nCOMPLETED: {maps.Count} branch(es), {totalRows:N0} rows in {sw.Elapsed.TotalSeconds:N1}s.");
                MessageBox.Show($"Branch migration completed successfully!\n\n{maps.Count} branch(es), {totalRows:N0} rows copied in {sw.Elapsed.TotalSeconds:N1}s.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "Migration cancelled - online database rolled back, nothing was changed.";
                Log("\nCANCELLED - all changes on the online database were rolled back.");
                MessageBox.Show("Migration cancelled. All changes on the online database were rolled back.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Migration failed - online database rolled back.";
                Log("\nFAILED: " + GetRootMessage(ex));
                ReportFailure(ex, "Branch migration");
            }
            finally
            {
                SetBusy(false);
                btnCancel.Enabled = false;
                _cts = null;
            }
        }

        private void Log(string line)
        {
            txtLog.AppendText(line + Environment.NewLine);
        }

        // ==================================================================
        //  FULL COPY MODE
        // ==================================================================

        private async void btnLoadTables_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            SetBusy(true);
            grdTables.Rows.Clear();
            lblSummary.Text = "Reading tables...";
            _cts = new CancellationTokenSource();

            var progress = new Progress<(string message, int percent)>(p =>
            {
                progressBar.Value = Math.Clamp(p.percent, 0, 100);
                lblStatus.Text = p.message;
            });

            try
            {
                var ct = _cts.Token;
                var migrator = new PgToPgMigrator(SourceConnStr, TargetConnStr);
                migrator.SetProgressCallback((m, p) => ((IProgress<(string, int)>)progress).Report((m, p)));
                _tables = await Task.Run(() => migrator.DiscoverTables(ct), ct);

                foreach (var t in _tables)
                {
                    int r = grdTables.Rows.Add(t.ExistsInTarget, t.Name, t.SourceRows.ToString("N0"),
                        t.ExistsInTarget ? t.TargetRows!.Value.ToString("N0") : "-", "",
                        t.ExistsInTarget ? "Ready" : "Missing in target");
                    grdTables.Rows[r].Tag = t;
                    if (!t.ExistsInTarget)
                    {
                        grdTables.Rows[r].DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 215);
                        grdTables.Rows[r].Cells[colSelect.Index].ReadOnly = true;
                    }
                }
                UpdateSummary();
                lblStatus.Text = "Tables loaded. Tick the tables to copy, then click Start Migration.";
                btnStart.Enabled = _tables.Any(t => t.ExistsInTarget);
            }
            catch (OperationCanceledException) { lblStatus.Text = "Cancelled."; }
            catch (Exception ex)
            {
                lblStatus.Text = "Failed to read tables.";
                MessageBox.Show("Failed to read tables:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SetBusy(false); _cts = null; }
        }

        private void UpdateSummary()
        {
            int selected = 0, missing = 0; long rows = 0;
            foreach (DataGridViewRow row in grdTables.Rows)
            {
                var t = (PgTableInfo)row.Tag!;
                if (!t.ExistsInTarget) { missing++; continue; }
                if (IsChecked(row)) { selected++; rows += t.SourceRows; }
            }
            lblSummary.Text = $"{_tables.Count} tables in offline DB   |   {selected} selected ({rows:N0} rows)   |   {missing} missing in online DB";
        }

        private bool IsChecked(DataGridViewRow row) => row.Cells[colSelect.Index].Value is true;

        private void grdTables_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == colSelect.Index) UpdateSummary();
        }

        private void grdTables_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (grdTables.IsCurrentCellDirty) grdTables.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void btnSelectAll_Click(object sender, EventArgs e) => SetAll(true);
        private void btnSelectNone_Click(object sender, EventArgs e) => SetAll(false);

        private void SetAll(bool value)
        {
            foreach (DataGridViewRow row in grdTables.Rows)
                if (((PgTableInfo)row.Tag!).ExistsInTarget) row.Cells[colSelect.Index].Value = value;
            UpdateSummary();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;
            if (BranchMode) StartBranchMigration(); else StartFullCopy();
        }

        private async void StartFullCopy()
        {
            var selected = grdTables.Rows.Cast<DataGridViewRow>().Where(IsChecked).Select(r => (PgTableInfo)r.Tag!).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("Select at least one table to copy.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            long srcRows = selected.Sum(t => t.SourceRows);
            long tgtRows = selected.Sum(t => t.TargetRows ?? 0);
            string warn =
                $"Copy {selected.Count} tables ({srcRows:N0} rows)\n\n" +
                $"FROM  offline : {txtSrcHost.Text.Trim()} / {cmbSrcDatabase.Text.Trim()}\n" +
                $"TO    online  : {txtTgtHost.Text.Trim()} / {cmbTgtDatabase.Text.Trim()}\n\n" +
                (chkTruncate.Checked
                    ? $"The selected online tables will be EMPTIED first ({tgtRows:N0} existing rows will be deleted).\n\n"
                    : "Existing online rows are kept; duplicate keys will make the copy fail.\n\n") +
                "Continue?";
            if (MessageBox.Show(warn, "Confirm Full Copy", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            var opts = new PgToPgOptions
            {
                TruncateTarget = chkTruncate.Checked,
                DisableTriggers = chkDisableTriggers.Checked,
                ResetSequences = chkResetSequences.Checked,
                SkipMissingTables = true
            };

            var rowByTable = grdTables.Rows.Cast<DataGridViewRow>()
                .ToDictionary(r => ((PgTableInfo)r.Tag!).Name, r => r, StringComparer.OrdinalIgnoreCase);
            foreach (var r in rowByTable.Values)
            {
                r.Cells[colCopied.Index].Value = "";
                if (((PgTableInfo)r.Tag!).ExistsInTarget)
                {
                    r.Cells[colStatus.Index].Value = IsChecked(r) ? "Waiting" : "Not selected";
                    r.DefaultCellStyle.BackColor = Color.White;
                }
            }

            SetBusy(true);
            btnCancel.Enabled = true;
            _cts = new CancellationTokenSource();
            progressBar.Value = 0;

            var progress = new Progress<(string message, int percent)>(p =>
            {
                progressBar.Value = Math.Clamp(p.percent, 0, 100);
                lblStatus.Text = p.message;
            });
            var tableDone = new Progress<PgTableResult>(res =>
            {
                if (!rowByTable.TryGetValue(res.Table, out var row)) return;
                row.Cells[colCopied.Index].Value = res.Ok && res.Status == "Done" ? res.RowsCopied.ToString("N0") : "";
                row.Cells[colStatus.Index].Value = res.Status + (res.Seconds > 0 ? $" ({res.Seconds:N1}s)" : "");
                row.DefaultCellStyle.BackColor = !res.Ok ? Color.FromArgb(255, 235, 235)
                    : res.Status == "Done" ? Color.FromArgb(240, 255, 244) : Color.FromArgb(255, 245, 215);
                grdTables.FirstDisplayedScrollingRowIndex = Math.Max(0, row.Index - 5);
            });

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var ct = _cts.Token;
                var srcCs = SourceConnStr; var tgtCs = TargetConnStr;
                await Task.Run(() =>
                {
                    var migrator = new PgToPgMigrator(srcCs, tgtCs);
                    migrator.SetProgressCallback((m, p) => ((IProgress<(string, int)>)progress).Report((m, p)));
                    migrator.SetTableDoneCallback(r => ((IProgress<PgTableResult>)tableDone).Report(r));
                    migrator.Run(selected, opts, ct);
                }, ct);

                sw.Stop();
                progressBar.Value = 100;
                lblStatus.Text = $"Migration completed in {sw.Elapsed.TotalSeconds:N1}s.";
                MessageBox.Show($"Migration completed successfully!\n\n{selected.Count} tables, {srcRows:N0} rows copied in {sw.Elapsed.TotalSeconds:N1}s.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "Migration cancelled - online database rolled back, nothing was changed.";
                MessageBox.Show("Migration cancelled. All changes on the online database were rolled back.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Migration failed - online database rolled back.";
                ReportFailure(ex, "Full copy");
            }
            finally
            {
                SetBusy(false);
                btnCancel.Enabled = false;
                _cts = null;
            }
        }

        // ------------------------------------------------------------------
        //  Shared
        // ------------------------------------------------------------------

        private void ReportFailure(Exception ex, string what)
        {
            var me = MigrationException.Find(ex);
            string tableName = me?.TableName ?? "(unknown)";

            string summary =
                "Table : " + tableName + Environment.NewLine +
                (string.IsNullOrEmpty(me?.FailingQuery) ? "" : Environment.NewLine + "Query / Script:" + Environment.NewLine + me!.FailingQuery + Environment.NewLine) +
                Environment.NewLine + "Error: " + GetRootMessage(ex) + Environment.NewLine + Environment.NewLine +
                "All changes on the online database were rolled back.";
            MessageBox.Show(summary, what + " failed - Table: " + tableName, MessageBoxButtons.OK, MessageBoxIcon.Error);

            string details =
                "Offline -> Online PostgreSQL migration (" + what + ")" + Environment.NewLine +
                "Source : " + txtSrcHost.Text.Trim() + " / " + cmbSrcDatabase.Text.Trim() + Environment.NewLine +
                "Target : " + txtTgtHost.Text.Trim() + " / " + cmbTgtDatabase.Text.Trim() + Environment.NewLine +
                "Failed Table   : " + tableName + Environment.NewLine +
                (me?.FailingQuery != null ? "Failing Query  : " + me.FailingQuery + Environment.NewLine : "") +
                Environment.NewLine + ExceptionFormatter.Describe(ex);

            string logPath = "";
            try
            {
                logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"pg-migration-error-{DateTime.Now:yyyyMMdd-HHmmss}.log");
                File.WriteAllText(logPath, details);
            }
            catch { }
            if (!string.IsNullOrEmpty(logPath)) details += Environment.NewLine + "Saved to: " + logPath;

            ShowErrorDetails(what + " failed - Table: " + tableName, details);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_cts == null) return;
            if (MessageBox.Show("Cancel the migration? The online database will be rolled back to its previous state.",
                    "Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                btnCancel.Enabled = false;
                lblStatus.Text = "Cancelling after the current table...";
                _cts.Cancel();
            }
        }

        private void SetBusy(bool busy)
        {
            grpSource.Enabled = !busy;
            grpTarget.Enabled = !busy;
            pnlOptions.Enabled = !busy;
            pnlMode.Enabled = !busy;
            btnLoadTables.Enabled = !busy;
            btnLoadBranches.Enabled = !busy;
            btnSelectAll.Enabled = !busy;
            btnSelectNone.Enabled = !busy;
            cmbTgtMainBranch.Enabled = !busy;
            grdBranchMap.ReadOnly = busy;
            grdTables.ReadOnly = busy;
            btnStart.Enabled = !busy && (BranchMode ? grdBranchMap.Rows.Count > 0 : _tables.Any(t => t.ExistsInTarget));
            UseWaitCursor = busy;
        }

        private void PgToPgMigrationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cts != null)
            {
                MessageBox.Show("A migration is running. Cancel it before closing.", "Busy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }
        }

        private static string GetRootMessage(Exception ex)
        {
            while (ex.InnerException != null) ex = ex.InnerException;
            return ex.Message;
        }

        /// <summary>Resizable, copyable error dialog (same as DataMigrationConvertForm).</summary>
        private void ShowErrorDetails(string title, string details)
        {
            using var dlg = new Form
            {
                Text = title, StartPosition = FormStartPosition.CenterParent,
                Size = new Size(800, 500), MinimizeBox = false, MaximizeBox = true
            };
            var txt = new TextBox
            {
                Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false,
                Dock = DockStyle.Fill, Font = new Font("Consolas", 9F), Text = details
            };
            var pnl = new Panel { Dock = DockStyle.Bottom, Height = 44 };
            var btnCopy = new Button { Text = "Copy", Width = 90, Anchor = AnchorStyles.Right, Location = new Point(dlg.ClientSize.Width - 200, 8) };
            btnCopy.Click += (s, e) => { try { Clipboard.SetText(details); } catch { } };
            var btnClose = new Button { Text = "Close", Width = 90, Anchor = AnchorStyles.Right, DialogResult = DialogResult.OK, Location = new Point(dlg.ClientSize.Width - 100, 8) };
            pnl.Controls.Add(btnCopy);
            pnl.Controls.Add(btnClose);
            dlg.Controls.Add(txt);
            dlg.Controls.Add(pnl);
            dlg.AcceptButton = btnClose;
            dlg.ShowDialog(this);
        }
    }
}
