namespace NBasis.OneTable.Validation
{
    internal class PatchValidator<TPatch> where TPatch : class
    {
        readonly TableContext _context;

        public PatchValidator(TableContext context)
        {
            _context = context;
        }

        public void Validate(TPatch item)
        {
            // validate item type first
            new ItemTypeValidator<TPatch>(_context).Validate();

            // must have keys and required values
        }
    }
}
