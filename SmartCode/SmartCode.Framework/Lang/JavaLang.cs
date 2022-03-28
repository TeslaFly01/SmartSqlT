using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartCode.Framework.PhysicalDataModel;

namespace SmartCode.Framework.Lang
{
    public class JavaLang : Lang
    {
        public JavaLang(string tableName, List<Column> columns) : base(tableName, columns)
        {

        }

        public override string BuildEntity()
        {
            return "";
        }
    }
}
