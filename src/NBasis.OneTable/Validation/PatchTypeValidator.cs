using NBasis.OneTable.Validation.Exceptions;

namespace NBasis.OneTable.Validation
{
    internal class PatchTypeValidator<TItem> where TItem : class
    {
        readonly TableContext _context;

        public PatchTypeValidator(TableContext context)
        {
            _context = context;
        }

        public void Validate()
        {
            List<string> fieldNames = new();

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
