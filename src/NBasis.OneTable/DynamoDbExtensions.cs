using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Operations;
using System.Xml.Linq;

namespace NBasis.OneTable
{
    internal static class DynamoDbExtensions
    {
        internal static void Apply(this UpdateItemRequest request, UpdateOperation operation)
        {
            request.ExpressionAttributeValues = operation.GetExpressionAttributeValues();
            request.ExpressionAttributeNames = operation.GetExpressionAttributeNames();
            request.UpdateExpression = operation.GetUpdateExpression();
        }

        internal static void AddUpdateItem(this UpdateItemRequest request, string name, AttributeValue value)
        {
            request.ExpressionAttributeValues ??= new Dictionary<string, AttributeValue>();
            request.ExpressionAttributeNames ??= new Dictionary<string, string>();
            request.UpdateExpression ??= "";

            var lowerKey = name.ToLower();
            request.ExpressionAttributeValues.Add(":" + lowerKey, value);
            request.ExpressionAttributeNames.Add("#" + lowerKey, name);

            if (request.UpdateExpression.Length == 0)
            {
                request.UpdateExpression += "SET ";
            }
            else
            {
                request.UpdateExpression += ", ";
            }

            request.UpdateExpression += string.Format("#{0} = :{0}", lowerKey);
        }

        internal static void Apply(this Update update, UpdateOperation operation)
        {
            update.ExpressionAttributeValues = operation.GetExpressionAttributeValues();
            update.ExpressionAttributeNames = operation.GetExpressionAttributeNames();
            update.UpdateExpression = operation.GetUpdateExpression();
        }      

        internal static void MergeAttributeNames(this QueryRequest request, Dictionary<string, string> attributeNames)
        {
            foreach (var name in attributeNames)
            {
                if (!request.ExpressionAttributeNames.ContainsKey(name.Key))
                    request.ExpressionAttributeNames.Add(name.Key, name.Value);
            }
        }

        internal static void MergeAttributeValues(this QueryRequest request, Dictionary<string, AttributeValue> attributeValues)
        {
            foreach (var value in attributeValues)
            {
                if (!request.ExpressionAttributeValues.ContainsKey(value.Key))
                    request.ExpressionAttributeValues.Add(value.Key, value.Value);
            }
        }

        internal static void MergeAttributeNames(this ScanRequest request, Dictionary<string, string> attributeNames)
        {
            foreach (var name in attributeNames)
            {
                if (!request.ExpressionAttributeNames.ContainsKey(name.Key))
                    request.ExpressionAttributeNames.Add(name.Key, name.Value);
            }
        }

        internal static void MergeAttributeValues(this ScanRequest request, Dictionary<string, AttributeValue> attributeValues)
        {
            foreach (var value in attributeValues)
            {
                if (!request.ExpressionAttributeValues.ContainsKey(value.Key))
                    request.ExpressionAttributeValues.Add(value.Key, value.Value);
            }
        }

        internal static void AddItemTypeFilter<TContext, TItem>(this ScanRequest request, TContext context) where TItem : class where TContext : TableContext
        {
            if (!string.IsNullOrWhiteSpace(context.Configuration.ItemTypeAttributeName))
            {
                request.FilterExpression = "#OTRT = :OTRT";
                request.ExpressionAttributeNames.Add("#OTRT", context.Configuration.ItemTypeAttributeName);

                var itemType = typeof(TItem).GetItemType();
                request.ExpressionAttributeValues.Add(":OTRT", new AttributeValue(itemType));
            }
        }
    }
}
