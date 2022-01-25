using Amazon.DynamoDBv2;
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
                var tables = await _fixture.DynamoDbClient.ListTablesAsync();
                Assert.NotNull(tables);
                Assert.True(tables.HttpStatusCode == System.Net.HttpStatusCode.OK);
                Assert.Contains(_tableName, tables.TableNames);
            });
        }
    }

    public class TestTableContext : TableContext
    {
        // config
        // hooks
        // item types?
    }
}
