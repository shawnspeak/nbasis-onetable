namespace NBasis.OneTable.Attributization.Converters
{
    public sealed class BuiltInConverters
    {
        public static AttributeConverter<bool> BooleanConverter => _boolean ??= new BooleanConverter();
        private static AttributeConverter<bool> _boolean;

        //public static AttributeConverter<bool?> NullableBooleanConverter => _nullableBoolean ??= new NullableBooleanConverter();
        //private static AttributeConverter<bool?> _nullableBoolean;

        public static AttributeConverter<DateTime> DateTimeConverter => _dateTime ??= new DateTimeEpochMillisecondsConverter();
        private static AttributeConverter<DateTime> _dateTime;

        public static AttributeConverter<DateTimeOffset> DateTimeOffsetConverter => _dateTimeOffset ??= new DateTimeOffsetEpochMillisecondsConverter();
        private static AttributeConverter<DateTimeOffset> _dateTimeOffset;

        public static AttributeConverter<decimal> DecimalConverter => _decimal ??= new DecimalConverter();
        private static AttributeConverter<decimal> _decimal;

        public static AttributeConverter<double> DoubleConverter => _double ??= new DoubleConverter();
        private static AttributeConverter<double> _double;

        public static AttributeConverter<int> IntConverter => _int ??= new IntConverter();
        private static AttributeConverter<int> _int;

        public static AttributeConverter<float> FloatConverter => _float ??= new FloatConverter();
        private static AttributeConverter<float> _float;

        public static AttributeConverter<Guid> GuidConverter => _guid ??= new GuidConverter();
        private static AttributeConverter<Guid> _guid;

        public static AttributeConverter<long> LongIntConverter => _long ??= new LongIntConverter();
        private static AttributeConverter<long> _long;

        public static AttributeConverter<short> ShortIntConverter => _short ??= new ShortIntConverter();
        private static AttributeConverter<short> _short;

        public static AttributeConverter<string> StringConverter => _string ??= new StringConverter();
        private static AttributeConverter<string> _string;
    }
}
