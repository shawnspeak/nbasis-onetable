using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Linq.Expressions;

namespace NBasis.OneTable
{
    public interface IItemStore<TContext> where TContext : TableContext
    {
        Task<TItem> Put<TItem>(TItem item) where TItem : class;

        Task<TItem> Update<TItem>(Expression<Func<TItem, bool>> keyPredicate, TItem item);

        Task Delete<TItem>(TItem item);

        Task Delete<TItem>(Expression<Func<TItem, bool>> keyPredicate) where TItem : class;
    }

    public class DynamoDbItemStore<TContext> : IItemStore<TContext> where TContext : TableContext
    {
        readonly IAmazonDynamoDB _client;
        readonly TContext _context;

        public DynamoDbItemStore(
            IAmazonDynamoDB client,
            TContext context
        )
        {
            _client = client;
            _context = context;
        }

        public Task Delete<TItem>(TItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            throw new NotImplementedException();
        }

        public async Task Delete<TItem>(Expression<Func<TItem, bool>> keyPredicate) where TItem : class
        {
            if (keyPredicate == null) throw new ArgumentNullException(nameof(keyPredicate));

            // get key
            var keyItem = (new KeyItemExpressionHandler<TItem>(_context).Handle(keyPredicate));

            var request = new DeleteItemRequest
            {
                TableName = _context.TableName,
                Key = keyItem
            };

            var response = await _client.DeleteItemAsync(request);
        }

        public async Task<TItem> Put<TItem>(TItem item) where TItem : class
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            return item;
        }

        public async Task<TItem> Update<TItem>(Expression<Func<TItem, bool>> keyPredicate, TItem item)
        {
            throw new NotImplementedException();
        }
    }
}
