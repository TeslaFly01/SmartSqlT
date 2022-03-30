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
        public static Lang.Lang CreateInstance(LangType type, string tableName, List<Column> columns)
        {
            switch (type)
            {
                case LangType.Csharp: return new CsharpLang(tableName, columns);
                case LangType.Java: return new JavaLang(tableName, columns);
                default: return new CsharpLang(tableName, columns);
            }
        }
    }
}
