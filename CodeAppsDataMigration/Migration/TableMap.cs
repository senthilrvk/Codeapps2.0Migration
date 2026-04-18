namespace CodeAppsDataMigration.Migration
{
    public class TableMap
    {
        public string SqlTable { get; set; }
        public string PgTable { get; set; }
      

        // SQL column → PostgreSQL column
        public (string Sql, string Pg, string PgType)[] Columns { get; set; }

        // PostgreSQL column → fixed value
        public Dictionary<string, object> Constants { get; set; }
            = new Dictionary<string, object>();
          public string condition { get; set; }
    }
}
