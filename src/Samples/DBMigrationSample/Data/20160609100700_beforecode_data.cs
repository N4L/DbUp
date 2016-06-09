using System;
using DbUp.Migrations;
using DbUp.Support;

namespace DBMigrationSample.Data
{
    public class BeforeCodeData : DataMigration
    {
        public override string UpScript => "";

        public override ExecutionSteps ShoudRunAt => ExecutionSteps.BeforeCode;
            
        public override long DependentSchemaVersionId => 20160609100800;
    }
}
