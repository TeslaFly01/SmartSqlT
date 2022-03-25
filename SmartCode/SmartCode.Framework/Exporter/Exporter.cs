using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SqlSugar;

namespace SmartCode.Framework.Exporter
{
    using PhysicalDataModel;

    public abstract class Exporter : IExporter
    {
        public Exporter(string connectionString)
        {
            DbConnectString = connectionString;
        }
        /// <summary>
        /// 数据库连接
        /// </summary>
        public string DbConnectString { get; private set; }
        /// <summary>
        /// 连接初始化获取对象列表
        /// </summary>
        /// <returns></returns>
        public abstract Model Init();
        /// <summary>
        /// 获取数据库列表
        /// </summary>
        /// <returns></returns>
        public abstract List<DataBase> GetDatabases();
        /// <summary>
        /// 获取对象列信息
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public abstract Columns GetColumnInfoById(string objectId);
        /// <summary>
        /// 获取脚本信息
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public abstract string GetScriptInfoById(string objectId, DbObjectType objectType);
        /// <summary>
        /// 创建表SQL
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public abstract string CreateTableSql(string tableName, List<Column> columns);

        /// <summary>
        /// 查询数据sql脚本
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public abstract string SelectSql(string tableName, List<Column> columns);

        /// <summary>
        /// 插入数据sql脚本
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public abstract string InsertSql(string tableName, List<Column> columns);

        /// <summary>
        /// 更新数据sql脚本
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public abstract string UpdateSql(string tableName, List<Column> columns);

        /// <summary>
        /// 删除数据sql脚本
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public abstract string DeleteSql(string tableName, List<Column> columns);

        /// <summary>
        /// 添加列sql脚本
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public abstract string AddColumnSql(string tableName, List<Column> columns);

        /// <summary>
        /// 修改列sql脚本
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public abstract string AlterColumnSql(string tableName, List<Column> columns);

        /// <summary>
        /// 删除列sql脚本
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public abstract string DropColumnSql(string tableName, List<Column> columns);
    }
}
