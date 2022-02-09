namespace NBasis.OneTable.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RecordTypeAttribute : Attribute
    {
        public string RecordType { get; private set; }

        public RecordTypeAttribute(string recordType)
        {
            RecordType = recordType;
        }
    }
}
