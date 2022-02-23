using NBasis.OneTable.Annotations;
using System;

namespace NBasis.OneTableTests.Unit.Keys
{
    public class TestClass
    {
        [PK]
        public string Pk { get; set; }

        [SK]
        public string Sk { get; set; }

        public static TestClass TestData()
        {
            return new TestClass
            {
                Pk = Guid.NewGuid().ToString(),
                Sk = Guid.NewGuid().ToString()
            };
        }
    }

    public class TestClassWithPrefix
    {
        [PK("PRF")]
        public string Pk { get; set; }

        [SK("USR")]
        public string Sk { get; set; }

        public static TestClassWithPrefix TestData()
        {
            return new TestClassWithPrefix
            {
                Pk = Guid.NewGuid().ToString(),
                Sk = Guid.NewGuid().ToString()
            };
        }
    }

    public class TestClassWithNonStringTypes
    {
        [PK]
        public Guid Pk { get; set; }

        [SK]
        public int Sk { get; set; }

        public static TestClassWithNonStringTypes TestData()
        {
            return new TestClassWithNonStringTypes
            {
                Pk = Guid.NewGuid(),
                Sk = Guid.NewGuid().GetHashCode()
            };
        }
    }

    public class TestClassWithGuidSk
    {
        [PK("PKP")]
        public string Pk { get; set; }

        [SK("SKP")]
        public Guid Sk { get; set; }

        public static TestClassWithGuidSk TestData()
        {
            return new TestClassWithGuidSk
            {
                Pk = Guid.NewGuid().ToString(),
                Sk = Guid.NewGuid()
            };
        }
    }

    public class TestClassWithNonStringTypesAndPrefixes
    {
        [PK("USR")]
        public Guid Pk { get; set; }

        [SK("INV")]
        public int Sk { get; set; }

        public static TestClassWithNonStringTypesAndPrefixes TestData()
        {
            return new TestClassWithNonStringTypesAndPrefixes
            {
                Pk = Guid.NewGuid(),
                Sk = Guid.NewGuid().GetHashCode()
            };
        }
    }
}
