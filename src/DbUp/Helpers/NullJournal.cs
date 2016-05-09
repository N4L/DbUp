using System;
using System.Collections.Generic;
using DbUp.Engine;
using DbUp.Entities;

namespace DbUp.Helpers
{
    /// <summary>
    /// Enables multiple executions of idempotent scripts.
    /// </summary>
    public class NullJournal : IJournal
    {
        /// <summary>
        /// Returns an empty array of length 0
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DBMigration> GetMigratedDBVersions()
        {
            return new List<DBMigration>();
        }

        /// <summary>
        /// Does not store the script, simply returns
        /// </summary>
        /// <param name="script"></param>
        public void StoreExecutedScript(SqlScript script)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="migrationScript"></param>
        public void StoreExecutedMigrationScript(DBMigrationScript migrationScript)
        {
        }

        public bool HasDBVersionMigrated(long versionId, MigrationTypes dbMigrationType)
        {
            throw new NotImplementedException();
        }
    }
}
