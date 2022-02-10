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
            // validate patch type first
            new PatchTypeValidator<TPatch>(_context).Validate();

            // must have required values
        }
    }
}
