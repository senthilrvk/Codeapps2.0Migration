using Npgsql;
using NpgsqlTypes;
using System;
using System.Globalization;

namespace CodeAppsDataMigration.Helpers
{
    public static class PgValueWriter
    {
        public static void Write(
            NpgsqlBinaryImporter w,
            object v,
            string pgDataType)
        {
            // ======================================
            // NULL / DBNULL / EMPTY → write default
            // ======================================
            bool isNull = (v == null || v == DBNull.Value);
            string s = isNull ? "" : (v.ToString()?.Trim() ?? "");
            bool isEmpty = string.IsNullOrEmpty(s);

            // ======================================
            // STRING TYPES → default ""
            // ======================================
            if (pgDataType is "text" or "varchar" or "character varying" or "character")
            {
                w.Write(isEmpty ? "" : s, NpgsqlDbType.Text);
                return;
            }

            // ======================================
            // BOOLEAN → default false
            // ======================================
            if (pgDataType == "boolean")
            {
                if (isEmpty)
                {
                    w.Write(false, NpgsqlDbType.Boolean);
                    return;
                }

                bool b =
                    s == "1" ||
                    s.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("y", StringComparison.OrdinalIgnoreCase);

                w.Write(b, NpgsqlDbType.Boolean);
                return;
            }

            // ======================================
            // SMALLINT → default 0
            // ======================================
            if (pgDataType == "smallint")
            {
                if (!isEmpty && short.TryParse(s, out short si))
                    w.Write(si, NpgsqlDbType.Smallint);
                else
                    w.Write((short)0, NpgsqlDbType.Smallint);
                return;
            }

            // ======================================
            // INTEGER → default 0
            // ======================================
            if (pgDataType == "integer")
            {
                if (!isEmpty && int.TryParse(s, out int i))
                    w.Write(i, NpgsqlDbType.Integer);
                else
                    w.Write(0, NpgsqlDbType.Integer);
                return;
            }

            // ======================================
            // BIGINT → default 0
            // ======================================
            if (pgDataType == "bigint")
            {
                if (!isEmpty && long.TryParse(s, out long l))
                    w.Write(l, NpgsqlDbType.Bigint);
                else
                    w.Write(0L, NpgsqlDbType.Bigint);
                return;
            }

            // ======================================
            // REAL (float4) → default 0
            // ======================================
            if (pgDataType == "real")
            {
                if (!isEmpty && float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out float f))
                    w.Write(f, NpgsqlDbType.Real);
                else
                    w.Write(0f, NpgsqlDbType.Real);
                return;
            }

            // ======================================
            // DOUBLE PRECISION (float8) → default 0
            // ======================================
            if (pgDataType == "double precision")
            {
                if (!isEmpty && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double dbl))
                    w.Write(dbl, NpgsqlDbType.Double);
                else
                    w.Write(0d, NpgsqlDbType.Double);
                return;
            }

            // ======================================
            // NUMERIC / DECIMAL → default 0
            // ======================================
            if (pgDataType is "numeric" or "decimal")
            {
                if (!isEmpty && decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal d))
                    w.Write(d, NpgsqlDbType.Numeric);
                else
                    w.Write(0m, NpgsqlDbType.Numeric);
                return;
            }

            // ======================================
            // DATE → default 1900-01-01
            // ======================================
            if (pgDataType == "date")
            {
                if (!isEmpty && DateTime.TryParse(s, out DateTime dt))
                    w.Write(DateOnly.FromDateTime(dt), NpgsqlDbType.Date);
                else
                    w.Write(new DateOnly(1900, 1, 1), NpgsqlDbType.Date);
                return;
            }

            // ======================================
            // TIMESTAMP → default 1900-01-01 00:00:00
            // ======================================
            if (pgDataType.StartsWith("timestamp"))
            {
                if (!isEmpty && DateTime.TryParse(s, out DateTime ts))
                    w.Write(ts, NpgsqlDbType.Timestamp);
                else
                    w.Write(new DateTime(1900, 1, 1), NpgsqlDbType.Timestamp);
                return;
            }

            // ======================================
            // BYTEA (binary) → default empty
            // ======================================
            if (pgDataType == "bytea")
            {
                if (!isNull && v is byte[] bytes)
                    w.Write(bytes, NpgsqlDbType.Bytea);
                else
                    w.Write(Array.Empty<byte>(), NpgsqlDbType.Bytea);
                return;
            }

            // ======================================
            // UUID → default empty guid
            // ======================================
            if (pgDataType == "uuid")
            {
                if (!isEmpty && Guid.TryParse(s, out Guid g))
                    w.Write(g, NpgsqlDbType.Uuid);
                else
                    w.Write(Guid.Empty, NpgsqlDbType.Uuid);
                return;
            }

            // ======================================
            // JSON / JSONB → default "{}"
            // ======================================
            if (pgDataType is "json" or "jsonb")
            {
                w.Write(isEmpty ? "{}" : s, pgDataType == "jsonb" ? NpgsqlDbType.Jsonb : NpgsqlDbType.Json);
                return;
            }

            // ======================================
            // FALLBACK → default ""
            // ======================================
            w.Write(isEmpty ? "" : s, NpgsqlDbType.Text);
        }
    }
}
