using Amazon.DynamoDBv2.Model;

namespace NBasis.OneTable
{
    public class QueryResults<TItem>
    {
        public long Count { get; internal set; }

        public long ScannedCount { get; internal set; }

        public IEnumerable<TItem> Results { get; internal set; }

        public bool CanContinue { get; internal set; }


        // This will probably need to change to a more formal "QueryContinuation" object that can be
        // de/serialized and passed around..

        internal void SetContinue(QueryRequest request, QueryResponse response)
        {
            if (response.LastEvaluatedKey != null)
            {
                CanContinue = true;

                QueryRequest = request;
                QueryResponse = response;
            }
        }

        internal QueryRequest QueryRequest { get; set; }

        internal QueryResponse QueryResponse { get; set; }
    }
}
