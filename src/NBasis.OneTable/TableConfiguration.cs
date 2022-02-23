namespace NBasis.OneTable
{
    public class TableConfiguration
    {
        public class KeyAttributeConfiguration
        {
            public string PKName { get; set; }

            public string SKName { get; set; }

            public string GPKNameFormat { get; set; }

            public string GSKNameFormat { get; set; }

            public string KeyPrefixDelimiter { get; set; }

            internal KeyAttributeConfiguration()
            {
                PKName = Constants.KeyAttributeNames.PK;
                SKName = Constants.KeyAttributeNames.SK;
                GPKNameFormat = Constants.KeyAttributeNames.GPK;
                GSKNameFormat = Constants.KeyAttributeNames.GSK;
                KeyPrefixDelimiter = Constants.DefaultKeyPrefixDelimiter;
            }

            internal KeyAttributeConfiguration(KeyAttributeConfiguration other)
            {
                PKName = other.PKName;
                SKName = other.SKName;
                GPKNameFormat = other.GPKNameFormat;
                GSKNameFormat = other.GSKNameFormat;
                KeyPrefixDelimiter = other.KeyPrefixDelimiter;
            }

            internal void Validate()
            {
                // names and formats required

                // must pass attribute naming rules

                // no attribute dups

                // formats must contain placeholder
            }
        }

        public KeyAttributeConfiguration KeyAttributes { get; private set; }

        public int GSIndexCount { get; set; }

        public string GSIndexNameFormat { get; set; }

        public string ItemTypeAttributeName { get; set; }

        public static TableConfiguration Default()
        {
            return new TableConfiguration();
        }

        internal void Validate()
        {
            // gsi count must be in range
            if ((GSIndexCount < 0) || (GSIndexCount > Constants.MaxGSIndexCount))
                throw new Exception();

            // index name format required

            // must pass index naming rules

            // formats must contain placeholder
        }

        private TableConfiguration()
        {
            GSIndexCount = Constants.DefaultGSIndexCount;
            GSIndexNameFormat = Constants.DefaultGSIndexNameFormat;
            ItemTypeAttributeName = Constants.ItemTypeAttributeName;
            KeyAttributes = new KeyAttributeConfiguration();
        }

        /// <summary>
        /// Create a deep copy of the table configuration
        /// </summary>
        internal TableConfiguration(TableConfiguration other)
        {
            GSIndexCount = other.GSIndexCount;
            GSIndexNameFormat= other.GSIndexNameFormat;
            ItemTypeAttributeName= other.ItemTypeAttributeName;
            KeyAttributes = new KeyAttributeConfiguration(other.KeyAttributes);
        }
    }
}

