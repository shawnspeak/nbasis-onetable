﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace NBasis.OneTable
{
    internal class DynamoDbTable<TContext> : ITable<TContext> where TContext : TableContext
    {
        readonly IAmazonDynamoDB _client;
        readonly TContext _context;

        public DynamoDbTable(IAmazonDynamoDB client, TContext context)
        {
            _client = client;
            _context = context;
        }

        private string GetTableName()
        {
            return _context.TableName;
        }

        public async Task Create()
        {
            // build out table based upon context
            var request = new CreateTableRequest
            {
                TableName = GetTableName(),
                AttributeDefinitions = new System.Collections.Generic.List<AttributeDefinition>
                {
                    new(_context.Configuration.KeyAttributes.PKName, ScalarAttributeType.S),
                    new(_context.Configuration.KeyAttributes.SKName, ScalarAttributeType.S),
                },
                BillingMode = BillingMode.PAY_PER_REQUEST,
                KeySchema = new System.Collections.Generic.List<KeySchemaElement>
                {
                    new(_context.Configuration.KeyAttributes.PKName, KeyType.HASH),
                    new(_context.Configuration.KeyAttributes.SKName, KeyType.RANGE)
                }
            };

            // add the indexes
            for (int i = 0; i < _context.Configuration.GSIndexCount; i++)
            {
                int idx = i + 1;

                request.AttributeDefinitions.Add(
                    new AttributeDefinition(_context.GPKAttributeName(idx), ScalarAttributeType.S)
                );
                request.AttributeDefinitions.Add(
                    new AttributeDefinition(_context.GSKAttributeName(idx), ScalarAttributeType.S)
                );

                request.GlobalSecondaryIndexes.Add(new GlobalSecondaryIndex
                {
                    IndexName = _context.GSIndexName(idx),
                    KeySchema = new List<KeySchemaElement>
                    {
                        new(_context.GPKAttributeName(idx), KeyType.HASH),
                        new(_context.GSKAttributeName(idx), KeyType.RANGE)
                    },
                    Projection = new Projection
                    {
                        ProjectionType = ProjectionType.ALL
                    }
                });
            }

            await _client.CreateTableAsync(request);

        }

        public async Task Delete()
        {
            var request = new DeleteTableRequest
            { 
                TableName = GetTableName() 
            };
            await _client.DeleteTableAsync(request);
            
            // wrap amazon exception 
        }

        public async Task Ensure()
        {
            var exists = await this.Exists();
            if (!exists)
            {
                await this.Create();
            }
        }

        public async Task<bool> Exists()
        {
            var request = new DescribeTableRequest
            {
                TableName = GetTableName()
            };
            var response = await _client.DescribeTableAsync(request);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                return (response.Table?.TableName == GetTableName());
            }
            return false;
        }
    }
}
