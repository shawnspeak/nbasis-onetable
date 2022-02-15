using Amazon.DynamoDBv2.Model;

namespace NBasis.OneTable.Attributization.Converters
{
    internal sealed class FloatConverter : AttributeConverter<float>
    {
        public override float Read(AttributeValue attribute)
        {
            return float.Parse(attribute.N);
        }

        public override AttributeValue Write(float value)
        {
            return new AttributeValue
            {
                N = value.ToString()
            };
        }
    }

    internal sealed class DecimalConverter : AttributeConverter<decimal>
    {
        public override decimal Read(AttributeValue attribute)
        {
            return decimal.Parse(attribute.N);
        }

        public override AttributeValue Write(decimal value)
        {
            return new AttributeValue
            {
                N = value.ToString()
            };
        }
    }

    internal sealed class DoubleConverter : AttributeConverter<double>
    {
        public override double Read(AttributeValue attribute)
        {
            return double.Parse(attribute.N);
        }

        public override AttributeValue Write(double value)
        {
            return new AttributeValue
            {
                N = value.ToString()
            };
        }
    }
}
