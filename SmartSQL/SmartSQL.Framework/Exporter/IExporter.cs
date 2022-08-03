using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SqlSugar;

namespace SmartSQL.Framework.Exporter
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
        /// 更新列注释
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        bool UpdateColumnRemark(string tableName, string columnName, string remark);
        /// <summary>
        /// 创建表SQL
        /// </summary>
        /// <returns></returns>
        string CreateTableSql();

        /// <summary>
        /// 查询数据sql脚本
        /// </summary>
        /// <returns></returns>
        string SelectSql();

        /// <summary>
        /// 插入数据sql脚本
        /// </summary>
        /// <returns></returns>
        string InsertSql();

        /// <summary>
        /// 更新数据sql脚本
        /// </summary>
        /// <returns></returns>
        string UpdateSql();

        /// <summary>
        /// 删除数据sql脚本
        /// </summary>
        /// <returns></returns>
        string DeleteSql();

        /// <summary>
        /// 添加列sql脚本
        /// </summary>
        /// <returns></returns>
        string AddColumnSql();

        /// <summary>
        /// 修改列sql脚本
        /// </summary>
        /// <returns></returns>
        string AlterColumnSql();

        /// <summary>
        /// 删除列sql脚本
        /// </summary>
        /// <returns></returns>
        string DropColumnSql();
    }
}
