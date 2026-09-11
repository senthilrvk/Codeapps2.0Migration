using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Npgsql;

namespace CodeAppsDataMigration.Migration
{
    /// <summary>
    /// One user table discovered in the source (offline) database.
    /// </summary>
    public sealed class PgTableInfo
    {
        public string Schema { get; set; } = "public";
        public string Name { get; set; } = "";
        public long SourceRows { get; set; }
        public long? TargetRows { get; set; }      // null = table missing in target
        public bool ExistsInTarget => TargetRows.HasValue;
        public string QualifiedName => Quote(Schema) + "." + Quote(Name);

        public static string Quote(string ident) => "\"" + ident.Replace("\"", "\"\"") + "\"";
    }

    /// <summary>
    /// Options for the offline -> online PostgreSQL copy.
    /// </summary>
    public sealed class PgToPgOptions
    {
        /// <summary>Empty the selected target tables before copying.</summary>
        public bool TruncateTarget { get; set; } = true;

        /// <summary>
        /// Try SET session_replication_role = replica on the target so FK/trigger checks
        /// are skipped during the load. Needs superuser (or rds_superuser) rights; if
        /// the server refuses it the copy still runs, in FK dependency order.
        /// </summary>
        public bool DisableTriggers { get; set; } = true;

        /// <summary>After the copy, move every serial / identity sequence past MAX(col).</summary>
        public bool ResetSequences { get; set; } = true;

        /// <summary>Skip tables that do not exist in the target instead of failing.</summary>
        public bool SkipMissingTables { get; set; } = true;
    }

    /// <summary>
    /// Per-table result reported back to the UI.
    /// </summary>
    public sealed class PgTableResult
    {
        public string Table { get; set; } = "";
        public long RowsCopied { get; set; }
        public string Status { get; set; } = "";
        public bool Ok { get; set; }
        public double Seconds { get; set; }
    }

    /// <summary>
    /// Copies data from one PostgreSQL database (the client's offline server) into
    /// another PostgreSQL database (the new online server) whose schema already exists.
    ///
    /// Strategy: for every selected table, stream
    ///     COPY (SELECT cols FROM src) TO STDOUT (FORMAT TEXT)
    /// straight into
    ///     COPY tgt (cols) FROM STDIN (FORMAT TEXT)
    /// without materialising rows in memory. Only columns present on BOTH sides are
    /// copied, so small schema drift (extra/removed columns) does not break the run.
    /// Everything on the target happens in one transaction: any failure rolls the
    /// whole copy back so the online database is never left half-loaded.
    /// </summary>
    public sealed class PgToPgMigrator
    {
        private readonly string _sourceConnStr;
        private readonly string _targetConnStr;

        private Action<string, int>? _onProgress;
        private Action<PgTableResult>? _onTableDone;

        public PgToPgMigrator(string sourceConnStr, string targetConnStr)
        {
            _sourceConnStr = sourceConnStr;
            _targetConnStr = targetConnStr;
        }

        public void SetProgressCallback(Action<string, int> onProgress) => _onProgress = onProgress;
        public void SetTableDoneCallback(Action<PgTableResult> onTableDone) => _onTableDone = onTableDone;

        private void ReportProgress(string message, int percent) => _onProgress?.Invoke(message, percent);

        // ------------------------------------------------------------------
        //  Discovery
        // ------------------------------------------------------------------

        /// <summary>
        /// Lists the user tables of the source database (schema "public"), ordered so that
        /// referenced (parent) tables come before the tables that reference them, with
        /// exact row counts from source and target.
        /// </summary>
        public List<PgTableInfo> DiscoverTables(CancellationToken ct = default)
        {
            using var src = new NpgsqlConnection(_sourceConnStr);
            src.Open();

            var tables = ListUserTables(src);
            tables = OrderByDependencies(src, tables);

            using var tgt = new NpgsqlConnection(_targetConnStr);
            tgt.Open();
            var targetTables = new HashSet<string>(ListUserTables(tgt).Select(t => t.Name),
                                                   StringComparer.OrdinalIgnoreCase);

            int i = 0;
            foreach (var t in tables)
            {
                ct.ThrowIfCancellationRequested();
                i++;
                ReportProgress($"Counting rows: {t.Name} ({i}/{tables.Count})",
                    (int)((double)i / tables.Count * 100));

                t.SourceRows = CountRows(src, t.QualifiedName);
                t.TargetRows = targetTables.Contains(t.Name) ? CountRows(tgt, t.QualifiedName) : (long?)null;
            }

            return tables;
        }

        private static List<PgTableInfo> ListUserTables(NpgsqlConnection conn)
        {
            const string sql = @"
                SELECT table_schema, table_name
                FROM information_schema.tables
                WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
                ORDER BY table_name";

            var list = new List<PgTableInfo>();
            using var cmd = new NpgsqlCommand(sql, conn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(new PgTableInfo { Schema = rd.GetString(0), Name = rd.GetString(1) });
            return list;
        }

        private static long CountRows(NpgsqlConnection conn, string qualifiedName)
        {
            using var cmd = new NpgsqlCommand("SELECT count(*) FROM " + qualifiedName, conn);
            return Convert.ToInt64(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Topological sort on the foreign-key graph (Kahn's algorithm). Parents first.
        /// Tables that take part in an FK cycle are appended at the end in name order.
        /// </summary>
        private static List<PgTableInfo> OrderByDependencies(NpgsqlConnection conn, List<PgTableInfo> tables)
        {
            const string sql = @"
                SELECT c.conrelid::regclass::text  AS child,
                       c.confrelid::regclass::text AS parent
                FROM pg_constraint c
                JOIN pg_namespace n ON n.oid = c.connamespace
                WHERE c.contype = 'f' AND n.nspname = 'public' AND c.conrelid <> c.confrelid";

            var byName = tables.ToDictionary(t => t.Name, t => t, StringComparer.OrdinalIgnoreCase);
            var parents = tables.ToDictionary(t => t.Name, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                                              StringComparer.OrdinalIgnoreCase);

            using (var cmd = new NpgsqlCommand(sql, conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    string child = StripRegclass(rd.GetString(0));
                    string parent = StripRegclass(rd.GetString(1));
                    if (parents.ContainsKey(child) && byName.ContainsKey(parent))
                        parents[child].Add(parent);
                }
            }

            var ordered = new List<PgTableInfo>();
            var remaining = new HashSet<string>(byName.Keys, StringComparer.OrdinalIgnoreCase);

            while (remaining.Count > 0)
            {
                var ready = remaining
                    .Where(n => parents[n].All(p => !remaining.Contains(p)))
                    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (ready.Count == 0)
                {
                    // FK cycle - emit the rest alphabetically.
                    ready = remaining.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
                }

                foreach (var n in ready)
                {
                    ordered.Add(byName[n]);
                    remaining.Remove(n);
                }
            }

            return ordered;
        }

        /// <summary>regclass::text yields "public.name" or "\"Name\"" - reduce to the bare table name.</summary>
        private static string StripRegclass(string regclass)
        {
            var s = regclass;
            int dot = s.IndexOf('.');
            if (dot >= 0) s = s.Substring(dot + 1);
            if (s.Length >= 2 && s[0] == '"' && s[^1] == '"')
                s = s.Substring(1, s.Length - 2).Replace("\"\"", "\"");
            return s;
        }

        // ------------------------------------------------------------------
        //  Copy
        // ------------------------------------------------------------------

        /// <summary>
        /// Copies the given tables. Everything on the target runs in one transaction.
        /// Throws MigrationException (with TableName set) on failure after rolling back.
        /// </summary>
        public void Run(IList<PgTableInfo> tables, PgToPgOptions opts, CancellationToken ct = default)
        {
            if (tables.Count == 0)
                throw new InvalidOperationException("No tables selected.");

            using var src = new NpgsqlConnection(_sourceConnStr);
            using var tgt = new NpgsqlConnection(_targetConnStr);
            src.Open();
            tgt.Open();

            using var tx = tgt.BeginTransaction();
            string currentTable = "(setup)";
            string? currentSql = null;

            try
            {
                // Source read snapshot: all tables are read as of the same instant so
                // parent/child rows stay consistent even if the offline DB is still in use.
                using (var srcTx = src.BeginTransaction(System.Data.IsolationLevel.RepeatableRead))
                {
                    var work = tables.Where(t => t.ExistsInTarget || !opts.SkipMissingTables).ToList();
                    var missing = tables.Where(t => !t.ExistsInTarget).ToList();

                    if (missing.Count > 0 && !opts.SkipMissingTables)
                        throw new InvalidOperationException(
                            "These tables do not exist in the target database: " +
                            string.Join(", ", missing.Select(m => m.Name)));

                    foreach (var m in missing)
                        _onTableDone?.Invoke(new PgTableResult { Table = m.Name, Ok = true, Status = "Skipped - missing in target" });

                    // ---- 1. optionally silence FK / trigger checks on the target ----
                    bool triggersDisabled = false;
                    if (opts.DisableTriggers)
                    {
                        currentSql = "SET LOCAL session_replication_role = replica";
                        try
                        {
                            Exec(tgt, tx, currentSql);
                            triggersDisabled = true;
                            ReportProgress("Target triggers/FK checks disabled for this session.", 0);
                        }
                        catch (PostgresException pex) when (pex.SqlState == "42501")
                        {
                            // insufficient_privilege - fall back to dependency order.
                            ReportProgress("No permission to disable triggers - copying in FK order instead.", 0);
                        }
                    }

                    // ---- 2. truncate ----
                    if (opts.TruncateTarget && work.Count > 0)
                    {
                        currentTable = "(truncate)";
                        // One statement for all tables satisfies FK checks between them.
                        currentSql = "TRUNCATE TABLE " +
                                     string.Join(", ", work.AsEnumerable().Reverse().Select(t => t.QualifiedName)) +
                                     " RESTART IDENTITY";
                        ReportProgress($"Truncating {work.Count} target tables...", 1);
                        Exec(tgt, tx, currentSql);
                    }

                    // ---- 3. copy table by table ----
                    int idx = 0;
                    foreach (var t in work)
                    {
                        ct.ThrowIfCancellationRequested();
                        idx++;
                        currentTable = t.Name;
                        int pct = 2 + (int)((double)(idx - 1) / work.Count * 88);   // tables own 2-90 %
                        ReportProgress($"[{idx}/{work.Count}] Copying: {t.Name} ({t.SourceRows:N0} rows)", pct);

                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        long rows;
                        currentSql = null;
                        try
                        {
                            rows = CopyTable(src, tgt, tx, t, out currentSql);
                        }
                        catch (Exception ex)
                        {
                            _onTableDone?.Invoke(new PgTableResult { Table = t.Name, Ok = false, Status = "FAILED: " + RootMessage(ex) });
                            throw;
                        }
                        sw.Stop();

                        _onTableDone?.Invoke(new PgTableResult
                        {
                            Table = t.Name,
                            RowsCopied = rows,
                            Ok = true,
                            Seconds = sw.Elapsed.TotalSeconds,
                            Status = "Done"
                        });
                        ReportProgress($"Done: {t.Name} - {rows:N0} rows ({sw.Elapsed.TotalSeconds:N1}s)", pct);
                    }

                    // ---- 4. sequences ----
                    if (opts.ResetSequences)
                    {
                        currentTable = "(sequences)";
                        ReportProgress("Resetting sequences...", 92);
                        int n = 0;
                        foreach (var t in work)
                        {
                            ct.ThrowIfCancellationRequested();
                            currentTable = t.Name;
                            n += ResetSequences(tgt, tx, t, out currentSql);
                        }
                        ReportProgress($"Sequences reset: {n}", 96);
                    }

                    if (triggersDisabled)
                    {
                        currentTable = "(setup)";
                        currentSql = "SET LOCAL session_replication_role = DEFAULT";
                        Exec(tgt, tx, currentSql);
                    }

                    srcTx.Rollback();   // read-only; nothing to keep
                }

                ct.ThrowIfCancellationRequested();
                ReportProgress("Committing...", 98);
                tx.Commit();
                ReportProgress("Completed.", 100);
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { /* connection may already be broken */ }

                if (ex is OperationCanceledException)
                    throw;

                throw new MigrationException(
                    $"Offline -> online copy failed at '{currentTable}': {RootMessage(ex)}",
                    currentTable, ex,
                    pgTableName: currentTable,
                    failingQuery: currentSql,
                    functionName: nameof(PgToPgMigrator) + ".Run");
            }
        }

        /// <summary>
        /// Streams one table from source to target using COPY TEXT on both sides.
        /// Returns the number of rows written.
        /// </summary>
        private static long CopyTable(NpgsqlConnection src, NpgsqlConnection tgt, NpgsqlTransaction tx,
                                      PgTableInfo t, out string lastSql)
        {
            var srcCols = GetColumns(src, t.Name);
            var tgtCols = GetColumns(tgt, t.Name);

            // Columns present in both, skipping target GENERATED (computed) columns.
            var tgtWritable = new HashSet<string>(
                tgtCols.Where(c => !c.IsGenerated).Select(c => c.Name), StringComparer.OrdinalIgnoreCase);

            var cols = srcCols.Select(c => c.Name)
                              .Where(tgtWritable.Contains)
                              .ToList();

            if (cols.Count == 0)
                throw new InvalidOperationException($"Table '{t.Name}' has no columns in common between source and target.");

            string colList = string.Join(", ", cols.Select(PgTableInfo.Quote));
            string exportSql = $"COPY (SELECT {colList} FROM {t.QualifiedName}) TO STDOUT (FORMAT TEXT)";
            string importSql = $"COPY {t.QualifiedName} ({colList}) FROM STDIN (FORMAT TEXT)";
            lastSql = importSql;

            long rows = 0;
            var buffer = new char[64 * 1024];

            using var reader = src.BeginTextExport(exportSql);
            using (var writer = tgt.BeginTextImport(importSql))
            {
                int read;
                while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    writer.Write(buffer, 0, read);
                    for (int i = 0; i < read; i++)
                        if (buffer[i] == '\n') rows++;
                }
                // Dispose completes the COPY; an exception here surfaces the PG error.
            }

            return rows;
        }

        private sealed record ColumnInfo(string Name, bool IsGenerated, bool IsSerialOrIdentity);

        private static List<ColumnInfo> GetColumns(NpgsqlConnection conn, string table)
        {
            const string sql = @"
                SELECT column_name,
                       COALESCE(is_generated = 'ALWAYS', false)                                   AS is_generated,
                       COALESCE(is_identity = 'YES', false) OR COALESCE(column_default LIKE 'nextval(%', false) AS is_serial
                FROM information_schema.columns
                WHERE table_schema = 'public' AND table_name = @t
                ORDER BY ordinal_position";

            var list = new List<ColumnInfo>();
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("t", table);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(new ColumnInfo(rd.GetString(0), rd.GetBoolean(1), rd.GetBoolean(2)));
            return list;
        }

        /// <summary>
        /// For each serial / identity column of the table, sets its sequence so the next
        /// value is MAX(col)+1 (or 1 on an empty table). Returns how many were reset.
        /// </summary>
        private static int ResetSequences(NpgsqlConnection tgt, NpgsqlTransaction tx, PgTableInfo t, out string? lastSql)
        {
            lastSql = null;
            int n = 0;
            foreach (var col in GetColumns(tgt, t.Name).Where(c => c.IsSerialOrIdentity))
            {
                lastSql = $"SELECT pg_get_serial_sequence('{t.QualifiedName.Replace("'", "''")}', '{col.Name.Replace("'", "''")}')";
                string? seq;
                using (var cmd = new NpgsqlCommand(lastSql, tgt, tx))
                    seq = cmd.ExecuteScalar() as string;
                if (string.IsNullOrEmpty(seq)) continue;

                lastSql = $"SELECT setval('{seq.Replace("'", "''")}', " +
                          $"GREATEST(COALESCE(MAX({PgTableInfo.Quote(col.Name)}), 0), 1), " +
                          $"COALESCE(MAX({PgTableInfo.Quote(col.Name)}), 0) > 0) FROM {t.QualifiedName}";
                using (var cmd = new NpgsqlCommand(lastSql, tgt, tx))
                    cmd.ExecuteScalar();
                n++;
            }
            return n;
        }

        private static void Exec(NpgsqlConnection conn, NpgsqlTransaction tx, string sql)
        {
            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.ExecuteNonQuery();
        }

        private static string RootMessage(Exception ex)
        {
            Exception cur = ex;
            while (cur.InnerException != null) cur = cur.InnerException;
            return cur.Message;
        }
    }
}
