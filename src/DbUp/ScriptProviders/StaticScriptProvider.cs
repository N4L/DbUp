using System;
using System.Collections.Generic;
using DbUp.Engine;
using DbUp.Engine.Transactions;
using DbUp.Migrations;

namespace DbUp.ScriptProviders
{
    /// <summary>
    /// Allows you to easily programatically supply scripts from code.
    /// </summary>
    public sealed class StaticScriptProvider : IScriptProvider
    {
        private readonly IEnumerable<SqlScript> scripts;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticScriptProvider"/> class.
        /// </summary>
        /// <param name="scripts">The scripts.</param>
        public StaticScriptProvider(IEnumerable<SqlScript> scripts)
        {
            this.scripts = scripts;
        }

        /// <summary>
        /// Gets all scripts that should be executed.
        /// </summary>
        public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            return scripts;
        }

        public IEnumerable<DBMigration> GetDBMigrations()
        {
            throw new NotImplementedException();
        }

        IEnumerable<DBMigration> IScriptProvider.GetDBMigrations()
        {
            throw new NotImplementedException();
        }
    }
}