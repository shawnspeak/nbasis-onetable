using NBasis.OneTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NBasis.OneTableTests.Unit
{
    public class ItemKeyHandlerTests
    {
        [Fact]
        public void Item_key_is_handled()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemKeyHandler<TestClass>(tableContext);

            var item = new TestClass
            {
                Pk = "PK123",
                Sk = "SK321"
            };

            var key = expHandler.BuildKey(item);

            Assert.NotNull(key);

            Assert.Equal(2, key.Count);

            Assert.True(key.ContainsKey(tableContext.Configuration.KeyAttributes.PKName));
            Assert.True(key.ContainsKey(tableContext.Configuration.KeyAttributes.SKName));

            Assert.Equal("PK123", key[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal("SK321", key[tableContext.Configuration.KeyAttributes.SKName].S);
        }

        [Fact]
        public void Item_key_is_handled_with_prefixes()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemKeyHandler<TestClassWithPrefix>(tableContext);

            var item = new TestClassWithPrefix
            {
                Pk = "PK123",
                Sk = "SK321"
            };

            var key = expHandler.BuildKey(item);

            Assert.NotNull(key);

            Assert.Equal(2, key.Count);

            Assert.True(key.ContainsKey(tableContext.Configuration.KeyAttributes.PKName));
            Assert.True(key.ContainsKey(tableContext.Configuration.KeyAttributes.SKName));

            Assert.Equal("PRF#PK123", key[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal("USR#SK321", key[tableContext.Configuration.KeyAttributes.SKName].S);
        }
    }
}
