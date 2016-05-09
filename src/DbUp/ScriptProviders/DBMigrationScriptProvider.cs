using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DbUp.Engine;
using DbUp.Engine.Transactions;
using DbUp.Helpers;
using DbUp.Migrations;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

namespace DbUp.ScriptProviders
{
    /// <summary>
    /// 
    /// </summary>
    public class DBMigrationScriptProvider : IScriptProvider
    {
        private readonly string _directoryPath;
        private readonly Encoding encoding;

        ///<summary>
        ///</summary>
        ///<param name="directoryPath">Path to SQL upgrade scripts</param>
        public DBMigrationScriptProvider(string directoryPath)
        {
            this._directoryPath = directoryPath;
            this.encoding = Encoding.Default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionManager"></param>
        /// <returns></returns>
        public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DBMigration> GetDBMigrations()
        {
            var migrations = GetMigrationsFromDirectory();
            return migrations;
        }

        /// <summary>
        /// Scan a directory for migration files and return a collection of Migration objects that are
        /// discovered.
        /// </summary>
        /// <returns>A collection of Migration object in an unordered state.</returns>
        private List<DBMigration> GetMigrationsFromDirectory()
        {
            var migrations = new List<DBMigration>();

            FileInfo[] files = (new DirectoryInfo(this._directoryPath)).GetFiles("*.cs");

            for (int i = 0; i < (int)files.Length; i++)
            {
                FileInfo fileInfo = files[i];
                DBMigration migration = null;
                if (TryGetMigration(fileInfo, ref migration))
                {
                    migrations.Add(migration);
                }
            }

            return migrations;
        }

        private static bool TryGetMigration(FileSystemInfo file, ref DBMigration migration)
        {
            StringBuilder stringBuilder = new StringBuilder();
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    while (true)
                    {
                        string str = streamReader.ReadLine();
                        string str1 = str;
                        if (str == null)
                        {
                            break;
                        }
                        stringBuilder.AppendLine(str1);
                    }
                }
            }
            InMemorySourceFile inMemorySourceFile = new InMemorySourceFile(file.FullName, stringBuilder.ToString());
            long? versionId = GetVersion(file.Name);
            if (!versionId.HasValue || !inMemorySourceFile.IsMigration)
            {
                return false;
            }
            DBMigration value = CreateInstanceFromSource(inMemorySourceFile);
            value.VersionId = versionId.Value;
            value.FileName = file.Name;
            migration = value;
            return true;
        }

        private static Assembly GetAssembly(string name)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly assembly = Array.Find<Assembly>(assemblies, (Assembly a) => a.GetName().Name == name);
            if (assembly == null)
            {
                var couldNotFindAssemblyInAppDomain = string.Format("Could not find assembly {0} in app domain", name);
                throw new InvalidOperationException(couldNotFindAssemblyInAppDomain);
            }
            return assembly;
        }

        internal static DBMigration CreateInstanceFromSource(InMemorySourceFile sourceFile)
        {
            //SourceDirectoryMigrationLoader.ForceLoadDependencies();
            CompilerParameters compilerParameter = new CompilerParameters()
            {
                GenerateInMemory = true
            };
            compilerParameter.ReferencedAssemblies.Add("System.Configuration.dll");
            compilerParameter.ReferencedAssemblies.Add("System.Web.dll");
            compilerParameter.ReferencedAssemblies.Add("System.Data.dll");
            compilerParameter.ReferencedAssemblies.Add("System.dll");
            compilerParameter.ReferencedAssemblies.Add("System.Xml.dll");
            compilerParameter.ReferencedAssemblies.Add("mscorlib.dll");
            var uri = new Uri(GetAssembly("DbUp").CodeBase);
            compilerParameter.ReferencedAssemblies.Add(uri.LocalPath);
            //var uri1 = new Uri(GetAssembly("Mindscape.LightSpeed.Generator.Model").CodeBase);
            //compilerParameter.ReferencedAssemblies.Add(uri1.LocalPath);
            CodeDomProvider provider = GetCodeDomProvider();
            string[] strArrays = new string[] { sourceFile.SourceCode };
            CompilerResults compilerResult = provider.CompileAssemblyFromSource(compilerParameter, strArrays);
            if (compilerResult.Errors.Count > 0 || compilerResult.CompiledAssembly == null)
            {
                if (compilerResult.Errors.Count > 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (CompilerError error in compilerResult.Errors)
                    {
                        stringBuilder.AppendLine(error.ErrorText);
                    }
                    var compilationFailed = string.Format("Migration script comilpation failed for file {0}", sourceFile.FileName);
                    throw new InvalidOperationException(compilationFailed);
                }
                if (compilerResult.CompiledAssembly == null)
                {
                    throw new InvalidOperationException("No assembly generated");
                }
            }
            var typeName = sourceFile.TypeName;
            var obj = compilerResult.CompiledAssembly.CreateInstance(typeName);
            if (obj == null)
            {
                throw new InvalidOperationException(string.Format("Could not parse the type name of the Migration class properly - could not find it: {0}", typeName));
            }
            var migration = obj as DBMigration;
            if (migration == null)
            {
                throw new InvalidOperationException(string.Format("Type {0} is not based on DBMigration", obj.GetType().FullName));
            }
            return migration;
        }

        private static CodeDomProvider GetCodeDomProvider()
        {
            return new CSharpCodeProvider();
        }

        private static long? GetVersion(string filename)
        {
            long? nullable;
            try
            {
                var str = filename.Substring(0, Constants.MigrationDateFormat.Length);
                nullable = long.Parse(str);
            }
            catch
            {
                return null;
            }
            return nullable;
        }

    }
}
