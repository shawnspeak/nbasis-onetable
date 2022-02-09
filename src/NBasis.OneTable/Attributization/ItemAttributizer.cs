using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Annotations;
using System.Reflection;

namespace NBasis.OneTable.Attributization
{
    public class ItemAttributizer<TItem> where TItem : class
    {
        readonly TableContext _context;

        public ItemAttributizer(TableContext context)
        {
            _context = context;
        }

        private AttributeValue GetAttributeValue(TItem item, PropertyInfo property, AttributeAttribute attrAttr)
        {
            // resolved converter
            if (attrAttr.Converter != null)
            {

            } 
            else
            {
                var value = property.GetValue(item);

                var converter = _context.AttributizerSettings.GetConverter(property.PropertyType);

                if (converter.TryWriteAsObject(value, out AttributeValue attrValue))
                    return attrValue;
            }
            return null;
        }

        private AttributeValue GetKeyAttributeValue(TItem item, PropertyInfo property, KeyAttribute keyAttr)
        {
            var value = property.GetValue(item);

            var converter = _context.AttributizerSettings.GetConverter(property.PropertyType);

            if (converter.TryWriteAsObject(value, out AttributeValue attrValue))
            {
                if (keyAttr.Prefix != null)
                {
                    // attribute must be string regardless of attribute type

                    // we support string or number types
                    string finalValue = attrValue.S;
                    if (attrValue.N != null)
                        finalValue = attrValue.N;

                    return new AttributeValue
                    {
                        S = keyAttr.Prefix + "#" + finalValue
                    };
                }
                else
                {
                    // key is what the converter sends back
                    return attrValue;
                }
            }   

            return null;
        }

        /// <summary>
        /// Turn an item into AttributeValues
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Dictionary<string, AttributeValue> Attributize(TItem item)
        {
            var i = new Dictionary<string, AttributeValue>();

            // deal with every property on item
            item.EnumerateItemAttributes((property, attr) =>
            {
                string fieldName = attr.FieldName ?? property.Name;

                var attributeValue = GetAttributeValue(item, property, attr);
                if (attributeValue != null)
                    i[fieldName] = attributeValue;
            });

            item.EnumerateItemKeys((property, attr) =>
            {
                string fieldName = attr.GetFieldName(_context);

                var attributeValue = GetKeyAttributeValue(item, property, attr);
                if (attributeValue != null)
                    i[fieldName] = attributeValue;
            });

            return i;
        }


        private void SetProperty(TItem i, PropertyInfo property, AttributeValue attributeValue)
        {
            var converter = _context.AttributizerSettings.GetConverter(property.PropertyType);

            if (converter.TryReadAsObject(attributeValue, out var value))
                property.SetValue(i, value);
        }

        /// <summary>
        /// Turn attribute values into an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public TItem Deattributize(Dictionary<string, AttributeValue> item)
        {
            // create item
            var i = Activator.CreateInstance<TItem>();

            // deal with every property on item
            typeof(TItem).EnumerateItemAttributes((property, attr) =>
            {
                string fieldName = attr.FieldName ?? property.Name;
                if (item.ContainsKey(fieldName))
                {
                    SetProperty(i, property, item[fieldName]);
                }
            });

            typeof(TItem).EnumerateItemKeys((property, attr) =>
            {
                string fieldName = attr.GetFieldName(_context);
                if (item.ContainsKey(fieldName))
                {
                    SetProperty(i, property, item[fieldName]);
                }
            });

            return i;
        }
    }
}
