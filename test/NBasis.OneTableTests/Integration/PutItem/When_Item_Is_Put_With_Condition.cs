using Microsoft.Extensions.DependencyInjection;
using NBasis.OneTable;
using NBasis.OneTable.Exceptions;
using NBasis.OneTableTests.Integration.TableCreation;
using System.Threading.Tasks;
using Xunit;

namespace NBasis.OneTableTests.Integration.PutItem
{
    [Collection("DynamoDbDocker")]
    public class When_Item_Is_Put_With_Condition : OneTableTestBase<TestTableContext>
    {
        public When_Item_Is_Put_With_Condition(DynamoDbDockerFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Then_item_is_added_with_none_existing()
        {
            var testClass = SimplePutItemTest.TestData();

            await Given();

            When(async (store, sp) =>
            {   
                await store.Put(testClass, tc => tc.PK.NotExists());
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                // can lookup item
                var lookup = Container.GetRequiredService<IItemLookup<TestTableContext>>();

                var gotItem = await lookup.Find<SimplePutItemTest>(i => i.PK == testClass.PK && i.SK == testClass.SK);

                Assert.NotNull(gotItem);
            });
        }

        [Fact]
        public async Task Then_item_fails_to_add_with_existing()
        {
            var testClass = SimplePutItemTest.TestData();            

            await Given(async (store, sp) =>
            {
                await store.Put(testClass);
            });

            When(async (store, sp) =>
            {
                await store.Put(testClass, tc => tc.PK.NotExists());
            });

            await Then(async (ex) =>
            {
                Assert.NotNull(ex);
                Assert.IsType<ConditionFailedException>(ex);
            });
        }
    }
}
