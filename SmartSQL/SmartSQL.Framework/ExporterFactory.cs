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
                case DbType.Dm: return new DmExporter(dbConnectionString);
                case DbType.Redis: return new RedisExporter(dbConnectionString);
                default: return new SqlServerExporter(dbConnectionString);
            }
        }

        public static Exporter.Exporter CreateInstance(DbType type, string dbConnectionString, string dbName)
        {
            switch (type)
            {
                case DbType.SqlServer: return new SqlServerExporter(dbConnectionString, dbName);
                case DbType.MySql: return new MySqlExporter(dbConnectionString, dbName);
                case DbType.PostgreSQL: return new PostgreSqlExporter(dbConnectionString, dbName);
                case DbType.Sqlite: return new SqliteExporter(dbConnectionString, dbName);
                case DbType.Oracle: return new OracleExporter(dbConnectionString, dbName);
                case DbType.Dm: return new DmExporter(dbConnectionString, dbName);
                case DbType.Redis: return new RedisExporter(dbConnectionString, dbName);
                default: return new SqlServerExporter(dbConnectionString, dbName);
            }
        }

        public static Exporter.Exporter CreateInstance(DbType type, Table table, List<Column> columns)
        {
            switch (type)
            {
                case DbType.SqlServer: return new SqlServerExporter(table, columns);
                case DbType.MySql: return new MySqlExporter(table, columns);
                case DbType.PostgreSQL: return new PostgreSqlExporter(table, columns);
                case DbType.Sqlite: return new SqliteExporter(table, columns);
                case DbType.Oracle: return new OracleExporter(table, columns);
                case DbType.Dm: return new DmExporter(table, columns);
                case DbType.Redis: return new RedisExporter(table, columns);
                default: return new SqlServerExporter(table, columns);
            }
        }
    }
}
