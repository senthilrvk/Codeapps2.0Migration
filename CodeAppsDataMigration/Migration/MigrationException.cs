using System;

namespace CodeAppsDataMigration.Migration
{
    /// <summary>
    /// Migration failure that carries WHICH table (and, when known, which column /
    /// row / failing query) as structured fields so the UI can show them directly
    /// instead of parsing them out of a message string.
    /// </summary>
    public class MigrationException : Exception
    {
        public string TableName { get; }
        public string? PgTableName { get; }
        public string? ColumnName { get; }
        public int? RowNumber { get; }
        public string? FailingQuery { get; }
        public string? FunctionName { get; }

        public MigrationException(
            string message,
            string tableName,
            Exception inner,
            string? pgTableName = null,
            string? columnName = null,
            int? rowNumber = null,
            string? failingQuery = null,
            string? functionName = null)
            : base(message, inner)
        {
            TableName = tableName;
            PgTableName = pgTableName;
            ColumnName = columnName;
            RowNumber = rowNumber;
            FailingQuery = failingQuery;
            FunctionName = functionName;
        }

        /// <summary>
        /// Walks an exception chain and returns the first MigrationException found,
        /// or null. Lets the UI recover the table/column regardless of how many
        /// times the error was re-wrapped on the way up.
        /// </summary>
        public static MigrationException? Find(Exception? ex)
        {
            for (Exception? cur = ex; cur != null; cur = cur.InnerException)
                if (cur is MigrationException me)
                    return me;
            return null;
        }
    }
}
