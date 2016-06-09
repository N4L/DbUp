using System;
using DbUp.Migrations;
using DbUp.Support;

namespace DBMigrationSample.Data
{
    public class FirstData : DataMigration
    {
        public override string UpScript
        {
            get { return ""; }
        }

        public override ExecutionSteps ShoudRunAt => ExecutionSteps.BeforeCode;

        public override long DependentSchemaVersionId
        {
            get { return 20160506100800; }
        } 
    }
}
