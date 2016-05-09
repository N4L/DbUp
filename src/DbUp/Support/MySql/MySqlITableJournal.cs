using System;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using DbUp.Entities;

namespace DbUp.Support.MySql
{
    /// <summary>
    /// An implementation of the <see cref="IJournal"/> interface which tracks version numbers for a 
    /// PostgreSQL database using a table called SchemaVersions.
    /// </summary>
    public sealed class MySqlITableJournal : IJournal
    {
        private readonly string schemaTableName;
        private readonly string table;
        private readonly string schema;
        private readonly Func<IConnectionManager> connectionManager;
        private readonly Func<IUpgradeLog> log;

        private static string QuoteIdentifier(string identifier)
        {
            return "`" + identifier + "`";
        }

        /// <summary>
        /// Creates a new MySql table journal.
        /// </summary>
        /// <param name="connectionManager">The MySql connection manager.</param>
        /// <param name="logger">The upgrade logger.</param>
        /// <param name="schema">The name of the schema the journal is stored in.</param>
        /// <param name="table">The name of the journal table.</param>
        public MySqlITableJournal(Func<IConnectionManager> connectionManager, Func<IUpgradeLog> logger, string schema, string table)
        {
            this.table = table;
            this.schema = schema;
            schemaTableName = string.IsNullOrEmpty(schema)
                ? QuoteIdentifier(table)
                : QuoteIdentifier(schema) + "." + QuoteIdentifier(table);
            this.connectionManager = connectionManager;
            log = logger;        
        }

        private static string CreateTableSql(string tableName)
        {
            return string.Format(
                @"CREATE TABLE {0} 
                    (
                        `Id` INT NOT NULL AUTO_INCREMENT,
                        `VersionId` LONG NOT NULL,
                        `MigrationType` INT NOT NULL,
                        `ScriptName` VARCHAR(512) NOT NULL,
                        `CreatedOn` TIMESTAMP NOT NULL,
                        PRIMARY KEY (`Id`));", tableName);
        }

        private void EnsureTable()
        {
            connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
            {
                using (var command = dbCommandFactory())
                {
                    command.CommandText = CreateTableSql(schemaTableName);

                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }

                log().WriteInformation(string.Format("The {0} table has been created", schemaTableName));
            });
        }

        public IEnumerable<DBMigration> GetMigratedDBVersions()
        {
            log().WriteInformation("Fetching list of already executed scripts.");
            var exists = DoesTableExist();
            if (!exists)
            {
                log().WriteInformation(string.Format("The {0} table could not be found. The database is assumed to be at version 0.", schemaTableName));

                EnsureTable();
            }

            var migratedDBVersions = new List<DBMigration>();
            connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
            {
                using (var command = dbCommandFactory())
                {
                    command.CommandText = GetExecutedScriptsSql(schemaTableName);
                    command.CommandType = CommandType.Text;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            migratedDBVersions.Add(
                                new DBMigration()
                                {
                                    Id = (int)reader["Id"],
                                    VersionId = long.Parse(reader["VersionId"].ToString()),
                                    MigrationType = (MigrationTypes)reader["MigrationType"],
                                    ScriptName = reader["ScriptName"].ToString(),
                                    CreatedOn = (DateTime)reader["CreatedOn"]
                                }
                                );
                    }
                }
            });

            return migratedDBVersions;
        }

        /// <summary>
        /// Records an upgrade script for a database.
        /// </summary>
        /// <param name="script">The script.</param>
        public void StoreExecutedScript(SqlScript script)
        {
            var exists = DoesTableExist();
            if (!exists)
            {
                log().WriteInformation(string.Format("Creating the {0} table", schemaTableName));

                EnsureTable();
            }

            connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
            {
                using (var command = dbCommandFactory())
                {
                    command.CommandText = string.Format("insert into {0} (ScriptName, Applied) values (@scriptName, @applied)", schemaTableName);

                    var scriptNameParam = command.CreateParameter();
                    scriptNameParam.ParameterName = "scriptName";
                    scriptNameParam.Value = script.Name;
                    command.Parameters.Add(scriptNameParam);

                    var appliedParam = command.CreateParameter();
                    appliedParam.ParameterName = "applied";
                    appliedParam.Value = DateTime.Now;
                    command.Parameters.Add(appliedParam);

                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="migrationScript"></param>
        public void StoreExecutedMigrationScript(DBMigrationScript migrationScript)
        {
            var exists = DoesTableExist();
            if (!exists)
            {
                log().WriteInformation(string.Format("Creating the {0} table", schemaTableName));

                EnsureTable();
            }

            connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
            {
                using (var command = dbCommandFactory())
                {
                    if (migrationScript.MigrationPerformType == MigrationPerformTypes.Up)
                    {
                        command.CommandText =
                            string.Format(
                                "insert into {0} (VersionId, MigrationType, ScriptName, CreatedOn) values (@versionId, @migrationType, @scriptName, @applied)",
                                schemaTableName);

                        var versionIdParam = command.CreateParameter();
                        versionIdParam.ParameterName = "versionId";
                        versionIdParam.Value = migrationScript.VersionId;
                        command.Parameters.Add(versionIdParam);

                        var migrationTypeParam = command.CreateParameter();
                        migrationTypeParam.ParameterName = "migrationType";
                        migrationTypeParam.Value = migrationScript.MigrationType;
                        command.Parameters.Add(migrationTypeParam);

                        var scriptNameParam = command.CreateParameter();
                        scriptNameParam.ParameterName = "scriptName";
                        scriptNameParam.Value = migrationScript.Name;
                        command.Parameters.Add(scriptNameParam);

                        var appliedParam = command.CreateParameter();
                        appliedParam.ParameterName = "applied";
                        appliedParam.Value = DateTime.Now;
                        command.Parameters.Add(appliedParam);
                    }
                    else
                    {
                        command.CommandText =
                            string.Format("delete from {0} where ScriptName=@scriptName", schemaTableName);

                        var scriptNameParam = command.CreateParameter();
                        scriptNameParam.ParameterName = "scriptName";
                        scriptNameParam.Value = migrationScript.Name;
                        command.Parameters.Add(scriptNameParam);
                    }

                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                    log().WriteInformation("-- {0} Migration '{1}' Applied", migrationScript.MigrationPerformType.ToString().ToUpper(), migrationScript.Name);

                }
            });
        }

        public bool HasDBVersionMigrated(long versionId, MigrationTypes dbMigrationType)
        {
            var exists = DoesTableExist();
            if (!exists)
            {
                log().WriteInformation(string.Format("Creating the {0} table", schemaTableName));

                EnsureTable();
            }

            return connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
            {
                using (var command = dbCommandFactory())
                {
                    command.CommandText =
                        string.Format(
                            "select Id from {0} where VersionId=@versionId and MigrationType=@migrationType",
                            schemaTableName);

                    var versionIdParam = command.CreateParameter();
                    versionIdParam.ParameterName = "versionId";
                    versionIdParam.Value = versionId;
                    command.Parameters.Add(versionIdParam);

                    var migrationTypeParam = command.CreateParameter();
                    migrationTypeParam.ParameterName = "MigrationType";
                    migrationTypeParam.Value = (int) dbMigrationType;
                    command.Parameters.Add(migrationTypeParam);

                    command.CommandType = CommandType.Text;
                    var excutedMigrationId = command.ExecuteScalar();

                    return excutedMigrationId != null;
                }
            });
        }

        private static string GetExecutedScriptsSql(string table)
        {
            return string.Format("select * from {0} order by VersionId", table);
        }

        private bool DoesTableExist()
        {
            return connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
            {
                try
                {
                    using (var command = dbCommandFactory())
                    {
                        return VerifyTableExistsCommand(command, table, schema);
                    }
                }
                catch (DbException)
                {
                    return false;
                }
            });
        }

        /// <summary>Verify, using database-specific queries, if the table exists in the database.</summary>
        /// <param name="command">The <c>IDbCommand</c> to be used for the query</param>
        /// <param name="tableName">The name of the table</param>
        /// <param name="schemaName">The schema for the table</param>
        /// <returns>True if table exists, false otherwise</returns>
        private bool VerifyTableExistsCommand(IDbCommand command, string tableName, string schemaName)
        {
            command.CommandText = string.IsNullOrEmpty(schema)
                            ? string.Format("select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}'", tableName)
                            : string.Format("select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}' and TABLE_SCHEMA = '{1}'", tableName, schemaName);
            command.CommandType = CommandType.Text;
            var result = Convert.ToInt32(command.ExecuteScalar());
            return result == 1;
        }
    }
}
