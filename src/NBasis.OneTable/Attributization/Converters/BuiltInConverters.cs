namespace NBasis.OneTable.Attributization.Converters
{
    public sealed class BuiltInConverters
    {
        public static AttributeConverter<bool> BooleanConverter => _boolean ??= new BooleanConverter();
        private static AttributeConverter<bool> _boolean;


        public static AttributeConverter<string> StringConverter => _string ??= new StringConverter();
        private static AttributeConverter<string> _string;
    }
}
