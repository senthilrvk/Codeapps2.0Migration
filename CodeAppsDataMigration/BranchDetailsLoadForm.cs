using CodeAppsDataMigration.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CodeAppsDataMigration
{
    public partial class BranchDetailsLoadForm : Form
    {
        public BranchDetailsLoadForm()
        {
            InitializeComponent();
        }

        private void BranchDetailsLoadForm_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        // Columns to display in the GridView
        private readonly string[] _displayColumns = { "branchid", "branchcode", "branchname", "branchaddr1", "branchaddr2" };

        private void LoadData()
        {
            try
            {
                using var conn = SqlServerConnection.Create();
                conn.Open();
                using var cmd = new SqlCommand("SELECT * FROM branchdetails", conn);
                using var reader = cmd.ExecuteReader();

                var dt = new DataTable();
                dt.Load(reader);

                dgvBranchDetails.DataSource = dt;

                // Hide all columns, then show only the display columns
                foreach (DataGridViewColumn col in dgvBranchDetails.Columns)
                {
                    if (col.Name != "colCreate")
                        col.Visible = false;
                }

                int displayIndex = 1;
                foreach (var colName in _displayColumns)
                {
                    var col = dgvBranchDetails.Columns[colName];
                    if (col != null)
                    {
                        col.Visible = true;
                        col.DisplayIndex = displayIndex++;
                    }
                }

                // Add Create button column if not already added
                if (dgvBranchDetails.Columns["colCreate"] == null)
                {
                    var btnCol = new DataGridViewButtonColumn
                    {
                        Name = "colCreate",
                        HeaderText = "Action",
                        Text = "Create",
                        UseColumnTextForButtonValue = true,
                        FlatStyle = FlatStyle.Flat,
                        Width = 80,
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                    };
                    btnCol.DefaultCellStyle.BackColor = Color.FromArgb(72, 187, 120);
                    btnCol.DefaultCellStyle.ForeColor = Color.White;
                    btnCol.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    btnCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvBranchDetails.Columns.Add(btnCol);
                }

                // Move the Create button column to the first (left) position
                dgvBranchDetails.Columns["colCreate"]!.DisplayIndex = 0;

                lblStatus.Text = $"Total branch details: {dt.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load branch details:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error loading branch details";
            }
        }

        private void dgvBranchDetails_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvBranchDetails.Columns[e.ColumnIndex].Name != "colCreate") return;

            var row = dgvBranchDetails.Rows[e.RowIndex];
            var dt = (DataTable)dgvBranchDetails.DataSource;

            using var detailForm = new BranchDetailForm(row, dt);
            if (detailForm.ShowDialog() == DialogResult.OK)
            {
                lblStatus.Text = $"Branch detail created in PostgreSQL successfully";
            }
        }
    }
}
