using Amazon.DynamoDBv2.Model;
using NBasis.OneTable.Annotations;
using NBasis.OneTable.Exceptions;
using System.Linq.Expressions;
using System.Reflection;

namespace NBasis.OneTable.Expressions
{
    /*
     * Expressions:
     * - Point here is to write expression in terms of the item we're working with
     * - Most key expressions should be PK + SK
     * - PK Must always be specified
     * - If TItem has an SK, then SK must always be specified
     * - GSI is ignored here
     */

    public class ItemKeyExpressionVisitor : ExpressionVisitor
    {
        readonly Stack<string> _fieldNames = new();
        readonly List<FoundKey> _foundKeys = new();
        private FoundKey _currentKey = null;

        public FoundKey[] FoundKeys
        {
            get { return _foundKeys.ToArray(); }
        }

        public class FoundKey
        {
            public MemberInfo Member { get; set; }

            public object Value { get; set; }

            private PKAttribute _pKAttribute = null;

            public PKAttribute PKAttribute 
            { 
                get
                {
                    _pKAttribute ??= Member.GetCustomAttribute(typeof(PKAttribute)) as PKAttribute;
                    return _pKAttribute;
                }
            }

            private SKAttribute _sKAttribute = null;

            public SKAttribute SKAttribute
            {
                get
                {
                    _sKAttribute ??= Member.GetCustomAttribute(typeof(SKAttribute)) as SKAttribute;
                    return _sKAttribute;
                }
            }
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
                        Member = node.Member
                    };
                    _foundKeys.Add(_currentKey);
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
                _currentKey.Value = GetValue(node.Value);
                _currentKey = null;
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
                // must be equal
                if (node.NodeType != ExpressionType.Equal)
                    throw new ArgumentException("Can only see Equal here");
            }

            Visit(node.Right);

            return node;
        }

        private object GetValue(object input)
        {
            var type = input.GetType();

            // if it is not simple value
            if (type.IsClass && type != typeof(string))
            {
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

    public class ItemKeyExpressionHandler<TItem> where TItem : class
    {
        readonly TableContext _context;

        public ItemKeyExpressionHandler(TableContext context)
        {
            _context = context;
        }

        public Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue> Handle(Expression<Func<TItem, bool>> predicate)
        {
            // visit expression
            var visitor = new ItemKeyExpressionVisitor();
            visitor.Visit(predicate);

            // get found keys from visitor
            var foundKeys = visitor.FoundKeys;

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

            var pk = foundKeys.SingleOrDefault(k => k.PKAttribute != null) ?? throw new ArgumentException("Missing PK from key expression");
            keyItem[_context.Configuration.KeyAttributes.PKName] = getAttribute(pk.Member, pk.Value, pk.PKAttribute);

            var sk = foundKeys.SingleOrDefault(k => k.SKAttribute != null);
            if (sk != null)
            {
                keyItem[_context.Configuration.KeyAttributes.SKName] = getAttribute(sk.Member, sk.Value, sk.SKAttribute);
            }

            return keyItem;
        }
    }
}
