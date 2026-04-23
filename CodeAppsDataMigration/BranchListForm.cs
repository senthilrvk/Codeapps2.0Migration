using CodeAppsDataMigration.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CodeAppsDataMigration
{
    public partial class BranchListForm : Form
    {
        public BranchListForm()
        {
            InitializeComponent();
        }

        private void BranchListForm_Load(object sender, EventArgs e)
        {
            LoadBranches();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadBranches();
        }

        private void LoadBranches()
        {
            try
            {
                using var conn = SqlServerConnection.Create();
                conn.Open();
                using var cmd = new SqlCommand("SELECT * FROM branch", conn);
                using var reader = cmd.ExecuteReader();

                var dt = new DataTable();
                dt.Load(reader);

                dgvBranches.DataSource = dt;

                // Add Create button column if not already added
                if (dgvBranches.Columns["colCreate"] == null)
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
                    dgvBranches.Columns.Add(btnCol);
                }

                // Move the Create button column to the last position
                dgvBranches.Columns["colCreate"]!.DisplayIndex = dgvBranches.Columns.Count - 1;

                lblStatus.Text = $"Total branches: {dt.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load branches:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error loading branches";
            }
        }

        private void dgvBranches_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvBranches.Columns[e.ColumnIndex].Name != "colCreate") return;

            var row = dgvBranches.Rows[e.RowIndex];
            var dt = (DataTable)dgvBranches.DataSource;

            using var detailForm = new BranchDetailForm(row, dt);
            if (detailForm.ShowDialog() == DialogResult.OK)
            {
                lblStatus.Text = $"Branch '{row.Cells["BranchName"]?.Value}' created in PostgreSQL";
            }
        }
    }
}
