﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Attributization;
using NBasis.OneTable.Expressions;
using NBasis.OneTable.Validation;
using System.Linq.Expressions;

namespace NBasis.OneTable
{
    public interface IItemStore<TContext> where TContext : TableContext
    {
        Task<TItem> Put<TItem>(TItem item) where TItem : class;

        Task<TItem> Update<TItem>(TItem item) where TItem : class;

        Task<TItem> Patch<TItem, TPatch>(Expression<Func<TItem, bool>> keyPredicate, TPatch patch) where TItem : class where TPatch : class;

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
                ReturnValues = ReturnValue.NONE
            };

            var values = new ItemAttributizer<TItem>(_context).Attributize(item);
            var nonKeyValues = values.Where(v => !itemKey.ContainsKey(v.Key));
            if (nonKeyValues.Count() == 0)
            {
                // must have some values to update
            }

            foreach (var value in nonKeyValues)
            {
                request.AddUpdateItem(value.Key, value.Value);
            }

            await _client.UpdateItemAsync(request);

            return item;
        }

        public async Task<TItem> Patch<TItem, TPatch>(Expression<Func<TItem, bool>> keyPredicate, TPatch patch) 
            where TItem : class
            where TPatch: class
        {
            if (keyPredicate == null) throw new ArgumentNullException(nameof(keyPredicate));
            if (patch == null) throw new ArgumentNullException(nameof(patch));

            // validate type and patch
            new ItemTypeValidator<TItem>(_context).Validate();
            new PatchValidator<TPatch>(_context).Validate(patch);

            // get key
            var itemKey = new ItemKeyExpressionHandler<TItem>(_context).Handle(keyPredicate);

            // build update request based on patch
            var request = new UpdateItemRequest
            {
                TableName = _context.TableName,
                Key = itemKey,
                ReturnValues = ReturnValue.ALL_NEW
            };

            var values = new ItemAttributizer<TPatch>(_context).Attributize(patch);
            if (values?.Count == 0)
            {
                // must have some values to update
            }

            foreach (var value in values)
            {
                request.AddUpdateItem(value.Key, value.Value);
            }

            // return the full (updated) item
            var response = await _client.UpdateItemAsync(request);
            if (response.Attributes?.Count > 0)
                return new ItemAttributizer<TItem>(_context).Deattributize(response.Attributes);

            return default;
        }
    }
}
