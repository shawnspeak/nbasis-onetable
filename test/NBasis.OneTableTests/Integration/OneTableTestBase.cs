using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NBasis.OneTableTests.Integration
{
    [Collection("DynamoDbDocker")]
    public abstract class OneTableTestBase
    {
        readonly DynamoDbDockerFixture _fixture;

        public OneTableTestBase(DynamoDbDockerFixture fixture)
        {
            _fixture = fixture;
        }

        public abstract void Given();

        public abstract void When();

        public abstract void Then();
    }
}
