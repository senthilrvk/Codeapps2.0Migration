using CodeAppsDataMigration.Data;
using Npgsql;
using System.Data;

namespace CodeAppsDataMigration
{
    public class BranchDetailForm : Form
    {
        private Panel pnlTop;
        private Button btnSave;
        private Label lblTitle;
        private Panel pnlBranchType;
        private RadioButton rbMainBranch;
        private RadioButton rbSubBranch;
        private Label lblParentBranch;
        private ComboBox cmbParentBranch;
        private Panel pnlContent;
        private readonly DataGridViewRow _row;
        private readonly DataTable _dataTable;
        private readonly Dictionary<string, TextBox> _textBoxes = new();

        public BranchDetailForm(DataGridViewRow row, DataTable dataTable)
        {
            _row = row;
            _dataTable = dataTable;
            InitializeControls();
            LoadData();
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

            btnSave = new Button
            {
                Text = "Save",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(72, 187, 120),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(380, 12),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(btnSave);

            // ── Branch Type panel (radio buttons + dropdown) ──
            pnlBranchType = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
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

            pnlBranchType.Controls.Add(lblBranchType);
            pnlBranchType.Controls.Add(rbMainBranch);
            pnlBranchType.Controls.Add(rbSubBranch);
            pnlBranchType.Controls.Add(lblParentBranch);
            pnlBranchType.Controls.Add(cmbParentBranch);

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

            // Adjust panel height
            pnlBranchType.Height = isSub ? 100 : 65;

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

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // Validate sub-branch selection
            if (rbSubBranch.Checked && cmbParentBranch.SelectedItem == null)
            {
                MessageBox.Show("Please select a parent main branch.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var columns = new List<string>();
                var paramNames = new List<string>();
                var parameters = new List<NpgsqlParameter>();

                int idx = 0;
                foreach (var kvp in _textBoxes)
                {
                    var colName = kvp.Key.ToLower();
                    var colType = _dataTable.Columns[kvp.Key]!.DataType;
                    columns.Add(colName);
                    paramNames.Add($"@p{idx}");

                    object val = kvp.Value.Text;
                    if (string.IsNullOrWhiteSpace(kvp.Value.Text))
                    {
                        val = DBNull.Value;
                    }
                    else if (colType == typeof(int))
                    {
                        val = int.Parse(kvp.Value.Text);
                    }
                    else if (colType == typeof(long))
                    {
                        val = long.Parse(kvp.Value.Text);
                    }
                    else if (colType == typeof(decimal))
                    {
                        val = decimal.Parse(kvp.Value.Text);
                    }
                    else if (colType == typeof(double))
                    {
                        val = double.Parse(kvp.Value.Text);
                    }
                    else if (colType == typeof(bool))
                    {
                        val = bool.Parse(kvp.Value.Text);
                    }
                    else if (colType == typeof(DateTime))
                    {
                        val = DateTime.Parse(kvp.Value.Text);
                    }

                    parameters.Add(new NpgsqlParameter($"@p{idx}", val));
                    idx++;
                }

                // Add parentbranchid column for sub-branches
                if (rbSubBranch.Checked)
                {
                    var parentItem = (ParentBranchItem)cmbParentBranch.SelectedItem!;
                    if (!columns.Contains("parentbranchid"))
                    {
                        columns.Add("parentbranchid");
                        paramNames.Add($"@p{idx}");
                        parameters.Add(new NpgsqlParameter($"@p{idx}", long.Parse(parentItem.Id)));
                    }
                }

                var sql = $"INSERT INTO branch ({string.Join(", ", columns)}) " +
                          $"VALUES ({string.Join(", ", paramNames)})";

                using var conn = PostgresConnection.Create();
                conn.Open();
                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddRange(parameters.ToArray());
                cmd.ExecuteNonQuery();

                var branchType = rbMainBranch.Checked ? "Main Branch" : "Sub Branch";
                MessageBox.Show($"{branchType} created successfully in PostgreSQL!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create branch in PostgreSQL:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
