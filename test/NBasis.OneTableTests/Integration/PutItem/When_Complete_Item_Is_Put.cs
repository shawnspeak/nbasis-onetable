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
    public class ComplexPutItemTest
    {
        [PK("PKT")]
        public Guid PK { get; set; }

        [SK("SKT")]
        public string SK { get; set; }

        [GPK1("OID")]
        public string OtherId { get; set; }

        [GSK1("OID2")]
        public string OtherId2 { get; set; }

        [Attribute]
        public string Something { get; set; }

        [Attribute]
        public string Another { get; set; }

        [Attribute]
        public int SomeNum { get; set; }

        public static ComplexPutItemTest TestData()
        {
            return new ComplexPutItemTest()
            {
                PK = Guid.NewGuid(),
                SK = Guid.NewGuid().ToString(),
                OtherId = Guid.NewGuid().ToString(),
                OtherId2 = null,
                Something = "Wonderful",
                Another = null,
                SomeNum = 1
            };
        }
    }

    [Collection("DynamoDbDocker")]
    public class When_Complex_Item_Is_Put : OneTableTestBase<TestTableContext>
    {
        public When_Complex_Item_Is_Put(DynamoDbDockerFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Then_item_is_added()
        {
            var testClass = ComplexPutItemTest.TestData();

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
                        { "PK", new AttributeValue("PKT#" + testClass.PK.ToString()) },
                        { "SK", new AttributeValue("SKT#" + testClass.SK) }
                    }
                };

                var response = await dynamo.GetItemAsync(request);

                Assert.True(response.IsItemSet);

                Assert.Equal(5, response.Item.Count);

                Assert.Equal(testClass.PK.ToString(), response.Item["PK"].S.Replace("PKT#",""));
                Assert.Equal(testClass.SK, response.Item["SK"].S.Replace("SKT#",""));
                Assert.Equal(testClass.Something, response.Item["Something"].S);
            });
        }

        //[Fact]
        //public async Task Then_item_can_be_round_tripped()
        //{
        //    var testClass = SimplePutItemTest.TestData();

        //    await Given();

        //    When(async (store, sp) =>
        //    {
        //        await store.Put(testClass);
        //    });

        //    await Then(async (ex) =>
        //    {
        //        Assert.Null(ex);

        //        // can lookup item
        //        var lookup = Container.GetRequiredService<IItemLookup<TestTableContext>>();

        //        var gotItem = await lookup.Find<SimplePutItemTest>(i => i.PK == testClass.PK && i.SK == testClass.SK);

        //        Assert.NotNull(gotItem);

        //        Assert.Equal(testClass.PK, gotItem.PK);
        //        Assert.Equal(testClass.SK, gotItem.SK);
        //        Assert.Equal(testClass.Something, gotItem.Something);
        //    });
        //}
    }
}


