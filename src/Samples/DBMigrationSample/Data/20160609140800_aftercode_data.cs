using System;
using DbUp.Migrations;
using DbUp.Support;

namespace DBMigrationSample.Data
{
    public class AfterCodeData : DataMigration
    {
        public override string UpScript => "";

        public override ExecutionSteps ShoudRunAt => ExecutionSteps.AfterCode;
            
        public override long DependentSchemaVersionId => 20160609150800;
    }
}
