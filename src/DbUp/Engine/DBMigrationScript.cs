using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbUp.Engine
{
    /// <summary>
    /// 
    /// </summary>
    public class DBMigrationScript : SqlScript, IComparable<DBMigrationScript>
    {
        /// <summary>
        /// 
        /// </summary>
        public long VersionId { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string UpScript { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string DownScript { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public MigrationTypes MigrationType { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public long? DependentSchemaVersionId { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public MigrationPerformTypes MigrationPerformType { get; private set; }

        private readonly string _sortingValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="name"></param>
        /// <param name="downScript"></param>
        /// <param name="dbMigrationType"></param>
        /// <param name="upScript"></param>
        /// <param name="depenedSchemaVersionId"></param>
        /// <param name="migrationPerformType"></param>
        public DBMigrationScript(long versionId, string name, string upScript, string downScript, MigrationTypes migrationType,
            long? depenedSchemaVersionId, MigrationPerformTypes migrationPerformType)
            : base(name, "")
        {
            this.VersionId = versionId;
            this.UpScript = upScript;
            this.DownScript = downScript;
            this.MigrationType = migrationType;
            this.DependentSchemaVersionId = depenedSchemaVersionId;
            this.MigrationPerformType = migrationPerformType;

            _sortingValue = depenedSchemaVersionId.HasValue
                ? string.Format("{0}{1}", depenedSchemaVersionId, versionId)
                : string.Format("{0}00000000000000", versionId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(DBMigrationScript other)
        {
            return string.Compare(_sortingValue, other._sortingValue, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum MigrationTypes
    {
        /// <summary>
        /// Schema migration
        /// </summary>
        Schema,
        /// <summary>
        /// Data Migration
        /// </summary>
        Data
    }

    /// <summary>
    /// 
    /// </summary>
    public enum MigrationPerformTypes
    {
        /// <summary>
        /// 
        /// </summary>
        Up,
        /// <summary>
        /// 
        /// </summary>
        Down
    }
}
