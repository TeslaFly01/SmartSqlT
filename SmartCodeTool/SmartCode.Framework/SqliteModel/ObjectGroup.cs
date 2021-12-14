using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace SmartCode.Framework.SqliteModel
{
    public class ObjectGroup
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        /// <summary>
        /// 所属连接名称
        /// </summary>
        public string ConnectionName { get; set; }
        /// <summary>
        /// 所属数据库名
        /// </summary>
        public string DataBaseName { get; set; }
        /// <summary>
        /// 分组名
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// 排序标记
        /// </summary>
        public DateTime OrderFlag { get; set; }=DateTime.Now;

        public bool IsSelected { get; set; }
    }
}
