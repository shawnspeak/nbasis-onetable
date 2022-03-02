namespace NBasis.OneTable
{
    public static class QueryExtensions
    {
        public static bool BeginsWith<T>(this T obj, T start)
        {
            return true;
        }

        public static bool Between<T>(this T obj, T start, T end)
        {
            return true;
        }

        public static bool AllByPrefix<T>(this T obj)
        {
            return true;
        }

        public static bool Exists<T>(this T obj)
        {
            return true;
        }

        public static bool NotExists<T>(this T obj)
        {
            return true;
        }

        public static bool DoesContain<T>(this T obj, T start)
        {
            return true;
        }
    }
}
