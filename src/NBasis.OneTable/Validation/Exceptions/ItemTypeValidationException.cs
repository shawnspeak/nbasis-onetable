namespace NBasis.OneTable.Validation.Exceptions
{
    public abstract class ItemTypeValidationException : Exception
    {
        public ItemTypeValidationException() : base()
        {
        }

        public ItemTypeValidationException(string message) : base(message)
        {
        }

        public ItemTypeValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class MissingPKAttributeException : ItemTypeValidationException
    {
        public MissingPKAttributeException()
        {
        }
    }

    public class MultiplePKAttributesException : ItemTypeValidationException
    {
        public MultiplePKAttributesException()
        {
        }
    }

    public class DuplicateAttributeNameException : ItemTypeValidationException
    {
        public DuplicateAttributeNameException(string propertyName, string fieldName)
        {
        }
    }

    public class IndexCountTooLowException : ItemTypeValidationException
    {
        public IndexCountTooLowException(string propertyName)
        {
        }
    }

    public class MissingItemTypeAttributeNameException : ItemTypeValidationException
    {
        public MissingItemTypeAttributeNameException()
        {
        }
    }
}
