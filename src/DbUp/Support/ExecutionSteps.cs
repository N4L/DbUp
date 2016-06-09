using System;

namespace DbUp.Support
{
    /// <summary>
    /// Enum execution step
    /// </summary>
    public enum ExecutionSteps
    {
        /// <summary>
        /// 
        /// </summary>
        NoPreference,

        /// <summary>
        /// Before code deployment
        /// </summary>
        BeforeCode,

        /// <summary>
        /// After code deployment
        /// </summary>
        AfterCode
    }
}
