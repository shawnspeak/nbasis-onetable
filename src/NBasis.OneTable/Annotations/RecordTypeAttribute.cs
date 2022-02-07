using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBasis.OneTable.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RecordTypeAttribute : Attribute
    {
        public RecordTypeAttribute(string recordType, string fieldNameOverride = null)
        {
        }
    }
}
