﻿using System;
using DbUp.Migrations;
using DbUp.Support;

namespace DBMigrationSample.Schema
{
    public class Firstschema : SchemaMigration
    {
        public override string UpScript => "";

        public override ExecutionSteps ShoudRunAt => ExecutionSteps.NoPreference;

        public override string DownScript => "";
    }
}
