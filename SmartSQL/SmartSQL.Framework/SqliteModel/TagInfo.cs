using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Framework.SqliteModel
{
    /// <summary>
    /// 标签信息表
    /// </summary>
    public class TagInfo
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int TagId { get; set; }
        /// <summary>
        /// 连接ID
        /// </summary>

        public int ConnectId { get; set; }
        /// <summary>
        /// 数据库名
        /// </summary>

        public string DataBaseName { get; set; }
        /// <summary>
        /// 标签名称
        /// </summary>

        public string TagName { get; set; }
        /// <summary>
        /// 对象数量
        /// </summary>
        public int SubCount { get; set; }
    }
}
