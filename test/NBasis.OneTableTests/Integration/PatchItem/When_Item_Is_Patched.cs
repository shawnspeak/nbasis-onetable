using Microsoft.Extensions.DependencyInjection;
using NBasis.OneTable;
using NBasis.OneTable.Annotations;
using NBasis.OneTableTests.Integration.TableCreation;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NBasis.OneTableTests.Integration.PatchItem
{
    public class SimplePatchItemTest
    {
        [PK]
        public string PK { get; set; }

        [SK]
        public string SK { get; set; }

        [Attribute]
        public string Something { get; set; }

        [Attribute]
        public string Another { get; set; }

        [Attribute]
        public int SomeNum { get; set; }

        public static SimplePatchItemTest TestData()
        {
            return new SimplePatchItemTest()
            {
                PK = Guid.NewGuid().ToString(),
                SK = Guid.NewGuid().ToString(),
                Something = "Wonderful",
                Another = Guid.NewGuid().ToString(),
                SomeNum = Guid.NewGuid().GetHashCode()
            };
        }
    }

    public class SimplePatchTest
    {
        [Attribute]
        public string Another { get; set; }

        [Attribute]
        public int SomeNum { get; set; }

        public static SimplePatchTest TestData()
        {
            return new SimplePatchTest()
            {
                Another = Guid.NewGuid().ToString(),
                SomeNum = Guid.NewGuid().GetHashCode()
            };
        }
    }

    [Collection("DynamoDbDocker")]
    public class When_Item_Is_Patched : OneTableTestBase<TestTableContext>
    {
        public When_Item_Is_Patched(DynamoDbDockerFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Then_roundtrip_item_is_patched()
        {
            var testClass = SimplePatchItemTest.TestData();
            var patchClass = SimplePatchTest.TestData();

            await Given(async (store, sp) =>
            {
                await store.Put(testClass);
            });

            When(async (store, sp) =>
            {
                await store.Patch<SimplePatchItemTest, SimplePatchTest>(i => i.PK == testClass.PK && i.SK == testClass.SK, patchClass);
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                // get item again
                var lookup = Container.GetRequiredService<IItemLookup<TestTableContext>>();

                var patcheditem = await lookup.Find<SimplePatchItemTest>(i => i.PK == testClass.PK && i.SK == testClass.SK);

                Assert.NotNull(patcheditem);

                Assert.Equal(patchClass.Another, patcheditem.Another);
                Assert.Equal(patchClass.SomeNum, patcheditem.SomeNum);

                Assert.NotEqual(patchClass.Another, testClass.Another);
                Assert.NotEqual(patchClass.SomeNum, testClass.SomeNum);

                Assert.Equal(testClass.PK, patcheditem.PK);
                Assert.Equal(testClass.SK, patcheditem.SK);
                Assert.Equal(testClass.Something, patcheditem.Something);
            });
        }
    }
}
