using NBasis.OneTable;
using NBasis.OneTable.Annotations;
using NBasis.OneTable.Expressions;
using NBasis.OneTableTests.Unit.Keys;
using System;
using Xunit;

namespace NBasis.OneTableTests.Unit.ConditionExpressions
{
    public class TestClassForConditional
    {
        [PK]
        public string Pk { get; set; }

        [SK]
        public int Sk { get; set; }

        [Attribute]
        public int Other { get; set; }

        public static TestClassWithNonStringTypes TestData()
        {
            return new TestClassWithNonStringTypes
            {
                Pk = Guid.NewGuid(),
                Sk = Guid.NewGuid().GetHashCode()
            };
        }
    }

    public class ConditionExpressionTests
    {
        [Fact]
        public void Compare_keys_to_values()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemConditionalExpressionHandler<TestClassForConditional>(tableContext);

            var details = expHandler.Handle(i => i.Sk == 12);
            Assert.Equal("#sk = :sk1", details.ConditionExpression);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Equal("12", details.AttributeValues[":sk1"].N);

            details = expHandler.Handle(i => i.Sk != 12);
            Assert.Equal("#sk <> :sk1", details.ConditionExpression);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Equal("12", details.AttributeValues[":sk1"].N);

            details = expHandler.Handle(i => i.Sk > 12);
            Assert.Equal("#sk > :sk1", details.ConditionExpression);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Equal("12", details.AttributeValues[":sk1"].N);

            details = expHandler.Handle(i => i.Sk >= 12);
            Assert.Equal("#sk >= :sk1", details.ConditionExpression);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Equal("12", details.AttributeValues[":sk1"].N);

            details = expHandler.Handle(i => i.Sk < 12);
            Assert.Equal("#sk < :sk1", details.ConditionExpression);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Equal("12", details.AttributeValues[":sk1"].N);

            details = expHandler.Handle(i => i.Sk <= 12);
            Assert.Equal("#sk <= :sk1", details.ConditionExpression);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Equal("12", details.AttributeValues[":sk1"].N);

            details = expHandler.Handle(i => i.Sk.Between(4321, 5321));            
            Assert.Equal("#sk BETWEEN :sk1a AND :sk1b", details.ConditionExpression);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Equal("4321", details.AttributeValues[":sk1a"].N);
            Assert.Equal("5321", details.AttributeValues[":sk1b"].N);

            details = expHandler.Handle(i => i.Pk.BeginsWith("test"));
            Assert.Equal("begins_with(#pk,:pk1)", details.ConditionExpression);
            Assert.Equal("PK", details.AttributeNames["#pk"]);
            Assert.Equal("test", details.AttributeValues[":pk1"].S);

            details = expHandler.Handle(i => i.Pk.DoesContain("test"));
            Assert.Equal("contains(#pk,:pk1)", details.ConditionExpression);
            Assert.Equal("PK", details.AttributeNames["#pk"]);
            Assert.Equal("test", details.AttributeValues[":pk1"].S);
        }

        [Fact]
        public void A_bunch_of_combos()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemConditionalExpressionHandler<TestClassForConditional>(tableContext);

            var details = expHandler.Handle(i => i.Pk.DoesContain("test") && (i.Sk == 12 || i.Sk > 45));
            Assert.Equal("contains(#pk,:pk1) AND (#sk = :sk2 OR #sk > :sk3)", details.ConditionExpression);
            Assert.Equal("PK", details.AttributeNames["#pk"]);
            Assert.Equal("SK", details.AttributeNames["#sk"]);

            Assert.Equal("test", details.AttributeValues[":pk1"].S);
            Assert.Equal("12", details.AttributeValues[":sk2"].N);
            Assert.Equal("45", details.AttributeValues[":sk3"].N);

            var details2 = expHandler.Handle(i => i.Sk > i.Other && i.Pk.BeginsWith("1234") && i.Sk.Exists());
            Assert.Equal("(#sk > #other AND begins_with(#pk,:pk2)) AND attribute_exists(#sk)", details2.ConditionExpression);
            Assert.Equal("PK", details2.AttributeNames["#pk"]);
            Assert.Equal("SK", details2.AttributeNames["#sk"]);
            Assert.Equal("Other", details2.AttributeNames["#other"]);
            Assert.Equal("1234", details2.AttributeValues[":pk2"].S);

            details2 = expHandler.Handle(i => i.Sk > i.Other && (i.Pk.BeginsWith("1234") && i.Sk.Exists()));
            Assert.Equal("#sk > #other AND (begins_with(#pk,:pk2) AND attribute_exists(#sk))", details2.ConditionExpression);
            Assert.Equal("PK", details2.AttributeNames["#pk"]);
            Assert.Equal("SK", details2.AttributeNames["#sk"]);
            Assert.Equal("Other", details2.AttributeNames["#other"]);
            Assert.Equal("1234", details2.AttributeValues[":pk2"].S);

            details2 = expHandler.Handle(i => i.Sk > i.Other && i.Other <= 12);
            Assert.Equal("#sk > #other AND #other <= :other2", details2.ConditionExpression);            
            Assert.Equal("SK", details2.AttributeNames["#sk"]);
            Assert.Equal("Other", details2.AttributeNames["#other"]);
            Assert.Equal("12", details2.AttributeValues[":other2"].N);
        }

        [Fact]
        public void Function_tests()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemConditionalExpressionHandler<TestClassForConditional>(tableContext);

            var details = expHandler.Handle(i => i.Sk.Exists());
            Assert.Equal("attribute_exists(#sk)", details.ConditionExpression);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Empty(details.AttributeValues);

            details = expHandler.Handle(i => i.Sk.NotExists());
            Assert.Equal("attribute_not_exists(#sk)", details.ConditionExpression);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Empty(details.AttributeValues);
        }

        [Fact]
        public void Compare_keys_to_attributes()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemConditionalExpressionHandler<TestClassForConditional>(tableContext);

            var details = expHandler.Handle(i => i.Sk == i.Other);
            Assert.Equal("#sk = #other", details.ConditionExpression);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Equal("Other", details.AttributeNames["#other"]);
            Assert.Empty(details.AttributeValues);

            details = expHandler.Handle(i => i.Sk != i.Other);
            Assert.Equal("#sk <> #other", details.ConditionExpression);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Equal("Other", details.AttributeNames["#other"]);
            Assert.Empty(details.AttributeValues);

            details = expHandler.Handle(i => i.Sk > i.Other);
            Assert.Equal("#sk > #other", details.ConditionExpression);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Equal("Other", details.AttributeNames["#other"]);
            Assert.Empty(details.AttributeValues);

            details = expHandler.Handle(i => i.Sk >= i.Other);
            Assert.Equal("#sk >= #other", details.ConditionExpression);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Equal("Other", details.AttributeNames["#other"]);
            Assert.Empty(details.AttributeValues);

            details = expHandler.Handle(i => i.Sk < i.Other);
            Assert.Equal("#sk < #other", details.ConditionExpression);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Equal("Other", details.AttributeNames["#other"]);
            Assert.Empty(details.AttributeValues);

            details = expHandler.Handle(i => i.Sk <= i.Other);
            Assert.Equal("#sk <= #other", details.ConditionExpression);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Equal("Other", details.AttributeNames["#other"]);
            Assert.Empty(details.AttributeValues);
        }
    }
}
