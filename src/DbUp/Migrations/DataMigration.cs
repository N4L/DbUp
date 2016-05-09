using System;

namespace DbUp.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DataMigration : DBMigration
    {
        /// <summary>
        /// Depended schema version id
        /// </summary>
        public abstract long DependentSchemaVersionId { get; }

    }
}
