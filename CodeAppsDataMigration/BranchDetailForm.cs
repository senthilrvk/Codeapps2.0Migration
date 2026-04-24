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
        private Button btnCreate;
        private Label lblTitle;
        private Panel pnlBranchType;
        private RadioButton rbMainBranch;
        private RadioButton rbSubBranch;
        private Label lblParentBranch;
        private ComboBox cmbParentBranch;
        private Label lblApiUrl;
        private TextBox txtApiUrl;
        private Panel pnlContent;
        private readonly DataGridViewRow _row;
        private readonly DataTable _dataTable;
        private readonly Dictionary<string, TextBox> _textBoxes = new();
        private static readonly HttpClient _httpClient = new();
        private string _mainBranchUrl = "";
        private string _subBranchUrl = "";

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
            Size = new Size(500, 600);
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

            btnCreate = new Button
            {
                Text = "Create Main Branch",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(72, 187, 120),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(150, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(330, 12),
                Cursor = Cursors.Hand
            };
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.Click += BtnCreate_Click;

            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(btnCreate);

            // ── Branch Type panel (radio buttons + dropdown + API URL) ──
            pnlBranchType = new Panel
            {
                Dock = DockStyle.Top,
                Height = 95,
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
                Size = new Size(380, 25),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(247, 250, 252)
            };

            pnlBranchType.Controls.Add(lblBranchType);
            pnlBranchType.Controls.Add(rbMainBranch);
            pnlBranchType.Controls.Add(rbSubBranch);
            pnlBranchType.Controls.Add(lblParentBranch);
            pnlBranchType.Controls.Add(cmbParentBranch);
            pnlBranchType.Controls.Add(lblApiUrl);
            pnlBranchType.Controls.Add(txtApiUrl);

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

            // Update API URL and Create button based on branch type
            if (isSub)
            {
                txtApiUrl.Text = _subBranchUrl;
                btnCreate.Text = "Create Sub Branch";
                lblApiUrl.Location = new Point(15, 95);
                txtApiUrl.Location = new Point(80, 92);
                pnlBranchType.Height = 125;
            }
            else
            {
                txtApiUrl.Text = _mainBranchUrl;
                btnCreate.Text = "Create Main Branch";
                lblApiUrl.Location = new Point(15, 65);
                txtApiUrl.Location = new Point(80, 62);
                pnlBranchType.Height = 95;
            }

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

        private async void BtnCreate_Click(object? sender, EventArgs e)
        {
            // Validate API URL
            if (string.IsNullOrWhiteSpace(txtApiUrl.Text))
            {
                MessageBox.Show("Please enter an API URL.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate sub-branch selection
            if (rbSubBranch.Checked && cmbParentBranch.SelectedItem == null)
            {
                MessageBox.Show("Please select a parent main branch.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var branchType = rbMainBranch.Checked ? "Main Branch" : "Sub Branch";

            try
            {
                btnCreate.Enabled = false;
                btnCreate.Text = $"Creating {branchType}...";

                // Build JSON payload from all textboxes
                var jsonData = new Dictionary<string, object?>();
                foreach (var kvp in _textBoxes)
                {
                    var colName = kvp.Key.ToLower();
                    var colType = _dataTable.Columns[kvp.Key]!.DataType;
                    var text = kvp.Value.Text;

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        jsonData[colName] = null;
                    }
                    else if (colType == typeof(int))
                    {
                        jsonData[colName] = int.Parse(text);
                    }
                    else if (colType == typeof(long))
                    {
                        jsonData[colName] = long.Parse(text);
                    }
                    else if (colType == typeof(decimal))
                    {
                        jsonData[colName] = decimal.Parse(text);
                    }
                    else if (colType == typeof(double))
                    {
                        jsonData[colName] = double.Parse(text);
                    }
                    else if (colType == typeof(bool))
                    {
                        jsonData[colName] = bool.Parse(text);
                    }
                    else if (colType == typeof(DateTime))
                    {
                        jsonData[colName] = DateTime.Parse(text);
                    }
                    else
                    {
                        jsonData[colName] = text;
                    }
                }

                // Add parentbranchid for sub-branches
                if (rbSubBranch.Checked)
                {
                    var parentItem = (ParentBranchItem)cmbParentBranch.SelectedItem!;
                    jsonData["parentbranchid"] = long.Parse(parentItem.Id);
                }

                var json = JsonSerializer.Serialize(jsonData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(txtApiUrl.Text.Trim(), content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"{branchType} created successfully!\n\nResponse: {responseBody}",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show($"API returned error ({response.StatusCode}):\n{responseBody}",
                        "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to call API:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCreate.Enabled = true;
                btnCreate.Text = rbMainBranch.Checked ? "Create Main Branch" : "Create Sub Branch";
            }
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
