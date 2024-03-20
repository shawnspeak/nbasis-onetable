using Microsoft.Extensions.DependencyInjection;
using NBasis.OneTable;
using NBasis.OneTable.Annotations;
using NBasis.OneTableTests.Integration.TableCreation;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NBasis.OneTableTests.Integration.UpdateItem
{
    //public class SimpleUpdateItemTest
    //{
    //    [PK("PKT")]
    //    [GSK1("PKT")]
    //    public Guid PK { get; set; }

    //    [SK("SKT")]
    //    public string SK { get; set; }

    //    [GPK1("OID")]
    //    public string OtherId { get; set; }
                
    //    [Attribute]
    //    public string Something { get; set; }

    //    [Attribute]
    //    public string Another { get; set; }

    //    [Attribute]
    //    public int SomeNum { get; set; }

    //    public static SimpleUpdateItemTest TestData()
    //    {
    //        return new SimpleUpdateItemTest()
    //        {
    //            PK = Guid.NewGuid(),
    //            SK = Guid.NewGuid().ToString(),
    //            OtherId = Guid.NewGuid().ToString(),
    //            Something = "Wonderful",
    //            Another = null,
    //            SomeNum = 1
    //        };
    //    }
    //}

    [Collection("DynamoDbDocker")]
    public class When_Item_Is_Updated_With_Null_GPK : OneTableTestBase<TestTableContext>
    {
        public When_Item_Is_Updated_With_Null_GPK(DynamoDbDockerFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Then_non_null_GPK_to_null_is_updated()
        {
            var testClass = SimpleUpdateItemTest.TestData();

            await Given(async (store, sp) =>
            {
                await store.Put(testClass);
            });

            When(async (store, sp) =>
            {
                testClass.OtherId = null;

                await store.Update(testClass);
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                // get item again
                var lookup = Container.GetRequiredService<IItemLookup<TestTableContext>>();

                var updateditem = await lookup.Find<SimpleUpdateItemTest>(i => i.PK == testClass.PK && i.SK == testClass.SK);

                Assert.NotNull(updateditem);

                Assert.Equal(testClass.Another, updateditem.Another);
                Assert.Equal(testClass.SomeNum, updateditem.SomeNum);

                Assert.Equal(testClass.PK, updateditem.PK);
                Assert.Equal(testClass.SK, updateditem.SK);
                Assert.Equal(testClass.Something, updateditem.Something);

                Assert.Null(updateditem.OtherId);
            });
        }

        [Fact]
        public async Task Then_null_GPK_to_null_is_still_null()
        {
            var testClass = SimpleUpdateItemTest.TestData();
            testClass.OtherId = null;

            await Given(async (store, sp) =>
            {
                await store.Put(testClass);
            });

            When(async (store, sp) =>
            {
                testClass.OtherId = null;

                await store.Update(testClass);
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                // get item again
                var lookup = Container.GetRequiredService<IItemLookup<TestTableContext>>();

                var updateditem = await lookup.Find<SimpleUpdateItemTest>(i => i.PK == testClass.PK && i.SK == testClass.SK);

                Assert.NotNull(updateditem);

                Assert.Equal(testClass.Another, updateditem.Another);
                Assert.Equal(testClass.SomeNum, updateditem.SomeNum);

                Assert.Equal(testClass.PK, updateditem.PK);
                Assert.Equal(testClass.SK, updateditem.SK);
                Assert.Equal(testClass.Something, updateditem.Something);

                Assert.Null(updateditem.OtherId);
            });
        }
    }
}
