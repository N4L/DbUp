using System;
using DbUp.Migrations;

namespace DBMigrationSample.Schema
{
    public class Firstschema : SchemaMigration
    {
        public override string UpScript
        {
            get { return ""; }
        }

        public override string DownScript
        {
            get { return ""; }
        }
    }
}
