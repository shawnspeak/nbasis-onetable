using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBasis.OneTable
{
    public static class QueryExtensions
    {
        public static bool BeginsWith<T>(this T obj, T start)
        {
            return true;
        }

        public static bool Between<T>(this T obj, T start, T end)
        {
            return true;
        }
    }
}
