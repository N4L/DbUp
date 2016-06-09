using System;

namespace DbUp.Support
{
    /// <summary>
    /// Enum execution step
    /// </summary>
    public enum ExecutionSteps
    {
        /// <summary>
        /// Before code deployment
        /// </summary>
        BeforeCode,

        /// <summary>
        /// After code deployment
        /// </summary>
        AfterCode
    }

    public enum TargetingSteps
    {
        All,
        BeforeCode,
        AfterCode
    }
}
