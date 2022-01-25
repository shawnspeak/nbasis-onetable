using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBasis.OneTable.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AttributeAttribute : Attribute
    {
        public string? FieldName { get; private set; }

        public ItemAttributeType AttributeType { get; private set; }

        public AttributeAttribute(string? fieldName = null, ItemAttributeType attributeType = ItemAttributeType.Auto)
        {
            FieldName = fieldName;
            AttributeType = attributeType;
        }
    }
}
