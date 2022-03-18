using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SqlSugar;

namespace SmartCode.Framework.Exporter
{
    using PhysicalDataModel;

    public interface IExporter
    {
        /// <summary>
        /// 连接初始化获取对象列表
        /// </summary>
        /// <returns></returns>
        Model Init();
        /// <summary>
        /// 获取数据库列表
        /// </summary>
        /// <returns></returns>
        List<DataBase> GetDatabases();
        /// <summary>
        /// 获取对象列信息
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        Columns GetColumnInfoById(string objectId);
        /// <summary>
        /// 获取脚本信息
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        string GetScriptInfoById(string objectId, DbObjectType objectType);
        /// <summary>
        /// 创建表SQL
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        string CreateTableSql(string tableName, List<Column> columns);
    }
}
