using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Annotations;
using NBasis.OneTable.Exceptions;
using System.Linq.Expressions;
using System.Reflection;
using static NBasis.OneTable.Expressions.ItemQueryExpressionVisitor;

namespace NBasis.OneTable.Expressions
{
    /*
     * Query Expressions:
     * - Point here is to write expression in terms of the item we're working with
     * - Query expressions will always be against either PK/SK OR a single GSI
     * - Possible that PK/SK attribute are combined with GSI attributes
     * - PK or GSIPK must always be specified
     * - PK or GSIPK will always be an "equal"
     * - PK/SK will take precident over GSI
     * - Some sort key conditions are going to be difficult to express (begins_with, between)
     * - Only one condition per key
     * 
     * 
     */

    public class ItemQueryExpressionVisitor : ExpressionVisitor
    {
        readonly Stack<string> _fieldNames = new();
        readonly List<FoundKey> _foundKeys = new();
        private FoundKey _currentKey = null;
        private QueryOperator? _methodOperator = null;

        public FoundKey[] FoundKeys
        {
            get { return _foundKeys.ToArray(); }
        }

        public enum QueryOperator
        {
            Equal,
            LessThanOrEqual,
            LessThan,
            GreaterThanOrEqual,
            GreaterThan,
            BeginsWith,
            Between,
            AllByPrefix,
        }

        public class FoundKey
        {
            public MemberInfo Member { get; set; }

            public object Value { get; set; }

            public object Value2 { get; set; }

            public int CurrentArg { get; set; }

            public QueryOperator Operator { get; set; }

            public KeyAttribute[] KeyAttributes { get; set; }
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.NodeType == ExpressionType.Constant ||
                node.Expression.NodeType == ExpressionType.MemberAccess)
            {
                _fieldNames.Push(node.Member.Name);
                Visit(node.Expression);
            }
            else
            {
                if (_currentKey == null)
                {
                    if (!node.Member.CustomAttributes.Any(ca => ca.AttributeType.IsAssignableTo(typeof(KeyAttribute))))
                        throw new ArgumentException("Key expressions must only be comprised of key properties");

                    _currentKey = new FoundKey()
                    {
                        Member = node.Member,
                        KeyAttributes = node.Member.GetCustomAttributes().Where(ca => ca.GetType().IsAssignableTo(typeof(KeyAttribute))).Select(ca => ca as KeyAttribute).ToArray()
                    };
                    _foundKeys.Add(_currentKey);

                    if (_methodOperator.HasValue)
                    {
                        _currentKey.Operator = _methodOperator.Value;
                        _methodOperator = null;
                    }
                }
                else
                {
                    _currentKey = null;
                }
            }

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (_currentKey == null)
            {
                throw new ArgumentException("Shouldn't see a constant here");
            }
            else
            {
                if (_currentKey.CurrentArg == 0)
                {
                    _currentKey.Value = GetValue(node.Value);
                } 
                else if (_currentKey.CurrentArg == 1)
                {
                    _currentKey.Value2 = GetValue(node.Value);
                }
                _currentKey.CurrentArg++;

                if (_currentKey.Operator == QueryOperator.Between)
                {
                    if (_currentKey.CurrentArg > 1)
                        _currentKey = null;
                } 
                else
                {
                    _currentKey = null;
                }
            }
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Visit(node.Left);

            if (_currentKey == null)
            {
                // must be and also
                if (node.NodeType != ExpressionType.AndAlso)
                    throw new ArgumentException("Can only see And also here");
            }
            else
            {
                // determine operator

                // we'll accept all operators here even for PK and validate for them later

                _currentKey.Operator = node.NodeType switch
                {
                    ExpressionType.Equal => QueryOperator.Equal,
                    ExpressionType.LessThanOrEqual => QueryOperator.LessThanOrEqual,
                    ExpressionType.LessThan => QueryOperator.LessThan,
                    ExpressionType.GreaterThan => QueryOperator.GreaterThan,
                    ExpressionType.GreaterThanOrEqual => QueryOperator.GreaterThanOrEqual,
                    _ => throw new ArgumentException("Invalid operator"),
                };
            }

            Visit(node.Right);

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Object != null)
                Visit(node.Object);

            // Console.WriteLine("Method:: {0}", node.Method.Name);

            // look for StartsWith and Between
            _methodOperator = node.Method.Name.ToLower() switch
            {
                "allbyprefix" => QueryOperator.AllByPrefix,
                "beginswith" => QueryOperator.BeginsWith,
                "between" => QueryOperator.Between,
                _ => throw new ArgumentException("Invalid method"),
            };
            foreach (var arg in node.Arguments)
                Visit(arg);

            return node;
        }

        private object GetValue(object input)
        {
            var type = input.GetType();

            // if it is not simple value
            if (type.IsClass && type != typeof(string))
            {
                if (_fieldNames.Count == 0)
                {
                    // if there is nothing left on the stack, then we assume that the input IS the value
                    return input;
                }

                // proper order of selected names provided by means of Stack structure
                var fieldName = _fieldNames.Pop();
                var fieldInfo = type.GetField(fieldName);
                object value;
                if (fieldInfo != null)
                {
                    value = fieldInfo.GetValue(input);
                }
                else
                {
                    value = type.GetProperty(fieldName).GetValue(input);
                }

                return GetValue(value);
            }
            else
            {
                return input;
            }
        }
    }

    public class ItemQueryDetails
    {
        public string QueryExpression { get; set; }

        public Dictionary<string, AttributeValue> AttributeValues { get; set; }

        public Dictionary<string, string> AttributeNames { get; set; }

        public string IndexName { get; set; }

        public ItemQueryDetails()
        {
            QueryExpression = "";
            AttributeValues = new();
            AttributeNames = new();
        }        
    }


    public class ItemQueryExpressionHandler<TItem> where TItem : class
    {
        readonly TableContext _context;

        readonly static Dictionary<QueryOperator, string> _skOperatorStrings = new()
        {
            { QueryOperator.Equal, "#sk = :sk" },
            { QueryOperator.GreaterThan, "#sk > :sk" },
            { QueryOperator.GreaterThanOrEqual, "#sk >= :sk" },
            { QueryOperator.LessThan, "#sk < :sk" },
            { QueryOperator.LessThanOrEqual, "#sk <= :sk" },
            { QueryOperator.BeginsWith, "begins_with(#sk,:sk)" },
            { QueryOperator.Between, "#sk BETWEEN :sk1 AND :sk2" },
            { QueryOperator.AllByPrefix, "begins_with(#sk,:sk)" }
        };

        public ItemQueryExpressionHandler(TableContext context)
        {
            _context = context;
        }
      
        public ItemQueryDetails Handle(Expression<Func<TItem, bool>> predicate)
        {
            var details = new ItemQueryDetails();

            // visit expression
            var visitor = new ItemQueryExpressionVisitor();
            visitor.Visit(predicate);

            // get found keys from visitor
            var foundKeys = visitor.FoundKeys;
            if (foundKeys.Count() > 2)
                throw new ArgumentException("Too many keys in the expression");

            // build key item dictionary   
            var keyItem = new Dictionary<string, AttributeValue>();

            var getAttribute = (MemberInfo member, object val, KeyAttribute keyAttr) =>
            {
                var propertyType = ((PropertyInfo)member).PropertyType;
                var converter = _context.AttributizerSettings.GetConverter(propertyType);

                if (converter.TryWriteAsObject(val, propertyType, out AttributeValue attrValue))
                {
                    if (keyAttr.Prefix != null)
                    {
                        // attribute must be string regardless of attribute type

                        // we support string or number types
                        string finalValue = attrValue.S;
                        if (attrValue.N != null)
                            finalValue = attrValue.N;

                        return new AttributeValue
                        {
                            S = _context.FormatKeyPrefix(keyAttr, finalValue)
                        };
                    }
                    else
                    {
                        // key is what the converter sends back
                        return attrValue;
                    }
                }

                throw new UnableToWriteAttributeValueException();
            };

            // get all the valid pk expressions
            var pks = foundKeys
                        .Where(k => k.KeyAttributes.Any(ka => ka.KeyType == KeyType.Partition) && k.Operator == QueryOperator.Equal);
            if (pks.Count() == 0) // will always have a PK
                throw new ArgumentException("Missing a valid PK or GPK key expression");
            var sks = foundKeys.Where(k => k.KeyAttributes.Any(ka => ka.KeyType == KeyType.Sort));

            // if more than one possible, then we need to look at SK to determine which
            ItemQueryExpressionVisitor.FoundKey pk = null;
            KeyAttribute keyAttribute = null;
            
            if (sks.Count() == 0)
            {
                // otherwise take the min index number
                pk = pks.OrderBy(k => k.KeyAttributes.Min(ka => ka.IndexNumber)).First();
                keyAttribute = pk.KeyAttributes.Where(ka => ka.KeyType == KeyType.Partition).OrderBy(ka => ka.IndexNumber).First();
            } 
            else
            {
                List<int> sksIndexNumbers = new();
                foreach (var sk in sks)
                {
                    var i = sk.KeyAttributes.Where(ka => ka.KeyType == KeyType.Sort).Select(ka => ka.IndexNumber);
                    if (i.Count() > 0)
                        sksIndexNumbers.AddRange(i);
                }

                // look for pk based upon sksIndexNumbers
                foreach (var skIndex in sksIndexNumbers.OrderBy(k => k))
                {
                    pk = pks.Where(k => k.KeyAttributes.Where(ka => ka.KeyType == KeyType.Partition && ka.IndexNumber == skIndex).Any()).FirstOrDefault();
                    if (pk != null)
                    {
                        keyAttribute = pk.KeyAttributes.Where(ka => ka.KeyType == KeyType.Partition && ka.IndexNumber == skIndex).First();
                        break;
                    }
                }
            }
         

            details.QueryExpression = "#pk = :pk";
            details.AttributeValues[":pk"] = getAttribute(pk.Member, pk.Value, keyAttribute);

            if (keyAttribute.IndexNumber == 0)
            {
                details.AttributeNames["#pk"] = _context.Configuration.KeyAttributes.PKName;
            }
            else
            {
                details.AttributeNames["#pk"] = _context.GPKAttributeName(keyAttribute.IndexNumber);
                details.IndexName = _context.GSIndexName(keyAttribute.IndexNumber);
            }

            if (sks.Count() > 0)
            {
                // select sks based on index number
                var sk = foundKeys.FirstOrDefault(k => k.KeyAttributes.Any(ka => ka.KeyType == KeyType.Sort && ka.IndexNumber == keyAttribute.IndexNumber));

                var skAttribute = sk.KeyAttributes.Where(ka => ka.KeyType == KeyType.Sort && ka.IndexNumber == keyAttribute.IndexNumber).First();

                // limit operators to type
                if (sk.Operator == QueryOperator.Between)
                {
                    details.AttributeValues[":sk1"] = getAttribute(sk.Member, sk.Value, skAttribute);
                    details.AttributeValues[":sk2"] = getAttribute(sk.Member, sk.Value2, skAttribute);
                } 
                else if (sk.Operator == QueryOperator.AllByPrefix)
                {
                    details.AttributeValues[":sk"] = new AttributeValue
                    {
                        S = _context.FormatKeyPrefix(skAttribute, "")
                    };
                }
                else
                {   
                    details.AttributeValues[":sk"] = getAttribute(sk.Member, sk.Value, skAttribute);
                }

                if (keyAttribute.IndexNumber == 0)
                {
                    details.AttributeNames["#sk"] = _context.Configuration.KeyAttributes.SKName;
                } 
                else
                {
                    details.AttributeNames["#sk"] = _context.GSKAttributeName(keyAttribute.IndexNumber);
                }

                details.QueryExpression += string.Format(" AND {0}", _skOperatorStrings[sk.Operator]);
            }

            return details;
        }
    }
}
