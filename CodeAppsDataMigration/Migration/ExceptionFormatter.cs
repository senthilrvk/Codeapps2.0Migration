using System;
using System.Text;
using Npgsql;
using Microsoft.Data.SqlClient;

namespace CodeAppsDataMigration.Migration
{
    /// <summary>
    /// Builds a full, human-readable description of an exception for the migration UI.
    /// Walks the whole InnerException chain and, for PostgreSQL/SQL Server errors,
    /// pulls out the rich detail the providers expose (which table, which column,
    /// which constraint, the failing value, the COPY line, etc.) so the operator can
    /// see exactly what failed instead of just a one-line message.
    /// </summary>
    public static class ExceptionFormatter
    {
        public static string Describe(Exception ex)
        {
            if (ex == null) return "Unknown error (no exception).";

            var sb = new StringBuilder();
            int level = 0;

            for (Exception? cur = ex; cur != null; cur = cur.InnerException, level++)
            {
                string indent = level == 0 ? "" : new string(' ', level * 2);

                if (level == 0)
                    sb.AppendLine($"{cur.GetType().Name}: {cur.Message}");
                else
                    sb.AppendLine($"{indent}↳ Caused by [{cur.GetType().Name}]: {cur.Message}");

                if (cur is PostgresException pg)
                    AppendPostgres(sb, indent, pg);
                else if (cur is SqlException sqlEx)
                    AppendSqlServer(sb, indent, sqlEx);
            }

            sb.AppendLine();
            sb.AppendLine("---- Stack Trace ----");
            sb.AppendLine(ex.StackTrace ?? "(none)");

            return sb.ToString();
        }

        private static void AppendPostgres(StringBuilder sb, string indent, PostgresException pg)
        {
            AppendIf(sb, indent, "SQL State", pg.SqlState);
            AppendIf(sb, indent, "Table", pg.TableName);
            AppendIf(sb, indent, "Column", pg.ColumnName);
            AppendIf(sb, indent, "Data Type", pg.DataTypeName);
            AppendIf(sb, indent, "Constraint", pg.ConstraintName);
            AppendIf(sb, indent, "Schema", pg.SchemaName);
            AppendIf(sb, indent, "Detail", pg.Detail);
            AppendIf(sb, indent, "Hint", pg.Hint);
            // Where often contains the COPY line number + column for bulk-import failures.
            AppendIf(sb, indent, "Where", pg.Where);
            AppendIf(sb, indent, "Internal Query", pg.InternalQuery);
        }

        private static void AppendSqlServer(StringBuilder sb, string indent, SqlException sqlEx)
        {
            foreach (SqlError err in sqlEx.Errors)
            {
                sb.AppendLine($"{indent}  • SQL Server Error {err.Number} (line {err.LineNumber}) " +
                              $"in {err.Procedure ?? "(no proc)"}: {err.Message}");
            }
        }

        private static void AppendIf(StringBuilder sb, string indent, string label, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                sb.AppendLine($"{indent}  • {label}: {value}");
        }

        /// <summary>
        /// Best-effort extraction of the target table name from a SQL statement
        /// (UPDATE x / INSERT INTO x / DELETE FROM x). Used so FK/bulk-update
        /// failures can clearly report WHICH table failed, even when the provider
        /// exception does not populate TableName (common for UPDATE ... FROM).
        /// </summary>
        public static string ExtractTableName(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return "(unknown)";

            var m = System.Text.RegularExpressions.Regex.Match(
                sql,
                @"(?:UPDATE|INSERT\s+INTO|DELETE\s+FROM)\s+(?:public\.)?""?([A-Za-z_][A-Za-z0-9_]*)""?",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                System.Text.RegularExpressions.RegexOptions.Singleline);

            return m.Success ? m.Groups[1].Value : "(unknown)";
        }

        /// <summary>
        /// Returns the migration function where the error actually originated, e.g.
        /// "MigrationRunner.fnControOrderUpdate". Walks the exception chain from the
        /// innermost (origin) outward and reads the first stack-trace frame that
        /// belongs to this application's code.
        /// </summary>
        public static string ExtractFailingFunction(Exception ex)
        {
            var chain = new System.Collections.Generic.List<Exception>();
            for (Exception? c = ex; c != null; c = c.InnerException)
                chain.Add(c);
            chain.Reverse(); // innermost (true origin) first

            foreach (var e in chain)
            {
                string? fn = FirstOwnFrame(e.StackTrace);
                if (fn != null) return fn;
            }
            return "(unknown)";
        }

        private static string? FirstOwnFrame(string? stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace)) return null;

            // Frame format: "   at CodeAppsDataMigration.Migration.MigrationRunner.fnX(...) in ..."
            var matches = System.Text.RegularExpressions.Regex.Matches(
                stackTrace,
                @"at\s+(CodeAppsDataMigration\.[\w\.]+)\(");

            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                string full = m.Groups[1].Value; // e.g. CodeAppsDataMigration.Migration.MigrationRunner.fnControOrderUpdate
                var parts = full.Split('.');
                if (parts.Length >= 2)
                    return parts[parts.Length - 2] + "." + parts[parts.Length - 1]; // Class.Method
                return full;
            }
            return null;
        }
    }
}
