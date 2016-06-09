using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using DbUp.Builder;
using DbUp.Entities;
using DbUp.Migrations;
using DbUp.Support;

namespace DbUp.Engine
{
    /// <summary>
    /// This class orchestrates the database upgrade process.
    /// </summary>
    public class UpgradeEngine
    {
        private readonly UpgradeConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpgradeEngine"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public UpgradeEngine(UpgradeConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Determines whether the database is out of date and can be upgraded.
        /// </summary>
        public bool IsUpgradeRequired()
        {
            return GetScriptsToExecute().Count() != 0;
        }

        /// <summary>
        /// Tries to connect to the database.
        /// </summary>
        /// <param name="errorMessage">Any error message encountered.</param>
        /// <returns></returns>
        public bool TryConnect(out string errorMessage)
        {
            return configuration.ConnectionManager.TryConnect(configuration.Log, out errorMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toVersionId"></param>
        /// <param name="executionStep"></param>
        /// <returns></returns>
        public DatabaseUpgradeResult PerformDBMigration(long toVersionId, ExecutionSteps executionStep)
        {
            var executed = new List<DBMigrationScript>();

            string executedScriptName = null;
            try
            {
                using (configuration.ConnectionManager.OperationStarting(configuration.Log, executed.Cast<SqlScript>().ToList()))
                {

                    configuration.Log.WriteInformation("Beginning database migration");

                    var migrationsToExecute = GetDBMigrationsToExecuteInsideOperation(toVersionId, executionStep);

                    if (migrationsToExecute.Count == 0)
                    {
                        configuration.Log.WriteInformation("No new Migration need to be executed - completing.");
                        return new DatabaseUpgradeResult(executed.Cast<SqlScript>().ToList(), true, null);
                    }

                    configuration.ScriptExecutor.VerifySchema();

                    foreach (var migration in migrationsToExecute)
                    {
                        executedScriptName = migration.Name;

                        var sqlScript = migration.MigrationPerformType == MigrationPerformTypes.Up
                            ? new SqlScript(migration.Name, migration.UpScript)
                            : new SqlScript(migration.Name, migration.DownScript);

                        if (migration.DependentSchemaVersionId.HasValue &&
                            !configuration.Journal.HasDBVersionMigrated(migration.DependentSchemaVersionId.Value, MigrationTypes.Schema))
                        {
                            var ex =
                                new InvalidOperationException(string.Format("Dependent migration {0} for migration {1} hasn't been executed yet.",
                                    migration.DependentSchemaVersionId, migration.Name));
                            configuration.Log.WriteError("Migration failed due to an exception: {0}", ex.ToString());
                            return new DatabaseUpgradeResult(executed.Cast<SqlScript>().ToList(), false, ex);
                        }

                        configuration.ScriptExecutor.Execute(sqlScript, configuration.Variables);

                        configuration.Journal.StoreExecutedMigrationScript(migration);

                        executed.Add(migration);
                    }

                    configuration.Log.WriteInformation("Migration successful");
                    return new DatabaseUpgradeResult(executed.Cast<SqlScript>().ToList(), true, null);
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("Error occurred in script: ", executedScriptName);
                configuration.Log.WriteError("Migration failed due to an unexpected exception:\r\n{0}", ex.ToString());
                return new DatabaseUpgradeResult(executed.Cast<SqlScript>().ToList(), false, ex);
            }
        }

        /// <summary>
        /// Performs the database upgrade.
        /// </summary>
        public DatabaseUpgradeResult PerformUpgrade()
        {
            var executed = new List<SqlScript>();

            string executedScriptName = null;
            try
            {
                using (configuration.ConnectionManager.OperationStarting(configuration.Log, executed))
                {

                    configuration.Log.WriteInformation("Beginning database upgrade");

                    var scriptsToExecute = GetScriptsToExecuteInsideOperation();

                    if (scriptsToExecute.Count == 0)
                    {
                        configuration.Log.WriteInformation("No new scripts need to be executed - completing.");
                        return new DatabaseUpgradeResult(executed, true, null);
                    }

                    configuration.ScriptExecutor.VerifySchema();

                    foreach (var script in scriptsToExecute)
                    {
                        executedScriptName = script.Name;

                        configuration.ScriptExecutor.Execute(script, configuration.Variables);

                        configuration.Journal.StoreExecutedScript(script);

                        executed.Add(script);
                    }

                    configuration.Log.WriteInformation("Upgrade successful");
                    return new DatabaseUpgradeResult(executed, true, null);
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("Error occurred in script: ", executedScriptName);
                configuration.Log.WriteError("Upgrade failed due to an unexpected exception:\r\n{0}", ex.ToString());
                return new DatabaseUpgradeResult(executed, false, ex);
            }
        }

        /// <summary>
        /// Returns a list of scripts that will be executed when the upgrade is performed
        /// </summary>
        /// <returns>The scripts to be executed</returns>
        public List<SqlScript> GetScriptsToExecute()
        {
            using (configuration.ConnectionManager.OperationStarting(configuration.Log, new List<SqlScript>()))
            {
                return GetScriptsToExecuteInsideOperation();
            }
        }

        private List<SqlScript> GetScriptsToExecuteInsideOperation()
        {
            var allScripts = configuration.ScriptProviders.SelectMany(scriptProvider => scriptProvider.GetScripts(configuration.ConnectionManager));
            var migratedDBVersions = configuration.Journal.GetMigratedDBVersions();

            return allScripts.Where(s => migratedDBVersions.All(y => y.ScriptName != s.Name)).ToList();
        }

        private List<DBMigrationScript> GetDBMigrationsToExecuteInsideOperation(long toVersionId, ExecutionSteps executionStep)
        {
            var allMigrations =
                configuration.ScriptProviders.SelectMany(scriptProvider => scriptProvider.GetDBMigrations()).ToList();

            //We only run migrations marked as BeforeCode or NoPreference at before code deployment step
            if (executionStep == ExecutionSteps.BeforeCode)
                allMigrations =
                    allMigrations.Where(m => m.ShoudRunAt == ExecutionSteps.BeforeCode).ToList();

            var migratedDBVersions = configuration.Journal.GetMigratedDBVersions();

            var missedMigrationScripts =
                allMigrations.Where(s => migratedDBVersions.All(y => y.ScriptName != s.FileName) && s.VersionId <= toVersionId)
                    .Select(
                        m =>
                            m.GetType().IsSubclassOf(typeof (DataMigration))
                                ? new DBMigrationScript(m.VersionId, m.FileName, m.UpScript, string.Empty, MigrationTypes.Data,
                                    ((DataMigration) m).DependentSchemaVersionId, MigrationPerformTypes.Up)
                                : new DBMigrationScript(m.VersionId, m.FileName, m.UpScript, ((SchemaMigration) m).DownScript, MigrationTypes.Schema,
                                    null, MigrationPerformTypes.Up))
                    .OrderBy(s => s)
                    .ToList();

            var downMigrationScripts =
                allMigrations.Where(
                    m =>
                        m.VersionId > toVersionId && migratedDBVersions.Any(s => s.ScriptName == m.FileName) &&
                        m.GetType().IsSubclassOf(typeof (SchemaMigration)))
                    .Select(
                        m =>
                            new DBMigrationScript(m.VersionId, m.FileName, m.UpScript, ((SchemaMigration) m).DownScript, MigrationTypes.Schema, null,
                                MigrationPerformTypes.Down))
                    .OrderByDescending(s => s)
                    .ToList();

            var migrationsToExecute = new List<DBMigrationScript>();
            migrationsToExecute.AddRange(missedMigrationScripts);
            migrationsToExecute.AddRange(downMigrationScripts);

            return migrationsToExecute;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Entities.DBMigration> GetMigratedDBVersions()
        {
            using (configuration.ConnectionManager.OperationStarting(configuration.Log, new List<SqlScript>()))
            {
                return configuration.Journal.GetMigratedDBVersions().ToList();
            }
        }

        ///<summary>
        /// Creates version record for any new migration scripts without executing them.
        /// Useful for bringing development environments into sync with automated environments
        ///</summary>
        ///<returns></returns>
        public DatabaseUpgradeResult MarkAsExecuted()
        {
            var marked = new List<SqlScript>();
            using (configuration.ConnectionManager.OperationStarting(configuration.Log, marked))
            {
                try
                {
                    var scriptsToExecute = GetScriptsToExecuteInsideOperation();

                    foreach (var script in scriptsToExecute)
                    {
                        configuration.Journal.StoreExecutedScript(script);
                        configuration.Log.WriteInformation("Marking script {0} as executed", script.Name);
                        marked.Add(script);
                    }

                    configuration.Log.WriteInformation("Script marking successful");
                    return new DatabaseUpgradeResult(marked, true, null);
                }
                catch (Exception ex)
                {
                    configuration.Log.WriteError("Upgrade failed due to an unexpected exception:\r\n{0}", ex.ToString());
                    return new DatabaseUpgradeResult(marked, false, ex);
                }
            }
        }

        public DatabaseUpgradeResult MarkAsExecuted(string latestScript)
        {
            var marked = new List<SqlScript>();
            using (configuration.ConnectionManager.OperationStarting(configuration.Log, marked))
            {
                try
                {
                    var scriptsToExecute = GetScriptsToExecuteInsideOperation();

                    foreach (var script in scriptsToExecute)
                    {
                        configuration.Journal.StoreExecutedScript(script);
                        configuration.Log.WriteInformation("Marking script {0} as executed", script.Name);
                        marked.Add(script);
                        if (script.Name.Equals(latestScript))
                        {
                            break;
                        }
                    }

                    configuration.Log.WriteInformation("Script marking successful");
                    return new DatabaseUpgradeResult(marked, true, null);
                }
                catch (Exception ex)
                {
                    configuration.Log.WriteError("Upgrade failed due to an unexpected exception:\r\n{0}", ex.ToString());
                    return new DatabaseUpgradeResult(marked, false, ex);
                }
            }
        }
    }
}