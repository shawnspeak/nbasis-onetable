namespace NBasis.OneTable
{
    internal static class Constants
    {
        internal const int DefaultGSIndexCount = 2;
        internal const int MaxGSIndexCount = 20;
        internal const string GSIndexFormat = "GSI{0}";
        internal const string DefaultKeyPrefixDelimiter = "#";
        internal const string DefaultGSIndexNameFormat = "gsi_{0}";
        internal const string ItemTypeAttributeName = "RT";

        internal static class KeyAttributeNames
        {
            internal const string PK = "PK";
            internal const string SK = "SK";
            internal const string GPK = "GPK{0}";
            internal const string GSK = "GSK{0}";
        }
    }
}
