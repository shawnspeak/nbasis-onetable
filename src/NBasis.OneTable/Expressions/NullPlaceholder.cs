namespace NBasis.OneTable.Expressions
{
    internal sealed class NullPlaceholder
    {
        readonly static NullPlaceholder _instance = new();

        internal static NullPlaceholder Value()
        {
            return _instance;
        }
    }
}
