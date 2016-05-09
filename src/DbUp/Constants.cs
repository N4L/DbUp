using System;

namespace DbUp
{
    internal static class Constants
    {
        public static readonly string MigrationDateFormat;

        static Constants()
        {
            Constants.MigrationDateFormat = "yyyyMMddHHmmss";
        }
    }
}
