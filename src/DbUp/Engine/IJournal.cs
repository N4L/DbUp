using System.Collections.Generic;
using DbUp.Entities;

namespace DbUp.Engine
{
    /// <summary>
    /// This interface is provided to allow different projects to store version information differently.
    /// </summary>
    public interface IJournal
    {
        /// <summary>
        /// Recalls the version number of the database.
        /// </summary>
        /// <returns></returns>
        IEnumerable<DBMigration> GetMigratedDBVersions();

        /// <summary>
        /// Records an upgrade script for a database.
        /// </summary>
        /// <param name="script">The script.</param>
        void StoreExecutedScript(SqlScript script);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="migrationScript"></param>
        void StoreExecutedMigrationScript(DBMigrationScript migrationScript);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="dbMigrationType"></param>
        /// <returns></returns>
        bool HasDBVersionMigrated(long versionId, MigrationTypes dbMigrationType);

    }
}