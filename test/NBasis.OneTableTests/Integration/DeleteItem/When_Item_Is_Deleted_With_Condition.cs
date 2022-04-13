using Microsoft.Extensions.DependencyInjection;
using NBasis.OneTable;
using NBasis.OneTable.Exceptions;
using NBasis.OneTableTests.Integration.TableCreation;
using System.Threading.Tasks;
using Xunit;

namespace NBasis.OneTableTests.Integration.DeleteItem
{
    [Collection("DynamoDbDocker")]
    public class When_Item_Is_Deleted_With_Condition : OneTableTestBase<TestTableContext>
    {
        public When_Item_Is_Deleted_With_Condition(DynamoDbDockerFixture fixture) : base(fixture)
        {
        }        

        [Fact]
        public async Task Then_item_is_gone()
        {
            var testClass = SimpleDeleteItemTest.TestData();

            await Given(async (store, sp) =>
            {
                await store.Put(testClass);
            });

            When(async (store, sp) =>
            {
                await store.Delete(testClass, tc => tc.Something == "Wonderful");
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                // make sure item is gone
                var lookup = Container.GetRequiredService<IItemLookup<TestTableContext>>();

                var item = await lookup.Find<SimpleDeleteItemTest>(i => i.PK == testClass.PK && i.SK == testClass.SK);

                Assert.Null(item);
            });
        }

        [Fact]
        public async Task Then_failed_condition_is_thrown()
        {
            var testClass = SimpleDeleteItemTest.TestData();

            await Given(async (store, sp) =>
            {
                await store.Put(testClass);
            });

            When(async (store, sp) =>
            {
                await store.Delete(testClass, tc => tc.Something == "Not so wonderful");
            });

            await Then(async (ex) =>
            {
                Assert.NotNull(ex);
                Assert.IsType<ConditionFailedException>(ex);

                // make sure item is still there
                var lookup = Container.GetRequiredService<IItemLookup<TestTableContext>>();
                var item = await lookup.Find<SimpleDeleteItemTest>(i => i.PK == testClass.PK && i.SK == testClass.SK);
                Assert.NotNull(item);
            });
        }

        [Fact]
        public async Task Then_failed_existence_condition_is_thrown()
        {
            var testClass = SimpleDeleteItemTest.TestData();

            await Given(null);

            When(async (store, sp) =>
            {
                await store.Delete(testClass, tc => tc.PK.Exists());
            });

            await Then((ex) =>
            {
                Assert.NotNull(ex);
                Assert.IsType<ConditionFailedException>(ex);

                return Task.CompletedTask;
            });
        }
    }
}
