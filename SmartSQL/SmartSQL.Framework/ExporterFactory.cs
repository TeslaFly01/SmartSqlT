using System;
using System.Collections.Generic;
using SmartSQL.Framework.Exporter;
using SmartSQL.Framework.PhysicalDataModel;
using SqlSugar;

namespace SmartSQL.Framework
{
    public class ExporterFactory
    {
        /// <summary>
        /// 创建访问数据库的实例工厂
        /// </summary>
        /// <param name="type">数据库类型</param>
        /// <param name="dbConnectionString">数据库连接字符串，访问数据库时必填</param>
        /// <returns></returns>
        public static Exporter.Exporter CreateInstance(DbType type, string dbConnectionString)
        {
            switch (type)
            {
                case DbType.SqlServer: return new SqlServerExporter(dbConnectionString);
                case DbType.MySql: return new MySqlExporter(dbConnectionString);
                case DbType.PostgreSQL: return new PostgreSqlExporter(dbConnectionString);
                case DbType.Sqlite: return new SqliteExporter(dbConnectionString);
                case DbType.Oracle: return new OracleExporter(dbConnectionString);
                default: return new SqlServerExporter(dbConnectionString);
            }
        }
        public static Exporter.Exporter CreateInstance(DbType type, string tableName, List<Column> columns)
        {
            switch (type)
            {
                case DbType.SqlServer: return new SqlServerExporter(tableName, columns);
                case DbType.MySql: return new MySqlExporter(tableName, columns);
                case DbType.PostgreSQL: return new PostgreSqlExporter(tableName, columns);
                case DbType.Sqlite: return new SqliteExporter(tableName, columns);
                case DbType.Oracle: return new OracleExporter(tableName, columns);
                default: return new SqlServerExporter(tableName, columns);
            }
        }
    }
}
