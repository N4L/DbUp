using System;

namespace DbUp.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class SchemaMigration : DBMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract string DownScript { get; }

    }
}
