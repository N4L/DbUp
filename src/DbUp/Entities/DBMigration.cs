using System;
using DbUp.Engine;

namespace DbUp.Entities
{
    public class DBMigration
    {
        public int Id { get; set; }
        public long VersionId { get; set; }
        public string ScriptName { get; set; }
        public DateTime CreatedOn { get; set; }
        public MigrationTypes MigrationType { get; set; }
    }
}
