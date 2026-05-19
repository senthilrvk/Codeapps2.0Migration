using System.Data;
using System.Text;
using System.Xml.Linq;
using ClosedXML.Excel;
using Npgsql;

namespace CodeAppsDataMigration
{
    public partial class ExcelImportForm : Form
    {
        private IXLWorkbook? _workbook;

        public ExcelImportForm()
        {
            InitializeComponent();
        }

        // ───────── Browse Excel File ─────────
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select Excel File",
                Filter = "Excel Files|*.xlsx;*.xls",
                FilterIndex = 1
            };

            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                txtFilePath.Text = ofd.FileName;
                _workbook = new XLWorkbook(ofd.FileName);

                cmbSheet.Items.Clear();
                foreach (var ws in _workbook.Worksheets)
                    cmbSheet.Items.Add(ws.Name);

                if (cmbSheet.Items.Count > 0)
                    cmbSheet.SelectedIndex = 0;

                // Auto-suggest table name from file name
                if (string.IsNullOrWhiteSpace(txtTableName.Text))
                {
                    var name = Path.GetFileNameWithoutExtension(ofd.FileName)
                        .ToLower().Replace(" ", "_").Replace("-", "_");
                    txtTableName.Text = name;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open Excel file:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ───────── Sheet selection changed ─────────
        private void cmbSheet_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_workbook == null || cmbSheet.SelectedIndex < 0) return;
            LoadSheetToGrid(_workbook.Worksheet(cmbSheet.SelectedItem!.ToString()!));
        }

        private void LoadSheetToGrid(IXLWorksheet ws)
        {
            var dt = new DataTable();
            var rangeUsed = ws.RangeUsed();
            if (rangeUsed == null)
            {
                dgvData.DataSource = null;
                lblStatus.Text = "Sheet is empty.";
                return;
            }

            var firstRow = rangeUsed.FirstRow();
            foreach (var cell in firstRow.CellsUsed())
            {
                var colName = cell.GetString().Trim();
                if (string.IsNullOrEmpty(colName)) colName = $"Column{cell.Address.ColumnNumber}";
                if (dt.Columns.Contains(colName)) colName += $"_{cell.Address.ColumnNumber}";
                dt.Columns.Add(colName);
            }

            foreach (var row in rangeUsed.RowsUsed().Skip(1))
            {
                var dr = dt.NewRow();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var cell = row.Cell(i + 1);
                    dr[i] = cell.IsEmpty() ? DBNull.Value : cell.GetString();
                }
                dt.Rows.Add(dr);
            }

            dgvData.DataSource = dt;
            lblStatus.Text = $"Loaded {dt.Rows.Count} rows, {dt.Columns.Count} columns";
        }

        // ───────── Load PostgreSQL Database List ─────────
        private void btnLoadDbs_Click(object sender, EventArgs e)
        {
            try
            {
                var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConnectionStrings.xml");
                var doc = XDocument.Load(xmlPath);
                var pg = doc.Root!.Element("Postgres")!;

                var connStr =
                    $"Host={pg.Element("Host")!.Value};" +
                    $"Port={pg.Element("Port")!.Value};" +
                    $"Database=postgres;" +
                    $"Username={pg.Element("Username")!.Value};" +
                    $"Password={pg.Element("Password")!.Value};" +
                    $"Timeout=5;";

                cmbDatabase.Items.Clear();

                using var conn = new NpgsqlConnection(connStr);
                conn.Open();
                using var cmd = new NpgsqlCommand(
                    "SELECT datname FROM pg_database WHERE datistemplate = false ORDER BY datname", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    cmbDatabase.Items.Add(reader.GetString(0));

                if (cmbDatabase.Items.Count > 0)
                    cmbDatabase.SelectedIndex = 0;

                lblStatus.Text = $"Loaded {cmbDatabase.Items.Count} databases";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load databases:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ───────── Save to PostgreSQL ─────────
        private async void btnSave_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (dgvData.DataSource is not DataTable dt || dt.Rows.Count == 0)
            {
                MessageBox.Show("No data to save. Please load an Excel file first.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbDatabase.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a target database.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var tableName = txtTableName.Text.Trim().ToLower().Replace(" ", "_");
            if (string.IsNullOrEmpty(tableName))
            {
                MessageBox.Show("Please enter a table name.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dbName = cmbDatabase.SelectedItem!.ToString()!;

            var confirm = MessageBox.Show(
                $"This will create table \"{tableName}\" in database \"{dbName}\" and insert {dt.Rows.Count} rows.\n\nContinue?",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            // Disable UI during save
            btnSave.Enabled = false;
            btnBrowse.Enabled = false;
            progressBar.Visible = true;
            progressBar.Value = 0;
            progressBar.Maximum = dt.Rows.Count;

            try
            {
                var connStr = BuildConnectionString(dbName);

                await Task.Run(() =>
                {
                    using var conn = new NpgsqlConnection(connStr);
                    conn.Open();

                    // Build column definitions with inferred types
                    var columns = new List<string>();
                    var colNames = new List<string>();
                    var colTypes = new List<string>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        var safeCol = SanitizeIdentifier(col.ColumnName);
                        var pgType = InferPgType(dt, col);
                        colNames.Add(safeCol);
                        colTypes.Add(pgType);
                        columns.Add($"\"{safeCol}\" {pgType}");
                    }

                    // Create table
                    var createSql = $"CREATE TABLE IF NOT EXISTS \"{tableName}\" (id serial PRIMARY KEY, {string.Join(", ", columns)})";
                    using (var cmd = new NpgsqlCommand(createSql, conn))
                        cmd.ExecuteNonQuery();

                    // Insert rows using COPY for performance
                    var colList = string.Join(", ", colNames.Select(c => $"\"{c}\""));
                    using var writer = conn.BeginBinaryImport(
                        $"COPY \"{tableName}\" ({colList}) FROM STDIN (FORMAT BINARY)");

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        writer.StartRow();
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            var val = dt.Rows[i][dt.Columns[c]];
                            var raw = (val == DBNull.Value || val == null) ? null : val.ToString();
                            var converted = ConvertValue(raw, colTypes[c]);
                            if (converted == null)
                                writer.WriteNull();
                            else
                                writer.Write(converted, MapNpgsqlType(colTypes[c]));
                        }

                        // Update progress on UI thread
                        var progress = i + 1;
                        Invoke(() =>
                        {
                            progressBar.Value = progress;
                            lblStatus.Text = $"Inserting row {progress} of {dt.Rows.Count}...";
                        });
                    }

                    writer.Complete();
                });

                lblStatus.Text = $"Done! {dt.Rows.Count} rows inserted into \"{tableName}\" with typed columns";
                MessageBox.Show(
                    $"Successfully created table \"{tableName}\" and inserted {dt.Rows.Count} rows in database \"{dbName}\".",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error during save.";
                MessageBox.Show($"Failed to save data:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSave.Enabled = true;
                btnBrowse.Enabled = true;
                progressBar.Visible = false;
            }
        }

        private string BuildConnectionString(string database)
        {
            var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConnectionStrings.xml");
            var doc = XDocument.Load(xmlPath);
            var pg = doc.Root!.Element("Postgres")!;

            return
                $"Host={pg.Element("Host")!.Value};" +
                $"Port={pg.Element("Port")!.Value};" +
                $"Database={database};" +
                $"Username={pg.Element("Username")!.Value};" +
                $"Password={pg.Element("Password")!.Value};" +
                $"Timeout={pg.Element("Timeout")!.Value};" +
                $"CommandTimeout={pg.Element("CommandTimeout")!.Value};" +
                $"Include Error Detail={pg.Element("IncludeErrorDetail")!.Value};";
        }

        // ───────── Infer PostgreSQL type from column values ─────────
        private static string InferPgType(DataTable dt, DataColumn col)
        {
            bool allEmpty = true;
            bool canInt = true, canBigInt = true, canNumeric = true, canBool = true, canDate = true;

            foreach (DataRow row in dt.Rows)
            {
                var val = row[col];
                if (val == DBNull.Value || val == null || string.IsNullOrWhiteSpace(val.ToString()))
                    continue;

                allEmpty = false;
                var s = val.ToString()!.Trim();

                if (canInt && !int.TryParse(s, out _))
                    canInt = false;

                if (canBigInt && !long.TryParse(s, out _))
                    canBigInt = false;

                if (canNumeric && !decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out _))
                    canNumeric = false;

                if (canBool && !bool.TryParse(s, out _) &&
                    s != "0" && s != "1" && !s.Equals("yes", StringComparison.OrdinalIgnoreCase) &&
                    !s.Equals("no", StringComparison.OrdinalIgnoreCase))
                    canBool = false;

                if (canDate && !DateTime.TryParse(s, out _))
                    canDate = false;
            }

            if (allEmpty) return "text";
            if (canBool) return "boolean";
            if (canInt) return "integer";
            if (canBigInt) return "bigint";
            if (canNumeric) return "numeric";
            if (canDate) return "timestamp";
            return "text";
        }

        private static object? ConvertValue(string? s, string pgType)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();
            return pgType switch
            {
                "integer" => int.Parse(s),
                "bigint" => long.Parse(s),
                "numeric" => decimal.Parse(s, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture),
                "boolean" => s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                             s.Equals("yes", StringComparison.OrdinalIgnoreCase),
                "timestamp" => DateTime.Parse(s),
                _ => s
            };
        }

        private static NpgsqlTypes.NpgsqlDbType MapNpgsqlType(string pgType)
        {
            return pgType switch
            {
                "integer" => NpgsqlTypes.NpgsqlDbType.Integer,
                "bigint" => NpgsqlTypes.NpgsqlDbType.Bigint,
                "numeric" => NpgsqlTypes.NpgsqlDbType.Numeric,
                "boolean" => NpgsqlTypes.NpgsqlDbType.Boolean,
                "timestamp" => NpgsqlTypes.NpgsqlDbType.Timestamp,
                _ => NpgsqlTypes.NpgsqlDbType.Text
            };
        }

        private static string SanitizeIdentifier(string name)
        {
            var sb = new StringBuilder();
            foreach (var c in name.Trim().ToLower())
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
                else
                    sb.Append('_');
            }
            var result = sb.ToString();
            if (result.Length == 0 || char.IsDigit(result[0]))
                result = "_" + result;
            return result;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _workbook?.Dispose();
            Close();
        }
    }
}
