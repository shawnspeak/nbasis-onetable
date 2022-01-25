using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Annotations;
using System.Linq.Expressions;

namespace NBasis.OneTable
{
    /*
     * Expressions:
     * - Point here is to write expression in terms of the item we're working with
     * - Most key expressions should be PK + SK
     * - Possible that PK has SK attribute and get's added twice
     * - PK Must always be specified
     */


    public class KeyItemExpressionHandler<TItem> where TItem : class
    {
        public Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue> Handle(Expression<Func<TItem, bool>> predicate)
        {
            var keyItem = new Dictionary<string, AttributeValue>();

            if (predicate.Parameters.Count != 1)
                throw new ArgumentException("Predicate must contain only one parameter");

            var param = predicate.Parameters[0] as ParameterExpression;
            var operation = predicate.Body as BinaryExpression;

            if (operation == null)
                throw new ArgumentException("Must be a binary expression");

            var left = operation.Left;
            var right = operation.Right;
            
            if (left.NodeType == ExpressionType.MemberAccess)
            {
                var leftNode = left as System.Linq.Expressions.MemberExpression;
                if (leftNode != null)
                {
                    // must contain a key attribute
                    if (leftNode.Member.CustomAttributes.Any(ca => ca.AttributeType == typeof(PKAttribute)))
                    {
                        // then right must contain a value

                        if (right.NodeType == ExpressionType.Constant)
                        {
                            var rightNode = right as System.Linq.Expressions.ConstantExpression;
                            if (rightNode != null)
                            {
                                if (rightNode.Value is string)
                                {
                                    keyItem["PK"] = new AttributeValue(rightNode.Value.ToString());
                                }
                            }
                        }
                    }
                }
            }

            return keyItem;
        }
    }
}
