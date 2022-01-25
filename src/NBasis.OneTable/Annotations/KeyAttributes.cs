using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBasis.OneTable.Annotations
{
    public abstract class KeyAttribute : Attribute
    {
        public string? Prefix { get; private set; }

        public KeyAttribute(string? prefix = null)
        {
            Prefix = prefix;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PKAttribute : KeyAttribute
    {
        public PKAttribute(string? prefix = null) : base(prefix)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SKAttribute : KeyAttribute
    {
        public SKAttribute(string? prefix = null) : base(prefix)
        {
        }
    }

    public abstract class GSIKeyAttribute : KeyAttribute
    {
        public string? Prefix { get; private set; }

        public int IndexNumber { get; private set; }

        public GSIKeyAttribute(int indexNumber, string? prefix = null) :    base(prefix)
        {
            IndexNumber = indexNumber;
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class GPK1Attribute : GSIKeyAttribute
    {
        public GPK1Attribute(string? prefix = null) : base(1, prefix)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GSK1Attribute : GSIKeyAttribute
    {
        public GSK1Attribute(string? prefix = null) : base(1, prefix)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GPK2Attribute : GSIKeyAttribute
    {
        public GPK2Attribute(string? prefix = null) : base(2, prefix)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GSK2Attribute : GSIKeyAttribute
    {
        public GSK2Attribute(string? prefix = null) : base(2, prefix)
        {
        }
    }
}
