using Amazon.DynamoDBv2.Model;

namespace NBasis.OneTable
{
    internal static class DynamoDbExtensions
    {
        internal static void AddUpdateItem(this UpdateItemRequest request, string name, AttributeValue value)
        {
            if (request.ExpressionAttributeValues == null)
                request.ExpressionAttributeValues = new Dictionary<string, AttributeValue>();
            if (request.ExpressionAttributeNames == null)
                request.ExpressionAttributeNames = new Dictionary<string, string>();
            if (request.UpdateExpression == null)
                request.UpdateExpression = "";

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

        internal static void AddUpdateItem(this Update update, string name, AttributeValue value)
        {
            if (update.ExpressionAttributeValues == null)
                update.ExpressionAttributeValues = new Dictionary<string, AttributeValue>();
            if (update.ExpressionAttributeNames == null)
                update.ExpressionAttributeNames = new Dictionary<string, string>();
            if (update.UpdateExpression == null)
                update.UpdateExpression = "";

            var lowerKey = name.ToLower();
            update.ExpressionAttributeValues.Add(":" + lowerKey, value);
            update.ExpressionAttributeNames.Add("#" + lowerKey, name);

            if (update.UpdateExpression.Length == 0)
            {
                update.UpdateExpression += "SET ";
            }
            else
            {
                update.UpdateExpression += ", ";
            }

            update.UpdateExpression += string.Format("#{0} = :{0}", lowerKey);
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
    }
}
