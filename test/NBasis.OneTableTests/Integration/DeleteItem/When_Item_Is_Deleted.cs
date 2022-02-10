using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.DependencyInjection;
using NBasis.OneTable;
using NBasis.OneTable.Annotations;
using NBasis.OneTableTests.Integration.TableCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NBasis.OneTableTests.Integration.DeleteItem
{
    public class SimpleDeleteItemTest
    {
        [PK]
        public string PK { get; set; }

        [SK]
        public string SK { get; set; }

        [Attribute]
        public string Something { get; set; }

        public static SimpleDeleteItemTest TestData()
        {
            return new SimpleDeleteItemTest()
            {
                PK = Guid.NewGuid().ToString(),
                SK = Guid.NewGuid().ToString(),
                Something = "Wonderful"
            };
        }
    }

    [Collection("DynamoDbDocker")]
    public class When_Item_Is_Deleted : OneTableTestBase<TestTableContext>
    {
        public When_Item_Is_Deleted(DynamoDbDockerFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Then_item_is_gone()
        {
            var testClass = SimpleDeleteItemTest.TestData();

            await Given(async (store, sp) =>
            {
                var request = new PutItemRequest
                {
                    TableName = TableName,
                    Item = new Dictionary<string, AttributeValue>
                    {
                        { "PK", new AttributeValue(testClass.PK) },
                        { "SK", new AttributeValue(testClass.SK) },
                        { "Something", new AttributeValue(testClass.Something) }
                    }
                };

                var dynamo = Container.GetRequiredService<IAmazonDynamoDB>();
                var response = await dynamo.PutItemAsync(request);
            });

            When(async (store, sp) =>
            {
                await store.Delete(testClass);
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                // make sure item is gone
                var dynamo = Container.GetRequiredService<IAmazonDynamoDB>();

                // should be no items
                var request = new ScanRequest
                {
                    TableName = TableName
                };

                var response = await dynamo.ScanAsync(request);
                Assert.Equal(0, response.Count);
            });
        }

        [Fact]
        public async Task Then_put_item_is_gone()
        {
            var testClass = SimpleDeleteItemTest.TestData();

            await Given(async (store, sp) =>
            {
                await store.Put(testClass);
            });

            When(async (store, sp) =>
            {
                await store.Delete(testClass);
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                // make sure item is gone
                var dynamo = Container.GetRequiredService<IAmazonDynamoDB>();

                // should be no items
                var request = new ScanRequest
                {
                    TableName = TableName
                };

                var response = await dynamo.ScanAsync(request);
                Assert.Equal(0, response.Count);
            });
        }

        [Fact]
        public async Task Then_roundtrip_item_is_gone()
        {
            var testClass = SimpleDeleteItemTest.TestData();

            await Given(async (store, sp) =>
            {
                await store.Put(testClass);
            });

            When(async (store, sp) =>
            {
                await store.Delete(testClass);
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
    }
}
