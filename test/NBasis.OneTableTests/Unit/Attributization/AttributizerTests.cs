using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal.Transform;
using NBasis.OneTable.Annotations;
using NBasis.OneTable.Attributization;
using System;
using System.Collections.Generic;
using Xunit;

namespace NBasis.OneTableTests.Unit.Attributization
{
    [ItemType("TEST")]
    public class AttributizerTestClass
    {
        [PK]
        public string Pk { get; set; }

        [SK]
        public string Sk { get; set; }

        [Attribute]
        public string Nothing { get; set; }

        [GPK1("KPG")]
        public string GPk { get; set; }

        [GSK1("KSG")]
        public string GSk { get; set; }

        public static AttributizerTestClass TestData()
        {
            return new AttributizerTestClass()
            {
                Pk = Guid.NewGuid().ToString(),
                Sk = Guid.NewGuid().ToString(),
                Nothing = null,
                GPk = "GPK1",
                GSk = null
            };
        }
    }

    public class AttributizerTests
    {
        [Fact]
        public void Item_is_deattributized()
        {
            var tableContext = new TestContext();
            var attributizer = new ItemAttributizer<AttributizerTestClass>(tableContext);

            var attributes = new Dictionary<string, AttributeValue>
            {
                { tableContext.Configuration.KeyAttributes.PKName, new AttributeValue { S = "PK" } },
                { tableContext.Configuration.KeyAttributes.SKName, new AttributeValue { S = "SK" } },
                { tableContext.Configuration.ItemTypeAttributeName, new AttributeValue { S = "TEST" } },
                { "Nothing", new AttributeValue { NULL = true } },
                { string.Format(tableContext.Configuration.KeyAttributes.GPKNameFormat, 1), new AttributeValue { S = "KPG#GPK1" } },
                { string.Format(tableContext.Configuration.KeyAttributes.GSKNameFormat, 1), new AttributeValue { NULL = true } }
            };

            var item = attributizer.Deattributize(attributes);

            Assert.NotNull(item);

            Assert.Equal("PK", item.Pk);
            Assert.Equal("SK", item.Sk);
            Assert.Equal("GPK1", item.GPk);

            Assert.Null(item.Nothing);
            Assert.Null(item.GSk);
        }

        [Fact]
        public void Item_is_attributized()
        {
            var tableContext = new TestContext();
            var attributizer = new ItemAttributizer<AttributizerTestClass>(tableContext);
            var item = AttributizerTestClass.TestData();

            var values = attributizer.Attributize(item);

            Assert.NotNull(values);

            Assert.Equal(6, values.Count);

            Assert.Equal(item.Pk, values[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal(item.Sk, values[tableContext.Configuration.KeyAttributes.SKName].S);
            Assert.Equal(item.GPk, values[string.Format(tableContext.Configuration.KeyAttributes.GPKNameFormat, 1)].S.Replace("KPG#", ""));

            Assert.Equal("TEST", values[tableContext.Configuration.ItemTypeAttributeName].S);

            Assert.True(values["Nothing"].NULL);
            Assert.True(values[string.Format(tableContext.Configuration.KeyAttributes.GSKNameFormat, 1)].NULL);
        }
    }    
}
