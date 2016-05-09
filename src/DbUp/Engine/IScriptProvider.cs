using System;
using System.Collections.Generic;
using DbUp.Engine.Transactions;
using DbUp.Migrations;

namespace DbUp.Engine
{
    /// <summary>
    /// Provides scripts to be executed.
    /// </summary>
    public interface IScriptProvider
    {
        /// <summary>
        /// Gets all scripts that should be executed.
        /// </summary>
        IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager);

        IEnumerable<DBMigration> GetDBMigrations();
    }
}