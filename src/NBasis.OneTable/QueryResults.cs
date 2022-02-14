using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBasis.OneTable
{
    public class QueryResults<TItem>
    {
        public long Count { get; internal set; }

        public long ScannedCount { get; internal set; }

        public IEnumerable<TItem> Results { get; internal set; }
    }
}
