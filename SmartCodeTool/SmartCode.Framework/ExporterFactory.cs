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
        public static Exporter.Exporter CreateInstance(DataBaseType type, string ConnectionString)
        {
            switch (type)
            {
                case DataBaseType.SqlServer: return new SqlServerExporter(ConnectionString);
                default: return new SqlServerExporter(ConnectionString);
            }
        }
    }
}
