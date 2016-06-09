using System;
using DbUp.Migrations;
using DbUp.Support;

namespace DBMigrationSample.Schema
{
    public class ShouldRunAtBeforeCodeSchema : SchemaMigration
    {
        public override string UpScript => "";

        public override ExecutionSteps ShoudRunAt => ExecutionSteps.BeforeCode;

        public override string DownScript => "";
    }
}
