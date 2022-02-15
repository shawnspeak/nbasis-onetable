using Xunit;

namespace NBasis.OneTableTests.Integration
{
    [CollectionDefinition("DynamoDbDocker")]
    public class DynamoDbDockerCollection : ICollectionFixture<DynamoDbDockerFixture>
    {
    }
}
