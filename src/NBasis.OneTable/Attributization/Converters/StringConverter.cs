using Amazon.DynamoDBv2.Model;

namespace NBasis.OneTable.Attributization.Converters
{
    internal sealed class StringConverter : AttributeConverter<string>
    {
        public override string Read(AttributeValue attribute)
        {
            if (attribute.NULL)
                return null;
            return attribute.S;
        }

        public override AttributeValue Write(string value)
        {
            if (value == null)
            {
                return new AttributeValue()
                {
                    NULL = true,
                };
            } 
            else
            {
                return new AttributeValue(value);
            }
        }
    }
}
