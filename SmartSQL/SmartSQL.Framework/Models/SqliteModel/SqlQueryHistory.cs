using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using SqlSugar;

namespace SmartSQL.Framework.SqliteModel
{
    /// <summary>
    /// SQL查询历史表
    /// </summary>
    public class SqlQueryHistory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        /// <summary>
        /// 连接名称
        /// </summary>
        public string ConnectName { get; set; }
        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DataBaseName { get; set; }
        /// <summary>
        /// 查询时间
        /// </summary>
        public DateTime QueryTime { get; set; }
        /// <summary>
        /// 查询SQL
        /// </summary>
        public string QuerySql { get; set; }
        /// <summary>
        /// 受影响行
        /// </summary>
        public int BackRows { get; set; }
        /// <summary>
        /// 耗时（毫秒）
        /// </summary>
        public double TimeConsuming { get; set; }
    }
}
