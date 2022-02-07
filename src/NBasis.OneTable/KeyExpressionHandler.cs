using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Annotations;
using System.Linq.Expressions;
using System.Reflection;

namespace NBasis.OneTable
{
    /*
     * Expressions:
     * - Point here is to write expression in terms of the item we're working with
     * - Most key expressions should be PK + SK
     * - Possible that PK has SK attribute and get's added twice
     * - PK Must always be specified
     * - If TItem has an SK, then SK must always be specified
     */

    public class ItemKeyExpressionHandler<TItem> where TItem : class
    {
        readonly TableContext _context;

        public ItemKeyExpressionHandler(TableContext context)
        {
            _context = context;
        }

        public static object GetValue(MemberInfo memberInfo, object forObject)
        {
            return memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)memberInfo).GetValue(forObject),
                MemberTypes.Property => ((PropertyInfo)memberInfo).GetValue(forObject),
                _ => throw new NotImplementedException(),
            };
        }

        public static object ResolveValue(Expression expression)
        {
            if (expression is System.Linq.Expressions.MemberExpression member)
            {
                // if next expression is contant, then return it's value
                if (member.Expression is System.Linq.Expressions.ConstantExpression valueExpression)
                {
                    return GetValue(member.Member, valueExpression.Value);
                }
                else
                {
                    var value = ResolveValue(member.Expression);
                    return GetValue(member.Member, value);
                }
            }
            return null;
        }

        private string ResolveRightValue(Expression right)
        {
            if (right.NodeType == ExpressionType.Constant)
            {
                if (right is System.Linq.Expressions.ConstantExpression rightNode)
                {
                    return rightNode.Value.ToString();
                }
            }
            else if (right.NodeType == ExpressionType.MemberAccess)
            {
                if (right is System.Linq.Expressions.MemberExpression rightNode)
                {
                    return ResolveValue(right).ToString();                         
                }
            }
            return null;
        }

        private void FindKeys(BinaryExpression operation, FoundKeys foundKeys)
        {
            var left = operation.Left;
            var right = operation.Right;
            
            if ((left != null) && (left is BinaryExpression leftBi))
                FindKeys(leftBi, foundKeys);

            if ((right != null) && (right is BinaryExpression rightBi))
                FindKeys(rightBi, foundKeys);

            if ((operation.NodeType == ExpressionType.Equal) &&
                     (left != null) && 
                     (right != null))
            {
                if (left.NodeType == ExpressionType.MemberAccess)
                {
                    if (left is System.Linq.Expressions.MemberExpression leftNode)
                    {
                        // then right must contain a value

                        // resolve right value
                        string rightValue = ResolveRightValue(right);

                        // apply the value to the correct key if specified on left
                        if (rightValue != null)
                        {
                            var pkAttribute = leftNode.Member.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == typeof(PKAttribute));
                            if (pkAttribute != null)
                            {
                                foundKeys.PKMember = leftNode.Member;
                                foundKeys.PKPrefix = pkAttribute.ConstructorArguments?.FirstOrDefault().Value?.ToString();
                                foundKeys.PKValue = rightValue;
                            }

                            var skAttribute = leftNode.Member.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == typeof(SKAttribute));
                            if (skAttribute != null)
                            {
                                foundKeys.SKMember = leftNode.Member;
                                foundKeys.SKPrefix = skAttribute.ConstructorArguments?.FirstOrDefault().Value?.ToString(); 
                                foundKeys.SKValue = rightValue;
                            }
                        }
                    }
                }
            }
        }

        public class FoundKeys
        {
            public MemberInfo PKMember { get; set; }

            public string PKValue { get; set; }

            public string PKPrefix { get; set; }

            public MemberInfo SKMember { get; set; }

            public string SKValue { get; set; }

            public string SKPrefix { get; set; }
        }

        public Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue> Handle(Expression<Func<TItem, bool>> predicate)
        {
            if (predicate.Parameters.Count != 1)
                throw new ArgumentException("Predicate must contain only one parameter");

            if (predicate.Body is not BinaryExpression operation)
                throw new ArgumentException("Must be a binary expression");

            // recurse expression tree and look for PK and SK
            var foundKeys = new FoundKeys();
            FindKeys(operation as BinaryExpression, foundKeys);

            // validation

            // will always have a PK

            // build key item dictionary   
            var keyItem = new Dictionary<string, AttributeValue>();

            var getAttribute = (string val, string prefix) =>
            {
                if (prefix != null)
                {
                    return new AttributeValue(prefix + "#" + val);
                }
                return new AttributeValue(val);
            };
            if (foundKeys.PKMember != null)
            {
                keyItem[_context.Configuration.KeyAttributes.PKName] = getAttribute(foundKeys.PKValue, foundKeys.PKPrefix);
            }
            if (foundKeys.SKMember != null)
            {
                keyItem[_context.Configuration.KeyAttributes.SKName] = getAttribute(foundKeys.SKValue, foundKeys.SKPrefix);
            }
            return keyItem;
        }
    }
}
