using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Attributization;
using NBasis.OneTable.Exceptions;
using NBasis.OneTable.Expressions;
using NBasis.OneTable.Validation;
using System.Linq.Expressions;

namespace NBasis.OneTable
{
    public enum Sort
    {
        Ascending,
        Descending,
    }

    public class QueryOptions
    {
        /// <summary>
        /// The maximum number of items to return
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// The sort direction of the query
        /// </summary>
        public Sort Sort { get; set; }

        /// <summary>
        /// Retrieve only the count of items that would be returned by the query
        /// </summary>
        public bool OnlyCount { get; set; }

        public QueryOptions()
        {
            Limit = int.MaxValue;
            Sort = Sort.Ascending;
            OnlyCount = false;
        }

        public static QueryOptions Default()
        {
            return new QueryOptions();
        }        
    }

    public class ScanOptions
    {
        /// <summary>
        /// The maximum number of items to return
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Retrieve only the count of items that would be returned by the query
        /// </summary>
        public bool OnlyCount { get; set; }

        public ScanOptions()
        {
            Limit = int.MaxValue;
            OnlyCount = false;
        }

        public static ScanOptions Default()
        {
            return new ScanOptions();
        }
    }

    public interface IItemLookup<TContext> where TContext : TableContext
    {
        Task<TItem> Find<TItem>(Expression<Func<TItem, bool>> keyPredicate) where TItem : class;

        Task<QueryResults<TItem>> Query<TItem>(Expression<Func<TItem, bool>> keyPredicate) where TItem : class;

        Task<QueryResults<TItem>> Query<TItem>(Expression<Func<TItem, bool>> keyPredicate, QueryOptions options) where TItem : class;

        Task<QueryResults<TItem>> Query<TItem>(Expression<Func<TItem, bool>> keyPredicate, Expression<Func<TItem, bool>> filterPredicate) where TItem : class;

        Task<QueryResults<TItem>> Query<TItem>(Expression<Func<TItem, bool>> keyPredicate, Expression<Func<TItem, bool>> filterPredicate, QueryOptions options) where TItem : class;

        Task<QueryResults<TItem>> ContinueQuery<TItem>(QueryResults<TItem> previousResults) where TItem : class;

        Task<ScanResults<TItem>> Scan<TItem>() where TItem : class;

        Task<ScanResults<TItem>> Scan<TItem>(Expression<Func<TItem, bool>> filterPredicate) where TItem : class;

        Task<ScanResults<TItem>> Scan<TItem>(Expression<Func<TItem, bool>> filterPredicate, ScanOptions options) where TItem : class;

        Task<ScanResults<TItem>> ContinueScan<TItem>(ScanResults<TItem> previousResults) where TItem : class;
    }

    public class DynamoDbItemLookup<TContext> : IItemLookup<TContext> where TContext : TableContext
    {
        readonly IAmazonDynamoDB _client;
        readonly TContext _context;

        public DynamoDbItemLookup(
            IAmazonDynamoDB client,
            TContext context
        )
        {
            _client = client;
            _context = context;
        }        

        public async Task<TItem> Find<TItem>(Expression<Func<TItem, bool>> predicate) where TItem : class
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            // validate item type
            new ItemTypeValidator<TItem>(_context).Validate();

            // get key
            var keyItem = new ItemKeyExpressionHandler<TItem>(_context).Handle(predicate);

            // call get item
            var request = new GetItemRequest
            {
                TableName = _context.TableName,
                Key = keyItem,
                ProjectionExpression = new ItemProjectionHandler<TItem>(_context).Build()
            };

            var response = await _client.GetItemAsync(request);

            if (response.IsItemSet)
                return (new ItemAttributizer<TItem>(_context)).Deattributize(response.Item);

            return default;
        }

        public Task<QueryResults<TItem>> Query<TItem>(Expression<Func<TItem, bool>> keyPredicate) where TItem : class
        {
            return this.Query(keyPredicate, null, null);
        }

        public Task<QueryResults<TItem>> Query<TItem>(Expression<Func<TItem, bool>> keyPredicate, QueryOptions options) where TItem : class
        {
            return this.Query(keyPredicate, null, options);
        }

        public Task<QueryResults<TItem>> Query<TItem>(Expression<Func<TItem, bool>> keyPredicate, Expression<Func<TItem, bool>> filterPredicate) where TItem : class
        {
            return this.Query(keyPredicate, filterPredicate, null);
        }

        public async Task<QueryResults<TItem>> Query<TItem>(Expression<Func<TItem, bool>> keyPredicate, Expression<Func<TItem, bool>> filterPredicate = null, QueryOptions options = null) where TItem : class
        {
            if (keyPredicate == null) throw new ArgumentNullException(nameof(keyPredicate));

            options ??= QueryOptions.Default();

            // validate item type
            new ItemTypeValidator<TItem>(_context).Validate();

            // build query from predicate
            var queryDetails = new ItemQueryExpressionHandler<TItem>(_context).Handle(keyPredicate);

            // start to build the query request
            var request = new QueryRequest
            {
                TableName = _context.TableName,
                ProjectionExpression = options.OnlyCount ? null : (new ItemProjectionHandler<TItem>(_context)).Build(),
                KeyConditionExpression = queryDetails.QueryExpression,
                ExpressionAttributeNames = queryDetails.AttributeNames,
                ExpressionAttributeValues = queryDetails.AttributeValues,
                ScanIndexForward = (options.Sort == Sort.Ascending),
                Limit = options.Limit,
                IndexName = queryDetails.IndexName,
                Select = options.OnlyCount ? Select.COUNT : Select.SPECIFIC_ATTRIBUTES
            };

            // construct and apply a filter if needed
            if (filterPredicate != null)
            {
                var filterDetails = new ItemConditionalExpressionHandler<TItem>(_context).Handle(filterPredicate, false);
                request.FilterExpression = filterDetails.ConditionExpression;
                request.MergeAttributeNames(filterDetails.AttributeNames);
                request.MergeAttributeValues(filterDetails.AttributeValues);
            }

            var response = await _client.QueryAsync(request);

            var results =  new QueryResults<TItem>
            {
                Count = response.Count,
                ScannedCount = response.ScannedCount,
                Results = options.OnlyCount ? Enumerable.Empty<TItem>() : response.Items.Select(i => (new ItemAttributizer<TItem>(_context)).Deattributize(i))
            };

            if (response.LastEvaluatedKey != null)
            {
                results.SetContinue(request, response);
            }

            return results;
        }

        public async Task<QueryResults<TItem>> ContinueQuery<TItem>(QueryResults<TItem> previousResults) where TItem : class
        {
            if (previousResults == null) throw new ArgumentNullException(nameof(previousResults));
            if (!previousResults.CanContinue) throw new UnableToContinueQueryException();

            // item type will have already been validated

            // we can reuse the previous query
            var request = new QueryRequest
            {
                TableName = _context.TableName, 
                ExclusiveStartKey = previousResults.QueryResponse.LastEvaluatedKey,
                ProjectionExpression = previousResults.QueryRequest.ProjectionExpression,
                KeyConditionExpression = previousResults.QueryRequest.KeyConditionExpression,
                ExpressionAttributeNames = previousResults.QueryRequest.ExpressionAttributeNames,
                ExpressionAttributeValues = previousResults.QueryRequest.ExpressionAttributeValues,
                FilterExpression = previousResults.QueryRequest.FilterExpression,
                ScanIndexForward = previousResults.QueryRequest.ScanIndexForward,
                Limit = previousResults.QueryRequest.Limit,
                IndexName = previousResults.QueryRequest.IndexName,
                Select = previousResults.QueryRequest.Select
            };

            var response = await _client.QueryAsync(request);

            var results = new QueryResults<TItem>
            {
                Count = response.Count,
                ScannedCount = response.ScannedCount,
                Results = (previousResults.QueryRequest.Select == Select.COUNT) ? Enumerable.Empty<TItem>() : response.Items.Select(i => (new ItemAttributizer<TItem>(_context)).Deattributize(i))
            };

            if (response.LastEvaluatedKey != null)
            {
                results.SetContinue(request, response);
            }

            return results;
        }

        public Task<ScanResults<TItem>> Scan<TItem>() where TItem : class
        {
            return Scan<TItem>(null, null);
        }

        public Task<ScanResults<TItem>> Scan<TItem>(Expression<Func<TItem, bool>> filterPredicate) where TItem : class
        {
            return Scan(filterPredicate, null);
        }

        public async Task<ScanResults<TItem>> Scan<TItem>(Expression<Func<TItem, bool>> filterPredicate, ScanOptions options) where TItem : class
        {
            options ??= ScanOptions.Default();

            var request = new ScanRequest()
            {
                TableName = _context.TableName,
                ProjectionExpression = options.OnlyCount ? null : (new ItemProjectionHandler<TItem>(_context)).Build(),
                ExpressionAttributeNames = new(),
                ExpressionAttributeValues = new(),
                Limit = options.Limit,
                Select = options.OnlyCount ? Select.COUNT : Select.SPECIFIC_ATTRIBUTES
            };

            // add record type filter to the scan request
            request.AddItemTypeFilter<TContext, TItem>(_context);

            // construct and apply a filter if needed
            if (filterPredicate != null)
            {
                var filterDetails = new ItemConditionalExpressionHandler<TItem>(_context).Handle(filterPredicate, true);
                request.FilterExpression = request.FilterExpression + " AND (" + filterDetails.ConditionExpression + ")";
                request.MergeAttributeNames(filterDetails.AttributeNames);
                request.MergeAttributeValues(filterDetails.AttributeValues);
            }

            var response = await _client.ScanAsync(request);

            var results = new ScanResults<TItem>
            {
                Count = response.Count,
                ScannedCount = response.ScannedCount,
                Results = options.OnlyCount ? Enumerable.Empty<TItem>() : response.Items.Select(i => (new ItemAttributizer<TItem>(_context)).Deattributize(i))
            };

            if (response.LastEvaluatedKey != null)
            {
                results.SetContinue(request, response);
            }

            return results;
        }

        public async Task<ScanResults<TItem>> ContinueScan<TItem>(ScanResults<TItem> previousResults) where TItem : class
        {
            if (previousResults == null) throw new ArgumentNullException(nameof(previousResults));
            if (!previousResults.CanContinue) throw new UnableToContinueQueryException();

            // item type will have already been validated

            // we can reuse the previous scan
            var request = new ScanRequest
            {
                TableName = _context.TableName,
                ExclusiveStartKey = previousResults.ScanResponse.LastEvaluatedKey,
                ProjectionExpression = previousResults.ScanRequest.ProjectionExpression,
                ExpressionAttributeNames = previousResults.ScanRequest.ExpressionAttributeNames,
                ExpressionAttributeValues = previousResults.ScanRequest.ExpressionAttributeValues,
                FilterExpression = previousResults.ScanRequest.FilterExpression,
                Limit = previousResults.ScanRequest.Limit,
                Select = previousResults.ScanRequest.Select
            };

            var response = await _client.ScanAsync(request);

            var results = new ScanResults<TItem>
            {
                Count = response.Count,
                ScannedCount = response.ScannedCount,
                Results = (previousResults.ScanRequest.Select == Select.COUNT) ? Enumerable.Empty<TItem>() : response.Items.Select(i => (new ItemAttributizer<TItem>(_context)).Deattributize(i))
            };

            if (response.LastEvaluatedKey != null)
            {
                results.SetContinue(request, response);
            }

            return results;
        }
    }    
}