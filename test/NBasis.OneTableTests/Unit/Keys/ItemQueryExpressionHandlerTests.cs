using NBasis.OneTable;
using NBasis.OneTable.Annotations;
using NBasis.OneTable.Expressions;
using System;
using Xunit;

namespace NBasis.OneTableTests.Unit.Keys
{
    public class KeyOverlapTestClass
    {
        [PK]
        [GPK2("G2PREF")]
        public string PK { get; set; }

        [GSK1("GSPREF")]
        [SK]
        public string SK { get; set; }

        [GPK1("GPPREF")]
        [GSK2("G2PREF")]
        public string GPK1 { get; set; }
    }

    public class InverseKeyOverlapTestClass
    {
        [PK]
        [GSK1]
        public string PK { get; set; }

        [GPK1]
        [SK]
        public string SK { get; set; }
    }

    public class ItemQueryExpressionHandlerTests
    {
        [Fact]
        public void PK_only_query()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemQueryExpressionHandler<TestClass>(tableContext);

            var details = expHandler.Handle(i => i.Pk == "12");

            Assert.NotNull(details);

            Assert.Single(details.AttributeNames);
            Assert.Single(details.AttributeValues);
            Assert.Equal("#pk = :pk", details.QueryExpression);

            Assert.Null(details.IndexName);

            Assert.Equal("PK", details.AttributeNames["#pk"]);
            Assert.Equal("12", details.AttributeValues[":pk"].S);
        }

        [Fact]
        public void PK_and_SK_query()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemQueryExpressionHandler<TestClass>(tableContext);

            var details = expHandler.Handle(i => i.Pk == "12" && i.Sk.BeginsWith("4321"));

            Assert.NotNull(details);

            Assert.Equal(2, details.AttributeNames.Count);
            Assert.Equal(2, details.AttributeValues.Count);
            Assert.Equal("#pk = :pk AND begins_with(#sk,:sk)", details.QueryExpression);

            Assert.Null(details.IndexName);

            Assert.Equal("PK", details.AttributeNames["#pk"]);
            Assert.Equal("12", details.AttributeValues[":pk"].S);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Equal("4321", details.AttributeValues[":sk"].S);
        }

        [Fact]
        public void PK_and_string_SK_all_the_operators()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemQueryExpressionHandler<TestClass>(tableContext);

            var details = expHandler.Handle(i => i.Pk == "12" && i.Sk.BeginsWith("4321"));
            Assert.Equal("#pk = :pk AND begins_with(#sk,:sk)", details.QueryExpression);

            var details2 = expHandler.Handle(i => i.Pk == "12" && i.Sk == "4321");
            Assert.Equal("#pk = :pk AND #sk = :sk", details2.QueryExpression);

            var details3 = expHandler.Handle(i => i.Pk == "12" && i.Sk.Between("4321", "5321"));
            Assert.Equal("#pk = :pk AND #sk BETWEEN :sk1 AND :sk2", details3.QueryExpression);            
        }

        [Fact]
        public void PK_and_number_SK_all_the_operators()
        {
            var pk = Guid.NewGuid();
            var tableContext = new TestContext();
            var expHandler = new ItemQueryExpressionHandler<TestClassWithNonStringTypes>(tableContext);

            var details1 = expHandler.Handle(i => i.Pk == pk && i.Sk == 4321);
            Assert.Equal("#pk = :pk AND #sk = :sk", details1.QueryExpression);

            var details3 = expHandler.Handle(i => i.Pk == pk && i.Sk.Between(4321, 5321));
            Assert.Equal("#pk = :pk AND #sk BETWEEN :sk1 AND :sk2", details3.QueryExpression);

            var details4 = expHandler.Handle(i => i.Pk == pk && i.Sk > 4321);
            Assert.Equal("#pk = :pk AND #sk > :sk", details4.QueryExpression);

            var details5 = expHandler.Handle(i => i.Pk == pk && i.Sk < 4321);
            Assert.Equal("#pk = :pk AND #sk < :sk", details5.QueryExpression);

            var details6 = expHandler.Handle(i => i.Pk == pk && i.Sk >= 4321);
            Assert.Equal("#pk = :pk AND #sk >= :sk", details6.QueryExpression);

            var details7 = expHandler.Handle(i => i.Pk == pk && i.Sk <= 4321);
            Assert.Equal("#pk = :pk AND #sk <= :sk", details7.QueryExpression);
        }


        [Fact]
        public void Key_overlap_test()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemQueryExpressionHandler<KeyOverlapTestClass>(tableContext);

            var details = expHandler.Handle(i => i.GPK1 == "12" && i.SK == "1234");
            Assert.Equal("#pk = :pk AND #sk = :sk", details.QueryExpression);

            Assert.Equal(2, details.AttributeNames.Count);
            Assert.Equal(2, details.AttributeValues.Count);
            Assert.Equal("gsi_1", details.IndexName);

            Assert.Equal("GPK1", details.AttributeNames["#pk"]);
            Assert.Equal("GPPREF#12", details.AttributeValues[":pk"].S);
            Assert.Equal("GSK1", details.AttributeNames["#sk"]);
            Assert.Equal("GSPREF#1234", details.AttributeValues[":sk"].S);
        }

        [Fact]
        public void Key_overlap_test2()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemQueryExpressionHandler<KeyOverlapTestClass>(tableContext);

            var details = expHandler.Handle(i => i.PK == "12" && i.GPK1 == "1234");
            Assert.Equal("#pk = :pk AND #sk = :sk", details.QueryExpression);

            Assert.Equal(2, details.AttributeNames.Count);
            Assert.Equal(2, details.AttributeValues.Count);
            Assert.Equal("gsi_2", details.IndexName);

            Assert.Equal("GPK2", details.AttributeNames["#pk"]);
            Assert.Equal("G2PREF#12", details.AttributeValues[":pk"].S);
            Assert.Equal("GSK2", details.AttributeNames["#sk"]);
            Assert.Equal("G2PREF#1234", details.AttributeValues[":sk"].S);
        }

        [Fact]
        public void All_by_prefix_test2()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemQueryExpressionHandler<KeyOverlapTestClass>(tableContext);

            var details = expHandler.Handle(i => i.PK == "12" && i.GPK1.AllByPrefix());
            Assert.Equal("#pk = :pk AND begins_with(#sk,:sk)", details.QueryExpression);

            Assert.Equal(2, details.AttributeNames.Count);
            Assert.Equal(2, details.AttributeValues.Count);
            Assert.Equal("gsi_2", details.IndexName);

            Assert.Equal("GPK2", details.AttributeNames["#pk"]);
            Assert.Equal("G2PREF#12", details.AttributeValues[":pk"].S);
            Assert.Equal("GSK2", details.AttributeNames["#sk"]);
            Assert.Equal("G2PREF#", details.AttributeValues[":sk"].S);
        }

        [Fact]
        public void All_by_prefix_test3()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemQueryExpressionHandler<TestClassWithGuidSk>(tableContext);

            var details = expHandler.Handle(i => i.Pk == "12" && i.Sk.AllByPrefix());
            Assert.Equal("#pk = :pk AND begins_with(#sk,:sk)", details.QueryExpression);

            Assert.Equal(2, details.AttributeNames.Count);
            Assert.Equal(2, details.AttributeValues.Count);

            Assert.Equal("PK", details.AttributeNames["#pk"]);
            Assert.Equal("PKP#12", details.AttributeValues[":pk"].S);
            Assert.Equal("SK", details.AttributeNames["#sk"]);
            Assert.Equal("SKP#", details.AttributeValues[":sk"].S);
        }


        [Fact]
        public void Inverse_pk_selects_gsi()
        {
            var tableContext = new TestContext();
            var expHandler = new ItemQueryExpressionHandler<InverseKeyOverlapTestClass>(tableContext);

            var details = expHandler.Handle(i => i.SK == "12");

            Assert.NotNull(details);

            Assert.Single(details.AttributeNames);
            Assert.Single(details.AttributeValues);
            Assert.Equal("#pk = :pk", details.QueryExpression);

            Assert.Equal("gsi_1",details.IndexName);

            Assert.Equal("GPK1", details.AttributeNames["#pk"]);
            Assert.Equal("12", details.AttributeValues[":pk"].S);
        }
    }
}