using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBasis.OneTable
{
    public interface ITableNameResolver
    {
        string GetTableName<TItem>() where TItem : class;
    }

    internal class TableNameResolver : ITableNameResolver
    {
        readonly OneTableConfiguration _configuration;

        public TableNameResolver(OneTableConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetTableName<TItem>() where TItem : class
        {
            throw new NotImplementedException();
        }
    }
}
