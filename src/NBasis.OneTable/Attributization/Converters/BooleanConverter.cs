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

    //internal sealed class NullableBooleanConverter : AttributeConverter<bool?>
    //{
    //    public override bool? Read(AttributeValue attribute)
    //    {
    //        if (attribute.NULL)
    //            return null;
    //        return attribute.BOOL;
    //    }

    //    public override AttributeValue Write(bool? value)
    //    {
    //        if (value == null)
    //        {
    //            return new AttributeValue()
    //            {
    //                NULL = true,
    //            };
    //        }
    //        else
    //        {
    //            return new AttributeValue
    //            {
    //                BOOL = value.Value
    //            };
    //        }
    //    }
    //}
}
