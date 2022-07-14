using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace SmartSQL.Framework.SqliteModel
{
    public class ObjectGroup
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        /// <summary>
        /// 连接ID
        /// </summary>
        public int ConnectId { get; set; }
        /// <summary>
        /// 所属数据库名
        /// </summary>
        public string DataBaseName { get; set; }
        /// <summary>
        /// 分组名
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// 展开层级：0.不展开，1.展开当前项，2.展开子项
        /// </summary>
        public int? OpenLevel { get; set; }
        /// <summary>
        /// 排序标记
        /// </summary>
        public DateTime OrderFlag { get; set; }=DateTime.Now;

        public bool IsSelected { get; set; }
    }
}
