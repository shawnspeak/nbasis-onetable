using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBasis.OneTable.Validation.Exceptions
{
    public abstract class ItemValidationException : Exception
    {
        public ItemValidationException() : base()
        {
        }

        public ItemValidationException(string message) : base(message)
        {
        }

        public ItemValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
