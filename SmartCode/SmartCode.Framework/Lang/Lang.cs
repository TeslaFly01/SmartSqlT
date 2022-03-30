using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSQL.Framework.PhysicalDataModel;

namespace SmartSQL.Framework.Lang
{
    public abstract class Lang
    {
        public string TableName { get; set; }
        public List<Column> Columns { get; set; }

        public Lang(string tableName, List<Column> columns)
        {
            TableName = tableName;
            Columns = columns;
        }

        public abstract string BuildEntity();
    }
}
