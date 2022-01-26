using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Linq.Expressions;

namespace NBasis.OneTable
{
    public interface IItemLookup<TContext> where TContext : TableContext
    {
        Task<TItem> Find<TItem>(Expression<Func<TItem, bool>> keyPredicate) where TItem : class;

        Task<QueryResults<TItem>> Query<TItem>(Func<TItem, bool> predicate) where TItem : class;
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

            // get key
            var keyItem = (new KeyItemExpressionHandler<TItem>(_context).Handle(predicate));

            // call get item
            var request = new GetItemRequest
            {
                TableName = _context.TableName,
                Key = keyItem
            };

            var response = await _client.GetItemAsync(request);

            if (response.IsItemSet)
                return ItemHydrator.Hydrate<TItem>(response.Item);

            return default(TItem);
        }

        public async Task<QueryResults<TItem>> Query<TItem>(Func<TItem, bool> predicate) where TItem : class
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            // build query from predicate

            var request = new QueryRequest
            {
                TableName = _context.TableName,
            };

            var response = await _client.QueryAsync(request);

            return null;
        }
    }  

    //// [OneTableItem( RecordType: "TYPE")]
    //// [Timestamp(Operation.Put | Operation.Update, "FIELDNAME")] // handled on the item, but not retrieve as a property
    //public class SomeItem
    //{
    //    [PK("PREFIX")] // required.. can combine.. prefix is optional
    //    [SK("PREFIX")] // optional.. can combine
    //    [GPK1] // can combine
    //    public string SomeId { get; set; }

    //    [GSK1]
    //    public string Email { get; set; }

    //    [Attribute("FIRST", AttributeType: String | Number, TypeConverter)]
    //    public string FirstName { get; set; }

    //    [Attribute()]
    //    public string LastName { get; set; }

    //    // [Timestamp(Operation.Put | Operation.Update, "FIELDNAME")] // exposes the timestamp via property
    //    public DateTimeOffset TS { get; set; }
    //}


    //public class QueryHandler<TResult>
    //{
    //    public async Task<TResult> Handle(IItemLookup lookup)
    //    {
    //        // set keys.. key checks are done based on 
    //        var item = await lookup.Find<SomeItem>(i => i.SomeId == "" && i.Email == "", FindOptions);


    //        var items = await lookup.Query<SomeItem>(i => i.SomeId == "" && i.Email == "", QueryOptions);
    //    }
    //}

    //public class CommandHandler<TResult>
    //{
    //    public async Task<TResult> Handle(IItemStore store)
    //    {
    //        // set keys.. key checks are done based on 
    //        await store.Delete<SomeItem>(i => i.SomeId == "" && i.Email == "", DeleteOptions);

    //        //await store.Delete<SomeItem>(item, DeleteOptions);

    //        //await store.Put<SomeItem>(put, PutOptions);
    //    }
    //}
}