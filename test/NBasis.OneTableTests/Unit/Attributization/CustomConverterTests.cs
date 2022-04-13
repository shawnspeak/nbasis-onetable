using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Annotations;
using NBasis.OneTable.Attributization;
using System;
using System.Collections.Generic;
using Xunit;

namespace NBasis.OneTableTests.Unit.Attributization
{
    public class CustomConverter : AttributeConverter<string>
    {
        public override string Read(AttributeValue attribute)
        {
            return "testread";
        }

        public override AttributeValue Write(string value)
        {
            return new AttributeValue
            {
                S = "testwrite"
            };
        }
    }

    public class TestClassWithCustomConverter
    {
        [PK]
        public string Pk { get; set; }

        [SK]
        public string Sk { get; set; }

        [Attribute(converter:typeof(CustomConverter))]
        public string SomeValue { get; set; }

        public static TestClassWithCustomConverter TestData()
        {
            return new TestClassWithCustomConverter()
            {
                Pk = Guid.NewGuid().ToString(),
                Sk = Guid.NewGuid().ToString(),
                SomeValue = Guid.NewGuid().ToString(),
            };
        }
    }


    public class CustomConverterTests
    {
        [Fact]
        public void Custom_converter_is_used_for_write()
        {
            var tableContext = new TestContext();
            var attributizer = new ItemAttributizer<TestClassWithCustomConverter>(tableContext);
            var item = TestClassWithCustomConverter.TestData();

            var values = attributizer.Attributize(item);

            Assert.NotNull(values);

            Assert.Equal(3, values.Count);

            Assert.Equal(item.Pk, values[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal(item.Sk, values[tableContext.Configuration.KeyAttributes.SKName].S);

            Assert.Equal("testwrite", values["SomeValue"].S);
        }

        [Fact]
        public void Custom_converter_is_used_for_read()
        {
            var tableContext = new TestContext();
            var attributizer = new ItemAttributizer<TestClassWithCustomConverter>(tableContext);

            var attrs = new Dictionary<string, AttributeValue>
            {
                { tableContext.Configuration.KeyAttributes.PKName, new AttributeValue("testpk") },
                { tableContext.Configuration.KeyAttributes.SKName, new AttributeValue("testsk") },
                { "SomeValue", new AttributeValue("wrongvalue") }
            };

            var item = attributizer.Deattributize(attrs);

            Assert.NotNull(item);

            Assert.Equal("testpk", item.Pk);
            Assert.Equal("testsk", item.Sk);
            Assert.Equal("testread", item.SomeValue);
        }
    }
}
