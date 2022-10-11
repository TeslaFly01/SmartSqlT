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
        public string TableComment { get; set; }
        public List<Column> Columns { get; set; }

        public Lang(string tbName, string tbComment, List<Column> columns)
        {
            TableName = tbName;
            TableComment = tbComment;
            Columns = columns;
        }

        public abstract string BuildEntity();
    }
}
