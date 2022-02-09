namespace NBasis.OneTable.Attributization.Converters
{
    public sealed class BuiltInConverters
    {
        public static AttributeConverter<bool> BooleanConverter => _boolean ??= new BooleanConverter();
        private static AttributeConverter<bool> _boolean;

        public static AttributeConverter<int> IntConverter => _int ??= new IntConverter();
        private static AttributeConverter<int> _int;

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
