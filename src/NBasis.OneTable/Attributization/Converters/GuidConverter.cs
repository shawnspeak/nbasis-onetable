using Amazon.DynamoDBv2.Model;

namespace NBasis.OneTable.Attributization.Converters
{
    internal sealed class GuidConverter : AttributeConverter<Guid>
    {
        public override Guid Read(AttributeValue attribute)
        {
            return Guid.Parse(attribute.S);
        }

        public override AttributeValue Write(Guid value)
        {
            return new AttributeValue(value.ToString());
        }
    }
}
