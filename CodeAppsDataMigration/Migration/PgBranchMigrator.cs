using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Npgsql;

namespace CodeAppsDataMigration.Migration
{
    /// <summary>One offline branch -> online branch mapping.</summary>
    public sealed class PgBranchMapping
    {
        public long SrcMainId { get; set; }
        public long SrcBranchId { get; set; }
        public long TgtMainId { get; set; }
        public long TgtBranchId { get; set; }
        public string Display { get; set; } = "";
    }

    public sealed class PgBranchOptions
    {
        /// <summary>Copy the branch profile (name, address, GST, bank, print settings) from the offline branch row onto the online branch row.</summary>
        public bool CopyBranchProfile { get; set; } = true;

        /// <summary>Copy setting values (branchsetting, mainsetting, hospitalsetting, voucherprefix, control order, price menu) by setting name.</summary>
        public bool MergeSettings { get; set; } = true;

        /// <summary>Try SET session_replication_role = replica on the target (falls back silently).</summary>
        public bool DisableTriggers { get; set; } = true;
    }

    /// <summary>
    /// Branch-level migration between two PostgreSQL databases with the CodeApps 2.0 schema,
    /// the PostgreSQL equivalent of the SQL Server -> PostgreSQL "Data Migration" page.
    ///
    /// The client's offline server holds main branch S with per-main tables (accountheadS,
    /// productmainS, issuemainS ...). The online server already has a main branch T and branch
    /// created through the online application, so its tables accountheadT ... exist (empty or
    /// with seed rows). This engine copies one offline branch into one online branch:
    ///
    ///  * Per-main tables  (name + main branch id): rows for the offline branch are copied with
    ///    their PRIMARY KEYS PRESERVED and branchid/mainbranchid rewritten. Because every main
    ///    branch owns its own set of these tables, no key can collide with another client, so
    ///    nothing has to be re-linked. Any existing rows of the online branch are deleted first.
    ///  * Shared tables  (area, category, billseries, doctor, godown ... which all clients share):
    ///    rows get NEW identity values (old value kept in tempid where the column exists) and an
    ///    old->new map is recorded. Every column that references a shared table (billserid,
    ///    categoryid, godownid, doctid ...) is then rewritten through that map, in every copied
    ///    table. This is the same idea as the tempid re-linking of the SQL Server migration.
    ///  * Settings tables are merged by setting name so options the online version added are kept.
    ///  * Reference tables the online app seeds itself (tax, accesslevel, branch) are never copied;
    ///    references to them are re-pointed by tax percent, access-level name and the branch map.
    ///
    /// Everything on the online database runs in ONE transaction and is rolled back on any error.
    /// </summary>
    public sealed class PgBranchMigrator
    {
        private readonly string _srcConnStr;
        private readonly string _tgtConnStr;
        private Action<string, int>? _onProgress;
        private Action<PgTableResult>? _onTableDone;

        public PgBranchMigrator(string sourceConnStr, string targetConnStr)
        {
            _srcConnStr = sourceConnStr;
            _tgtConnStr = targetConnStr;
        }

        public void SetProgressCallback(Action<string, int> cb) => _onProgress = cb;
        public void SetTableDoneCallback(Action<PgTableResult> cb) => _onTableDone = cb;
        private void Progress(string msg, int pct) => _onProgress?.Invoke(msg, pct);
        private void Done(string table, long rows, string status, bool ok = true) =>
            _onTableDone?.Invoke(new PgTableResult { Table = table, RowsCopied = rows, Status = status, Ok = ok });

        // ------------------------------------------------------------------
        //  Configuration
        // ------------------------------------------------------------------

        /// <summary>Tables that are never copied (owned / seeded by the online application).</summary>
        private static readonly HashSet<string> NeverCopy = new(StringComparer.OrdinalIgnoreCase)
        {
            "branch", "mainbranch", "accesslevel", "tax", "superuser", "menuhead", "menuitems", "menupermissions",
            "controlorder", "pricemenu", "printfilename", "head", "otherhead", "businesstype", "product", "v_productname",
            "price_list_report_results", "taxfilter"
        };

        /// <summary>
        /// Settings tables merged by key: target rows are kept, values copied from the source row with the same key.
        /// (table, key columns, value columns)
        /// </summary>
        private static readonly (string Table, string[] Keys, string[] Values, bool PerMainOnly)[] MergeTables =
        {
            ("branchsetting",          new[] { "settingname" },                new[] { "settingvalue", "settingbillno" },               false),
            ("hospitalsetting",        new[] { "hossettingname" },             new[] { "hossettingsvalue" },                            false),
            ("voucherprefix",          new[] { "vprefixid" },                  new[] { "prefix", "voucherdescription", "voucherno", "uniquevoucherid" }, false),
            ("mainsetting",            new[] { "settingname" },                new[] { "settingvalue", "settingbillno" },               true),
            ("controlordermainbranch", new[] { "controlname", "controltype" }, new[] { "controlorder", "active", "physicalorder" },     true),
            ("pricemenuonmain",        new[] { "pricemenuname" },              new[] { "displayname", "active", "orderno", "bpermission" }, true),
        };

        /// <summary>
        /// Foreign-key columns that point at a SHARED table (or a seeded one) and therefore must be
        /// rewritten through the old->new id map. Applied by column name to every copied table.
        /// Derived from MigrationRunner.ExecuteBulkUpdates.
        /// </summary>
        private static readonly (string Parent, string[] Columns)[] RemapRules =
        {
            ("billseries",             new[] { "billserid", "salesbillserid" }),
            ("area",                   new[] { "areaid" }),
            ("category",               new[] { "categoryid", "unitid", "reasonid" }),
            ("categoryhead",           new[] { "categoryhead_id", "categoryheadid" }),
            ("godown",                 new[] { "godownid" }),
            ("doctor",                 new[] { "doctid", "doctorid", "visitdoctorid" }),
            ("department",             new[] { "department_id", "departmentid" }),
            ("specialist",             new[] { "specialistid" }),
            ("diagnosis",              new[] { "diagnosisid" }),
            ("bloodgroup",             new[] { "bloodgroupid" }),
            ("hospital",               new[] { "hospitalid" }),
            ("revisiting",             new[] { "revisitid" }),
            ("labbill",                new[] { "labbillid" }),
            ("test",                   new[] { "testid" }),
            ("testsub",                new[] { "testsubid" }),
            ("testresult",             new[] { "testresultid" }),
            ("symptoms",               new[] { "diseasesid" }),
            ("pmrsheet",               new[] { "pmruniquekey" }),
            ("creditdebitnotedismain", new[] { "creditdebitnotemainid" }),
            ("restotable",             new[] { "tableid" }),
            ("userprivilage",          new[] { "privilageid" }),
            ("packagemain",            new[] { "packageid" }),
            ("kotmain",                new[] { "kotid" }),
            ("notes",                  new[] { "noteid" }),
            ("producttype",            new[] { "producttypeid" }),
            ("accesslevel",            new[] { "accesslevel", "accessid" }),
            ("branch",                 new[] { "tobranch" }),
            ("tax",                    new[] { "taxid" }),
        };

        // ------------------------------------------------------------------
        //  Metadata
        // ------------------------------------------------------------------

        private sealed record Col(string Name, string DataType, bool IsIdentity, bool IsSerial, bool IsGenerated);

        private sealed class Meta
        {
            public Dictionary<string, List<Col>> Tables = new(StringComparer.OrdinalIgnoreCase);
            public bool Has(string t) => Tables.ContainsKey(t);
            public bool HasCol(string t, string c) => Tables.TryGetValue(t, out var l) && l.Any(x => x.Name.Equals(c, StringComparison.OrdinalIgnoreCase));
            public Col? GetCol(string t, string c) => Tables.TryGetValue(t, out var l) ? l.FirstOrDefault(x => x.Name.Equals(c, StringComparison.OrdinalIgnoreCase)) : null;
            public Col? KeyCol(string t) => Tables.TryGetValue(t, out var l) ? (l.FirstOrDefault(x => x.IsIdentity) ?? l.FirstOrDefault(x => x.IsSerial)) : null;
        }

        private static Meta LoadMeta(NpgsqlConnection conn)
        {
            const string sql = @"
                SELECT c.table_name, c.column_name, c.data_type,
                       COALESCE(c.is_identity = 'YES', false),
                       COALESCE(c.column_default LIKE 'nextval(%', false),
                       COALESCE(c.is_generated = 'ALWAYS', false)
                FROM information_schema.columns c
                JOIN information_schema.tables t ON t.table_schema = c.table_schema AND t.table_name = c.table_name
                WHERE c.table_schema = 'public' AND t.table_type = 'BASE TABLE'
                ORDER BY c.table_name, c.ordinal_position";
            var m = new Meta();
            using var cmd = new NpgsqlCommand(sql, conn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                string t = rd.GetString(0);
                if (!m.Tables.TryGetValue(t, out var list)) m.Tables[t] = list = new List<Col>();
                list.Add(new Col(rd.GetString(1), rd.GetString(2), rd.GetBoolean(3), rd.GetBoolean(4), rd.GetBoolean(5)));
            }
            return m;
        }

        private static readonly Regex TrailingDigits = new(@"^(.*?[a-z_])(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>Base names of per-main tables: "accounthead" when both "accounthead" and "accounthead{n}" exist.</summary>
        private static HashSet<string> Templates(Meta m)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in m.Tables.Keys)
            {
                if (t.StartsWith("temp", StringComparison.OrdinalIgnoreCase) || t.StartsWith("tbltemp", StringComparison.OrdinalIgnoreCase)) continue;
                var mt = TrailingDigits.Match(t);
                if (mt.Success && m.Has(mt.Groups[1].Value)) set.Add(mt.Groups[1].Value);
            }
            return set;
        }

        private static string Q(string ident) => PgTableInfo.Quote(ident);

        // ------------------------------------------------------------------
        //  Run
        // ------------------------------------------------------------------

        public void Run(IList<PgBranchMapping> maps, PgBranchOptions opts, CancellationToken ct = default)
        {
            if (maps.Count == 0) throw new InvalidOperationException("No branch mappings.");

            // A target main branch may only be fed from one source main branch (keys are preserved per main).
            foreach (var g in maps.GroupBy(x => x.TgtMainId))
                if (g.Select(x => x.SrcMainId).Distinct().Count() > 1)
                    throw new InvalidOperationException($"Online main branch {g.Key} is mapped from more than one offline main branch. Map one offline main branch per online main branch.");
            if (maps.Select(x => x.TgtBranchId).Distinct().Count() != maps.Count)
                throw new InvalidOperationException("The same online branch is used more than once.");
            if (maps.Select(x => x.SrcBranchId).Distinct().Count() != maps.Count)
                throw new InvalidOperationException("The same offline branch is mapped more than once.");

            using var src = new NpgsqlConnection(_srcConnStr);
            using var tgt = new NpgsqlConnection(_tgtConnStr);
            src.Open();
            tgt.Open();

            using var srcTx = src.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
            using var tx = tgt.BeginTransaction();

            string current = "(setup)";
            string? currentSql = null;

            try
            {
                Progress("Reading table definitions...", 1);
                var sm = LoadMeta(src);
                var tm = LoadMeta(tgt);
                var templates = Templates(sm);

                // ---------- validation ----------
                current = "(validation)";
                var problems = new List<string>();
                foreach (var g in maps.GroupBy(x => (x.SrcMainId, x.TgtMainId)))
                {
                    var tgtBranches = g.Select(x => x.TgtBranchId).ToList();
                    foreach (var baseName in templates.OrderBy(x => x))
                    {
                        string sT = baseName + g.Key.SrcMainId, tT = baseName + g.Key.TgtMainId;
                        if (!sm.Has(sT)) continue;
                        long srcRows = Scalar<long>(src, srcTx, $"SELECT count(*) FROM {Q(sT)}");
                        if (!tm.Has(tT))
                        {
                            if (srcRows > 0)
                                problems.Add($"Online table '{tT}' does not exist (offline '{sT}' has {srcRows:N0} rows). Create the main branch in the online application first.");
                            continue;
                        }
                        string? bc = BranchColumn(tm, tT);
                        if (bc != null)
                        {
                            long other = Scalar<long>(tgt, tx, $"SELECT count(*) FROM {Q(tT)} WHERE {Q(bc)} NOT IN ({string.Join(",", tgtBranches)})");
                            if (other > 0 && srcRows > 0)
                                problems.Add($"Online table '{tT}' already holds {other:N0} rows of other branches; keys of '{sT}' could collide. Use an online main branch that has no data yet.");
                        }
                    }
                }
                if (problems.Count > 0)
                    throw new InvalidOperationException("Migration cannot start:" + Environment.NewLine + string.Join(Environment.NewLine, problems.Select(p => " - " + p)));

                // ---------- setup ----------
                current = "(setup)";
                if (opts.DisableTriggers)
                {
                    currentSql = "SET LOCAL session_replication_role = replica";
                    try { Exec(tgt, tx, currentSql); }
                    catch (PostgresException pex) when (pex.SqlState == "42501") { Progress("No permission to disable triggers - continuing.", 2); }
                }
                Exec(tgt, tx, "CREATE TEMP TABLE pgmig_idmap (tbl text NOT NULL, oldid bigint NOT NULL, newid bigint NOT NULL) ON COMMIT DROP");
                Exec(tgt, tx, "CREATE INDEX ON pgmig_idmap (tbl, oldid)");

                // branch map
                foreach (var mp in maps)
                    Exec(tgt, tx, $"INSERT INTO pgmig_idmap VALUES ('branch', {mp.SrcBranchId}, {mp.TgtBranchId})");

                // tax map by (percent, type)
                if (sm.Has("tax") && tm.Has("tax"))
                {
                    current = "tax";
                    StageCopy(src, srcTx, tgt, tx, sm, tm, "tax", "tax", "", null, "stage_tax", out currentSql);
                    string typeJoin = tm.HasCol("tax", "taxtype") && sm.HasCol("tax", "taxtype") ? " AND COALESCE(t.taxtype,'') = COALESCE(s.taxtype,'')" : "";
                    currentSql = $"INSERT INTO pgmig_idmap SELECT 'tax', s.taxid, t.taxid FROM stage_tax s JOIN tax t ON t.taxpercent = s.taxpercent{typeJoin} WHERE s.taxid <> t.taxid";
                    Exec(tgt, tx, currentSql);
                    Exec(tgt, tx, "DROP TABLE stage_tax");
                }

                // classify shared tables (exist in both, unsuffixed, not template)
                var mergeNames = new HashSet<string>(MergeTables.Select(x => x.Table), StringComparer.OrdinalIgnoreCase);
                var shared = sm.Tables.Keys
                    .Where(t => tm.Has(t) && !templates.Contains(t) && !TrailingDigits.IsMatch(t)
                                && !NeverCopy.Contains(t) && !mergeNames.Contains(t)
                                && !t.StartsWith("temp", StringComparison.OrdinalIgnoreCase) && !t.StartsWith("tbltemp", StringComparison.OrdinalIgnoreCase)
                                && (sm.HasCol(t, "branchid") || sm.HasCol(t, "mainbranchid")))
                    .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var sharedPerBranch = shared.Where(t => sm.HasCol(t, "branchid")).ToList();
                var sharedPerMain = shared.Where(t => !sm.HasCol(t, "branchid")).ToList();

                // work list for progress
                int totalUnits = maps.Count * (templates.Count + sharedPerBranch.Count) + maps.Select(x => x.TgtMainId).Distinct().Count() * sharedPerMain.Count;
                int unit = 0;
                int Pct() => 3 + (int)((double)unit / Math.Max(1, totalUnits) * 80);

                var copied = new List<(string Table, long TgtMain, long TgtBranch, string? BranchCol)>();

                // ---------- per main-branch group ----------
                foreach (var g in maps.GroupBy(x => (x.SrcMainId, x.TgtMainId)))
                {
                    long sM = g.Key.SrcMainId, tM = g.Key.TgtMainId;
                    ct.ThrowIfCancellationRequested();

                    // access level map by name
                    if (sm.Has("accesslevel") && tm.Has("accesslevel"))
                    {
                        current = "accesslevel";
                        StageCopy(src, srcTx, tgt, tx, sm, tm, "accesslevel", "accesslevel", $"WHERE mainbranchid = {sM}", null, "stage_acc", out currentSql);
                        currentSql = $"INSERT INTO pgmig_idmap SELECT 'accesslevel', s.accessid, t.accessid FROM stage_acc s JOIN accesslevel t ON lower(t.accessname) = lower(s.accessname) AND t.mainbranchid = {tM} WHERE s.accessid <> t.accessid";
                        Exec(tgt, tx, currentSql);
                        Exec(tgt, tx, "DROP TABLE stage_acc");
                    }

                    // per-main shared tables (mainbranchid only)
                    foreach (var t in sharedPerMain)
                    {
                        ct.ThrowIfCancellationRequested();
                        current = t; unit++;
                        Progress($"[{unit}/{totalUnits}] {t} (main {sM} -> {tM})", Pct());
                        currentSql = $"DELETE FROM {Q(t)} WHERE mainbranchid = {tM}";
                        Exec(tgt, tx, currentSql);
                        long n = CopyNewIds(src, srcTx, tgt, tx, sm, tm, t, t, $"WHERE mainbranchid = {sM}",
                            new Dictionary<string, string> { ["mainbranchid"] = tM.ToString() }, t, out currentSql);
                        copied.Add((t, tM, -1, null));
                        Done(t, n, "Done (new ids)");
                    }

                    // per-main settings merge
                    if (opts.MergeSettings)
                        foreach (var mt in MergeTables.Where(x => x.PerMainOnly))
                        {
                            if (!sm.Has(mt.Table) || !tm.Has(mt.Table)) continue;
                            current = mt.Table;
                            long n = MergeByKey(src, srcTx, tgt, tx, sm, tm, mt.Table, mt.Keys, mt.Values,
                                $"WHERE mainbranchid = {sM}", $"t.mainbranchid = {tM}", out currentSql);
                            Done(mt.Table, n, "Merged by name");
                        }

                    // ---------- per branch ----------
                    foreach (var mp in g)
                    {
                        long sB = mp.SrcBranchId, tB = mp.TgtBranchId;
                        var ov = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["branchid"] = tB.ToString(), ["mainbranchid"] = tM.ToString() };

                        // shared per-branch tables: delete, then copy with new ids
                        foreach (var t in sharedPerBranch)
                        {
                            ct.ThrowIfCancellationRequested();
                            current = t; unit++;
                            Progress($"[{unit}/{totalUnits}] {t} ({mp.Display})", Pct());
                            string tw = tm.HasCol(t, "mainbranchid") ? $"branchid = {tB} AND mainbranchid = {tM}" : $"branchid = {tB}";
                            string sw = sm.HasCol(t, "mainbranchid") ? $"WHERE branchid = {sB} AND mainbranchid = {sM}" : $"WHERE branchid = {sB}";
                            currentSql = $"DELETE FROM {Q(t)} WHERE {tw}";
                            Exec(tgt, tx, currentSql);
                            long n = CopyNewIds(src, srcTx, tgt, tx, sm, tm, t, t, sw, ov, t, out currentSql);
                            copied.Add((t, tM, tB, "branchid"));
                            if (n > 0) Done(t, n, "Done (new ids)");
                        }

                        // per-main tables: delete branch rows, copy with keys preserved
                        foreach (var baseName in templates.OrderBy(x => x))
                        {
                            ct.ThrowIfCancellationRequested();
                            string sT = baseName + sM, tT = baseName + tM;
                            current = sT; unit++;
                            if (!sm.Has(sT) || !tm.Has(tT)) continue;
                            Progress($"[{unit}/{totalUnits}] {sT} -> {tT} ({mp.Display})", Pct());

                            string? sbc = BranchColumn(sm, sT), tbc = BranchColumn(tm, tT);
                            var ov2 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["mainbranchid"] = tM.ToString() };
                            string sw;
                            if (sbc != null) { sw = $"WHERE {Q(sbc)} = {sB}"; ov2[sbc] = tB.ToString(); }
                            else sw = $"WHERE mainbranchid = {sM}";
                            if (tbc != null) { currentSql = $"DELETE FROM {Q(tT)} WHERE {Q(tbc)} = {tB}"; }
                            else { currentSql = $"DELETE FROM {Q(tT)} WHERE mainbranchid = {tM}"; }
                            Exec(tgt, tx, currentSql);

                            long n = CopyPreserveKeys(src, srcTx, tgt, tx, sm, tm, sT, tT, sw, ov2, out currentSql);
                            copied.Add((tT, tM, tB, tbc));
                            if (n > 0) Done($"{sT} -> {tT}", n, "Done (keys preserved)");
                        }

                        // per-branch settings merge
                        if (opts.MergeSettings)
                            foreach (var mt in MergeTables.Where(x => !x.PerMainOnly))
                            {
                                if (!sm.Has(mt.Table) || !tm.Has(mt.Table)) continue;
                                current = mt.Table;
                                long n = MergeByKey(src, srcTx, tgt, tx, sm, tm, mt.Table, mt.Keys, mt.Values,
                                    $"WHERE branchid = {sB}", $"t.branchid = {tB} AND t.mainbranchid = {tM}", out currentSql);
                                Done(mt.Table, n, "Merged by name");
                            }

                        // branch profile
                        if (opts.CopyBranchProfile && sm.Has("branch") && tm.Has("branch"))
                        {
                            current = "branch";
                            var keyCols = new[] { "branchid" };
                            var valCols = sm.Tables["branch"].Select(c => c.Name)
                                .Where(c => tm.HasCol("branch", c))
                                .Where(c => !c.Equals("branchid", StringComparison.OrdinalIgnoreCase) && !c.Equals("mainbranchid", StringComparison.OrdinalIgnoreCase)
                                         && !c.Equals("tempid", StringComparison.OrdinalIgnoreCase)
                                         && !Regex.IsMatch(c, "user|pwd|password|code|apikey|token", RegexOptions.IgnoreCase))
                                .ToArray();
                            long n = MergeByKey(src, srcTx, tgt, tx, sm, tm, "branch", keyCols, valCols,
                                $"WHERE branchid = {sB}", $"t.branchid = {tB} AND t.mainbranchid = {tM}",
                                out currentSql, keyOverride: (sB, tB));
                            Done("branch", n, "Profile copied");
                        }
                    }
                }

                // ---------- re-link references to shared tables ----------
                current = "(re-link)";
                Progress("Re-linking references to shared tables...", 85);
                int remaps = 0;
                var distinctCopied = copied.Distinct().ToList();
                int ri = 0;
                foreach (var c in distinctCopied)
                {
                    ct.ThrowIfCancellationRequested();
                    ri++;
                    if (ri % 25 == 0) Progress($"Re-linking {ri}/{distinctCopied.Count}: {c.Table}", 85 + (int)((double)ri / distinctCopied.Count * 8));
                    var cols = tm.Tables[c.Table];
                    var filter = new List<string>();
                    if (c.TgtBranch >= 0 && c.BranchCol != null) filter.Add($"c.{Q(c.BranchCol)} = {c.TgtBranch}");
                    if (tm.HasCol(c.Table, "mainbranchid")) filter.Add($"c.mainbranchid = {c.TgtMain}");
                    string where = filter.Count > 0 ? " AND " + string.Join(" AND ", filter) : "";

                    string baseName = TrailingDigits.Match(c.Table) is { Success: true } mm && templates.Contains(mm.Groups[1].Value) ? mm.Groups[1].Value : c.Table;
                    var pk = tm.KeyCol(c.Table);

                    foreach (var rule in RemapRules)
                        foreach (var colName in rule.Columns)
                        {
                            var col = cols.FirstOrDefault(x => x.Name.Equals(colName, StringComparison.OrdinalIgnoreCase));
                            if (col == null) continue;
                            if (col.IsIdentity || col.IsGenerated || col.IsSerial) continue;   // the table's own key, never a reference
                            if (baseName.Equals(rule.Parent, StringComparison.OrdinalIgnoreCase) && pk != null && pk.Name.Equals(col.Name, StringComparison.OrdinalIgnoreCase)) continue;
                            bool text = col.DataType.Contains("char") || col.DataType == "text";
                            bool number = col.DataType is "bigint" or "integer" or "smallint" or "numeric" or "double precision" or "real";
                            if (!text && !number) continue;   // arrays, json, etc. are not key references
                            current = c.Table;
                            currentSql = text
                                ? $"UPDATE {Q(c.Table)} c SET {Q(col.Name)} = m.newid::text FROM pgmig_idmap m WHERE m.tbl = '{rule.Parent}' AND c.{Q(col.Name)} ~ '^[0-9]+$' AND m.oldid = c.{Q(col.Name)}::bigint{where}"
                                : $"UPDATE {Q(c.Table)} c SET {Q(col.Name)} = m.newid FROM pgmig_idmap m WHERE m.tbl = '{rule.Parent}' AND m.oldid = c.{Q(col.Name)}{where}";
                            remaps += Exec(tgt, tx, currentSql);
                        }
                }
                Progress($"References re-linked: {remaps:N0} values updated.", 93);

                // ---------- sequences of per-main tables ----------
                current = "(sequences)";
                Progress("Resetting sequences...", 94);
                foreach (var tT in distinctCopied.Select(x => x.Table).Distinct())
                {
                    foreach (var col in tm.Tables[tT].Where(x => x.IsIdentity || x.IsSerial))
                    {
                        currentSql = $"SELECT pg_get_serial_sequence('{Q(tT).Replace("'", "''")}', '{col.Name.Replace("'", "''")}')";
                        var seq = ScalarObj(tgt, tx, currentSql) as string;
                        if (string.IsNullOrEmpty(seq)) continue;
                        currentSql = $"SELECT setval('{seq.Replace("'", "''")}', GREATEST(COALESCE(MAX({Q(col.Name)}), 0), 1), COALESCE(MAX({Q(col.Name)}), 0) > 0) FROM {Q(tT)}";
                        ScalarObj(tgt, tx, currentSql);
                    }
                }

                if (opts.DisableTriggers)
                {
                    current = "(setup)";
                    try { Exec(tgt, tx, "SET LOCAL session_replication_role = DEFAULT"); } catch (PostgresException) { }
                }

                ct.ThrowIfCancellationRequested();
                Progress("Committing...", 98);
                tx.Commit();
                srcTx.Rollback();
                Progress("Completed.", 100);
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                if (ex is OperationCanceledException) throw;
                throw new MigrationException(
                    $"Branch migration failed at '{current}': {Root(ex)}", current, ex,
                    pgTableName: current, failingQuery: currentSql, functionName: nameof(PgBranchMigrator) + ".Run");
            }
        }

        // ------------------------------------------------------------------
        //  Copy primitives
        // ------------------------------------------------------------------

        /// <summary>Column used to scope a per-main table to one branch: branchid, else frombranch.</summary>
        private static string? BranchColumn(Meta m, string table)
        {
            if (m.HasCol(table, "branchid")) return "branchid";
            if (m.HasCol(table, "frombranch")) return "frombranch";
            return null;
        }

        private static List<string> CommonColumns(Meta sm, Meta tm, string sT, string tT)
        {
            var tgtCols = tm.Tables[tT].Where(c => !c.IsGenerated).Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            return sm.Tables[sT].Select(c => c.Name).Where(tgtCols.Contains).ToList();
        }

        private static string SelectList(IEnumerable<string> cols, IDictionary<string, string>? overrides)
        {
            return string.Join(", ", cols.Select(c =>
                overrides != null && overrides.TryGetValue(c, out var v) ? $"{v} AS {Q(c)}" : Q(c)));
        }

        /// <summary>Streams source rows straight into the target table, keys preserved.</summary>
        private static long CopyPreserveKeys(NpgsqlConnection src, NpgsqlTransaction srcTx, NpgsqlConnection tgt, NpgsqlTransaction tx,
            Meta sm, Meta tm, string sT, string tT, string srcWhere, IDictionary<string, string> overrides, out string lastSql)
        {
            var cols = CommonColumns(sm, tm, sT, tT);
            if (cols.Count == 0) throw new InvalidOperationException($"'{sT}' and '{tT}' have no columns in common.");
            string exportSql = $"COPY (SELECT {SelectList(cols, overrides)} FROM {Q(sT)} {srcWhere}) TO STDOUT (FORMAT TEXT)";
            string importSql = $"COPY {Q(tT)} ({string.Join(", ", cols.Select(Q))}) FROM STDIN (FORMAT TEXT)";
            lastSql = importSql;
            return Stream(src, exportSql, tgt, importSql);
        }

        /// <summary>Copies source rows into a temp staging table on the target (same column names, no constraints).</summary>
        private static long StageCopy(NpgsqlConnection src, NpgsqlTransaction srcTx, NpgsqlConnection tgt, NpgsqlTransaction tx,
            Meta sm, Meta tm, string sT, string tT, string srcWhere, IDictionary<string, string>? overrides, string stage, out string lastSql)
        {
            var cols = CommonColumns(sm, tm, sT, tT);
            if (cols.Count == 0) throw new InvalidOperationException($"'{sT}' and '{tT}' have no columns in common.");
            lastSql = $"CREATE TEMP TABLE {stage} ON COMMIT DROP AS SELECT {string.Join(", ", cols.Select(Q))} FROM {Q(tT)} WHERE false";
            Exec(tgt, tx, lastSql);
            string exportSql = $"COPY (SELECT {SelectList(cols, overrides)} FROM {Q(sT)} {srcWhere}) TO STDOUT (FORMAT TEXT)";
            string importSql = $"COPY {stage} ({string.Join(", ", cols.Select(Q))}) FROM STDIN (FORMAT TEXT)";
            lastSql = importSql;
            return Stream(src, exportSql, tgt, importSql);
        }

        /// <summary>
        /// Copies rows giving each a NEW key from the target sequence, records old->new in pgmig_idmap
        /// under <paramref name="mapKey"/>, and stores the old key in tempid when that column exists.
        /// </summary>
        private static long CopyNewIds(NpgsqlConnection src, NpgsqlTransaction srcTx, NpgsqlConnection tgt, NpgsqlTransaction tx,
            Meta sm, Meta tm, string sT, string tT, string srcWhere, IDictionary<string, string> overrides, string mapKey, out string lastSql)
        {
            string stage = "stage_" + Regex.Replace(tT, "[^a-z0-9_]", "_");
            long n = StageCopy(src, srcTx, tgt, tx, sm, tm, sT, tT, srcWhere, overrides, stage, out lastSql);
            var cols = CommonColumns(sm, tm, sT, tT);
            var key = tm.KeyCol(tT);

            if (n > 0 && key != null && cols.Contains(key.Name, StringComparer.OrdinalIgnoreCase))
            {
                lastSql = $"SELECT pg_get_serial_sequence('{Q(tT).Replace("'", "''")}', '{key.Name.Replace("'", "''")}')";
                var seq = ScalarObj(tgt, tx, lastSql) as string;
                if (!string.IsNullOrEmpty(seq))
                {
                    Exec(tgt, tx, $"ALTER TABLE {stage} ADD COLUMN _newid bigint");
                    lastSql = $"UPDATE {stage} SET _newid = nextval('{seq.Replace("'", "''")}')";
                    Exec(tgt, tx, lastSql);
                    lastSql = $"INSERT INTO pgmig_idmap SELECT '{mapKey}', {Q(key.Name)}, _newid FROM {stage} WHERE {Q(key.Name)} IS NOT NULL";
                    Exec(tgt, tx, lastSql);

                    bool hasTemp = cols.Contains("tempid", StringComparer.OrdinalIgnoreCase);
                    string selList = string.Join(", ", cols.Select(c =>
                        c.Equals(key.Name, StringComparison.OrdinalIgnoreCase) ? "_newid" :
                        hasTemp && c.Equals("tempid", StringComparison.OrdinalIgnoreCase) ? $"COALESCE({Q(key.Name)}, 0)" : Q(c)));
                    lastSql = $"INSERT INTO {Q(tT)} ({string.Join(", ", cols.Select(Q))}) OVERRIDING SYSTEM VALUE SELECT {selList} FROM {stage}";
                    Exec(tgt, tx, lastSql);
                    Exec(tgt, tx, $"DROP TABLE {stage}");
                    return n;
                }
            }

            // no generated key: plain insert
            if (n > 0)
            {
                var insCols = key != null ? cols.Where(c => !c.Equals(key.Name, StringComparison.OrdinalIgnoreCase)).ToList() : cols;
                lastSql = $"INSERT INTO {Q(tT)} ({string.Join(", ", insCols.Select(Q))}) SELECT {string.Join(", ", insCols.Select(Q))} FROM {stage}";
                Exec(tgt, tx, lastSql);
            }
            Exec(tgt, tx, $"DROP TABLE {stage}");
            return n;
        }

        /// <summary>UPDATE target rows from source rows with the same key. Returns rows updated.</summary>
        private static long MergeByKey(NpgsqlConnection src, NpgsqlTransaction srcTx, NpgsqlConnection tgt, NpgsqlTransaction tx,
            Meta sm, Meta tm, string table, string[] keys, string[] values, string srcWhere, string tgtWhere, out string lastSql,
            (long src, long tgt)? keyOverride = null)
        {
            var keysOk = keys.Where(k => sm.HasCol(table, k) && tm.HasCol(table, k)).ToArray();
            var valsOk = values.Where(v => sm.HasCol(table, v) && tm.HasCol(table, v)).ToArray();
            lastSql = null!;
            if (keysOk.Length == 0 || valsOk.Length == 0) return 0;

            string stage = "stage_m_" + Regex.Replace(table, "[^a-z0-9_]", "_");
            var ov = keyOverride.HasValue ? new Dictionary<string, string> { [keysOk[0]] = keyOverride.Value.tgt.ToString() } : null;
            long n = StageCopy(src, srcTx, tgt, tx, sm, tm, table, table, srcWhere, ov, stage, out lastSql);
            if (n == 0) { Exec(tgt, tx, $"DROP TABLE {stage}"); return 0; }

            string set = string.Join(", ", valsOk.Select(v => $"{Q(v)} = s.{Q(v)}"));
            string on = string.Join(" AND ", keysOk.Select(k => $"t.{Q(k)} IS NOT DISTINCT FROM s.{Q(k)}"));
            lastSql = $"UPDATE {Q(table)} t SET {set} FROM {stage} s WHERE {on} AND {tgtWhere}";
            int updated = Exec(tgt, tx, lastSql);
            Exec(tgt, tx, $"DROP TABLE {stage}");
            return updated;
        }

        private static long Stream(NpgsqlConnection src, string exportSql, NpgsqlConnection tgt, string importSql)
        {
            long rows = 0;
            var buffer = new char[64 * 1024];
            using var reader = src.BeginTextExport(exportSql);
            using (var writer = tgt.BeginTextImport(importSql))
            {
                int read;
                while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    writer.Write(buffer, 0, read);
                    for (int i = 0; i < read; i++) if (buffer[i] == '\n') rows++;
                }
            }
            return rows;
        }

        private static int Exec(NpgsqlConnection c, NpgsqlTransaction tx, string sql)
        {
            using var cmd = new NpgsqlCommand(sql, c, tx);
            return cmd.ExecuteNonQuery();
        }

        private static T Scalar<T>(NpgsqlConnection c, NpgsqlTransaction tx, string sql) =>
            (T)Convert.ChangeType(ScalarObj(c, tx, sql)!, typeof(T));

        private static object? ScalarObj(NpgsqlConnection c, NpgsqlTransaction tx, string sql)
        {
            using var cmd = new NpgsqlCommand(sql, c, tx);
            var v = cmd.ExecuteScalar();
            return v is DBNull ? null : v;
        }

        private static string Root(Exception ex)
        {
            while (ex.InnerException != null) ex = ex.InnerException;
            return ex.Message;
        }
    }
}
