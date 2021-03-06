﻿using System;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.CompilerServices;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.Engine.Output;
using NDesk.Options;

namespace DbUp.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = "";
            var database = "";
            var schemaDirectory = "";
            var dataDirectory = "";
            var username = "";
            var password = "";
            var dbProvider = "";
            var toVersionId = long.MaxValue;
            bool mark = false;
            var connectionString = "";

            bool show_help = false;
            bool ensure_database = false;

            var optionSet = new OptionSet() {
                { "s|server=", "the SQL Server host", s => server = s },
                { "db|database=", "database to upgrade", d => database = d},
                { "sd|schemaDirectory=", "directory containing DB Schema Update files", dir => schemaDirectory = dir },
                { "dd|dataDirectory=", "directory containing DB Data Update files", dir => dataDirectory = dir },
                { "v|version=", "apply the migration until version id", vid => toVersionId = long.Parse(vid) },
                { "e|ensure", "ensure datbase exists", e => ensure_database = e != null },
                { "u|user=", "Database username", u => username = u},
                { "p|password=", "Database password", p => password = p},
                { "cs|connectionString=", "Full connection string", cs => connectionString = cs},
                { "dbp|databaseProvider=", "database provider (SQLServer, MySql or PostgreSql)", dbp => dbProvider = dbp },
                { "h|help",  "show this message and exit", v => show_help = v != null },
                { "mark", "Mark scripts as executed but take no action", m => mark = true}
            };

            optionSet.Parse(args);

            if (args.Length == 0)
                show_help = true;


            if (show_help)
            {
                optionSet.WriteOptionDescriptions(System.Console.Out);
                return;
            }

            if (String.IsNullOrEmpty(connectionString))
            {
                connectionString = BuildConnectionString(server, database, username, password);
            }

            UpgradeEngineBuilder dbEngineBuilder;
            switch (dbProvider.ToLower())
            {
                case "sqlserver":
                    dbEngineBuilder = DeployChanges.To.SqlDatabase(connectionString);
                    break;
                case "mysql":
                    dbEngineBuilder = DeployChanges.To.MySqlDatabase(connectionString);
                    break;
                case "postgresql":
                    dbEngineBuilder = DeployChanges.To.PostgresqlDatabase(connectionString);
                    break;
                default:
                    new ConsoleUpgradeLog().WriteError("Unknown DB Provider: {0}", dbProvider);
                    Environment.ExitCode = 1;
                    return;
            }

            var dbup = dbEngineBuilder.LogToConsole()
                .WithScriptsFromDBMigrationClass(BuildDirectoryPath(schemaDirectory), BuildDirectoryPath(dataDirectory))
                .Build();

            DatabaseUpgradeResult result = null;
            if (!mark)
            {
                if (ensure_database) EnsureDatabase.For.SqlDatabase(connectionString);
                result = dbup.PerformDBMigration(toVersionId);
            }
            else
            {
                result = dbup.MarkAsExecuted();
            }

            if (!result.Successful)
            {
                Environment.ExitCode = 1;
            }
        }

        private static string BuildConnectionString(string server, string database, string username, string password)
        {
            var conn = new SqlConnectionStringBuilder();
            conn.DataSource = server;
            conn.InitialCatalog = database;
            if (!String.IsNullOrEmpty(username))
            {
                conn.UserID = username;
                conn.Password = password;
                conn.IntegratedSecurity = false;
            }
            else
            {
                conn.IntegratedSecurity = true;
            }

            return conn.ToString();
        }

        private static string BuildDirectoryPath(string inputPath)
        {
            var path = Directory.GetCurrentDirectory();
            if(string.IsNullOrEmpty(inputPath)) return path;
            if (Directory.Exists(inputPath)) return inputPath;
            return $"{path}/{inputPath}";
        }
    }
}
