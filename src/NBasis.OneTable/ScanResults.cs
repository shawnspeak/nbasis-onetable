using Amazon.DynamoDBv2.Model;

namespace NBasis.OneTable
{
    public class ScanResults<TItem>
    {
        public long Count { get; internal set; }

        public long ScannedCount { get; internal set; }

        public IEnumerable<TItem> Results { get; internal set; }

        public bool CanContinue { get; internal set; }


        // This will probably need to change to a more formal "QueryContinuation" object that can be
        // de/serialized and passed around..

        internal void SetContinue(ScanRequest request, ScanResponse response)
        {
            if ((response.LastEvaluatedKey?.Count ?? 0) > 0)
            {
                CanContinue = true;

                ScanRequest = request;
                ScanResponse = response;
            }
        }

        internal ScanRequest ScanRequest { get; set; }

        internal ScanResponse ScanResponse { get; set; }
    }
}
