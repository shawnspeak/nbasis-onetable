using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Attributization;
using NBasis.OneTable.Validation;
using System.Linq.Expressions;

namespace NBasis.OneTable
{
    public interface IItemStore<TContext> where TContext : TableContext
    {
        Task<TItem> Put<TItem>(TItem item) where TItem : class;

        Task<TItem> Update<TItem>(TItem item) where TItem : class;

        Task Delete<TItem>(TItem item) where TItem : class;

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

        public async Task Delete<TItem>(TItem item) where TItem : class
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            // validate item
            new ItemValidator<TItem>(_context).ValidateKeys(item);

            // get the key from the item
            var itemKey = (new ItemKeyHandler<TItem>(_context)).BuildKey(item);

            var request = new DeleteItemRequest
            {
                TableName = _context.TableName,
                Key = itemKey
            };

            await _client.DeleteItemAsync(request);
        }

        public async Task Delete<TItem>(Expression<Func<TItem, bool>> keyPredicate) where TItem : class
        {
            if (keyPredicate == null) throw new ArgumentNullException(nameof(keyPredicate));

            // validate item type
            new ItemTypeValidator<TItem>(_context).Validate();

            // get key
            var itemKey = new ItemKeyExpressionHandler<TItem>(_context).Handle(keyPredicate);

            var request = new DeleteItemRequest
            {
                TableName = _context.TableName,
                Key = itemKey
            };

            await _client.DeleteItemAsync(request);
        }

        public async Task<TItem> Put<TItem>(TItem item) where TItem : class
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            // validate item
            new ItemValidator<TItem>(_context).Validate(item);

            // attributize item
            var attributes = new ItemAttributizer<TItem>(_context).Attributize(item);

            var request = new PutItemRequest
            {
                TableName = _context.TableName,
                Item = attributes
            };

            await _client.PutItemAsync(request);

            return item;
        }

        public async Task<TItem> Update<TItem>(TItem item) where TItem : class
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            // validate item
            new ItemValidator<TItem>(_context).Validate(item);

            // get the key from the item
            var itemKey = (new ItemKeyHandler<TItem>(_context)).BuildKey(item);

            var request = new UpdateItemRequest
            {
                TableName = _context.TableName,
                Key = itemKey,
                ReturnValues = ReturnValue.ALL_NEW,
            };

            await _client.UpdateItemAsync(request);

            return item;
        }
    }
}
