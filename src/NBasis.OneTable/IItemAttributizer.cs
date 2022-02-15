using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Attributization;

namespace NBasis.OneTable
{
    public interface IItemAttributizer<TContext> where TContext : TableContext
    {
        Dictionary<string, AttributeValue> Attributize<TItem>(TItem item) where TItem : class;

        TItem Deattributize<TItem>(Dictionary<string, AttributeValue> item) where TItem : class;
    }

    public class DynamoDbItemAttributizer<TContext> : IItemAttributizer<TContext> where TContext : TableContext
    {
        readonly TContext _context;

        public DynamoDbItemAttributizer(
            TContext context
        )
        {
            _context = context;
        }

        public Dictionary<string, AttributeValue> Attributize<TItem>(TItem item) where TItem : class
        {
            return new ItemAttributizer<TItem>(_context).Attributize(item);
        }

        public TItem Deattributize<TItem>(Dictionary<string, AttributeValue> item) where TItem : class
        {
            return new ItemAttributizer<TItem>(_context).Deattributize(item);
        }
    }
}
