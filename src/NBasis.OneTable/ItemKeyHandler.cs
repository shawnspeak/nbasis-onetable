﻿using Amazon.DynamoDBv2.Model;

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
            typeof(TItem).EnumerateItemKeys((property, keyAttr) =>
            {
                // get value fom item
                object value = property.GetValue(item);

                // use converter to get attribute value
                var converter = _context.AttributizerSettings.GetConverter(property.PropertyType);

                if (converter.TryWriteAsObject(value, out AttributeValue attrValue))
                {
                    string fieldName = keyAttr.GetFieldName(_context);

                    if (keyAttr.Prefix != null)
                    {
                        // attribute must be string regardless of attribute type

                        // we support string or number types
                        string finalValue = attrValue.S;
                        if (attrValue.N != null)
                            finalValue = attrValue.N;

                        keyItem[fieldName] = new AttributeValue
                        {
                            S = keyAttr.Prefix + "#" + finalValue
                        };
                    } 
                    else
                    {
                        // key is what the converter sends back
                        keyItem[fieldName] = attrValue;
                    }
                }
            });

            return keyItem;
        }
    }
}
