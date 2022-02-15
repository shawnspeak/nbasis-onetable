using Amazon.DynamoDBv2.Model;

namespace NBasis.OneTable.Attributization
{
    /// <summary>
    /// Converts a value to/from a OneTable attribute
    /// </summary>
    public abstract class AttributeConverter
    {
        public abstract bool CanConvert(Type typeToConvert);

        internal abstract Type TypeToConvert { get; }

        internal abstract bool TryWriteAsObject(object value, Type objectType, out AttributeValue attributeValue);

        internal abstract bool TryReadAsObject(AttributeValue attributeValue, Type objectType, out object obj);
    }

    public abstract class AttributeConverter<T> : AttributeConverter
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(T);
        }

        internal sealed override Type TypeToConvert => typeof(T);

        public abstract T Read(AttributeValue attribute);

        internal virtual bool TryRead(AttributeValue attributeValue, out T obj)
        {
            obj = Read(attributeValue);
            return true;
        }

        internal sealed override bool TryReadAsObject(AttributeValue attribute, Type objectType, out object obj)
        {
            Type underlying = Nullable.GetUnderlyingType(objectType);
            if (underlying != null)
            {
                if (attribute.NULL)
                {
                    obj = null;
                    return true;
                }
            }

            var ret = TryRead(attribute, out T objT);
            obj = objT;
            return ret;
        }

        public abstract AttributeValue Write(T value);

        internal virtual bool TryWrite(T value, out AttributeValue attributeValue)
        {
            attributeValue = Write(value);
            return true;
        }

        internal sealed override bool TryWriteAsObject(object value, Type objectType, out AttributeValue attributeValue)
        {
            Type underlying = Nullable.GetUnderlyingType(objectType);
            if (underlying != null)
            {
                if (value == null)
                {
                    attributeValue = new AttributeValue
                    {
                        NULL = true
                    };
                    return true;
                }                
            }
            
            T valueOfT = (T)value!;
            return TryWrite(valueOfT, out attributeValue);
        }
    }
}
