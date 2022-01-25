using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NBasis.OneTable
{
    public interface IItemStore
    {
        Task<TItem> Put<TItem>(TItem item) where TItem : class;

        Task<TItem> Update<TItem>(Expression<Func<TItem, bool>> keyPredicate, TItem item);

        Task Delete<TItem>(TItem item);

        Task Delete<TItem>(Expression<Func<TItem, bool>> keyPredicate) where TItem : class;
    }

    public class DynamoDbItemStore : IItemStore
    {
        readonly IAmazonDynamoDB _client;
        readonly ITableNameResolver _tableNameResolver;

        public DynamoDbItemStore(
            IAmazonDynamoDB client,
            ITableNameResolver tableNameResolver
        )
        {
            _client = client;
            _tableNameResolver = tableNameResolver;
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
            var keyItem = (new KeyItemExpressionHandler<TItem>().Handle(keyPredicate));

            var request = new DeleteItemRequest
            {
                TableName = _tableNameResolver.GetTableName<TItem>(),
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
