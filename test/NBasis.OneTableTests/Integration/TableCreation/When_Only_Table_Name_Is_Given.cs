using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.DependencyInjection;
using NBasis.OneTable;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NBasis.OneTableTests.Integration.TableCreation
{
    [Collection("DynamoDbDocker")]
    public class When_Only_Table_Name_Is_Given : ServiceProviderTestBase
    {
        readonly DynamoDbDockerFixture _fixture;
        readonly string _tableName = Guid.NewGuid().ToString();

        public When_Only_Table_Name_Is_Given(DynamoDbDockerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Then_table_is_created()
        {
            Given((sc) =>
            {
                sc.AddSingleton<IAmazonDynamoDB>(_fixture.DynamoDbClient);

                sc.AddOneTable<TestTableContext>(_tableName);
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
                    TableName = _tableName,
                };
                var table = await _fixture.DynamoDbClient.DescribeTableAsync(request);
                Assert.NotNull(table);
                Assert.True(table.HttpStatusCode == System.Net.HttpStatusCode.OK);
                Assert.Contains(_tableName, table.Table?.TableName);
            });
        }
    }

    public class TestTableContext : TableContext
    {
        // config
        // hooks        
    }
}
