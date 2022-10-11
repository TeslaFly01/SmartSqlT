using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSQL.Framework.Lang;
using SmartSQL.Framework.PhysicalDataModel;

namespace SmartSQL.Framework
{
    public class LangFactory
    {
        public static Lang.Lang CreateInstance(LangType type, string tableName,string tableComment, List<Column> columns)
        {
            switch (type)
            {
                case LangType.Csharp: return new CsharpLang(tableName, tableComment, columns);
                case LangType.Java: return new JavaLang(tableName,tableComment, columns);
                default: return new CsharpLang(tableName, tableComment, columns);
            }
        }
    }
}
