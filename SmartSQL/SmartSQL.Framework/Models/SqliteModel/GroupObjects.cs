using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace SmartSQL.Framework.SqliteModel
{
    public class GroupObjects
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        /// <summary>
        /// 所属分组ID
        /// </summary>
        public int GroupId { get; set; }
        /// <summary>
        /// 对象ID（数据库ID）
        /// </summary>
        public string ObjectId { get; set; }
        /// <summary>
        /// 对象名称
        /// </summary>
        public string ObjectName { get; set; }
        /// <summary>
        /// 所属连接ID
        /// </summary>
        public int ConnectId { get; set; }
        /// <summary>
        /// 所属数据库名称
        /// </summary>
        public string DatabaseName { get; set; }
    }
}
