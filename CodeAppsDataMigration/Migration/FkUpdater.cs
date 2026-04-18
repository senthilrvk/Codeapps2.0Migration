using Npgsql;
using System;
using System.Linq;

namespace CodeAppsDataMigration.Migration
{
    public static class FkUpdater
    {
        
        public static void UpdateAll(string pgConnectionString)
        {
            using var conn = new NpgsqlConnection(pgConnectionString);
            conn.Open();

            Console.WriteLine("Updating foreign keys...\n");

            foreach (var fk in FkRegistry.Relations)
            {
                UpdateRelation(conn, fk);
            }

            Console.WriteLine("\n Foreign key update completed.");
        }

        private static void UpdateRelation(
            NpgsqlConnection conn,
            FkRelation fk)
        {
            string sql = $@"
                UPDATE {fk.ChildTable} c
                SET {fk.ChildColumn} = p.{fk.ParentPk}
                FROM {fk.ParentTable} p
                WHERE c.{fk.ChildColumn} = p.tempid
                  AND p.tempid IS NOT NULL;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);

            int rows = cmd.ExecuteNonQuery();

            Console.WriteLine(
                $"✔ {fk.ChildTable}.{fk.ChildColumn} → {fk.ParentTable} ({rows} rows)");
        }
    }
}
