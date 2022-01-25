using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBasis.OneTable.Attributization
{
    /// <summary>
    /// Converts a value to/from a OneTable attribute
    /// </summary>
    public abstract class AttributeConverter
    {
        public abstract bool CanConvert(Type typeToConvert);
    }

    public abstract class AttributeConverter<T> : AttributeConverter
    {
        public override bool CanConvert(Type typeToConvert)
        {
            throw new NotImplementedException();
        }

        public abstract T? Read(ItemAttribute attribute, Type typeToConvert);

        public abstract ItemAttribute Write(T value);
    }
}
