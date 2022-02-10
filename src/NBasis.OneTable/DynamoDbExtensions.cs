using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
