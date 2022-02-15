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
        public DateTime DateTimeProp { get; set; }

        [Attribute]
        public DateTimeOffset DateTimeOffsetProp { get; set; }

        [Attribute]
        public decimal DecimalProp { get; set; }

        [Attribute]
        public double DoubleProp { get; set; }

        [Attribute]
        public float FloatProp { get; set; }

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
            var rand = new Random();

            return new TestClassWithAllBuiltInTypes()
            {
                Pk = Guid.NewGuid().ToString(),
                Sk = Guid.NewGuid().ToString(),

                BoolProp = (Guid.NewGuid().GetHashCode() % 2) == 0,
                DateTimeProp = DateTime.UtcNow,
                DateTimeOffsetProp = DateTimeOffset.UtcNow,
                DecimalProp = (decimal)rand.NextDouble(),
                DoubleProp = (double)rand.NextDouble(),
                FloatProp = (float)rand.NextDouble(),
                GuidProp = Guid.NewGuid(),
                IntProp = Guid.NewGuid().GetHashCode(),
                LongProp = Guid.NewGuid().GetHashCode(),
                ShortProp = (short)Guid.NewGuid().GetHashCode(),
                StringProp = Guid.NewGuid().ToString(),
            };
        }
    }

    public class TestClassWithAllBuiltInTypesNullable
    {
        [PK]
        public string Pk { get; set; }

        [SK]
        public string Sk { get; set; }

        [Attribute]
        public bool? BoolProp { get; set; }

        [Attribute]
        public DateTime? DateTimeProp { get; set; }

        [Attribute]
        public DateTimeOffset? DateTimeOffsetProp { get; set; }

        [Attribute]
        public decimal? DecimalProp { get; set; }

        [Attribute]
        public double? DoubleProp { get; set; }

        [Attribute]
        public float? FloatProp { get; set; }

        [Attribute]
        public Guid? GuidProp { get; set; }

        [Attribute]
        public int? IntProp { get; set; }

        [Attribute]
        public long? LongProp { get; set; }

        [Attribute]
        public short? ShortProp { get; set; }

        [Attribute]
        public string StringProp { get; set; }

        public static TestClassWithAllBuiltInTypesNullable TestDataAllNull()
        {
            var rand = new Random();

            return new TestClassWithAllBuiltInTypesNullable()
            {
                Pk = Guid.NewGuid().ToString(),
                Sk = Guid.NewGuid().ToString()
            };
        }
    }


    public class BuiltInTypeConverterTests
    {
        static readonly DateTime UnixEpochDateTimeUtc = new DateTime(621355968000000000L, DateTimeKind.Utc);

        [Fact]
        public void All_built_in_types_are_converted()
        {
            var tableContext = new TestContext();
            var attributizer = new ItemAttributizer<TestClassWithAllBuiltInTypes>(tableContext);
            var item = TestClassWithAllBuiltInTypes.TestData();

            var values = attributizer.Attributize(item);

            Assert.NotNull(values);

            Assert.Equal(13, values.Count);

            Assert.Equal(item.Pk, values[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal(item.Sk, values[tableContext.Configuration.KeyAttributes.SKName].S);

            Assert.Equal(item.BoolProp, values["BoolProp"].BOOL);
            Assert.Equal(Convert.ToInt64(item.DateTimeProp.Subtract(UnixEpochDateTimeUtc).TotalMilliseconds).ToString(), values["DateTimeProp"].N);
            Assert.Equal(item.DateTimeOffsetProp.ToUnixTimeMilliseconds().ToString(), values["DateTimeOffsetProp"].N);
            Assert.Equal(item.DecimalProp.ToString(), values["DecimalProp"].N);
            Assert.Equal(item.DoubleProp.ToString(), values["DoubleProp"].N);
            Assert.Equal(item.FloatProp.ToString(), values["FloatProp"].N);
            Assert.Equal(item.GuidProp.ToString(), values["GuidProp"].S);
            Assert.Equal(item.IntProp.ToString(), values["IntProp"].N);
            Assert.Equal(item.LongProp.ToString(), values["LongProp"].N);
            Assert.Equal(item.ShortProp.ToString(), values["ShortProp"].N);
            Assert.Equal(item.StringProp, values["StringProp"].S);
        }

        [Fact]
        public void All_built_in_types_with_nulls_are_handled()
        {
            var tableContext = new TestContext();
            var attributizer = new ItemAttributizer<TestClassWithAllBuiltInTypesNullable>(tableContext);
            var item = TestClassWithAllBuiltInTypesNullable.TestDataAllNull();

            var values = attributizer.Attributize(item);

            Assert.NotNull(values);

            Assert.Equal(13, values.Count);

            Assert.Equal(item.Pk, values[tableContext.Configuration.KeyAttributes.PKName].S);
            Assert.Equal(item.Sk, values[tableContext.Configuration.KeyAttributes.SKName].S);
                        
            Assert.True(values["BoolProp"].NULL);
            Assert.True(values["DateTimeProp"].NULL);
            Assert.True(values["DateTimeOffsetProp"].NULL);
            Assert.True(values["DecimalProp"].NULL);
            Assert.True(values["DoubleProp"].NULL);
            Assert.True(values["FloatProp"].NULL);
            Assert.True(values["GuidProp"].NULL);
            Assert.True(values["IntProp"].NULL);
            Assert.True(values["LongProp"].NULL);
            Assert.True(values["ShortProp"].NULL);
            Assert.True(values["StringProp"].NULL);
        }
    }
}
