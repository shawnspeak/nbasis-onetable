using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.DependencyInjection;
using NBasis.OneTable;
using NBasis.OneTable.Annotations;
using NBasis.OneTableTests.Integration.TableCreation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace NBasis.OneTableTests.Integration.PutItem
{
    public class SimplePutItemTest
    {
        [PK]
        public string PK { get; set; }

        [SK]
        public string SK { get; set; }

        [Attribute]
        public string Something { get; set; }

        public static SimplePutItemTest TestData()
        {
            return new SimplePutItemTest()
            {
                PK = Guid.NewGuid().ToString(),
                SK = Guid.NewGuid().ToString(),
                Something = "Wonderful"
            };
        }
    }

    [Collection("DynamoDbDocker")]
    public class When_Item_Is_Put : OneTableTestBase<TestTableContext>
    {
        public When_Item_Is_Put(DynamoDbDockerFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Then_item_is_added()
        {
            var testClass = SimplePutItemTest.TestData();

            await Given();

            When(async (store, sp) =>
            {   
                await store.Put(testClass);
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                // make sure item is added
                var dynamo = Container.GetRequiredService<IAmazonDynamoDB>();

                var request = new GetItemRequest
                {
                    TableName = TableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "PK", new AttributeValue(testClass.PK) },
                        { "SK", new AttributeValue(testClass.SK) }
                    }
                };

                var response = await dynamo.GetItemAsync(request);

                Assert.True(response.IsItemSet);

                Assert.Equal(3, response.Item.Count);

                Assert.Equal(testClass.PK, response.Item["PK"].S);
                Assert.Equal(testClass.SK, response.Item["SK"].S);
                Assert.Equal(testClass.Something, response.Item["Something"].S);
            });
        }

        [Fact]
        public async Task Then_item_can_be_round_tripped()
        {
            var testClass = SimplePutItemTest.TestData();

            await Given();

            When(async (store, sp) =>
            {
                await store.Put(testClass);
            });

            await Then(async (ex) =>
            {
                Assert.Null(ex);

                // can lookup item
                var lookup = Container.GetRequiredService<IItemLookup<TestTableContext>>();

                var gotItem = await lookup.Find<SimplePutItemTest>(i => i.PK == testClass.PK && i.SK == testClass.SK);

                Assert.NotNull(gotItem);

                Assert.Equal(testClass.PK, gotItem.PK);
                Assert.Equal(testClass.SK, gotItem.SK);
                Assert.Equal(testClass.Something, gotItem.Something);
            });
        }
    }
}
