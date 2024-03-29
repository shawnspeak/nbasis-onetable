﻿using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Annotations;
using NBasis.OneTable.Exceptions;
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
            AttributeConverter converter;

            // resolve converter
            if (attrAttr.Converter != null)
            {
                converter = Activator.CreateInstance(attrAttr.Converter) as AttributeConverter;
            } 
            else
            {
                converter = _context.AttributizerSettings.GetConverter(property.PropertyType);
            }

            if (converter == null)
                throw new MissingAttributeConverterException();

            var value = property.GetValue(item);
            if (converter.TryWriteAsObject(value, property.PropertyType, out AttributeValue attrValue))
                return attrValue;

            return null;
        }

        private AttributeValue GetKeyAttributeValue(TItem item, PropertyInfo property, KeyAttribute keyAttr)
        {
            var value = property.GetValue(item);

            var converter = _context.AttributizerSettings.GetConverter(property.PropertyType);

            if (converter.TryWriteAsObject(value, property.PropertyType, out AttributeValue attrValue))
            {
                // if value is null, then it's null regardless of prefix
                if (attrValue.NULL)
                    return attrValue;

                if (keyAttr.Prefix != null)
                {
                    // attribute must be string regardless of attribute type

                    // we support string or number types
                    string finalValue = attrValue.S;
                    if (attrValue.N != null)
                        finalValue = attrValue.N;

                    return new AttributeValue
                    {
                        S = _context.FormatKeyPrefix(keyAttr, finalValue)
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

            // add item type if specified
            if (!string.IsNullOrWhiteSpace(_context.Configuration.ItemTypeAttributeName))
            {
                var itemType = typeof(TItem).GetItemType();
                if (!string.IsNullOrWhiteSpace(itemType))
                {
                    i[_context.Configuration.ItemTypeAttributeName] = new AttributeValue
                    {
                        S = itemType
                    }; 
                }
            }

            return i;
        }

        private void SetProperty(TItem i, PropertyInfo property, AttributeValue attributeValue, AttributeAttribute attrAttr)
        {
            AttributeConverter converter;

            // resolve converter
            if (attrAttr.Converter != null)
            {
                converter = Activator.CreateInstance(attrAttr.Converter) as AttributeConverter;
            }
            else
            {
                converter = _context.AttributizerSettings.GetConverter(property.PropertyType);
            }

            if (converter == null)
                throw new MissingAttributeConverterException();

            if (converter.TryReadAsObject(attributeValue, property.PropertyType, out var value))
                property.SetValue(i, value);
        }

        private void SetKey(TItem i, PropertyInfo property, AttributeValue attributeValue, KeyAttribute keyAttr)
        {
            var converter = _context.AttributizerSettings.GetConverter(property.PropertyType);

            var strippedAttributeValue = _context.StripKeyPrefix(keyAttr, attributeValue);

            if (converter.TryReadAsObject(strippedAttributeValue, property.PropertyType, out var value))
            {
                property.SetValue(i, value);
            }   
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
                if (item.TryGetValue(fieldName, out AttributeValue value))
                {
                    SetProperty(i, property, value, attr);
                }
            });

            typeof(TItem).EnumerateItemKeys((property, attr) =>
            {
                string fieldName = attr.GetFieldName(_context);
                if (item.TryGetValue(fieldName, out AttributeValue value))
                {
                    SetKey(i, property, value, attr);
                }
            });

            return i;
        }
    }
}
