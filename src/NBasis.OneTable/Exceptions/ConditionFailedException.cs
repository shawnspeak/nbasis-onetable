namespace NBasis.OneTable.Exceptions
{
    public class ConditionFailedException : Exception
    {
        public ConditionFailedException(Amazon.DynamoDBv2.Model.ConditionalCheckFailedException inner) : base("Condition check failed", inner)
        {
        }
    }
}
