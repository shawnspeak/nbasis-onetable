using NBasis.OneTable;
using Xunit;

namespace NBasis.OneTableTests.Unit.Keys
{
    public class ItemKeyHandlerTests
    {
        [Fact]
        public void Item_key_is_handled()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemKeyHandler<TestClass>(tableContext);
            var item = TestClass.TestData();

            var key = expHandler.BuildKey(item);

            Assert.NotNull(key);

            Assert.Equal(2, key.Count);
            
            Assert.Equal(item.Pk, key[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal(item.Sk, key[tableContext.Configuration.KeyAttributes.SKName].S);
        }

        [Fact]
        public void Item_key_is_handled_with_prefixes()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemKeyHandler<TestClassWithPrefix>(tableContext);
            var item = TestClassWithPrefix.TestData();

            var key = expHandler.BuildKey(item);

            Assert.NotNull(key);

            Assert.Equal(2, key.Count);

            Assert.Equal("PRF#" + item.Pk, key[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal("USR#" + item.Sk, key[tableContext.Configuration.KeyAttributes.SKName].S);
        }

        [Fact]
        public void Item_key_with_non_string_is_handled()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemKeyHandler<TestClassWithNonStringTypes>(tableContext);
            var item = TestClassWithNonStringTypes.TestData();

            var key = expHandler.BuildKey(item);

            Assert.NotNull(key);

            Assert.Equal(2, key.Count);

            Assert.Equal(item.Pk.ToString(), key[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal(item.Sk.ToString(), key[tableContext.Configuration.KeyAttributes.SKName].N);
        }


        [Fact]
        public void Item_key_with_non_string_and_prefix_is_handled()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemKeyHandler<TestClassWithNonStringTypesAndPrefixes>(tableContext);
            var item = TestClassWithNonStringTypesAndPrefixes.TestData();

            var key = expHandler.BuildKey(item);

            Assert.NotNull(key);

            Assert.Equal(2, key.Count);

            // prefixed keys are always attribute type S
            Assert.Equal("USR#" + item.Pk.ToString(), key[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal("INV#" + item.Sk.ToString(), key[tableContext.Configuration.KeyAttributes.SKName].S);
        }
    }
}
