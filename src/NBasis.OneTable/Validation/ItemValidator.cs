using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBasis.OneTable.Validation
{
    internal class ItemValidator<TItem> where TItem : class
    {
        readonly TableContext _context;

        public ItemValidator(TableContext context)
        {
            _context = context;
        }

        public void Validate(TItem item)
        {
            // validate item type first
            new ItemTypeValidator<TItem>(_context).Validate();

            // must have keys and required values
        }

        public void ValidateKeys(TItem item)
        {
            // validate item type first
            new ItemTypeValidator<TItem>(_context).Validate();

            // must have key values

        }
    }
}
