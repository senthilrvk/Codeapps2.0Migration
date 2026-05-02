using CodeAppsDataMigration.Data;
using Npgsql;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace CodeAppsDataMigration
{
    public class BranchDetailForm : Form
    {
        private Panel pnlTop;
        private Label lblTitle;
        private Panel pnlBranchType;
        private RadioButton rbMainBranch;
        private RadioButton rbSubBranch;
        private Label lblParentBranch;
        private ComboBox cmbParentBranch;
        private Label lblApiUrl;
        private TextBox txtApiUrl;
        private Button btnSaveApiUrl;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnSaveApiUrlTop;
        private FlowLayoutPanel pnlTopButtons;
        private Panel pnlContent;
        private readonly DataGridViewRow _row;
        private readonly DataTable _dataTable;
        private readonly Dictionary<string, TextBox> _textBoxes = new();
        private static readonly HttpClient _httpClient = new();
        private string _mainBranchUrl = "";
        private string _subBranchUrl = "";

        // MSSQL column -> (PG column name, PG type). Only mapped fields are sent.
        // BranchId is intentionally omitted so the constant tempid=0 is preserved.
        private static readonly Dictionary<string, (string PgName, string PgType)> _branchFieldMap =
          new(StringComparer.OrdinalIgnoreCase)
          {
              // Note: BranchId NOT included — PG auto-generates mainbranchid
              ["BranchCode"] = ("mainbranchcode", "text"),
              ["BranchName"] = ("mainbranchname", "text"),
              ["BranchAdr1"] = ("mainbranchadr1", "text"),
              ["BranchAdr2"] = ("mainbranchadr2", "text"),
              ["BranchAdr3"] = ("mainbranchadr3", "text"),
              ["Branch_NoField"] = ("mainbranchpincode", "text"),
              ["BranchFtr1"] = ("mainbranchftr1", "text"),
              ["BranchFtr2"] = ("mainbranchftr2", "text"),
              ["BranchFtr3"] = ("mainbranchftr3", "text"),
              ["Phone"] = ("mainphone", "text"),
              ["MobileNo"] = ("mainmobileno", "text"),
              ["Branch_WhatsAppNo"] = ("mainbranchwhatsappno", "text"),
              ["MailId"] = ("mainmailid", "text"),
              ["Active"] = ("mainactive", "boolean"),
              ["TinNo1"] = ("maingstinno", "text"),
              ["DLNo1"] = ("maindlno1", "text"),
              ["DLNo2"] = ("maindlno2", "text"),
              ["StartDate"] = ("mainstartdate", "date"),
              ["Branch_StateName"] = ("statename", "text"),
              ["Branch_StateCode"] = ("statecode", "integer"),
          };

        // Sub Branch (PG `branch` table) field map. Same shape as _branchFieldMap.
        private static readonly Dictionary<string, (string PgName, string PgType)> _subBranchFieldMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["BranchCode"]            = ("branchcode",            "text"),
                ["BranchName"]            = ("branchname",            "text"),
                ["BranchAdr1"]            = ("branchadr1",            "text"),
                ["BranchAdr2"]            = ("branchadr2",            "text"),
                ["BranchAdr3"]            = ("branchadr3",            "text"),
                ["BranchFtr1"]            = ("branchftr1",            "text"),
                ["BranchFtr2"]            = ("branchftr2",            "text"),
                ["BranchFtr3"]            = ("branchftr3",            "text"),
                ["Phone"]                 = ("branchphone",           "text"),
                ["Mail"]                  = ("branchmail",            "text"),
                ["Active"]                = ("branchactive",          "boolean"),
                ["TinNo1"]                = ("branchgstno",          "text"),
                ["TinNo2"]                = ("branchtinno2",          "text"),
                ["DLNo1"]                 = ("branchdlno",           "text"),
                ["DLNo2"]                 = ("branchdlno2",           "text"),
                ["MobileNo"]              = ("branchmobileno",        "text"),
                ["MailId"]                = ("branchmailid",          "text"),
                ["MailPwd"]               = ("branchmailpwd",         "text"),
                ["BarCodeName"]           = ("branchbarcodename",     "text"),
                ["BarCodeHeaderName"]     = ("barcodeheadername",     "text"),
                ["ComImage"]              = ("branchcomimage",        "text"),
                ["Branch_StateCode"]      = ("branchstatecode",       "integer"),
                ["Branch_StateName"]      = ("branchstatename",       "text"),
                ["Branch_BankName"]       = ("branchbankname",        "text"),
                ["Branch_BankAddr1"]      = ("branchbankaddr1",       "text"),
                ["Branch_BankAddr2"]      = ("branchbankaddr2",       "text"),
                ["Branch_BankAcNo"]       = ("branchbankacno",        "text"),
                ["Branch_IFSCCODE"]       = ("branchifsccode",        "text"),
                ["Branch_PanCardNo"]      = ("branchpancardno",       "text"),
                ["Branch_QRCode"]         = ("branchqrcode",          "text"),
                ["Branch_Declaration1"]   = ("branchdeclaration1",    "text"),
                ["Branch_Declaration2"]   = ("branchdeclaration2",    "text"),
                ["Branch_Declaration3"]   = ("branchdeclaration3",    "text"),
                ["Branch_Declaration4"]   = ("branchdeclaration4",    "text"),
                ["Branch_BankHolderName"] = ("branchbankholdername",  "text"),
                ["Branch_OrderUserName"]  = ("branchorderusername",   "text"),
                ["Branch_OrderPwd"]       = ("branchorderpwd",        "text"),
                ["Branch_WhatsAppNo"]     = ("branchwhatsappno",      "text"),
                ["Branch_WhatsAppTokenNo"]= ("branchwhatsapptokenno", "text"),
                ["Branch_WhatsAppUrl"]    = ("branchwhatsappurl",     "text"),
                ["Branch_SecurePwd"]      = ("branchsecurepwd",       "text"),
                ["Branch_BarCodeDesign"]  = ("branchbarcodedesign",   "text"),
                ["AcId"]                  = ("acid",                  "bigint"),
                ["Branch_NoField"]        = ("pincode",               "bigint"),
            };

        public BranchDetailForm(DataGridViewRow row, DataTable dataTable)
        {
            _row = row;
            _dataTable = dataTable;
            LoadApiUrlsFromXml();
            InitializeControls();
            LoadData();
        }

        private void LoadApiUrlsFromXml()
        {
            var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConnectionStrings.xml");
            if (!File.Exists(xmlPath)) return;

            var doc = XDocument.Load(xmlPath);
            var apiUrls = doc.Root?.Element("ApiUrls");
            _mainBranchUrl = apiUrls?.Element("MainBranchUrl")?.Value ?? "";
            _subBranchUrl = apiUrls?.Element("SubBranchUrl")?.Value ?? "";
        }

        private void InitializeControls()
        {
            Text = "Branch Details";
            Size = new Size(500, 660);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;

            // ── Top panel with Save button ──
            pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.FromArgb(45, 55, 72),
                Padding = new Padding(10)
            };

            lblTitle = new Label
            {
                Text = "Branch Details",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, 15)
            };

            btnSaveApiUrlTop = new Button
            {
                Text = "Save",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(49, 130, 206),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(95, 32),
                Margin = new Padding(0, 0, 8, 0),
                Padding = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter,
                UseCompatibleTextRendering = false,
                Cursor = Cursors.Hand
            };
            btnSaveApiUrlTop.FlatAppearance.BorderSize = 0;
            btnSaveApiUrlTop.Click += BtnSaveData_Click;

            pnlTopButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 11, 10, 0)
            };
            pnlTopButtons.Controls.Add(btnSaveApiUrlTop);

            pnlTop.Controls.Add(pnlTopButtons);
            pnlTop.Controls.Add(lblTitle);

            // ── Branch Type panel (radio buttons + dropdown + API URL) ──
            pnlBranchType = new Panel
            {
                Dock = DockStyle.Top,
                Height = 190,
                BackColor = Color.FromArgb(237, 242, 247),
                Padding = new Padding(15, 10, 15, 10)
            };

            var lblBranchType = new Label
            {
                Text = "Branch Type",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 55, 72),
                Location = new Point(15, 10),
                AutoSize = true
            };

            rbMainBranch = new RadioButton
            {
                Text = "Main Branch",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(45, 55, 72),
                Location = new Point(15, 35),
                AutoSize = true,
                Checked = true
            };
            rbMainBranch.CheckedChanged += RbBranchType_CheckedChanged;

            rbSubBranch = new RadioButton
            {
                Text = "Sub Branch",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(45, 55, 72),
                Location = new Point(150, 35),
                AutoSize = true
            };
            rbSubBranch.CheckedChanged += RbBranchType_CheckedChanged;

            lblParentBranch = new Label
            {
                Text = "Select Main Branch:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(74, 85, 104),
                Location = new Point(15, 65),
                AutoSize = true,
                Visible = false
            };

            cmbParentBranch = new ComboBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(160, 62),
                Size = new Size(300, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(247, 250, 252),
                Visible = false
            };

            lblApiUrl = new Label
            {
                Text = "API URL:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(74, 85, 104),
                Location = new Point(15, 95),
                AutoSize = true
            };

            txtApiUrl = new TextBox
            {
                Text = _mainBranchUrl,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(80, 62),
                Size = new Size(310, 25),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(247, 250, 252)
            };

            btnSaveApiUrl = new Button
            {
                Text = "Save",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(49, 130, 206),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(65, 27),
                Location = new Point(395, 60),
                Cursor = Cursors.Hand
            };
            btnSaveApiUrl.FlatAppearance.BorderSize = 0;
            btnSaveApiUrl.Click += BtnSaveApiUrl_Click;

            lblUsername = new Label
            {
                Text = "Username:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(74, 85, 104),
                Location = new Point(15, 95),
                AutoSize = true
            };

            txtUsername = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(80, 92),
                Size = new Size(380, 25),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(247, 250, 252)
            };

            lblPassword = new Label
            {
                Text = "Password:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(74, 85, 104),
                Location = new Point(15, 125),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(80, 122),
                Size = new Size(380, 25),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(247, 250, 252),
                UseSystemPasswordChar = true
            };

            pnlBranchType.Controls.Add(lblBranchType);
            pnlBranchType.Controls.Add(rbMainBranch);
            pnlBranchType.Controls.Add(rbSubBranch);
            pnlBranchType.Controls.Add(lblParentBranch);
            pnlBranchType.Controls.Add(cmbParentBranch);
            pnlBranchType.Controls.Add(lblApiUrl);
            pnlBranchType.Controls.Add(txtApiUrl);
            pnlBranchType.Controls.Add(btnSaveApiUrl);
            pnlBranchType.Controls.Add(lblUsername);
            pnlBranchType.Controls.Add(txtUsername);
            pnlBranchType.Controls.Add(lblPassword);
            pnlBranchType.Controls.Add(txtPassword);

            // ── Scrollable content panel for textboxes ──
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20, 15, 20, 15)
            };

            // Add order matters: Fill goes first, then Top panels in reverse
            Controls.Add(pnlContent);
            Controls.Add(pnlBranchType);
            Controls.Add(pnlTop);
        }

        private void RbBranchType_CheckedChanged(object? sender, EventArgs e)
        {
            bool isSub = rbSubBranch.Checked;
            lblParentBranch.Visible = isSub;
            cmbParentBranch.Visible = isSub;

            // Update API URL / username / password rows based on branch type
            pnlBranchType.SuspendLayout();
            // Username/password are now shown for both branch types
            lblUsername.Visible = true;
            txtUsername.Visible = true;
            lblPassword.Visible = true;
            txtPassword.Visible = true;
            if (isSub)
            {
                txtApiUrl.Text = _subBranchUrl;
                lblApiUrl.Location = new Point(15, 95);
                txtApiUrl.Location = new Point(80, 92);
                btnSaveApiUrl.Location = new Point(395, 90);
                lblUsername.Location = new Point(15, 125);
                txtUsername.Location = new Point(80, 122);
                lblPassword.Location = new Point(15, 155);
                txtPassword.Location = new Point(80, 152);
            }
            else
            {
                txtApiUrl.Text = _mainBranchUrl;
                lblApiUrl.Location = new Point(15, 65);
                txtApiUrl.Location = new Point(80, 62);
                btnSaveApiUrl.Location = new Point(395, 60);
                lblUsername.Location = new Point(15, 95);
                txtUsername.Location = new Point(80, 92);
                lblPassword.Location = new Point(15, 125);
                txtPassword.Location = new Point(80, 122);
            }
            pnlBranchType.ResumeLayout(true);
            pnlBranchType.Refresh();

            if (isSub && cmbParentBranch.Items.Count == 0)
            {
                LoadMainBranches();
            }
        }

        private void LoadMainBranches()
        {
            try
            {
                cmbParentBranch.Items.Clear();

                using var conn = PostgresConnection.Create();
                conn.Open();
                using var cmd = new NpgsqlCommand(
                    "SELECT branchid, branchname FROM branch ORDER BY branchname", conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var id = reader["branchid"]?.ToString() ?? "";
                    var name = reader["branchname"]?.ToString() ?? "";
                    cmbParentBranch.Items.Add(new ParentBranchItem(id, name));
                }

                if (cmbParentBranch.Items.Count > 0)
                    cmbParentBranch.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load main branches from PostgreSQL:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadData()
        {
            var dataRow = ((DataRowView)_row.DataBoundItem).Row;
            int yPos = 10;

            for (int i = 0; i < _dataTable.Columns.Count; i++)
            {
                var colName = _dataTable.Columns[i].ColumnName;
                var value = dataRow[i]?.ToString() ?? "";

                var lbl = new Label
                {
                    Text = colName,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(74, 85, 104),
                    Location = new Point(5, yPos),
                    AutoSize = true
                };

                yPos += 22;

                var txt = new TextBox
                {
                    Text = value,
                    Font = new Font("Segoe UI", 10F),
                    Location = new Point(5, yPos),
                    Size = new Size(430, 30),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(247, 250, 252)
                };

                _textBoxes[colName] = txt;
                pnlContent.Controls.Add(lbl);
                pnlContent.Controls.Add(txt);

                yPos += 38;

            }

        }

        private void BtnSaveApiUrl_Click(object? sender, EventArgs e)
        {
            var url = txtApiUrl.Text.Trim();
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Please enter an API URL.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var elementName = rbSubBranch.Checked ? "SubBranchUrl" : "MainBranchUrl";
            var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConnectionStrings.xml");

            try
            {
                if (!File.Exists(xmlPath))
                {
                    MessageBox.Show("ConnectionStrings.xml not found at:\n" + xmlPath,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var doc = XDocument.Load(xmlPath);
                var apiUrls = doc.Root?.Element("ApiUrls");
                if (apiUrls == null)
                {
                    apiUrls = new XElement("ApiUrls");
                    doc.Root!.Add(apiUrls);
                }

                var element = apiUrls.Element(elementName);
                if (element == null)
                {
                    apiUrls.Add(new XElement(elementName, url));
                }
                else
                {
                    element.Value = url;
                }

                doc.Save(xmlPath);

                if (rbSubBranch.Checked)
                    _subBranchUrl = url;
                else
                    _mainBranchUrl = url;

                MessageBox.Show("API URL saved successfully.",
                    "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save API URL:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSaveData_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtApiUrl.Text))
            {
                MessageBox.Show("Please enter an API URL.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (rbSubBranch.Checked && cmbParentBranch.SelectedItem == null)
            {
                MessageBox.Show("Please select a parent main branch.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Username is required.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Password is required.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            if (rbMainBranch.Checked && string.IsNullOrWhiteSpace(_subBranchUrl))
            {
                MessageBox.Show(
                    "Sub Branch URL is not configured in ConnectionStrings.xml.\n" +
                    "Main mode now creates both branches in one save and needs both URLs.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var originalText = btnSaveApiUrlTop.Text;
            btnSaveApiUrlTop.Enabled = false;
            btnSaveApiUrlTop.Text = "Saving...";

            var combinedLog = new StringBuilder();
            combinedLog.AppendLine($"Timestamp: {DateTime.Now:O}");

            try
            {
                if (rbMainBranch.Checked)
                {
                    // 1) POST main branch
                    var (mainJson, mainDebug) = BuildMainPayload();
                    var mainUrl = txtApiUrl.Text.Trim();
                    var (mainResp, mainBody) = await PostJsonAsync(mainUrl, mainJson);
                    AppendCallToLog(combinedLog, "Main Branch", mainUrl, mainResp, mainJson, mainBody, mainDebug);

                    if (!mainResp.IsSuccessStatusCode)
                    {
                        WriteLogFile(combinedLog);
                        ShowApiError("Main Branch", mainResp, mainBody, mainJson);
                        return;
                    }

                    // 2) Extract new mainbranchid from main response
                    long newMainBranchId = ExtractMainBranchId(mainBody);
                    combinedLog.AppendLine($"[INFO] Extracted mainbranchid from main response: {newMainBranchId}");

                    if (newMainBranchId <= 0)
                    {
                        WriteLogFile(combinedLog);
                        MessageBox.Show(
                            "Main Branch was created, but no mainbranchid could be parsed from the response.\n" +
                            "Sub Branch was NOT created. Check last_api_call.txt for the response shape.\n\n" +
                            "Response body:\n" + mainBody,
                            "Partial Success", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 3) POST sub branch with mainbranchid = newMainBranchId
                    var (subJson, subDebug) = BuildSubPayload(newMainBranchId);
                    var (subResp, subBody) = await PostJsonAsync(_subBranchUrl, subJson);
                    AppendCallToLog(combinedLog, "Sub Branch (auto)", _subBranchUrl, subResp, subJson, subBody, subDebug);
                    WriteLogFile(combinedLog);

                    if (!subResp.IsSuccessStatusCode)
                    {
                        ShowApiError($"Sub Branch (Main {newMainBranchId} already created)", subResp, subBody, subJson);
                        return;
                    }

                    MessageBox.Show(
                        $"Both branches created successfully.\n\n" +
                        $"Main Branch ID: {newMainBranchId}\n\n" +
                        $"Main response: {mainBody}\n\n" +
                        $"Sub response: {subBody}",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    // Sub-only flow (unchanged)
                    var parentItem = (ParentBranchItem)cmbParentBranch.SelectedItem!;
                    var parentId = long.Parse(parentItem.Id);
                    var (subJson, subDebug) = BuildSubPayload(parentId);
                    var subUrl = txtApiUrl.Text.Trim();
                    var (subResp, subBody) = await PostJsonAsync(subUrl, subJson);
                    AppendCallToLog(combinedLog, "Sub Branch", subUrl, subResp, subJson, subBody, subDebug);
                    WriteLogFile(combinedLog);

                    if (subResp.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Sub Branch saved successfully!\n\nResponse: {subBody}",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        ShowApiError("Sub Branch", subResp, subBody, subJson);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save data:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSaveApiUrlTop.Enabled = true;
                btnSaveApiUrlTop.Text = originalText;
            }
        }

        private (string json, StringBuilder debug) BuildMainPayload()
        {
            var jsonData = new Dictionary<string, object?>
            {
                ["tempid"] = 0,
                ["businesstype"] = ""
            };
            var debug = BuildPayloadFromMap(_branchFieldMap, jsonData, "Main");
            jsonData["mainusername"] = txtUsername.Text.Trim();
            jsonData["mainpwd"] = txtPassword.Text;   // do NOT trim password
            var json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
            return (json, debug);
        }

        private (string json, StringBuilder debug) BuildSubPayload(long mainBranchId)
        {
            var jsonData = new Dictionary<string, object?>();
            var debug = new StringBuilder();
            debug.AppendLine($"[INFO] mode=Sub _textBoxes count={_textBoxes.Count}, map count={_subBranchFieldMap.Count}");
            int mappedCount = 0, defaultedCount = 0, skippedCount = 0;

            // 1) Iterate textboxes, look up in map, parse with try/catch + default fallback
            foreach (var kvp in _textBoxes)
            {
                var mssqlCol = kvp.Key;
                if (!_subBranchFieldMap.TryGetValue(mssqlCol, out var mapping))
                {
                    skippedCount++;
                    debug.AppendLine($"[SKIP] '{mssqlCol}' not in _subBranchFieldMap");
                    continue;
                }

                var pgName  = mapping.PgName;
                var pgType  = mapping.PgType;
                var rawText = kvp.Value.Text;

                if (string.IsNullOrWhiteSpace(rawText))
                {
                    jsonData[pgName] = GetDefaultValue(pgType);
                    debug.AppendLine($"[DEF ] '{mssqlCol}' -> '{pgName}' = default({pgType})");
                    defaultedCount++;
                    continue;
                }

                try
                {
                    object value;
                    // Special-case: digit-strip for statecode (int) and pincode (bigint)
                    if (pgName == "branchstatecode" || pgName == "pincode")
                    {
                        var sb = new StringBuilder();
                        foreach (var ch in rawText)
                            if (char.IsDigit(ch)) sb.Append(ch);
                        if (sb.Length == 0)
                            value = GetDefaultValue(pgType);
                        else if (pgType == "bigint")
                            value = long.Parse(sb.ToString());
                        else
                            value = int.Parse(sb.ToString());
                    }
                    else
                    {
                        value = pgType switch
                        {
                            "text"    => rawText.Trim(),
                            "boolean" => (object)ParseBoolOrDefault(rawText),
                            "integer" => (object)ParseIntOrDefault(rawText),
                            "bigint"  => (object)ParseLongOrDefault(rawText),
                            "date"    => (object)ParseDateOrDefault(rawText),
                            _         => GetDefaultValue(pgType)
                        };
                    }
                    jsonData[pgName] = value;
                    debug.AppendLine($"[MAP ] '{mssqlCol}' -> '{pgName}' = {value}");
                    mappedCount++;
                }
                catch (Exception ex)
                {
                    jsonData[pgName] = GetDefaultValue(pgType);
                    debug.AppendLine($"[DEF ] '{mssqlCol}' -> '{pgName}' = default({pgType}) (parse failed: {ex.Message})");
                    defaultedCount++;
                }
            }

            // 2) Inject PG-only fields (no MSSQL source) with defaults if not already present
            void AddIfMissing(string key, object value)
            {
                if (!jsonData.ContainsKey(key))
                {
                    jsonData[key] = value;
                    debug.AppendLine($"[ADD ] PG-only '{key}' = {value}");
                }
            }
            AddIfMissing("branchgstno",         "");
            AddIfMissing("branchtinno1",        "");
            AddIfMissing("branchdlno1",         "");
            AddIfMissing("accountmail",         "");
            AddIfMissing("branchlocation",      "");
            AddIfMissing("billpassword",        "");
            AddIfMissing("taxtype",             "");
            AddIfMissing("fssai",               "");
            AddIfMissing("lutno",               "");
            AddIfMissing("expectregno",         "");
            AddIfMissing("branchtokenpassword", "");
            AddIfMissing("branchappcode",       0);
            AddIfMissing("tempid",              0);

            // 3) Runtime fields
            jsonData["mainbranchid"] = (int)mainBranchId;
            jsonData["username"]     = string.IsNullOrEmpty(txtUsername.Text) ? "" : txtUsername.Text.Trim();
            jsonData["pwd"]          = txtPassword.Text ?? "";   // do NOT trim password

            debug.AppendLine($"[INFO] mapped={mappedCount} defaulted={defaultedCount} skipped={skippedCount}");

            var json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
            return (json, debug);
        }

        private StringBuilder BuildPayloadFromMap(
            Dictionary<string, (string PgName, string PgType)> activeMap,
            Dictionary<string, object?> jsonData,
            string mode)
        {
            var debug = new StringBuilder();
            debug.AppendLine($"[INFO] mode={mode} _textBoxes count = {_textBoxes.Count}, activeMap count = {activeMap.Count}");
            int mappedCount = 0, defaultedCount = 0, missingCount = 0;

            foreach (var kvp in activeMap)
            {
                var mssqlCol = kvp.Key;
                var pgName   = kvp.Value.PgName;
                var pgType   = kvp.Value.PgType;

                string? rawText = null;
                if (_textBoxes.TryGetValue(mssqlCol, out var tb))
                    rawText = tb.Text;
                else
                    missingCount++;

                if (string.IsNullOrWhiteSpace(rawText))
                {
                    var def = GetDefaultValue(pgType);
                    jsonData[pgName] = def;
                    debug.AppendLine($"[DEF ] '{mssqlCol}' -> '{pgName}' = {def} (default for {pgType}; raw='{rawText}')");
                    defaultedCount++;
                    continue;
                }

                var converted = ConvertToPgValue(pgName, pgType, rawText);
                if (converted == null)
                {
                    var def = GetDefaultValue(pgType);
                    jsonData[pgName] = def;
                    debug.AppendLine($"[DEF ] '{mssqlCol}' -> '{pgName}' = {def} (cast yielded null from '{rawText}')");
                    defaultedCount++;
                    continue;
                }

                jsonData[pgName] = converted;
                debug.AppendLine($"[MAP ] '{mssqlCol}' -> '{pgName}' = {converted}");
                mappedCount++;
            }

            debug.AppendLine($"[INFO] mapped={mappedCount} defaulted={defaultedCount} missingTextbox={missingCount}");
            return debug;
        }

        private static async Task<(HttpResponseMessage resp, string body)> PostJsonAsync(string url, string json)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _httpClient.PostAsync(url, content);
            var body = await resp.Content.ReadAsStringAsync();
            return (resp, body);
        }

        private static void AppendCallToLog(StringBuilder log, string label, string url,
            HttpResponseMessage resp, string requestJson, string responseBody, StringBuilder fieldTrace)
        {
            log.AppendLine();
            log.AppendLine($"==== {label} ====");
            log.AppendLine($"POST {url}");
            log.AppendLine($"Status: {(int)resp.StatusCode} {resp.StatusCode}");
            log.AppendLine("---- Field Trace ----");
            log.Append(fieldTrace);
            log.AppendLine("---- Request ----");
            log.AppendLine(requestJson);
            log.AppendLine("---- Response ----");
            log.AppendLine(responseBody);
        }

        private static void WriteLogFile(StringBuilder log)
        {
            try
            {
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "last_api_call.txt");
                File.WriteAllText(logPath, log.ToString());
            }
            catch { /* logging is best-effort */ }
        }

        private static void ShowApiError(string label, HttpResponseMessage resp, string body, string requestJson)
        {
            var preview = requestJson.Length > 1500 ? requestJson.Substring(0, 1500) + "...(truncated)" : requestJson;
            MessageBox.Show(
                $"{label} API returned error ({(int)resp.StatusCode} {resp.StatusCode}):\n{body}\n\n" +
                $"---- Request sent ----\n{preview}\n\n" +
                "Full request + response written to last_api_call.txt next to the exe.",
                "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static long ExtractMainBranchId(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody)) return 0;

            var trimmed = responseBody.Trim();
            if (long.TryParse(trimmed, out var direct))
                return direct;

            try
            {
                using var doc = JsonDocument.Parse(trimmed);
                return TryFindLongInJson(doc.RootElement,
                    "mainbranchid", "MainBranchId", "mainBranchId",
                    "id", "Id", "branchid", "BranchId");
            }
            catch
            {
                return 0;
            }
        }

        private static long TryFindLongInJson(JsonElement element, params string[] names)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var name in names)
                {
                    if (element.TryGetProperty(name, out var prop))
                    {
                        if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt64(out var val))
                            return val;
                        if (prop.ValueKind == JsonValueKind.String &&
                            long.TryParse(prop.GetString(), out var sval))
                            return sval;
                    }
                }
                foreach (var wrapper in new[] { "data", "result", "Data", "Result", "Content", "content" })
                {
                    if (element.TryGetProperty(wrapper, out var w))
                    {
                        var v = TryFindLongInJson(w, names);
                        if (v > 0) return v;
                    }
                }
            }
            return 0;
        }

        private static object? ConvertToPgValue(string pgName, string pgType, string text)
        {
            var trimmed = text.Trim();

            if (pgName == "mainbranchpincode")
                return trimmed;

            if (pgName == "statecode" || pgName == "mainbranchstatecode")
            {
                var sb = new StringBuilder();
                foreach (var ch in trimmed)
                    if (char.IsDigit(ch)) sb.Append(ch);
                return sb.Length == 0 ? null : (object)int.Parse(sb.ToString());
            }

            if (pgName == "pincode")
            {
                var sb = new StringBuilder();
                foreach (var ch in trimmed)
                    if (char.IsDigit(ch)) sb.Append(ch);
                return sb.Length == 0 ? null : (object)long.Parse(sb.ToString());
            }

            return pgType switch
            {
                "bigint"  => long.Parse(trimmed),
                "integer" => int.Parse(trimmed),
                "numeric" => decimal.Parse(trimmed),
                "boolean" => ParseFlexibleBool(trimmed),
                "date" => DateTime.Parse(text).ToString("yyyy-MM-dd"),
                _         => trimmed
            };
        }

        private static object GetDefaultValue(string pgType) => pgType switch
        {
            "text"    => "",
            "boolean" => false,
            "integer" => 0,
            "bigint"  => 0L,
            "numeric" => 0m,
            "date"    => "1900-01-01",
            _         => ""
        };

        private static bool ParseBoolOrDefault(string text)
        {
            try { return ParseFlexibleBool((text ?? "").Trim()); }
            catch { return false; }
        }

        private static int ParseIntOrDefault(string text)
        {
            return int.TryParse((text ?? "").Trim(), out var v) ? v : 0;
        }

        private static long ParseLongOrDefault(string text)
        {
            return long.TryParse((text ?? "").Trim(), out var v) ? v : 0L;
        }

        private static string ParseDateOrDefault(string text)
        {
            return DateTime.TryParse((text ?? "").Trim(), out var dt)
                ? dt.ToString("yyyy-MM-dd")
                : "1900-01-01";
        }

        private static bool ParseFlexibleBool(string t)
        {
            if (t.Equals("true",   StringComparison.OrdinalIgnoreCase)) return true;
            if (t.Equals("false",  StringComparison.OrdinalIgnoreCase)) return false;
            if (t == "1" || t.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("active", StringComparison.OrdinalIgnoreCase)) return true;
            if (t == "0" || t.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("n", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("inactive", StringComparison.OrdinalIgnoreCase)) return false;
            return bool.Parse(t);
        }

        // Helper class for ComboBox items
        private class ParentBranchItem
        {
            public string Id { get; }
            public string Name { get; }

            public ParentBranchItem(string id, string name)
            {
                Id = id;
                Name = name;
            }

            public override string ToString() => $"{Name} (ID: {Id})";
        }
    }
}
