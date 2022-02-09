using NBasis.OneTable.Annotations;
using System.Reflection;

namespace NBasis.OneTable
{
    internal static class ItemExtensions
    {
        internal static void EnumerateItemAttributes(this Type itemtype, Action<PropertyInfo, AttributeAttribute> action)
        {
            // deal with every property on item
            var properties = itemtype.GetProperties();
            foreach (var property in properties)
            {
                // look for attribute property
                var attrAttr = property.GetCustomAttributes(true).OfType<AttributeAttribute>().FirstOrDefault();
                if (attrAttr != null)
                {
                    action(property, attrAttr);
                }
            }
        }

#pragma warning disable IDE0060 // Remove unused parameter
        internal static void EnumerateItemAttributes<TItem>(this TItem item, Action<PropertyInfo, AttributeAttribute> action)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            // deal with every property on item
            typeof(TItem).EnumerateItemAttributes(action);
        }

        internal static void EnumerateItemKeys(this Type itemType, Action<PropertyInfo, KeyAttribute> action)
        {
            // deal with every property on item
            var properties = itemType.GetProperties();
            foreach (var property in properties)
            {
                // look for key property
                foreach (var keyAttr in property.GetCustomAttributes(true).OfType<KeyAttribute>())
                {
                    action(property, keyAttr);
                }
            }
        }

#pragma warning disable IDE0060 // Remove unused parameter
        internal static void EnumerateItemKeys<TItem>(this TItem item, Action<PropertyInfo, KeyAttribute> action)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            typeof(TItem).EnumerateItemKeys(action);
        }
    }
}
