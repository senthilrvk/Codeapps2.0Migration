using CodeAppsDataMigration.Migration;
using System.Text;
using System.Xml.Linq;

namespace CodeAppsDataMigration
{
    /// <summary>
    /// "Create Main Branch" step of the offline -> online migration.
    /// Picks an offline main branch, then calls the online application's API to create the
    /// main branch and its branches online, so the per-main tables and seed rows exist before
    /// the data is migrated. Same API URLs as the Branch List page (ConnectionStrings.xml / ApiUrls).
    ///
    /// Uses the SAME methods as the SQL Server -> PostgreSQL Branch Details page (BranchDetailForm):
    /// same field maps and defaults for the request JSON (see PgBranchApiClient), same PostJsonAsync,
    /// HTTP / API-Flag checks, mainbranchid extraction and last_api_call.txt log format.
    /// </summary>
    public class PgCreateBranchForm : Form
    {
        private readonly string _srcConnStr;
        private readonly string _xmlPath;

        private ComboBox cmbMain = null!;
        private CheckedListBox lstBranches = null!;
        private TextBox txtMainUrl = null!, txtSubUrl = null!, txtUser = null!, txtPwd = null!, txtBusinessType = null!, txtLog = null!;
        private Button btnCreate = null!, btnClose = null!, btnSaveUrls = null!, btnShowPwd = null!;
        private List<PgBranchApiClient.OfflineMain> _mains = new();

        /// <summary>Set when the main branch was created online.</summary>
        public long CreatedMainBranchId { get; private set; }
        public long OfflineMainBranchId { get; private set; }

        public PgCreateBranchForm(string sourceConnStr)
        {
            _srcConnStr = sourceConnStr;
            _xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConnectionStrings.xml");
            Build();
            LoadUrls();
            Shown += (s, e) => LoadMains();
        }

        private void Build()
        {
            Text = "Create Main Branch on the Online Server";
            ClientSize = new Size(760, 640);
            MinimumSize = new Size(700, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            Font = new Font("Segoe UI", 9F);

            var header = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.FromArgb(45, 55, 72), Padding = new Padding(20, 8, 20, 6) };
            header.Controls.Add(new Label
            {
                Dock = DockStyle.Fill, ForeColor = Color.FromArgb(203, 213, 224), Font = new Font("Segoe UI", 9F), TextAlign = ContentAlignment.MiddleLeft,
                Text = "Creates the main branch and its branches on the ONLINE server through the online application's API. Do this before migrating data."
            });
            header.Controls.Add(new Label { Dock = DockStyle.Top, Height = 24, ForeColor = Color.White, Font = new Font("Segoe UI", 12F, FontStyle.Bold), Text = "Create Main Branch Online" });

            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(15, 10, 15, 6), ColumnCount = 3 };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            var rows = new[] { 30F, 110F, 30F, 30F, 30F, 30F, 30F, 0F };
            foreach (var h in rows) tlp.RowStyles.Add(h == 0 ? new RowStyle(SizeType.Percent, 100F) : new RowStyle(SizeType.Absolute, h));

            int r = 0;
            tlp.Controls.Add(Lbl("Offline main branch"), 0, r);
            cmbMain = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right, Font = new Font("Segoe UI", 10F) };
            cmbMain.SelectedIndexChanged += (s, e) => MainChanged();
            tlp.Controls.Add(cmbMain, 1, r); tlp.SetColumnSpan(cmbMain, 2);

            r++;
            tlp.Controls.Add(Lbl("Branches to create"), 0, r);
            lstBranches = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, BorderStyle = BorderStyle.FixedSingle };
            tlp.Controls.Add(lstBranches, 1, r); tlp.SetColumnSpan(lstBranches, 2);

            r++;
            tlp.Controls.Add(Lbl("Business type"), 0, r);
            txtBusinessType = Txt(); txtBusinessType.PlaceholderText = "PHARMACY / PHARMACEUTICALS / DISTRIBUTION (blank = offline value)";
            tlp.Controls.Add(txtBusinessType, 1, r); tlp.SetColumnSpan(txtBusinessType, 2);

            r++;
            tlp.Controls.Add(Lbl("Online login username"), 0, r);
            txtUser = Txt();
            tlp.Controls.Add(txtUser, 1, r); tlp.SetColumnSpan(txtUser, 2);

            r++;
            tlp.Controls.Add(Lbl("Online login password"), 0, r);
            // (shown decrypted for editing; encrypted again before it is sent)
            txtPwd = Txt(); txtPwd.UseSystemPasswordChar = true;
            txtPwd.PlaceholderText = "plain text - the online server encrypts it when saving";
            tlp.Controls.Add(txtPwd, 1, r);
            btnShowPwd = Btn("Show", Color.FromArgb(113, 128, 150), 100, 24);
            btnShowPwd.Click += (s, e) =>
            {
                txtPwd.UseSystemPasswordChar = !txtPwd.UseSystemPasswordChar;
                btnShowPwd.Text = txtPwd.UseSystemPasswordChar ? "Show" : "Hide";
            };
            tlp.Controls.Add(btnShowPwd, 2, r);

            r++;
            tlp.Controls.Add(Lbl("Main Branch API URL"), 0, r);
            txtMainUrl = Txt();
            tlp.Controls.Add(txtMainUrl, 1, r);
            btnSaveUrls = Btn("Save URLs", Color.FromArgb(113, 128, 150), 100, 24);
            btnSaveUrls.Click += (s, e) => SaveUrls();
            tlp.Controls.Add(btnSaveUrls, 2, r);

            r++;
            tlp.Controls.Add(Lbl("Sub Branch API URL"), 0, r);
            txtSubUrl = Txt();
            tlp.Controls.Add(txtSubUrl, 1, r); tlp.SetColumnSpan(txtSubUrl, 2);

            r++;
            txtLog = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Font = new Font("Consolas", 9F), BackColor = Color.FromArgb(250, 250, 250), Margin = new Padding(0, 6, 0, 0) };
            tlp.Controls.Add(txtLog, 0, r); tlp.SetColumnSpan(txtLog, 3);

            var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 50, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10, 8, 10, 8), BackColor = Color.FromArgb(247, 250, 252) };
            btnClose = Btn("Close", Color.FromArgb(113, 128, 150));
            btnClose.Click += (s, e) => Close();
            btnCreate = Btn("▶  Create Online", Color.FromArgb(72, 187, 120), 150);
            btnCreate.Click += async (s, e) => await CreateAsync();
            footer.Controls.Add(btnClose);
            footer.Controls.Add(btnCreate);

            Controls.Add(tlp);
            Controls.Add(footer);
            Controls.Add(header);
        }

        private static Label Lbl(string t) => new() { Text = t, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(74, 85, 104), Margin = new Padding(0) };
        private static TextBox Txt() => new() { Anchor = AnchorStyles.Left | AnchorStyles.Right, Margin = new Padding(0, 3, 0, 3) };
        private static Button Btn(string t, Color back, int w = 100, int h = 30) => new()
        {
            Text = t, Size = new Size(w, h), MinimumSize = new Size(w, h), FlatStyle = FlatStyle.Flat, BackColor = back, ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(5, 2, 0, 2), UseVisualStyleBackColor = false,
            FlatAppearance = { BorderSize = 0 }
        };

        private void LoadUrls()
        {
            try
            {
                if (!File.Exists(_xmlPath)) return;
                var api = XDocument.Load(_xmlPath).Root?.Element("ApiUrls");
                txtMainUrl.Text = api?.Element("MainBranchUrl")?.Value ?? "";
                txtSubUrl.Text = api?.Element("SubBranchUrl")?.Value ?? "";
            }
            catch { }
        }

        private void SaveUrls()
        {
            try
            {
                var doc = File.Exists(_xmlPath) ? XDocument.Load(_xmlPath) : new XDocument(new XElement("ConnectionStrings"));
                var api = doc.Root!.Element("ApiUrls");
                if (api == null) { api = new XElement("ApiUrls"); doc.Root.Add(api); }
                api.SetElementValue("MainBranchUrl", txtMainUrl.Text.Trim());
                api.SetElementValue("SubBranchUrl", txtSubUrl.Text.Trim());
                doc.Save(_xmlPath);
                Log("API URLs saved to ConnectionStrings.xml.");
            }
            catch (Exception ex) { MessageBox.Show("Failed to save URLs: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void LoadMains()
        {
            try
            {
                _mains = PgBranchApiClient.LoadOfflineMains(_srcConnStr);
                cmbMain.Items.Clear();
                foreach (var m in _mains) cmbMain.Items.Add(m);
                if (cmbMain.Items.Count > 0) cmbMain.SelectedIndex = 0;
                Log($"{_mains.Count} offline main branch(es) found.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read offline main branches:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainChanged()
        {
            lstBranches.Items.Clear();
            if (cmbMain.SelectedItem is not PgBranchApiClient.OfflineMain m) return;
            foreach (var b in m.Branches) lstBranches.Items.Add($"{b.Name} [{b.BranchId}]", true);
            txtUser.Text = m.Username;
            txtPwd.Text = TryDecrypt(m.Password);   // stored AES-encrypted; shown as plain text, re-encrypted when sent
            txtBusinessType.Text = m.BusinessType;
        }

        private void Log(string s) => txtLog.AppendText(s + Environment.NewLine);

        /// <summary>Passwords are stored AES-encrypted (see Helpers.AesEncryption). Returns the text unchanged if it is not encrypted.</summary>
        private static string TryDecrypt(string stored) => PgBranchApiClient.TryDecrypt(stored);

        private async Task CreateAsync()
        {
            if (cmbMain.SelectedItem is not PgBranchApiClient.OfflineMain m) { MessageBox.Show("Select the offline main branch.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(txtMainUrl.Text) || string.IsNullOrWhiteSpace(txtSubUrl.Text)) { MessageBox.Show("Enter both API URLs of the online application.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrEmpty(txtPwd.Text)) { MessageBox.Show("Enter the online login username and password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            var branches = m.Branches.Where((b, i) => lstBranches.GetItemChecked(i)).ToList();
            if (branches.Count == 0) { MessageBox.Show("Tick at least one branch to create.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (MessageBox.Show($"Create main branch '{m.Name}' and {branches.Count} branch(es) on the online server?\n\nMain URL: {txtMainUrl.Text.Trim()}\nSub URL : {txtSubUrl.Text.Trim()}",
                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            btnCreate.Enabled = false; btnClose.Enabled = false; UseWaitCursor = true;

            // Same sequence as BranchDetailForm.BtnSaveData_Click (Main mode):
            //   POST main -> HTTP check -> API Flag check -> extract mainbranchid -> POST sub branch(es).
            var combinedLog = new StringBuilder();
            combinedLog.AppendLine($"Timestamp: {DateTime.Now:O}");
            try
            {
                // The online API encrypts the password itself (MainBranchHandlers / BranchHandler call
                // AesEncryption.Encrypt on the request), so it must be sent as PLAIN text.
                string user = txtUser.Text, pwd = txtPwd.Text;
                string mainUrl = txtMainUrl.Text.Trim(), subUrl = txtSubUrl.Text.Trim();

                // 1) POST main branch
                Log($"Creating main branch '{m.Name}' ...");
                var (mainJson, mainDebug) = PgBranchApiClient.BuildMainPayload(_srcConnStr, m.MainBranchId, user, pwd, txtBusinessType.Text);
                var (mainResp, mainBody) = await BranchDetailForm.PostJsonAsync(mainUrl, mainJson);
                BranchDetailForm.AppendCallToLog(combinedLog, "Main Branch", mainUrl, mainResp, mainJson, mainBody, mainDebug);

                if (!mainResp.IsSuccessStatusCode)
                {
                    BranchDetailForm.WriteLogFile(combinedLog);
                    Log($"FAILED: HTTP {(int)mainResp.StatusCode} {mainResp.StatusCode}\n{mainBody}");
                    BranchDetailForm.ShowApiError("Main Branch", mainResp, mainBody, mainJson);
                    return;
                }

                // 1a) Check the API's own success flag (HTTP 200 can still mean business failure).
                if (!BranchDetailForm.IsApiResponseSuccess(mainBody, out var mainApiMsg))
                {
                    BranchDetailForm.WriteLogFile(combinedLog);
                    Log($"FAILED: {(string.IsNullOrWhiteSpace(mainApiMsg) ? "(no message)" : mainApiMsg)}\n{mainBody}");
                    MessageBox.Show(
                        "Main Branch was NOT created.\n\n" +
                        "API message: " + (string.IsNullOrWhiteSpace(mainApiMsg) ? "(no message)" : mainApiMsg) + "\n\n" +
                        "Branches were NOT created. See last_api_call.txt for full details.",
                        "Not Created", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2) Extract new mainbranchid from main response
                long newMain = BranchDetailForm.ExtractMainBranchId(mainBody);
                combinedLog.AppendLine($"[INFO] Extracted mainbranchid from main response: {newMain}");

                if (newMain <= 0)
                {
                    BranchDetailForm.WriteLogFile(combinedLog);
                    Log("Main branch created but no mainbranchid in the response:\n" + mainBody);
                    MessageBox.Show(
                        "Main Branch was created, but no mainbranchid could be parsed from the response.\n" +
                        "Branches were NOT created. Check last_api_call.txt for the response shape.\n\n" +
                        "Response body:\n" + mainBody,
                        "Partial Success", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                Log($"OK  main branch created online with mainbranchid = {newMain}");

                // 3) POST each ticked branch with mainbranchid = newMain
                int ok = 0;
                foreach (var b in branches)
                {
                    Log($"Creating branch '{b.Name}' ...");
                    var (subJson, subDebug) = PgBranchApiClient.BuildSubPayload(_srcConnStr, b.BranchId, newMain, user, pwd);
                    var (subResp, subBody) = await BranchDetailForm.PostJsonAsync(subUrl, subJson);
                    BranchDetailForm.AppendCallToLog(combinedLog, $"Sub Branch {b.BranchId} ({b.Name})", subUrl, subResp, subJson, subBody, subDebug);
                    BranchDetailForm.WriteLogFile(combinedLog);

                    if (!subResp.IsSuccessStatusCode)
                    {
                        Log($"FAILED: HTTP {(int)subResp.StatusCode} {subResp.StatusCode}\n{subBody}");
                        BranchDetailForm.ShowApiError($"Sub Branch '{b.Name}' (Main {newMain} already created)", subResp, subBody, subJson);
                        continue;
                    }

                    if (!BranchDetailForm.IsApiResponseSuccess(subBody, out var subApiMsg))
                    {
                        Log($"FAILED: {(string.IsNullOrWhiteSpace(subApiMsg) ? "(no message)" : subApiMsg)}\n{subBody}");
                        MessageBox.Show(
                            $"Sub Branch '{b.Name}' was NOT created (Main {newMain} already created).\n\n" +
                            "API message: " + (string.IsNullOrWhiteSpace(subApiMsg) ? "(no message)" : subApiMsg) + "\n\n" +
                            "See last_api_call.txt for full details.",
                            "Not Created", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    ok++;
                    long newBranch = BranchDetailForm.ExtractMainBranchId(subBody);
                    Log($"OK  branch created online{(newBranch > 0 ? " with branchid = " + newBranch : "")}");
                }

                CreatedMainBranchId = newMain;
                OfflineMainBranchId = m.MainBranchId;
                Log($"\nDone: main branch {newMain}, {ok}/{branches.Count} branch(es) created. Close this window and click Load Branches.");
                MessageBox.Show(
                    $"Main Branch created online (mainbranchid {newMain}), {ok} of {branches.Count} branch(es) created.\n\n" +
                    $"Main response: {mainBody}\n\n" +
                    "Back on the migration page click Load Branches; the new branches are mapped automatically.",
                    ok == branches.Count ? "Success" : "Partial", MessageBoxButtons.OK, ok == branches.Count ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                Log("ERROR: " + ex.Message);
                MessageBox.Show("Failed to save data:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                BranchDetailForm.WriteLogFile(combinedLog);
                btnCreate.Enabled = true; btnClose.Enabled = true; UseWaitCursor = false;
            }
        }
    }
}
