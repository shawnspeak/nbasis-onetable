using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NBasis.OneTableTests.Integration
{
    [Collection("DynamoDbDocker")]
    public class DynamoDbDockerTest
    {
        readonly DynamoDbDockerFixture _fixture;

        public DynamoDbDockerTest(DynamoDbDockerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CanCreateTable()
        {
            var tableName = Guid.NewGuid().ToString();
            var request = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new System.Collections.Generic.List<AttributeDefinition>
                {
                    new AttributeDefinition("p", ScalarAttributeType.S),
                    new AttributeDefinition("s", ScalarAttributeType.S),
                },
                BillingMode = BillingMode.PAY_PER_REQUEST,
                KeySchema = new System.Collections.Generic.List<KeySchemaElement>
                {
                    new KeySchemaElement("p", KeyType.HASH),
                    new KeySchemaElement("s", KeyType.RANGE)
                }
            };

            var response = await _fixture.DynamoDbClient.CreateTableAsync(request);
        
            Assert.NotNull(response);
            Assert.True(response.HttpStatusCode == System.Net.HttpStatusCode.OK);
            Assert.NotNull(response.TableDescription);
            Assert.Equal(response.TableDescription.TableName, tableName);
        }
    }
}

