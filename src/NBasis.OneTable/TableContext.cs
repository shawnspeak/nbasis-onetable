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
    }
}
