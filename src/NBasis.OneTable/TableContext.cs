using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Annotations;
using NBasis.OneTable.Attributization;

namespace NBasis.OneTable
{
    public abstract class TableContext
    {
        public string TableName { get; internal set; }

        public TableConfiguration Configuration { get; internal set; }

        public AttributizerSettings AttributizerSettings { get; internal set; }

        internal void SetTableName(string tableName)
        {
            this.TableName = tableName;
        }

        public virtual void OnTableConfiguration(TableConfiguration config)
        {
        }

        public virtual void OnAttributizerSetup(AttributizerSettings settings)
        {
        }

        internal void ValidateConfiguration()
        {
            if (TableName == null)
                throw new Exception();
            if ((TableName.Length < 3) || (TableName.Length > 255))
                throw new Exception();

            // build configuration
            if (Configuration == null)
            {
                // create default config
                var config = TableConfiguration.Default();

                // allow overrides
                OnTableConfiguration(config);

                // construct final config
                Configuration = new TableConfiguration(config);
            }

            // setup attributizer
            if (AttributizerSettings == null)
            {
                var settings = AttributizerSettings.Default();

                // allow overrides
                OnAttributizerSetup(settings);

                AttributizerSettings = settings;
            }

            // validate config
            Configuration.Validate();
        }

        // some helper methods
        internal string GSIndexName(int indexNumber)
        {
            return string.Format(Configuration.GSIndexNameFormat, indexNumber);
        }

        internal string GPKAttributeName(int indexNumber)
        {
            return string.Format(Configuration.KeyAttributes.GPKNameFormat, indexNumber);
        }

        internal string GSKAttributeName(int indexNumber)
        {
            return string.Format(Configuration.KeyAttributes.GSKNameFormat, indexNumber);
        }

        internal string FormatKeyPrefix(KeyAttribute keyAttr, string value)
        {
            if (string.IsNullOrEmpty(keyAttr.Prefix))
                return value;
            if (!string.IsNullOrWhiteSpace(this.Configuration.KeyAttributes.KeyPrefixDelimiter))
                return keyAttr.Prefix + this.Configuration.KeyAttributes.KeyPrefixDelimiter + value;
            return keyAttr.Prefix + value;
        }

        internal AttributeValue StripKeyPrefix(KeyAttribute keyAttr, AttributeValue value)
        {
            if (string.IsNullOrEmpty(keyAttr.Prefix))
                return value;

            // prefixed key must be string
            string keyString = value.S;
            if (!string.IsNullOrEmpty(keyString))
            {
                if (!string.IsNullOrWhiteSpace(this.Configuration.KeyAttributes.KeyPrefixDelimiter))
                {
                    return new AttributeValue
                    {
                        S = keyString.Substring(this.Configuration.KeyAttributes.KeyPrefixDelimiter.Length + keyAttr.Prefix.Length)
                    };
                }   
                else
                {
                    return new AttributeValue
                    {
                        S = keyString.Substring(keyAttr.Prefix.Length)
                    };
                }
            }

            return new AttributeValue { S = "" };
        }
    }
}
