using NBasis.OneTable.Annotations;
using NBasis.OneTable.Attributization;
using System;
using Xunit;

namespace NBasis.OneTableTests.Unit.Attributization
{
    public class TestClassWithAllBuiltInTypes
    {
        [PK]
        public string Pk { get; set; }

        [SK]
        public string Sk { get; set; }

        [Attribute]
        public bool BoolProp { get; set; }

        [Attribute]
        public Guid GuidProp { get; set; }

        [Attribute]
        public int IntProp { get; set; }

        [Attribute]
        public long LongProp { get; set; }

        [Attribute]
        public short ShortProp { get; set; }

        [Attribute]
        public string StringProp { get; set; }        

        public static TestClassWithAllBuiltInTypes TestData()
        {
            return new TestClassWithAllBuiltInTypes()
            {
                Pk = Guid.NewGuid().ToString(),
                Sk = Guid.NewGuid().ToString(),
                StringProp = Guid.NewGuid().ToString(),
                GuidProp = Guid.NewGuid(),
                BoolProp = (Guid.NewGuid().GetHashCode() % 2) == 0,
                IntProp = Guid.NewGuid().GetHashCode(),
                LongProp = Guid.NewGuid().GetHashCode(),
                ShortProp = (short)Guid.NewGuid().GetHashCode()
            };
        }
    }


    public class BuiltInTypeConverterTests
    {
        [Fact]
        public void All_built_in_types_are_converted()
        {
            var tableContext = new TestContext();
            var attributizer = new ItemAttributizer<TestClassWithAllBuiltInTypes>(tableContext);
            var item = TestClassWithAllBuiltInTypes.TestData();

            var values = attributizer.Attributize(item);

            Assert.NotNull(values);

            Assert.Equal(8, values.Count);
            
            Assert.Equal(item.Pk, values[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal(item.Sk, values[tableContext.Configuration.KeyAttributes.SKName].S);
            Assert.Equal(item.BoolProp, values["BoolProp"].BOOL);
            Assert.Equal(item.GuidProp.ToString(), values["GuidProp"].S);
            Assert.Equal(item.IntProp.ToString(), values["IntProp"].N);
            Assert.Equal(item.LongProp.ToString(), values["LongProp"].N);
            Assert.Equal(item.ShortProp.ToString(), values["ShortProp"].N);
            Assert.Equal(item.StringProp, values["StringProp"].S);
        }
    }
}
