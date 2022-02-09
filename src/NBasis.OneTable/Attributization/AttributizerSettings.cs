using NBasis.OneTable.Attributization.Converters;

namespace NBasis.OneTable.Attributization
{
    public sealed class AttributizerSettings
    {
        // The list of built-in converters.
        readonly Dictionary<Type, AttributeConverter> _builtInConverters;
        readonly Dictionary<Type, AttributeConverter> _overrideConverters;

        internal AttributizerSettings()
        {
            var converters = new Dictionary<Type, AttributeConverter>();
            var add = (AttributeConverter ac) =>
            {
                converters.Add(ac.TypeToConvert, ac);
            };

            add(BuiltInConverters.BooleanConverter);
            add(BuiltInConverters.IntConverter);            
            add(BuiltInConverters.GuidConverter);
            add(BuiltInConverters.LongIntConverter);
            add(BuiltInConverters.ShortIntConverter);
            add(BuiltInConverters.StringConverter);

            _builtInConverters = converters;
            _overrideConverters = new Dictionary<Type, AttributeConverter>();
        }

        public static AttributizerSettings Default()
        {
            return new AttributizerSettings();
        }

        public void AddConverter(AttributeConverter converter)
        {
            _overrideConverters[converter.TypeToConvert] = converter;
        }

        internal AttributeConverter GetConverter(Type typeToConvert)
        {
            if (typeToConvert == null)
            {
                throw new ArgumentNullException(nameof(typeToConvert));
            }

            // do we have an override converter?
            if (_overrideConverters.ContainsKey(typeToConvert))
                return _overrideConverters[typeToConvert];

            // do we have a built-in converter?
            if (_builtInConverters.ContainsKey(typeToConvert))
                return _builtInConverters[typeToConvert];

            return null;
        }
    }
}
