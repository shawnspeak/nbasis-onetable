namespace NBasis.OneTable
{
    public class ItemProjectionHandler<TItem>
    {
        readonly TableContext _context;

        public ItemProjectionHandler(TableContext context)
        {
            _context = context;
        }

        public string Build()
        {
            List<string> fields = new();

            // deal with every property and key on item
            typeof(TItem).EnumerateItemAttributes((property, attr) =>
            {
                string fieldName = attr.FieldName ?? property.Name;
                fields.Add(fieldName);
            });
            typeof(TItem).EnumerateItemKeys((property, attr) =>
            {
                string fieldName = attr.GetFieldName(_context);
                if (!fields.Contains(fieldName))
                    fields.Add(fieldName);
            });

            return string.Join(',', fields);
        }
    }
}
