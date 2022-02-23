using NBasis.OneTable.Annotations;
using NBasis.OneTable.Attributization;
using System;
using Xunit;

namespace NBasis.OneTableTests.Unit.Attributization
{
    [ItemType("TEST")]
    public class TestClassWithItemType
    {
        [PK]
        public string Pk { get; set; }

        [SK]
        public string Sk { get; set; }

        public static TestClassWithItemType TestData()
        {
            return new TestClassWithItemType()
            {
                Pk = Guid.NewGuid().ToString(),
                Sk = Guid.NewGuid().ToString(),
            };
        }
    }


    public class ItemTypeTests
    {
        [Fact]
        public void Item_type_is_added_to_attributes()
        {
            var tableContext = new TestContext();
            var attributizer = new ItemAttributizer<TestClassWithItemType>(tableContext);
            var item = TestClassWithItemType.TestData();

            var values = attributizer.Attributize(item);

            Assert.NotNull(values);

            Assert.Equal(3, values.Count);

            Assert.Equal(item.Pk, values[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal(item.Sk, values[tableContext.Configuration.KeyAttributes.SKName].S);

            Assert.Equal("TEST", values[tableContext.Configuration.ItemTypeAttributeName].S);
        }       
    }
}
