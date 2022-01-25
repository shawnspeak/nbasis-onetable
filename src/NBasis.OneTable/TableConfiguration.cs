namespace NBasis.OneTable
{
    public class TableConfiguration
    {
        public class KeyAttributeConfiguration
        {
            public string PKName { get; set; }

            public string SKName { get; set; }

            public string GPKPrefix { get; set; }

            public string GSKPrefix { get; set; }

            internal KeyAttributeConfiguration()
            {
                PKName = Constants.KeyAttributeNames.PK;
                SKName = Constants.KeyAttributeNames.SK;
                GPKPrefix = Constants.KeyAttributeNames.GPK;
                GSKPrefix = Constants.KeyAttributeNames.GSK;
            }

            internal KeyAttributeConfiguration(KeyAttributeConfiguration other)
            {
                PKName = other.PKName;
                SKName = other.SKName;
                GPKPrefix = other.GPKPrefix;
                GSKPrefix = other.GSKPrefix;
            }

            internal void Validate()
            {
                
            }
        }

        public KeyAttributeConfiguration KeyAttributes { get; private set; }

        public int GSIndexCount { get; set; }

        public static TableConfiguration Default()
        {
            return new TableConfiguration();
        }

        internal void Validate()
        {
            if ((GSIndexCount < 0) || (GSIndexCount > Constants.MaxGSIndexCount))
                throw new Exception();
        }

        private TableConfiguration()
        {
            GSIndexCount = Constants.DefaultGSIndexCount;
            KeyAttributes = new KeyAttributeConfiguration();
        }

        /// <summary>
        /// Create a deep copy of the table configuration
        /// </summary>
        internal TableConfiguration(TableConfiguration other)
        {
            GSIndexCount = other.GSIndexCount;
            KeyAttributes = new KeyAttributeConfiguration(other.KeyAttributes);
        }
    }
}

