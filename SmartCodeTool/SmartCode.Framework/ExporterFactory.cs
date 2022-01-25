using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartCode.Framework.Exporter;

namespace SmartCode.Framework
{
    public class ExporterFactory
    {
        public static Exporter.Exporter CreateInstance(DBType type, string ConnectionString)
        {
            switch (type)
            {
                case DBType.SqlServer: return new SqlServerExporter(ConnectionString);
                default: return new SqlServerExporter(ConnectionString);
            }
        }
    }
}
