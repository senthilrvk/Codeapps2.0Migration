using CodeAppsDataMigration.Data;
using CodeAppsDataMigration.Migration;
using Microsoft.Data.SqlClient;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace CodeAppsDataMigration
{
    public partial class Form1 : Form
    {
        int nFromBranchId = 0;
        int nBranchId = 0, nMainBranchId = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnDataTransfer_Click(object sender, EventArgs e)
          {
            string sql = SqlServerConnection.GetConnectionString();
            string pg = PostgresConnection.GetConnectionString();

            var runner = new MigrationRunner(sql, pg);
            nBranchId = 4; nMainBranchId = 4;
            nFromBranchId = 17;

            runner.RunAll(nMainBranchId, nBranchId, nFromBranchId);
            runner.UpdatePrimaryKeyColumns(nMainBranchId, nBranchId);
            MessageBox.Show("Migration completed successfully");
        }

    }
}
