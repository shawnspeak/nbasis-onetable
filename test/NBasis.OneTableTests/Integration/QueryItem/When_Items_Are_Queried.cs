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
    public class SimpleQueryItemTest
    {
        [PK]
        public string PK { get; set; }

        [SK]
        public string SK { get; set; }

        [Attribute]
        public string Something { get; set; }

        public static SimpleQueryItemTest TestData(string something = null)
        {
            return new SimpleQueryItemTest()
            {
                PK = Guid.NewGuid().ToString(),
                SK = Guid.NewGuid().ToString(),
                Something = something   
            };
        }
    }

    [Collection("DynamoDbDocker")]
    public class When_Items_Are_Queried : OneTableTestBase<TestTableContext>
    {
        public When_Items_Are_Queried(DynamoDbDockerFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Then_can_query_items()
        {
            var testClass1 = SimpleQueryItemTest.TestData();
            var testClass2 = SimpleQueryItemTest.TestData();
            testClass2.PK = testClass1.PK;

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

                var items = await lookup.Query<SimpleQueryItemTest>(i => i.PK == testClass1.PK);

                Assert.NotNull(items);
                Assert.Equal(2, items.Results.Count());
            });
        }

        [Fact]
        public async Task Only_non_keys_can_be_filtered()
        {
            var testClass1 = SimpleQueryItemTest.TestData();
            var testClass2 = SimpleQueryItemTest.TestData();
            testClass2.PK = testClass1.PK;

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

                Exception noKeysException = null;
                try
                {
                    var items = await lookup.Query<SimpleQueryItemTest>(i => i.PK == testClass1.PK, i => i.SK == "TEST");
                }
                catch (Exception nke)
                {
                    noKeysException = nke;
                }
                
                Assert.NotNull(noKeysException);
            });
        }

        [Fact]
        public async Task Then_items_can_be_filtered()
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
                var items = await lookup.Query<SimpleQueryItemTest>(i => i.PK == testClass1.PK, i => i.Something == "filterTest");

                Assert.NotNull(items);
                Assert.Equal(2, items.Results.Count());
                Assert.Equal(3, items.ScannedCount);
            });
        }

        [Fact]
        public async Task Then_items_can_be_filtered_on_nulls()
        {
            var testClass1 = SimpleQueryItemTest.TestData();
            var testClass2 = SimpleQueryItemTest.TestData("else");
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
                var items = await lookup.Query<SimpleQueryItemTest>(i => i.PK == testClass1.PK, i => i.Something == null);

                Assert.NotNull(items);
                Assert.Equal(1, items.Results.Count());
                Assert.Equal(3, items.ScannedCount);
            });
        }
    }
}
