using Amazon.DynamoDBv2.Model;

namespace NBasis.OneTable.Attributization.Converters
{
    internal sealed class ShortIntConverter : AttributeConverter<short>
    {
        public override short Read(AttributeValue attribute)
        {
            return short.Parse(attribute.N);
        }

        public override AttributeValue Write(short value)
        {
            return new AttributeValue
            {
                N = value.ToString()
            };
        }
    }

    internal sealed class IntConverter : AttributeConverter<int>
    {
        public override int Read(AttributeValue attribute)
        {
            return int.Parse(attribute.N);
        }

        public override AttributeValue Write(int value)
        {
            return new AttributeValue
            {
                N = value.ToString()
            };
        }
    }

    internal sealed class LongIntConverter : AttributeConverter<long>
    {
        public override long Read(AttributeValue attribute)
        {
            return long.Parse(attribute.N);
        }

        public override AttributeValue Write(long value)
        {
            return new AttributeValue
            {
                N = value.ToString()
            };
        }
    }
}
