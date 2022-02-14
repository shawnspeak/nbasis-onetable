using NBasis.OneTable.Expressions;
using System;
using Xunit;

namespace NBasis.OneTableTests.Unit.Keys
{
    public class ItemKeyExpressionHandlerTests
    {
        [Fact]
        public void PK_is_set_correctly()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemKeyExpressionHandler<TestClass>(tableContext);

            var keyItem = expHandler.Handle(i => i.Pk == "12");

            Assert.NotNull(keyItem);

            Assert.Single(keyItem);

            Assert.Equal("12", keyItem[tableContext.Configuration.KeyAttributes.PKName].S);
        }

        [Fact]
        public void PK_and_SK_are_set_correctly()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemKeyExpressionHandler<TestClass>(tableContext);

            var keyItem = expHandler.Handle(i => i.Pk == "12" && i.Sk == "321");

            Assert.NotNull(keyItem);

            Assert.Equal(2, keyItem.Count);

            Assert.Equal("12", keyItem[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal("321", keyItem[tableContext.Configuration.KeyAttributes.SKName].S);
        }

        [Fact]
        public void PK_and_SK_are_set_correctly_with_variables()
        {
            string pkVariable = "21" + "2";
            string skVariable = "12" + "3";

            var tableContext = new TestContext();
            var expHandler = new ItemKeyExpressionHandler<TestClass>(tableContext);

            var keyItem = expHandler.Handle(i => i.Pk == pkVariable && i.Sk == skVariable);

            Assert.NotNull(keyItem);

            Assert.Equal(2, keyItem.Count);

            Assert.Equal(pkVariable, keyItem[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal(skVariable, keyItem[tableContext.Configuration.KeyAttributes.SKName].S);
        }

        [Fact]
        public void PK_and_SK_are_set_correctly_with_prefixes()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemKeyExpressionHandler<TestClassWithPrefix>(tableContext);

            var keyItem = expHandler.Handle(i => i.Pk == "12" && i.Sk == "321");

            Assert.NotNull(keyItem);

            Assert.Equal(2, keyItem.Count);


            Assert.Equal("PRF#12", keyItem[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal("USR#321", keyItem[tableContext.Configuration.KeyAttributes.SKName].S);
        }

        [Fact]
        public void PK_and_SK_are_set_correctly_with_class_properties()
        {
            var testClass = new TestClass()
            {
                Pk = "21" + "2",
                Sk = "12" + "3"
            };

            var tableContext = new TestContext();
            var expHandler = new ItemKeyExpressionHandler<TestClass>(tableContext);

            var keyItem = expHandler.Handle(i => i.Pk == testClass.Pk && i.Sk == testClass.Sk);

            Assert.NotNull(keyItem);

            Assert.Equal(2, keyItem.Count);

            Assert.Equal(testClass.Pk, keyItem[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal(testClass.Sk, keyItem[tableContext.Configuration.KeyAttributes.SKName].S);
        }

        [Fact]
        public void PK_and_SK_are_set_correctly_with_non_string()
        {
            Guid pkVariable = Guid.NewGuid();
            int skVariable = Guid.NewGuid().GetHashCode();

            var tableContext = new TestContext();
            var expHandler = new ItemKeyExpressionHandler<TestClassWithNonStringTypes>(tableContext);

            var keyItem = expHandler.Handle(i => i.Pk == pkVariable && i.Sk == skVariable);

            Assert.NotNull(keyItem);

            Assert.Equal(2, keyItem.Count);

            Assert.Equal(pkVariable.ToString(), keyItem[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal(skVariable.ToString(), keyItem[tableContext.Configuration.KeyAttributes.SKName].N);
        }
    }
}