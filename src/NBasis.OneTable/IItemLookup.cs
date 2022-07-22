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
        public int Limit { get; set; }

        public Sort Sort { get; set; }

        public static QueryOptions Default()
        {
            return new QueryOptions()
            {
                Limit = int.MaxValue,
                Sort = Sort.Ascending
            };
        }
    }

    public interface IItemLookup<TContext> where TContext : TableContext
    {
        Task<TItem> Find<TItem>(Expression<Func<TItem, bool>> keyPredicate) where TItem : class;

        Task<QueryResults<TItem>> Query<TItem>(Expression<Func<TItem, bool>> keyPredicate) where TItem : class;

        Task<QueryResults<TItem>> Query<TItem>(Expression<Func<TItem, bool>> keyPredicate, QueryOptions options) where TItem : class;

        Task<QueryResults<TItem>> Query<TItem>(Expression<Func<TItem, bool>> krePredicate, Expression<Func<TItem, bool>> filterPredicate, QueryOptions options) where TItem : class;

        Task<QueryResults<TItem>> Count<TItem>(Func<TItem, bool> predicate) where TItem : class;

        Task<QueryResults<TItem>> ContinueQuery<TItem>(QueryResults<TItem> previousResults) where TItem : class;
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

        public async Task<QueryResults<TItem>> Query<TItem>(Expression<Func<TItem, bool>> keyPredicate, Expression<Func<TItem, bool>> filterPredicate, QueryOptions options = null) where TItem : class
        {
            if (keyPredicate == null) throw new ArgumentNullException(nameof(keyPredicate));

            options ??= QueryOptions.Default();

            // validate item type
            new ItemTypeValidator<TItem>(_context).Validate();

            // build query from predicate
            var queryDetails = new ItemQueryExpressionHandler<TItem>(_context).Handle(keyPredicate);

            var request = new QueryRequest
            {
                TableName = _context.TableName,
                ProjectionExpression = (new ItemProjectionHandler<TItem>(_context)).Build(),
                KeyConditionExpression = queryDetails.QueryExpression,
                ExpressionAttributeNames = queryDetails.AttributeNames,
                ExpressionAttributeValues = queryDetails.AttributeValues,
                ScanIndexForward = (options.Sort == Sort.Ascending),
                Limit = options.Limit,
                IndexName = queryDetails.IndexName
            };

            var response = await _client.QueryAsync(request);

            var results =  new QueryResults<TItem>
            {
                Count = response.Count,
                ScannedCount = response.ScannedCount,
                Results = response.Items.Select(i => (new ItemAttributizer<TItem>(_context)).Deattributize(i))
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
                ScanIndexForward = previousResults.QueryRequest.ScanIndexForward,
                Limit = previousResults.QueryRequest.Limit,
                IndexName = previousResults.QueryRequest.IndexName
            };

            var response = await _client.QueryAsync(request);

            var results = new QueryResults<TItem>
            {
                Count = response.Count,
                ScannedCount = response.ScannedCount,
                Results = response.Items.Select(i => (new ItemAttributizer<TItem>(_context)).Deattributize(i))
            };

            if (response.LastEvaluatedKey != null)
            {
                results.SetContinue(request, response);
            }

            return results;
        }

        public Task<QueryResults<TItem>> Count<TItem>(Func<TItem, bool> predicate) where TItem : class
        {
            throw new NotImplementedException();
        }
    }    
}