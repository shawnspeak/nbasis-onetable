namespace NBasis.OneTable
{
    public abstract class TableContext
    {
        public string TableName { get; internal set; }

        public TableConfiguration Configuration { get; internal set; }

        internal void SetTableName(string tableName)
        {
            this.TableName = tableName;
        }

        public virtual void OnTableConfiguration(TableConfiguration config)
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

            // validate config
            Configuration.Validate();
        }
    }
}
