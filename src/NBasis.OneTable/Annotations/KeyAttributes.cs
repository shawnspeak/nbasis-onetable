namespace NBasis.OneTable.Annotations
{
    internal enum KeyType
    {
        Partition,
        Sort
    }

    public abstract class KeyAttribute : Attribute
    {
        public int IndexNumber { get; protected set; }

        public string Prefix { get; private set; }

        public Type Converter { get; private set; }

        public KeyAttribute(
            string prefix = null,
            Type converter = null
        )
        {
            Prefix = prefix;
            Converter = converter;
        }

        internal abstract KeyType KeyType { get;}

        internal abstract string GetFieldName(TableContext context);
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class ItemKeyAttribute : KeyAttribute
    {
        public ItemKeyAttribute(
            string prefix = null,
            Type converter = null
        ) : base(prefix, converter)
        {            
        }        
    }
    
    public sealed class PKAttribute : ItemKeyAttribute
    {
        public PKAttribute(
            string prefix = null,
            Type converter = null
        ) : base(prefix, converter)
        {
        }

        internal override KeyType KeyType => KeyType.Partition;

        internal override string GetFieldName(TableContext context)
        {
            return context.Configuration.KeyAttributes.PKName;
        }
    }

    
    public sealed class SKAttribute : ItemKeyAttribute
    {
        public SKAttribute(
            string prefix = null,
            Type converter = null
        ) : base(prefix, converter)
        {
        }

        internal override KeyType KeyType => KeyType.Sort;

        internal override string GetFieldName(TableContext context)
        {
            return context.Configuration.KeyAttributes.SKName;
        }
    }

    public abstract class GSIKeyAttribute : KeyAttribute
    {
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

    // these GSI attributes are a bit excessive, but it makes for a cleaner GSI scheme setup

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class GSI1KeyAttribute : GSIKeyAttribute
    {
        public GSI1KeyAttribute
        (
            string prefix = null,
            Type converter = null
        ) : base(1, prefix, converter)
        {
        }
    }
    
    public sealed class GPK1Attribute : GSI1KeyAttribute
    {
        public GPK1Attribute(
            string prefix = null,
            Type converter = null
        ) : base(prefix, converter)
        {
        }

        internal override KeyType KeyType => KeyType.Partition;

        internal override string GetFieldName(TableContext context)
        {
            return string.Format(context.Configuration.KeyAttributes.GPKPrefix, IndexNumber);
        }
    }

    public sealed class GSK1Attribute : GSI1KeyAttribute
    {
        public GSK1Attribute(
            string prefix = null,
            Type converter = null
        ) : base(prefix, converter)
        {
        }

        internal override KeyType KeyType => KeyType.Sort;

        internal override string GetFieldName(TableContext context)
        {
            return string.Format(context.Configuration.KeyAttributes.GSKPrefix, IndexNumber);
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class GSI2KeyAttribute : GSIKeyAttribute
    {
        public GSI2KeyAttribute
        (
            string prefix = null,
            Type converter = null
        ) : base(2, prefix, converter)
        {
        }
    }

    public sealed class GPK2Attribute : GSI2KeyAttribute
    {
        public GPK2Attribute(
            string prefix = null,
            Type converter = null
        ) : base(prefix, converter)
        {
        }

        internal override KeyType KeyType => KeyType.Partition;

        internal override string GetFieldName(TableContext context)
        {
            return string.Format(context.Configuration.KeyAttributes.GPKPrefix, IndexNumber);
        }
    }
    
    public sealed class GSK2Attribute : GSI2KeyAttribute
    {
        public GSK2Attribute(
            string prefix = null,
            Type converter = null
        ) : base(prefix, converter)
        {
        }

        internal override KeyType KeyType => KeyType.Sort;

        internal override string GetFieldName(TableContext context)
        {
            return string.Format(context.Configuration.KeyAttributes.GSKPrefix, IndexNumber);
        }
    }
}
