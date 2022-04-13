using Microsoft.Extensions.DependencyInjection;
using NBasis.OneTable;
using NBasis.OneTable.Annotations;
using NBasis.OneTableTests.Integration.TableCreation;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NBasis.OneTableTests.Integration.QueryItem
{
    public class QueryItemWithGSITest
    {
        [PK]
        [GSK1]
        public string PK { get; set; }

        [SK]
        [GPK1]
        public string SK { get; set; }

        public static QueryItemWithGSITest TestData()
        {
            return new QueryItemWithGSITest()
            {
                PK = Guid.NewGuid().ToString(),
                SK = Guid.NewGuid().ToString()
            };
        }
    }

    [Collection("DynamoDbDocker")]
    public class When_Items_Are_Queried_With_Gsi : OneTableTestBase<TestTableContext>
    {
        public When_Items_Are_Queried_With_Gsi(DynamoDbDockerFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Then_can_query_items()
        {
            var testClass1 = QueryItemWithGSITest.TestData();
            var testClass2 = QueryItemWithGSITest.TestData();

            await Given();

            When(async (store, sp) =>
            {
                await store.Put(testClass1);
                await store.Put(testClass2);
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                // make sure item is added
                var lookup = Container.GetRequiredService<IItemLookup<TestTableContext>>();

                var items = await lookup.Query<QueryItemWithGSITest>(i => i.SK == testClass1.SK);

                Assert.NotNull(items);
                Assert.Single(items.Results);
            });
        }
    }
}
