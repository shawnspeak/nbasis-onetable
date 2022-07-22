namespace NBasis.OneTable.Exceptions
{
    public class UnableToContinueQueryException : Exception
    {
        public UnableToContinueQueryException() : base("This query cannot be continued")
        {
        }
    }
}
