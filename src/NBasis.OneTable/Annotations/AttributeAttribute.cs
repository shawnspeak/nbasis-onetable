using NBasis.OneTable.Attributization;

namespace NBasis.OneTable.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AttributeAttribute : Attribute
    {
        public string FieldName { get; private set; }

        public Type Converter { get; private set; }

        public AttributeAttribute(
            string fieldName = null, 
            Type coverter = null)
        {
            FieldName = fieldName;
            Converter = coverter;
        }
    }
}
