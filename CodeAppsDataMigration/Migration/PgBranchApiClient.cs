using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using Npgsql;

namespace CodeAppsDataMigration.Migration
{
    /// <summary>
    /// Builds the CreateMainBranch / CreateBranch request bodies for the Offline -> Online PG flow
    /// from the OFFLINE PostgreSQL mainbranch / branch rows.
    ///
    /// Uses exactly the same methods as the SQL Server -> PostgreSQL "Branch Details" page
    /// (BranchDetailForm): the same field maps (_branchFieldMap / _subBranchFieldMap), the same
    /// defaults per PG type, the same value conversion (ConvertToPgValue) and the same PG-only
    /// sub branch defaults. The only difference is where the raw values come from: the offline
    /// PG row instead of the SQL Server textboxes.
    /// </summary>
    public static class PgBranchApiClient
    {
        /// <summary>Offline main branch with its branches, for the picker.</summary>
        public sealed class OfflineMain
        {
            public long MainBranchId { get; set; }
            public string Name { get; set; } = "";
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
            public string BusinessType { get; set; } = "";
            public List<(long BranchId, string Name)> Branches { get; set; } = new();
            public override string ToString() => $"{Name} [{MainBranchId}]  ({Branches.Count} branch{(Branches.Count == 1 ? "" : "es")})";
        }

        public static List<OfflineMain> LoadOfflineMains(string srcConnStr)
        {
            var list = new List<OfflineMain>();
            using var c = new NpgsqlConnection(srcConnStr);
            c.Open();
            using (var cmd = new NpgsqlCommand("SELECT mainbranchid, mainbranchname, COALESCE(mainusername,''), COALESCE(mainpwd,''), COALESCE(businesstype,'') FROM mainbranch ORDER BY mainbranchid", c))
            using (var rd = cmd.ExecuteReader())
                while (rd.Read())
                    list.Add(new OfflineMain
                    {
                        MainBranchId = rd.GetInt64(0), Name = rd.IsDBNull(1) ? "" : rd.GetString(1),
                        Username = rd.GetString(2), Password = rd.GetString(3), BusinessType = rd.GetString(4)
                    });
            using (var cmd = new NpgsqlCommand("SELECT branchid, mainbranchid, branchname FROM branch ORDER BY branchid", c))
            using (var rd = cmd.ExecuteReader())
                while (rd.Read())
                {
                    long mid = rd.GetInt64(1);
                    var m = list.FirstOrDefault(x => x.MainBranchId == mid);
                    m?.Branches.Add((rd.GetInt64(0), rd.IsDBNull(2) ? "" : rd.GetString(2)));
                }
            return list;
        }

        // ------------------------------------------------------------------
        //  Payloads (same shape as BranchDetailForm.BuildMainPayload / BuildSubPayload)
        // ------------------------------------------------------------------

        /// <summary>
        /// JSON for CreateMainBranch: the fields of BranchDetailForm._branchFieldMap read from the
        /// offline mainbranch row, tempid = offline mainbranchid, businesstype, login fields.
        /// </summary>
        public static (string json, StringBuilder debug) BuildMainPayload(string srcConnStr, long srcMainId, string username, string password, string? businessType)
        {
            var row = ReadRow(srcConnStr, "mainbranch", "mainbranchid", srcMainId)
                      ?? throw new InvalidOperationException($"Offline main branch {srcMainId} not found.");

            var jsonData = new Dictionary<string, object?>
            {
                ["tempid"] = srcMainId,
                ["businesstype"] = ResolveBusinessType(row, businessType)
            };
            var debug = BuildPayloadFromMap(BranchDetailForm._branchFieldMap, row, jsonData, "Main");
            jsonData["mainusername"] = username.Trim();
            jsonData["mainpwd"] = password;   // do NOT trim password; the API encrypts it

            if (string.IsNullOrWhiteSpace(jsonData["mainbranchname"] as string))
                throw new InvalidOperationException("Offline main branch has no name.");

            var json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
            return (json, debug);
        }

        /// <summary>
        /// Business type sent to the API: the value typed on the form, else the offline
        /// mainbranch.businesstype, else DISTRIBUTION (same fallback as the SQL Server flow).
        /// </summary>
        private static string ResolveBusinessType(Dictionary<string, string?> row, string? overrideValue)
        {
            if (!string.IsNullOrWhiteSpace(overrideValue)) return overrideValue.Trim();
            if (row.TryGetValue("businesstype", out var bt) && !string.IsNullOrWhiteSpace(bt)) return bt.Trim();
            return "DISTRIBUTION";
        }

        /// <summary>
        /// JSON for CreateBranch: the fields of BranchDetailForm._subBranchFieldMap read from the
        /// offline branch row, the PG-only defaults, tempid = offline branchid,
        /// mainbranchid = the NEW online main branch id, login fields.
        /// </summary>
        public static (string json, StringBuilder debug) BuildSubPayload(string srcConnStr, long srcBranchId, long newMainId, string username, string password)
        {
            var row = ReadRow(srcConnStr, "branch", "branchid", srcBranchId)
                      ?? throw new InvalidOperationException($"Offline branch {srcBranchId} not found.");

            var jsonData = new Dictionary<string, object?>();
            var debug = BuildPayloadFromMap(BranchDetailForm._subBranchFieldMap, row, jsonData, "Sub");

            // The online BranchHandler AES-encrypts these on save; the offline row already holds
            // them encrypted, so send the decrypted value or they would be encrypted twice.
            foreach (var f in new[] { "branchorderpwd", "branchmailpwd", "billpassword" })
                if (jsonData.TryGetValue(f, out var v) && v is string s && s.Length > 0)
                {
                    jsonData[f] = TryDecrypt(s);
                    debug.AppendLine($"[DEC ] '{f}' decrypted before send");
                }

            // PG-only fields with defaults if not already present (same list as the SQL Server flow)
            foreach (var (pgName, value) in BranchDetailForm._subBranchPgOnlyDefaults)
                if (!jsonData.ContainsKey(pgName))
                {
                    jsonData[pgName] = value;
                    debug.AppendLine($"[ADD ] PG-only '{pgName}' = {value}");
                }
            if (!jsonData.ContainsKey("tempid"))
            {
                jsonData["tempid"] = srcBranchId;
                debug.AppendLine($"[ADD ] PG-only 'tempid' = {srcBranchId}");
            }

            // Runtime fields
            jsonData["mainbranchid"] = newMainId;
            jsonData["username"]     = string.IsNullOrEmpty(username) ? "" : username.Trim();
            jsonData["pwd"]          = password ?? "";   // do NOT trim password; the API encrypts it

            var json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
            return (json, debug);
        }

        /// <summary>
        /// Same as BranchDetailForm.BuildPayloadFromMap: for every mapped field take the raw text,
        /// blank -> default for the PG type, else ConvertToPgValue; a failed conversion -> default.
        /// The raw text is the offline PG column with the mapped PG name.
        /// </summary>
        private static StringBuilder BuildPayloadFromMap(
            Dictionary<string, (string PgName, string PgType)> activeMap,
            Dictionary<string, string?> row,
            Dictionary<string, object?> jsonData,
            string mode)
        {
            var debug = new StringBuilder();
            debug.AppendLine($"[INFO] mode={mode} offline columns = {row.Count}, activeMap count = {activeMap.Count}");
            int mappedCount = 0, defaultedCount = 0, missingCount = 0;

            foreach (var kvp in activeMap)
            {
                var pgName = kvp.Value.PgName;
                var pgType = kvp.Value.PgType;

                string? rawText = null;
                if (row.TryGetValue(pgName, out var text))
                    rawText = text;
                else
                    missingCount++;

                if (string.IsNullOrWhiteSpace(rawText))
                {
                    var def = BranchDetailForm.GetDefaultValue(pgType);
                    jsonData[pgName] = def;
                    debug.AppendLine($"[DEF ] '{pgName}' = {def} (default for {pgType}; raw='{rawText}')");
                    defaultedCount++;
                    continue;
                }

                object? converted;
                try { converted = BranchDetailForm.ConvertToPgValue(pgName, pgType, rawText); }
                catch (Exception ex)
                {
                    var def = BranchDetailForm.GetDefaultValue(pgType);
                    jsonData[pgName] = def;
                    debug.AppendLine($"[DEF ] '{pgName}' = {def} (default for {pgType}; parse failed: {ex.Message})");
                    defaultedCount++;
                    continue;
                }

                if (converted == null)
                {
                    var def = BranchDetailForm.GetDefaultValue(pgType);
                    jsonData[pgName] = def;
                    debug.AppendLine($"[DEF ] '{pgName}' = {def} (cast yielded null from '{rawText}')");
                    defaultedCount++;
                    continue;
                }

                jsonData[pgName] = converted;
                debug.AppendLine($"[MAP ] '{pgName}' = {converted}");
                mappedCount++;
            }

            debug.AppendLine($"[INFO] mapped={mappedCount} defaulted={defaultedCount} missingColumn={missingCount}");
            return debug;
        }

        /// <summary>Decrypts an AES value stored by the application; returns the input unchanged if it is not encrypted.</summary>
        public static string TryDecrypt(string stored)
        {
            if (string.IsNullOrEmpty(stored)) return "";
            try { return Helpers.AesEncryption.Decrypt(stored); }
            catch { return stored; }
        }

        /// <summary>
        /// Reads one offline row as column -> raw text, the way the SQL Server flow sees the
        /// values in its textboxes (NULL -> null, dates as yyyy-MM-dd, numbers invariant).
        /// </summary>
        private static Dictionary<string, string?>? ReadRow(string connStr, string table, string keyCol, long key)
        {
            using var c = new NpgsqlConnection(connStr);
            c.Open();
            using var cmd = new NpgsqlCommand($"SELECT * FROM {PgTableInfo.Quote(table)} WHERE {PgTableInfo.Quote(keyCol)} = @k", c);
            cmd.Parameters.AddWithValue("k", key);
            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return null;

            var d = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < rd.FieldCount; i++)
                d[rd.GetName(i)] = rd.IsDBNull(i) ? null : ToText(rd.GetValue(i));
            return d;
        }

        private static string? ToText(object v) => v switch
        {
            string s => s,
            DateTime dt => dt.ToString("yyyy-MM-dd"),
            DateOnly d => d.ToString("yyyy-MM-dd"),
            byte[] b => Convert.ToBase64String(b),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => v.ToString()
        };
    }
}
