using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBasis.OneTable
{
    internal static class Constants
    {
        internal const int DefaultGSIndexCount = 2;
        internal const int MaxGSIndexCount = 20;

        internal static class KeyAttributeNames
        {
            public const string PK = "PK";
            public const string SK = "SK";
            public const string GPK = "GPK";
            public const string GSK = "GSK";
        }
    }
}
