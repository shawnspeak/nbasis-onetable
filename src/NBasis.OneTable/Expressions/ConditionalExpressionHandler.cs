using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Annotations;
using NBasis.OneTable.Exceptions;
using System.Linq.Expressions;
using System.Reflection;
using static NBasis.OneTable.Expressions.ItemQueryExpressionVisitor;

namespace NBasis.OneTable.Expressions
{
    public class ItemConditionalExpressionVisitor : ExpressionVisitor
    {
        readonly TableContext _context;
        readonly bool _allowKeys;
        readonly Stack<string> _fieldNames = new();
        readonly List<FoundCondition> _foundConditions = new();
        private FoundCondition _currentCondition = null;
        private ConditionComparator? _methodOperator = null;

        private string _conditionExpression = "";
        private int _level = 0;

        private readonly Dictionary<string, string> _expressionNames = new();
        private readonly Dictionary<string, AttributeValue> _expressionValues = new();

        public ItemConditionalExpressionVisitor(TableContext context, bool allowKeys)
        {
            _context = context;
            _allowKeys = allowKeys;
        }

        public string ConditionExpression
        {
            get { return _conditionExpression; }
        }

        public Dictionary<string, AttributeValue> AttributeValues
        {
            get { return _expressionValues; }
        }

        public Dictionary<string, string> AttributeNames
        {
            get { return _expressionNames; }
        }

        public enum ConditionComparator
        {
            Equal,
            NotEqual,
            LessThanOrEqual,
            LessThan,
            GreaterThanOrEqual,
            GreaterThan,
            BeginsWith,
            Between,
            AllByPrefix,
            Contains,
            Exists,
            NotExists,
        }

        public class FoundCondition
        {
            public MemberInfo Member { get; set; }

            public object Value { get; set; }

            public object Value2 { get; set; }

            public int CurrentArg { get; set; }

            public ConditionComparator Comparator  { get; set; }

            public KeyAttribute[] KeyAttributes { get; set; }

            public AttributeAttribute AttributeAttribute { get; set; }

            public bool Method { get; set; }

            public int N { get; set; }

            public string ComparatorString
            {
                get
                {
                    return Comparator switch
                    {
                        ConditionComparator.Equal => " = ",
                        ConditionComparator.NotEqual => " <> ",
                        ConditionComparator.LessThanOrEqual => " <= ",
                        ConditionComparator.LessThan => " < ",
                        ConditionComparator.GreaterThan => " > ",
                        ConditionComparator.GreaterThanOrEqual => " >= ",

                        ConditionComparator.Contains => "contains({0},",
                        ConditionComparator.BeginsWith => "begins_with({0},",
                        ConditionComparator.Exists => "attribute_exists({0})",
                        ConditionComparator.NotExists => "attribute_not_exists({0})",
                        _ => throw new ArgumentException("Invalid operator"),
                    };
                }
            }
        }

        private KeyValuePair<string, string> GetExpessionName(MemberInfo memberInfo, KeyAttribute[] keys, AttributeAttribute attr)
        {
            if (attr != null)
            {
                // standard attr - either member name or override
                string name = attr.FieldName ?? memberInfo.Name;
                return new KeyValuePair<string, string>("#" + memberInfo.Name.ToLower(), name);
            } 
            else
            {
                // must be key - get the lowest index key
                var key = keys.OrderBy(k => k.IndexNumber).First();
                return new KeyValuePair<string, string>("#" + memberInfo.Name.ToLower(), key.GetFieldName(_context));
            }            
        }

        private KeyValuePair<string, AttributeValue> GetExpessionValue(FoundCondition condition, object val)
        {
            string placeHolder = ":" + condition.Member.Name.ToLower() + condition.N;

            // special case for null value
            if (val == NullPlaceholder.Value())
            {
                return new KeyValuePair<string, AttributeValue>(placeHolder, new AttributeValue
                {
                    NULL = true,
                });
            }

            var propertyType = ((PropertyInfo)condition.Member).PropertyType;
            var converter = _context.AttributizerSettings.GetConverter(propertyType);

            if (condition.AttributeAttribute != null)
            {
                // standard attr
                if (converter.TryWriteAsObject(val, propertyType, out AttributeValue attrValue))
                {
                    return new KeyValuePair<string, AttributeValue>(placeHolder, attrValue);
                }
            } 
            else
            {
                // key attr - get the lowest index key
                var keyAttr = condition.KeyAttributes.OrderBy(k => k.IndexNumber).First();
                if (converter.TryWriteAsObject(val, propertyType, out AttributeValue attrValue))
                {
                    if (keyAttr.Prefix != null)
                    {
                        // attribute must be string regardless of attribute type

                        // we support string or number types
                        string finalValue = attrValue.S;
                        if (attrValue.N != null)
                            finalValue = attrValue.N;

                        return new KeyValuePair<string, AttributeValue>(placeHolder, new AttributeValue
                        {
                            S = _context.FormatKeyPrefix(keyAttr, finalValue)
                        });
                    }
                    else
                    {
                        // what the converter sends back
                        return new KeyValuePair<string, AttributeValue>(placeHolder, attrValue);
                    }
                }                
            }
            throw new UnableToWriteAttributeValueException();
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
                // must be either a key or attr
                var keys = node.Member.GetCustomAttributes().Where(a => a.GetType().IsAssignableTo(typeof(KeyAttribute))).Select(ca => ca as KeyAttribute).ToArray();
                var attr = node.Member.GetCustomAttribute<AttributeAttribute>();

                if (((keys == null) || (keys.Length == 0)) && (attr == null))
                    throw new ArgumentException("Condition expressions must be comprised of key or attribute properties");
                if ((!_allowKeys) && (keys != null) && (keys.Length > 0))
                    throw new ArgumentException("Filter expressions cannot contain key properties");

                if (_currentCondition == null)
                {
                    // not in a condition

                    _currentCondition = new FoundCondition()
                    {
                        Member = node.Member,
                        KeyAttributes = keys,
                        AttributeAttribute = attr,
                        N = _foundConditions.Count + 1
                    };
                    _foundConditions.Add(_currentCondition);

                    // replace member name with #name
                    var expName = GetExpessionName(node.Member, keys, attr);
                    _expressionNames[expName.Key] = expName.Value;

                    if (_methodOperator.HasValue)
                    {
                        _currentCondition.Comparator = _methodOperator.Value;
                        _methodOperator = null;
                        _currentCondition.Method = true;

                        if ((_currentCondition.Comparator == ConditionComparator.Exists) ||
                            (_currentCondition.Comparator == ConditionComparator.NotExists))
                        {
                            _conditionExpression += string.Format(_currentCondition.ComparatorString, expName.Key);

                            // done with condition
                            _currentCondition = null;
                        } 
                        else if (_currentCondition.Comparator == ConditionComparator.Between)
                        {
                            _conditionExpression += expName.Key;                            
                        }
                        else
                        {
                            // do nothing
                        }
                    } 
                    else
                    {
                        _conditionExpression += expName.Key;
                    }
                }
                else
                {
                    // in a condition

                    _conditionExpression += _currentCondition.ComparatorString;

                    var expName = GetExpessionName(node.Member, keys, attr);
                    _expressionNames[expName.Key] = expName.Value;
                    _conditionExpression += expName.Key;

                    _currentCondition = null;
                }
            }

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (_currentCondition == null)
            {
                // must be in a condition to see a constant
                throw new ArgumentException("Shouldn't see a constant here");
            }
            else
            {
                // in a condition

                if (_currentCondition.CurrentArg == 0)
                {
                    _currentCondition.Value = GetValue(node.Value);
                } 
                else if (_currentCondition.CurrentArg == 1)
                {
                    _currentCondition.Value2 = GetValue(node.Value);
                }
                _currentCondition.CurrentArg++;

                if (_currentCondition.Comparator  == ConditionComparator.Between)
                {
                    if (_currentCondition.CurrentArg > 1)
                    {
                        var expValue1 = GetExpessionValue(_currentCondition, _currentCondition.Value);
                        var expValue2 = GetExpessionValue(_currentCondition, _currentCondition.Value2);
                        _expressionValues[expValue1.Key + "a"] = expValue1.Value;
                        _expressionValues[expValue2.Key + "b"] = expValue2.Value;

                        _conditionExpression += string.Format(" BETWEEN {0}a AND {0}b ", expValue1.Key);

                        _currentCondition = null;
                    }   
                } 
                else
                {
                    // add operator and value
                    var expName = GetExpessionName(_currentCondition.Member, _currentCondition.KeyAttributes, _currentCondition.AttributeAttribute);
                    _conditionExpression += string.Format(_currentCondition.ComparatorString, expName.Key);

                    var expValue = GetExpessionValue(_currentCondition, _currentCondition.Value);
                    _expressionValues[expValue.Key] = expValue.Value;
                    _conditionExpression += expValue.Key;

                    if (_currentCondition.Method)
                    {
                        _conditionExpression += ")";
                    }

                    _currentCondition = null;
                }
            }
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            // must be and also
            _conditionExpression += node.NodeType switch
            {
                ExpressionType.Not => " NOT ",
                _ => throw new ArgumentException("Invalid operator"),
            };

            Visit(node.Operand);
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            bool paren = (node.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.OrElse);
            if (paren && _level++ > 0)
            {
                _conditionExpression += "(";
            }

            Visit(node.Left);

            if (_currentCondition == null)
            {
                // must be and also
                _conditionExpression += node.NodeType switch
                {
                    ExpressionType.AndAlso => " AND ",
                    ExpressionType.OrElse => " OR ",                    
                    _ => throw new ArgumentException("Invalid operator"),
                };
            }
            else
            {
                // determine comparator
                _currentCondition.Comparator = node.NodeType switch
                {
                    ExpressionType.Equal => ConditionComparator.Equal,
                    ExpressionType.NotEqual => ConditionComparator.NotEqual,
                    ExpressionType.LessThanOrEqual => ConditionComparator.LessThanOrEqual,
                    ExpressionType.LessThan => ConditionComparator.LessThan,
                    ExpressionType.GreaterThan => ConditionComparator.GreaterThan,
                    ExpressionType.GreaterThanOrEqual => ConditionComparator.GreaterThanOrEqual,
                    _ => throw new ArgumentException("Invalid operator"),
                };
            }

            Visit(node.Right);

            if (paren && --_level > 0)
            {
                _conditionExpression += ")";
            }

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
                "allbyprefix" => ConditionComparator.AllByPrefix,
                "beginswith" => ConditionComparator.BeginsWith,                
                "between" => ConditionComparator.Between,
                "doescontain" => ConditionComparator.Contains,
                "exists" => ConditionComparator.Exists,
                "notexists" => ConditionComparator.NotExists,
                _ => throw new ArgumentException("Invalid method"),
            };
            foreach (var arg in node.Arguments)
                Visit(arg);

            return node;
        }

        private object GetValue(object input)
        {
            if (input == null)
                return NullPlaceholder.Value();

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

    public class ItemConditionalDetails
    {
        public string ConditionExpression { get; set; }

        public Dictionary<string, AttributeValue> AttributeValues { get; set; }

        public Dictionary<string, string> AttributeNames { get; set; }

        public ItemConditionalDetails()
        {
            ConditionExpression = "";
            AttributeValues = new();
            AttributeNames = new();
        }        
    }

    /// <summary>
    /// Handle the conditional and filter expressions
    /// </summary>
    public class ItemConditionalExpressionHandler<TItem> where TItem : class
    {
        readonly TableContext _context;

        public ItemConditionalExpressionHandler(TableContext context)
        {
            _context = context;
        }
      
        public ItemConditionalDetails Handle(Expression<Func<TItem, bool>> predicate, bool allowKeys)
        {
            // visit expression
            var visitor = new ItemConditionalExpressionVisitor(_context, allowKeys);
            visitor.Visit(predicate.Body);

            return new ItemConditionalDetails
            {
                ConditionExpression = visitor.ConditionExpression.Trim(),
                AttributeNames = visitor.AttributeNames,
                AttributeValues = visitor.AttributeValues,
            };
        }
    }
}
