using NBasis.OneTable.Annotations;
using NBasis.OneTable.Validation.Exceptions;

namespace NBasis.OneTable.Validation
{
    internal class ItemTypeValidator<TItem> where TItem : class
    {
        readonly TableContext _context;

        public ItemTypeValidator(TableContext context)
        {
            _context = context;
        }

        public void Validate()
        {
            List<string> fieldNames = new();

            // must have a PK
            // PK must only been listed once
            // GS index can't exceed configured count
            // all keys must be scalar
            // keys with prefix must be asString
            // index number may not be greater than context is configured for

            bool pkFound = false;
            typeof(TItem).EnumerateItemKeys((property, attr) =>
            {
                if (attr.IndexNumber > _context.Configuration.GSIndexCount)
                {
                    throw new IndexCountTooLowException(property.Name);
                }

                if (attr is PKAttribute)
                {
                    if (pkFound)
                    {
                        // shouldn't happen
                        // duplicate pk
                        throw new MultiplePKAttributesException();
                    }
                    pkFound = true;
                }

                string fieldName = attr.GetFieldName(_context);
                if (fieldNames.Contains(fieldName))
                {
                    // duplicate field name found
                    throw new DuplicateAttributeNameException(property.Name, fieldName);
                }
                fieldNames.Add(fieldName);
            });

            if (!pkFound)
            {
                // no pk
                throw new MissingPKAttributeException();
            }

            // no duplicate attribute names
            typeof(TItem).EnumerateItemAttributes((property, attr) =>
            {
                string fieldName = attr.FieldName ?? property.Name;
                if (fieldNames.Contains(fieldName))
                {
                    // duplicate field name found
                    throw new DuplicateAttributeNameException(property.Name, fieldName);
                }
                fieldNames.Add(fieldName);
            });

            // all attributes meet dynamodb rules

            // if record type is specified but not configured
            var recordType = typeof(TItem).GetItemType();
            if ((recordType != null) && (_context.Configuration.ItemTypeAttributeName == null))
            {
                throw new MissingItemTypeAttributeNameException();
            }
        }
    }
}
