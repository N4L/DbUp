using System;
using DbUp.Helpers;
using DbUp.Support;

namespace DbUp.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DBMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract string UpScript { get; }

        /// <summary>
        /// 
        /// </summary>
        public abstract ExecutionSteps ShoudRunAt { get; }

        /// <summary>
        /// 
        /// </summary>
        public long VersionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FileName { get; set; }

    }
}
