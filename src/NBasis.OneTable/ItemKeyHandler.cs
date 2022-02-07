using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Annotations;

namespace NBasis.OneTable
{
    public class ItemKeyHandler<TItem>
    {
        readonly TableContext _context;

        public ItemKeyHandler(TableContext context)
        {
            _context = context;
        }

        public Dictionary<string, AttributeValue> BuildKey(TItem item)
        {
            var keyItem = new Dictionary<string, AttributeValue>();

            // deal with every property and key on item
            typeof(TItem).EnumerateItemKeys((property, attr) =>
            {
                // get value fom item
                object value = property.GetValue(item);

                // use converter to get attribute value
                var converter = _context.AttributizerSettings.GetConverter(property.PropertyType);

                if (converter.TryWriteAsObject(value, out AttributeValue attrValue))
                {
                    string fieldName = attr.GetFieldName(_context);

                    if (attr.Prefix != null)
                    {
                        attrValue.S = attr.Prefix + "#" + attrValue.S;
                    }

                    keyItem[fieldName] = attrValue;
                }
            });

            return keyItem;
        }
    }
}
