using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Attributization;
using NBasis.OneTable.Expressions;
using NBasis.OneTable.Validation;
using System.Linq.Expressions;

namespace NBasis.OneTable
{
    public interface IItemTransactionResult
    {

    }

    public interface IItemTransaction
    {
        void AddPut<TItem>(TItem item, Expression<Func<TItem, bool>> conditionalExpression = null) where TItem : class;

        void AddUpdate<TItem>(TItem item, Expression<Func<TItem, bool>> conditionalExpression) where TItem : class;

        void AddPatch<TItem, TPatch>(Expression<Func<TItem, bool>> keyPredicate, TPatch patch) where TItem : class where TPatch : class;

        void AddDelete<TItem>(TItem item, Expression<Func<TItem, bool>> conditionalExpression = null) where TItem : class;

        void AddDelete<TItem>(Expression<Func<TItem, bool>> keyPredicate, Expression<Func<TItem, bool>> conditionalExpression = null) where TItem : class;

        void AddConditionCheck<TItem>(Expression<Func<TItem, bool>> keyPredicate, Expression<Func<TItem, bool>> conditionalExpression) where TItem : class;

        Task<IItemTransactionResult> Commit();
    }

    public class DynamoDbItemTransaction<TContext> : IItemTransaction where TContext : TableContext
    {
        readonly IAmazonDynamoDB _client;
        readonly TContext _context;

        private readonly List<TransactWriteItem> _transactItems = new();
        private TransactWriteItemsResponse _response;

        public DynamoDbItemTransaction(
            IAmazonDynamoDB client,
            TContext context
        )
        {
            _client = client;
            _context = context;
        }

        public async Task<IItemTransactionResult> Commit()
        {
            if (_response != null) throw new ApplicationException("already committed");

            var request = new TransactWriteItemsRequest
            {
                TransactItems = _transactItems
            };

            _response = await _client.TransactWriteItemsAsync(request);

            return null;
        }

        public void AddDelete<TItem>(TItem item, Expression<Func<TItem, bool>> conditionalExpression = null) where TItem : class
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            // validate item
            new ItemValidator<TItem>(_context).Validate(item);
            
            // get the key from the item
            var itemKey = (new ItemKeyHandler<TItem>(_context)).BuildKey(item);

            AddDelete<TItem>(itemKey, conditionalExpression);
        }        

        public void AddDelete<TItem>(Expression<Func<TItem, bool>> keyPredicate, Expression<Func<TItem, bool>> conditionalExpression = null) where TItem : class
        {
            if (keyPredicate == null) throw new ArgumentNullException(nameof(keyPredicate));

            // validate item type
            new ItemTypeValidator<TItem>(_context).Validate();

            // get key
            var itemKey = new ItemKeyExpressionHandler<TItem>(_context).Handle(keyPredicate);
            AddDelete<TItem>(itemKey, conditionalExpression);
        }

        private void AddDelete<TItem>(Dictionary<string, AttributeValue> key, Expression<Func<TItem, bool>> conditionalExpression) where TItem : class
        {
            var delete = new Delete
            {
                TableName = _context.TableName,
                Key = key
            };

            // deal with conditional
            if (conditionalExpression != null)
            {
                var conditionalDetails = new ItemConditionalExpressionHandler<TItem>(_context).Handle(conditionalExpression, true);
                delete.ExpressionAttributeNames = conditionalDetails.AttributeNames;
                delete.ExpressionAttributeValues = conditionalDetails.AttributeValues;
                delete.ConditionExpression = conditionalDetails.ConditionExpression;
            }

            // add to transaction
            _transactItems.Add(new TransactWriteItem
            {
                Delete = delete,
            });
        }


        public void AddPatch<TItem, TPatch>(Expression<Func<TItem, bool>> keyPredicate, TPatch patch)
            where TItem : class
            where TPatch : class
        {
            throw new NotImplementedException();
        }
        
        public void AddPut<TItem>(TItem item, Expression<Func<TItem, bool>> conditionalExpression = null) where TItem : class
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            // validate item
            new ItemValidator<TItem>(_context).Validate(item);

            // attributize item
            var attributes = new ItemAttributizer<TItem>(_context).Attributize(item);

            var put = new Put
            {
                TableName = _context.TableName,
                Item = attributes
            };

            // deal with conditional
            if (conditionalExpression != null)
            {
                var conditionalDetails = new ItemConditionalExpressionHandler<TItem>(_context).Handle(conditionalExpression, true);

                put.ExpressionAttributeNames = conditionalDetails.AttributeNames;
                put.ExpressionAttributeValues = conditionalDetails.AttributeValues;
                put.ConditionExpression = conditionalDetails.ConditionExpression;
            }

            // add to transaction
            _transactItems.Add(new TransactWriteItem
            {
                Put = put,
            });
        }

        public void AddUpdate<TItem>(TItem item, Expression<Func<TItem, bool>> conditionalExpression = null) where TItem : class
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            // validate item
            new ItemValidator<TItem>(_context).Validate(item);

            // get the key from the item
            var itemKey = (new ItemKeyHandler<TItem>(_context)).BuildKey(item);

            var update = new Update
            {
                TableName = _context.TableName,
                Key = itemKey
            };

            var values = new ItemAttributizer<TItem>(_context).Attributize(item);
            var nonKeyValues = values.Where(v => !itemKey.ContainsKey(v.Key));
            if (!nonKeyValues.Any())
            {
                // must have some values to update
            }

            foreach (var value in nonKeyValues)
            {
                update.AddUpdateItem(value.Key, value.Value);
            }

            // deal with conditional
            if (conditionalExpression != null)
            {
                var conditionalDetails = new ItemConditionalExpressionHandler<TItem>(_context).Handle(conditionalExpression, true);

                foreach (var conditionalName in conditionalDetails.AttributeNames)
                {
                    update.ExpressionAttributeNames.Add(conditionalName.Key, conditionalName.Value);
                }

                foreach (var conditionalValue in conditionalDetails.AttributeValues)
                {
                    update.ExpressionAttributeValues.Add(conditionalValue.Key, conditionalValue.Value);
                }

                update.ConditionExpression = conditionalDetails.ConditionExpression;
            }

            // add to transaction            
            _transactItems.Add(new TransactWriteItem
            {
                Update = update,
            });
        }

        public void AddConditionCheck<TItem>(Expression<Func<TItem, bool>> keyPredicate, Expression<Func<TItem, bool>> conditionalExpression) where TItem : class
        {
            if (keyPredicate == null) throw new ArgumentNullException(nameof(keyPredicate));
            if (conditionalExpression == null) throw new ArgumentNullException(nameof(conditionalExpression));

            // validate item type
            new ItemTypeValidator<TItem>(_context).Validate();

            // get key
            var itemKey = new ItemKeyExpressionHandler<TItem>(_context).Handle(keyPredicate);

            var check = new ConditionCheck
            {
                TableName = _context.TableName,
                Key = itemKey
            };

            // deal with conditional
            if (conditionalExpression != null)
            {
                var conditionalDetails = new ItemConditionalExpressionHandler<TItem>(_context).Handle(conditionalExpression, true);

                check.ExpressionAttributeNames = conditionalDetails.AttributeNames;
                check.ExpressionAttributeValues = conditionalDetails.AttributeValues;
                check.ConditionExpression = conditionalDetails.ConditionExpression;
            }

            // add to transaction
            _transactItems.Add(new TransactWriteItem
            {
                ConditionCheck = check,
            });
        }
    }
}
