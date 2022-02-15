using Amazon.DynamoDBv2.Model;

namespace NBasis.OneTable.Attributization.Converters
{
    public abstract class DateTimeEpochConverterBase : AttributeConverter<DateTime>
    {
        protected static readonly DateTime UnixEpochDateTimeUtc = new DateTime(621355968000000000L, DateTimeKind.Utc);
    }

    public sealed class DateTimeEpochMillisecondsConverter : DateTimeEpochConverterBase
    {
        public override DateTime Read(AttributeValue attribute)
        {
            var milliseconds = long.Parse(attribute.N);
            return UnixEpochDateTimeUtc.AddMilliseconds(milliseconds);
        }

        public override AttributeValue Write(DateTime value)
        {
            return new AttributeValue
            {
                N = Convert.ToInt64(value.Subtract(UnixEpochDateTimeUtc).TotalMilliseconds).ToString()
            };
        }
    }

    public sealed class DateTimeEpochSecondsConverter : DateTimeEpochConverterBase
    {
        public override DateTime Read(AttributeValue attribute)
        {
            var seconds = long.Parse(attribute.N);
            return UnixEpochDateTimeUtc.AddSeconds(seconds);
        }

        public override AttributeValue Write(DateTime value)
        {
            return new AttributeValue
            {
                N = Convert.ToInt64(value.Subtract(UnixEpochDateTimeUtc).TotalSeconds).ToString()
            };
        }
    }

    public sealed class DateTimeOffsetEpochMillisecondsConverter : AttributeConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(AttributeValue attribute)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(attribute.N));
        }

        public override AttributeValue Write(DateTimeOffset value)
        {
            return new AttributeValue
            {
                N = value.ToUnixTimeMilliseconds().ToString()
            };
        }
    }

    public sealed class DateTimeOffsetEpochSecondsConverter : AttributeConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(AttributeValue attribute)
        {
            return DateTimeOffset.FromUnixTimeSeconds(long.Parse(attribute.N));
        }

        public override AttributeValue Write(DateTimeOffset value)
        {
            return new AttributeValue
            {
                N = value.ToUnixTimeSeconds().ToString()
            };
        }
    }
}
