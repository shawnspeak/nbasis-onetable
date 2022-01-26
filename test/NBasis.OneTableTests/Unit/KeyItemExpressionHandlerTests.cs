using NBasis.OneTable;
using NBasis.OneTable.Annotations;
using Xunit;

namespace NBasis.OneTableTests
{
    public class TestClass
    {
        [PK]
        public string Pk { get; set; }

        [SK]
        public string Sk { get; set; }
    }

    public class TestContext : TableContext
    {
        public TestContext()
        {
            Configuration = TableConfiguration.Default();
        }
    }

    public class KeyItemExpressionHandlerTests
    {
        [Fact]
        public void PK_is_set_correctly()
        {
            var tableContext = new TestContext();
            var expHandler = new KeyItemExpressionHandler<TestClass>(tableContext);

            var keyItem = expHandler.Handle(i => i.Pk == "12");

            Assert.NotNull(keyItem);

            Assert.Single(keyItem);

            Assert.True(keyItem.ContainsKey(tableContext.Configuration.KeyAttributes.PKName));
            Assert.Equal("12", keyItem[tableContext.Configuration.KeyAttributes.PKName].S);
        }

        [Fact]
        public void PK_and_SK_are_set_correctly()
        {
            var tableContext = new TestContext();
            var expHandler = new KeyItemExpressionHandler<TestClass>(tableContext);

            var keyItem = expHandler.Handle(i => i.Pk == "12" && i.Sk == "321");

            Assert.NotNull(keyItem);

            Assert.Equal(2, keyItem.Count);

            Assert.True(keyItem.ContainsKey(tableContext.Configuration.KeyAttributes.PKName));
            Assert.Equal("12", keyItem[tableContext.Configuration.KeyAttributes.PKName].S);

            Assert.True(keyItem.ContainsKey(tableContext.Configuration.KeyAttributes.SKName));
            Assert.Equal("321", keyItem[tableContext.Configuration.KeyAttributes.SKName].S);
        }
    }
}