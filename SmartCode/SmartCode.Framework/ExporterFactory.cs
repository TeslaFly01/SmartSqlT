using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartCode.Framework.Exporter;
using SqlSugar;

namespace SmartCode.Framework
{
    public class ExporterFactory
    {
        /// <summary>
        /// 创建访问数据库的实例工厂
        /// </summary>
        /// <param name="type">数据库类型</param>
        /// <param name="dbConnectionString">数据库连接字符串，访问数据库时必填</param>
        /// <returns></returns>
        public static Exporter.Exporter CreateInstance(DbType type, string dbConnectionString = "")
        {
            switch (type)
            {
                case DbType.SqlServer: return new SqlServerExporter(dbConnectionString);
                case DbType.MySql: return new MySqlExporter(dbConnectionString);
                default: return new SqlServerExporter(dbConnectionString);
            }
        }
    }
}
