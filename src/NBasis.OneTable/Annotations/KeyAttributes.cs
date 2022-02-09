namespace NBasis.OneTable.Annotations
{
    public abstract class KeyAttribute : Attribute
    {
        public string Prefix { get; private set; }

        public Type Converter { get; private set; }

        public KeyAttribute(
            string prefix = null,
            Type coverter = null
        )
        {
            Prefix = prefix;
            Converter = coverter;
        }

        internal abstract string GetFieldName(TableContext context);
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class PKAttribute : KeyAttribute
    {
        public PKAttribute(string prefix = null) : base(prefix)
        {
        }

        internal override string GetFieldName(TableContext context)
        {
            return context.Configuration.KeyAttributes.PKName;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SKAttribute : KeyAttribute
    {
        public SKAttribute(string prefix = null) : base(prefix)
        {
        }

        internal override string GetFieldName(TableContext context)
        {
            return context.Configuration.KeyAttributes.SKName;
        }
    }

    public abstract class GSIKeyAttribute : KeyAttribute
    {
        public int IndexNumber { get; private set; }

        public GSIKeyAttribute
        (
            int indexNumber, 
            string prefix = null,
            Type converter = null
        ) : base(prefix, converter)
        {
            IndexNumber = indexNumber;
        }
    }


    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class GPK1Attribute : GSIKeyAttribute
    {
        public GPK1Attribute(string prefix = null) : base(1, prefix)
        {
        }

        internal override string GetFieldName(TableContext context)
        {
            return string.Format(context.Configuration.KeyAttributes.GPKPrefix, IndexNumber);
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class GSK1Attribute : GSIKeyAttribute
    {
        public GSK1Attribute(string prefix = null) : base(1, prefix)
        {
        }

        internal override string GetFieldName(TableContext context)
        {
            return string.Format(context.Configuration.KeyAttributes.GSKPrefix, IndexNumber);
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class GPK2Attribute : GSIKeyAttribute
    {
        public GPK2Attribute(string prefix = null) : base(2, prefix)
        {
        }
        internal override string GetFieldName(TableContext context)
        {
            return string.Format(context.Configuration.KeyAttributes.GPKPrefix, IndexNumber);
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class GSK2Attribute : GSIKeyAttribute
    {
        public GSK2Attribute(string prefix = null) : base(2, prefix)
        {
        }

        internal override string GetFieldName(TableContext context)
        {
            return string.Format(context.Configuration.KeyAttributes.GSKPrefix, IndexNumber);
        }
    }
}
