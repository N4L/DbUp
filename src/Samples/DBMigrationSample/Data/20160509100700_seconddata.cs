using System;
using DbUp.Migrations;

namespace DBMigrationSample.Data
{
    public class SecondData : DataMigration
    {
        public override string UpScript => "";

        public override long DependentSchemaVersionId => 20160506100800;
    }
}
