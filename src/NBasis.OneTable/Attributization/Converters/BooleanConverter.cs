using Amazon.DynamoDBv2.Model;

namespace NBasis.OneTable.Attributization.Converters
{
    internal sealed class BooleanConverter : AttributeConverter<bool>
    {
        public override bool Read(AttributeValue attribute)
        {
            return attribute.BOOL;
        }

        public override AttributeValue Write(bool value)
        {
            return new AttributeValue
            {
                BOOL = value
            };
        }
    }
}
