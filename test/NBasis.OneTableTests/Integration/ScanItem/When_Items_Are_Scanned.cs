using Microsoft.Extensions.DependencyInjection;
using NBasis.OneTable;
using NBasis.OneTable.Annotations;
using NBasis.OneTableTests.Integration.TableCreation;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NBasis.OneTableTests.Integration.ScanItem
{
    [ItemType("scanTest")]
    public class SimpleScanItemTest
    {
        [PK]
        public string PKScan { get; set; }

        [SK]
        public string SKScan { get; set; }

        [Attribute]
        public string Something { get; set; }

        public static SimpleScanItemTest TestData(string something = null)
        {
            return new SimpleScanItemTest()
            {
                PKScan = Guid.NewGuid().ToString(),
                SKScan = Guid.NewGuid().ToString(),
                Something = something   
            };
        }
    }

    [ItemType("scanTest2")]
    public class SimpleScanItemTest2
    {
        [PK]
        public string PKScan { get; set; }

        [SK]
        public string SKScan { get; set; }

        [Attribute]
        public string Something { get; set; }

        public static SimpleScanItemTest2 TestData(string something = null)
        {
            return new SimpleScanItemTest2()
            {
                PKScan = Guid.NewGuid().ToString(),
                SKScan = Guid.NewGuid().ToString(),
                Something = something
            };
        }
    }

    [Collection("DynamoDbDocker")]
    public class When_Items_Are_Scanned : OneTableTestBase<TestTableContext>
    {
        public When_Items_Are_Scanned(DynamoDbDockerFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Then_can_scan_items()
        {
            var testClass1 = SimpleScanItemTest.TestData("some");
            var testClass2 = SimpleScanItemTest.TestData("thing");

            await Given();

            When(async (store, sp) =>
            {
                await store.Put(testClass1);
                await store.Put(testClass2);
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                var lookup = Container.GetRequiredService<IItemLookup<TestTableContext>>();

                var items = await lookup.Scan<SimpleScanItemTest>(i => i.Something == "some");

                Assert.NotNull(items);
                Assert.Single(items.Results);
                Assert.Equal(1, items.Count);
                Assert.Equal(2, items.ScannedCount);
            });
        }

        [Fact]
        public async Task Then_can_scan_items_by_keys()
        {
            var testClass1 = SimpleScanItemTest.TestData("some");
            var testClass2 = SimpleScanItemTest.TestData("thing");

            await Given();

            When(async (store, sp) =>
            {
                await store.Put(testClass1);
                await store.Put(testClass2);
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                var lookup = Container.GetRequiredService<IItemLookup<TestTableContext>>();

                var items = await lookup.Scan<SimpleScanItemTest>(i => i.PKScan == testClass1.PKScan);

                Assert.NotNull(items);
                Assert.Single(items.Results);
                Assert.Equal(1, items.Count);
                Assert.Equal(2, items.ScannedCount);
            });
        }

        [Fact]
        public async Task Then_can_scan_items_by_item_types()
        {
            var testClass1 = SimpleScanItemTest.TestData("some");
            var testClass2 = SimpleScanItemTest.TestData("thing");
            var testClass3 = SimpleScanItemTest2.TestData("some");
            var testClass4 = SimpleScanItemTest2.TestData("thing");

            await Given();

            When(async (store, sp) =>
            {
                await store.Put(testClass1);
                await store.Put(testClass2);
                await store.Put(testClass3);
                await store.Put(testClass4);
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                // make sure item is added
                var lookup = Container.GetRequiredService<IItemLookup<TestTableContext>>();

                var items = await lookup.Scan<SimpleScanItemTest>(i => i.Something == "some");

                Assert.NotNull(items);
                Assert.Single(items.Results);
                Assert.Equal(1, items.Count);
                Assert.Equal(4, items.ScannedCount);
            });
        }
    }
}
