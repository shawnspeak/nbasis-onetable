using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.DependencyInjection;
using NBasis.OneTable;
using NBasis.OneTable.Annotations;
using NBasis.OneTableTests.Integration.TableCreation;
using System;
using System.Collections.Generic;
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

        public static SimpleQueryItemTest TestData()
        {
            return new SimpleQueryItemTest()
            {
                PK = Guid.NewGuid().ToString(),
                SK = Guid.NewGuid().ToString()
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
    }
}
