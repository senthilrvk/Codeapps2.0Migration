namespace CodeAppsDataMigration.Migration
{
    public class FkRelation
    {
        
        public string ChildTable { get; set; }

        public string ChildColumn { get; set; }

        public string ParentTable { get; set; }

       
        public string ParentPk { get; set; }

       
        public string ParentTempId { get; set; } = "tempid";
    }
}
