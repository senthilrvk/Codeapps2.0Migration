using Microsoft.Data.SqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CodeAppsDataMigration.Migration
{
    public class DynamicMigrator
    {
        private readonly string _sqlConn;
        private readonly string _pgConn;

        public DynamicMigrator(string sqlConn, string pgConn)
        {
            _sqlConn = sqlConn;
            _pgConn = pgConn;
        }

        public int Run(TableMap map)
        {
            using var pg = new NpgsqlConnection(_pgConn);
            pg.Open();
            return Run(map, pg, null);
        }

        /// <summary>
        /// Migrates a single table using a caller-supplied PostgreSQL connection and
        /// (optional) transaction. When a transaction is supplied, the COPY import and all
        /// commands participate in it, so the work can be rolled back as a unit.
        /// </summary>
        public int Run(TableMap map, NpgsqlConnection pg, NpgsqlTransaction tx)
        {
            string strTableName = "";
            string strPosTableName = "";
            int currentRow = 0;
            string currentCol = "";
            string sqlQuery = "";
            string copyCmd = "";
            try
            {
                using var sql = new SqlConnection(_sqlConn);
                sql.Open();

                // =====================================================
                // 1 READ SQL TABLE
                // =====================================================
                var sqlTable = new DataTable();
                map.PgTable = map.PgTable.Replace("@", "");

                sqlQuery = $"SELECT tbl.*, {MigrationConfig.nMainBranchId} mainbranchid FROM dbo.{map.SqlTable} tbl " + map.condition;
                new SqlDataAdapter(sqlQuery, sql).Fill(sqlTable);
                strTableName = map.SqlTable;
                strPosTableName = map.PgTable;

                // =====================================================
                // 2 READ POSTGRES SCHEMA
                // =====================================================
                var pgColumns =
                    new List<(string Name, string Type, bool IsIdentity)>();

                using (var cmd = new NpgsqlCommand(@"
                SELECT
                    column_name,
                    data_type,
                    is_identity
                FROM information_schema.columns
                WHERE table_name = @t
                ORDER BY ordinal_position", pg, tx))
                {
                    cmd.Parameters.AddWithValue("t", map.PgTable);

                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        pgColumns.Add((
                            r.GetString(0),
                            r.GetString(1),
                            r.GetString(2) == "YES"
                        ));
                    }
                }

                // =====================================================
                // 3 REMOVE IDENTITY / PRIMARY KEY
                // =====================================================
                var insertColumns = pgColumns
                    .Where(c => !c.IsIdentity)
                    .ToList();

                // =====================================================
                // 4 BUILD COLUMN MAP
                // =====================================================
                var columnMap = map.Columns.ToDictionary(
                    x => x.Pg.ToLower(),
                    x => x.Sql
                );

                // =====================================================
                // 5 TEXT COPY (PostgreSQL handles type conversion)
                // =====================================================
                copyCmd = $"COPY public.{map.PgTable} ({string.Join(",", insertColumns.Select(c => c.Name))}) FROM STDIN (FORMAT TEXT, NULL '\\N')";

                using var writer = pg.BeginTextImport(copyCmd);

                int count = 0;

                // =====================================================
                // 6 DATA TRANSFER
                // =====================================================
                foreach (DataRow row in sqlTable.Rows)
                {
                    currentRow++;
                    var values = new List<string>();

                    foreach (var col in insertColumns)
                    {
                        currentCol = col.Name;
                        object val = null;

                        // ---------- CONSTANT ----------
                        if (map.Constants.TryGetValue(col.Name, out var constant))
                        {
                            val = constant;
                        }
                        // ---------- MAPPED SQL COLUMN ----------
                        else if (columnMap.TryGetValue(col.Name.ToLower(), out var sqlCol)
                            && sqlTable.Columns.Contains(sqlCol))
                        {
                            if (sqlCol == "branchid")
                                val = MigrationConfig.nBranchId;
                            else if (sqlCol == "mainbranchid")
                                val = MigrationConfig.nMainBranchId;
                            else
                                val = row[sqlCol];
                        }

                        values.Add(FormatValue(val, col.Type));
                    }

                    writer.WriteLine(string.Join("\t", values));
                    count++;
                }

                return count;
            }
            catch (Exception Ex)
            {
                string context =
                    $"Migration failed on table '{strTableName}' (PostgreSQL table '{strPosTableName}'), " +
                    $"approx. row #{currentRow}, column '{currentCol}'.";

                Console.WriteLine("ERROR " + context);
                Console.WriteLine(ExceptionFormatter.Describe(Ex));

                // Re-throw with the table/row/column context as structured fields so the UI
                // can show exactly where it failed. The original exception (incl.
                // PostgresException detail) is preserved as InnerException.
                string script =
                    "SQL Server read query:" + Environment.NewLine + sqlQuery + Environment.NewLine +
                    (string.IsNullOrEmpty(copyCmd) ? "" : "PostgreSQL COPY command:" + Environment.NewLine + copyCmd);

                throw new MigrationException(
                    context + " " + Ex.Message,
                    tableName: strTableName,
                    inner: Ex,
                    pgTableName: strPosTableName,
                    columnName: currentCol,
                    rowNumber: currentRow,
                    failingQuery: script,
                    functionName: "DynamicMigrator." + nameof(Run));
            }
        }

        /// <summary>
        /// Format value for PostgreSQL TEXT COPY (tab-separated).
        /// Returns \N for null, escaped text for strings, plain text for others.
        /// </summary>
        private static string FormatValue(object v, string pgDataType)
        {
            // NULL / DBNull → default value based on type
            if (v == null || v == DBNull.Value)
            {
                return GetDefault(pgDataType);
            }

            // byte[] → hex format for bytea
            if (v is byte[] bytes)
            {
                if (bytes.Length == 0) return GetDefault(pgDataType);
                return "\\\\x" + BitConverter.ToString(bytes).Replace("-", "");
            }

            string s = v.ToString()?.Trim() ?? "";

            if (string.IsNullOrEmpty(s))
            {
                return GetDefault(pgDataType);
            }

            // Boolean conversion
            if (pgDataType == "boolean")
            {
                bool b = s == "1" ||
                         s.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                         s.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                         s.Equals("y", StringComparison.OrdinalIgnoreCase);
                return b ? "true" : "false";
            }

            // Date → yyyy-MM-dd
            if (pgDataType == "date")
            {
                if (DateTime.TryParse(s, out DateTime dt))
                    return dt.ToString("yyyy-MM-dd");
                return "1900-01-01";
            }

            // Timestamp → yyyy-MM-dd HH:mm:ss.fff
            if (pgDataType.StartsWith("timestamp"))
            {
                if (DateTime.TryParse(s, out DateTime ts))
                    return ts.ToString("yyyy-MM-dd HH:mm:ss.fff");
                return "1900-01-01 00:00:00.000";
            }

            // Numeric types → ensure invariant culture
            if (pgDataType is "numeric" or "decimal" or "real" or "double precision")
            {
                if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal d))
                    return d.ToString(CultureInfo.InvariantCulture);
                return "0";
            }

            // Integer types
            if (pgDataType is "smallint" or "integer" or "bigint")
            {
                if (long.TryParse(s, out long l))
                    return l.ToString();
                // Source may be numeric/decimal (e.g. "45.00" or "45.5"); round to nearest integer.
                if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal dec))
                    return Math.Round(dec, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture);
                return "0";
            }

            // Text → escape special characters for COPY TEXT format
            return EscapeForCopy(s);
        }

        /// <summary>
        /// Returns a safe default value string for each PostgreSQL type.
        /// </summary>
        private static string GetDefault(string pgDataType)
        {
            if (pgDataType is "text" or "varchar" or "character varying" or "character")
                return "";
            if (pgDataType == "boolean")
                return "false";
            if (pgDataType is "smallint" or "integer" or "bigint")
                return "0";
            if (pgDataType is "numeric" or "decimal" or "real" or "double precision")
                return "0";
            if (pgDataType == "date")
                return "1900-01-01";
            if (pgDataType.StartsWith("timestamp"))
                return "1900-01-01 00:00:00.000";
            if (pgDataType == "bytea")
                return "\\\\x";
            if (pgDataType == "uuid")
                return "00000000-0000-0000-0000-000000000000";
            if (pgDataType is "json" or "jsonb")
                return "{}";
            return "";
        }

        /// <summary>
        /// Escape special characters for PostgreSQL COPY TEXT format.
        /// Backslash, tab, newline, carriage return must be escaped.
        /// </summary>
        private static string EscapeForCopy(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            return s
                .Replace("\\", "\\\\")
                .Replace("\t", "\\t")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r");
        }
    }
}
