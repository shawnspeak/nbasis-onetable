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

            bool pkFound = false;
            typeof(TItem).EnumerateItemKeys((property, attr) =>
            {
                if (attr is PKAttribute)
                {
                    if (pkFound)
                    {
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
        }
    }
}
