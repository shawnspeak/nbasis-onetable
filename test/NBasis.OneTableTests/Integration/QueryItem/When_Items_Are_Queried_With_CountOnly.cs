using Microsoft.Extensions.DependencyInjection;
using NBasis.OneTable;
using NBasis.OneTableTests.Integration.TableCreation;
using System.Threading.Tasks;
using Xunit;

namespace NBasis.OneTableTests.Integration.QueryItem
{
    [Collection("DynamoDbDocker")]
    public class When_Items_Are_Queried_With_CountOnly : OneTableTestBase<TestTableContext>
    {
        public When_Items_Are_Queried_With_CountOnly(DynamoDbDockerFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Then_can_query_items_for_count()
        {
            var testClass1 = SimpleQueryItemTest.TestData();
            var testClass2 = SimpleQueryItemTest.TestData();
            var testClass3 = SimpleQueryItemTest.TestData("none");
            testClass2.PK = testClass1.PK;

            await Given();

            When(async (store, sp) =>
            {
                await store.Put(testClass1);
                await store.Put(testClass2);
                await store.Put(testClass3);
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                // make sure item is added
                var lookup = Container.GetRequiredService<IItemLookup<TestTableContext>>();

                var items = await lookup.Query<SimpleQueryItemTest>(i => i.PK == testClass1.PK, new QueryOptions() { OnlyCount = true });

                Assert.NotNull(items);
                Assert.Empty(items.Results);
                Assert.Equal(2, items.Count);
                Assert.Equal(2, items.ScannedCount);
            });
        }

        [Fact]
        public async Task Then_item_count_and_scan_count_are_accurate()
        {
            var testClass1 = SimpleQueryItemTest.TestData("filterTest");
            var testClass2 = SimpleQueryItemTest.TestData("filterTest");
            var testClass3 = SimpleQueryItemTest.TestData("none");
            testClass2.PK = testClass1.PK;
            testClass3.PK = testClass1.PK;

            await Given();

            When(async (store, sp) =>
            {
                await store.Put(testClass1);
                await store.Put(testClass2);
                await store.Put(testClass3);
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                // make sure item is added
                var lookup = Container.GetRequiredService<IItemLookup<TestTableContext>>();
                var items = await lookup.Query<SimpleQueryItemTest>(i => i.PK == testClass1.PK, i => i.Something == "filterTest", new QueryOptions() { OnlyCount = true });

                Assert.NotNull(items);
                Assert.Empty(items.Results);
                Assert.Equal(2, items.Count);
                Assert.Equal(3, items.ScannedCount);
            });
        }
    }
}
