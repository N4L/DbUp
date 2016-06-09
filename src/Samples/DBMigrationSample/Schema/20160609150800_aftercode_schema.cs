using System;
using DbUp.Migrations;
using DbUp.Support;

namespace DBMigrationSample.Schema
{
    public class ShouldRunAtAfterCodeSchema : SchemaMigration
    {
        public override string UpScript => "";
        public override ExecutionSteps ShoudRunAt => ExecutionSteps.AfterCode;

        public override string DownScript => "";
    }
}
