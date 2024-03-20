using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Attributization;

namespace NBasis.OneTable.Operations
{
    /// <summary>
    /// Abstraction of an update operation
    /// </summary>
    internal class UpdateOperation
    {
        internal Dictionary<string, AttributeValue> Key { get; private set; }

        internal Dictionary<string, AttributeValue> Sets { get; private set; }

        internal Dictionary<string, AttributeValue> Removes { get; private set; }

        internal Dictionary<string, AttributeValue> GetExpressionAttributeValues()
        {
            var values = new Dictionary<string, AttributeValue>();

            if (Sets != null)
            {
                foreach (var set in Sets)
                {
                    values.Add(":" + set.Key.ToLower(), set.Value);
                }
            }

            return values;
        }

        internal Dictionary<string, string> GetExpressionAttributeNames()
        {
            var names = new Dictionary<string, string>();

            if (Sets != null)
            {
                foreach (var set in Sets)
                {
                    names.Add("#" + set.Key.ToLower(), set.Key);
                }
            }

            if (Removes != null)
            {
                foreach (var remove in Removes)
                {
                    names.Add("#" + remove.Key.ToLower(), remove.Key);
                }
            }

            return names;
        }

        internal string GetUpdateExpression()
        {
            string expression = "";

            if (Sets != null)
            {
                string setExpression = "";
                foreach (var set in Sets)
                {
                    if (setExpression.Length == 0)
                    {
                        setExpression += "SET ";
                    }
                    else
                    {
                        setExpression += ", ";
                    }

                    setExpression += string.Format("#{0} = :{0}", set.Key.ToLower());
                }
                expression += setExpression;
            }

            if (Removes != null)
            {
                string removeExpression = "";
                foreach (var remove in Removes)
                {
                    if (removeExpression.Length == 0)
                    {
                        removeExpression += " REMOVE ";
                    }
                    else
                    {
                        removeExpression += ", ";
                    }

                    removeExpression += string.Format("#{0}", remove.Key.ToLower());
                }
                expression += removeExpression;
            }

            return expression.Trim();
        }

        public static UpdateOperation Create<TContext, TItem>(TContext context, TItem item) where TContext : TableContext where TItem : class
        {
            var operation = new UpdateOperation
            {
                Key = (new ItemKeyHandler<TItem>(context)).BuildKey(item),
            };

            var values = new ItemAttributizer<TItem>(context).Attributize(item);
            var nonKeyValues = values.Where(v => !operation.Key.ContainsKey(v.Key));

            operation.Sets = nonKeyValues.Where(v => !v.Value.NULL).ToDictionary(v => v.Key, v => v.Value);
            operation.Removes = nonKeyValues.Where(v => v.Value.NULL).ToDictionary(v => v.Key, v => v.Value);

            return operation;
        }
    }
}
