using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NBasis.OneTableTests.Integration
{
    [CollectionDefinition("DynamoDbDocker")]
    public class DynamoDbDockerCollection : ICollectionFixture<DynamoDbDockerFixture>
    {
    }
}
