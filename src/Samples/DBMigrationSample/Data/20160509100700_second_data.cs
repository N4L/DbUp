using System;
using DbUp.Migrations;
using DbUp.Support;

namespace DBMigrationSample.Data
{
    public class SecondData : DataMigration
    {
        public override string UpScript => "";

        public override ExecutionSteps ShoudRunAt => ExecutionSteps.NoPreference;
            
        public override long DependentSchemaVersionId => 20160506100800;
    }
}
