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
    }
}
