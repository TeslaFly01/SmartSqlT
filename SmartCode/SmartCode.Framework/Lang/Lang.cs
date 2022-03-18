using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartCode.Framework.PhysicalDataModel;

namespace SmartCode.Framework.Lang
{
    public abstract class Lang
    {

        public abstract string BuildEntity(string tableName, List<Column> columns);
    }
}
