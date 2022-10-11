using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSQL.Framework.PhysicalDataModel;

namespace SmartSQL.Framework.Lang
{
    public class JavaLang : Lang
    {
        public JavaLang(string tbName, string tbComment, List<Column> columns) : base(tbName, tbComment, columns)
        {

        }

        public override string BuildEntity()
        {
            return "";
        }
    }
}
