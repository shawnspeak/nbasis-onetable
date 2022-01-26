using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.DependencyInjection;
using NBasis.OneTable;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NBasis.OneTableTests.Integration.TableCreation
{
    [Collection("DynamoDbDocker")]
    public class When_Only_Table_Name_Is_Given : ServiceProviderTestBase
    {
        readonly DynamoDbDockerFixture _fixture;

        public When_Only_Table_Name_Is_Given(DynamoDbDockerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Then_table_is_created()
        {
            string tableName = Guid.NewGuid().ToString();
            Given((sc) =>
            {
                sc.AddSingleton<IAmazonDynamoDB>(_fixture.DynamoDbClient);

                sc.AddOneTable<TestTableContext>(tableName);
            });

            When(async (sp) =>
            {
                var oneTable = sp.GetRequiredService<ITable<TestTableContext>>();

                await oneTable.CreateAsync();
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                // see if table exists
                var request = new DescribeTableRequest()
                {
                    TableName = tableName,
                };
                var table = await _fixture.DynamoDbClient.DescribeTableAsync(request);
                Assert.NotNull(table);
                Assert.True(table.HttpStatusCode == System.Net.HttpStatusCode.OK);
                Assert.Contains(tableName, table.Table?.TableName);
            });
        }

        [Fact]
        public async Task Then_table_default_indexes_are_created()
        {
            string tableName = Guid.NewGuid().ToString();
            Given((sc) =>
            {
                sc.AddSingleton<IAmazonDynamoDB>(_fixture.DynamoDbClient);

                sc.AddOneTable<TestTableContext>(tableName);
            });

            When(async (sp) =>
            {
                var oneTable = sp.GetRequiredService<ITable<TestTableContext>>();

                await oneTable.CreateAsync();
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                // see if table exists
                var request = new DescribeTableRequest()
                {
                    TableName = tableName,
                };
                var table = await _fixture.DynamoDbClient.DescribeTableAsync(request);
                Assert.NotNull(table);
                Assert.True(table.HttpStatusCode == System.Net.HttpStatusCode.OK);

                // two indexes
                Assert.Equal(2, table.Table?.GlobalSecondaryIndexes?.Count ?? 0);

                // correct names
                Assert.True(table.Table?.GlobalSecondaryIndexes.Any(gsi => gsi.IndexName == "gsi_1"));
                Assert.True(table.Table?.GlobalSecondaryIndexes.Any(gsi => gsi.IndexName == "gsi_2"));
            });
        }
    }

    public class TestTableContext : TableContext
    {
        // config
        // hooks        
    }
}
