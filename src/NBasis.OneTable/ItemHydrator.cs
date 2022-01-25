using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NBasis.OneTable
{
    internal static class ItemHydrator
    {
        public static TItem Hydrate<TItem>(IDictionary<string, AttributeValue> item) where TItem : class
        {
            // create item
            var i = Activator.CreateInstance<TItem>();

            // deal with every property on item
            var properties = typeof(TItem).GetProperties();
            foreach(var property in properties)
            {
                // look for attribute property
                var attrAttr = property.GetCustomAttributes(true).OfType<AttributeAttribute>().FirstOrDefault();
                if (attrAttr != null)
                {
                    string fieldName = attrAttr.FieldName ?? property.Name;
                    if (item.ContainsKey(fieldName))
                    {
                        SetProperty(i, property, item[fieldName]);
                    }
                }
            }

            return i;
        }

        private static void SetProperty<TItem>(TItem item, PropertyInfo property, AttributeValue value)
        {

        }

        public static IDictionary<string,AttributeValue> Dehydrate<TItem>(TItem item) where TItem : class
        {
            var i = new Dictionary<string,AttributeValue>();

            // deal with every property on item
            var properties = typeof(TItem).GetProperties();
            foreach (var property in properties)
            {
                // look for attribute property
                var attrAttr = property.GetCustomAttributes(true).OfType<AttributeAttribute>().FirstOrDefault();
                if (attrAttr != null)
                {
                    string fieldName = attrAttr.FieldName ?? property.Name;

                    var attributeValue = GetAttributeValue(item, property);
                    if (attributeValue != null)
                        i[fieldName] = attributeValue;
                }
            }

            return i;
        }

        private static AttributeValue GetAttributeValue<TItem>(TItem item, PropertyInfo property)
        {
            return null;
        }
    }
}
