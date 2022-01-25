using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBasis.OneTable
{
    public class QueryResults<TItem>
    {
        public IEnumerable<TItem> Results { get; }
    }
}
