using System;
using DbUp.Migrations;

namespace DBMigrationSample.Data
{
    public class FirstData : DataMigration
    {
        public override string UpScript
        {
            get { return ""; }
        }

        public override long DependentSchemaVersionId
        {
            get { return 20160506100800; }
        } 
    }
}
