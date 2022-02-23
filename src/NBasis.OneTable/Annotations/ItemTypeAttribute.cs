namespace NBasis.OneTable.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ItemTypeAttribute : Attribute
    {
        public string ItemType { get; private set; }

        public ItemTypeAttribute(string itemType)
        {
            ItemType = itemType ?? throw new ArgumentNullException(nameof(itemType));
        }
    }
}
