using System;
using DbUp.Migrations;

namespace DBMigrationSample.Data
{
    public class SecondData : DataMigration
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
