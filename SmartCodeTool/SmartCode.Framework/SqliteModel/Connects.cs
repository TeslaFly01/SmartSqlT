using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace SmartCode.Framework.SqliteModel
{
    public class Connects
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        /// <summary>
        /// 连接名称
        /// </summary>
        public string ConnectName { get; set; }
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string ServerAddress { get; set; }
        /// <summary>
        /// 服务器端口号
        /// </summary>
        public int ServerPort { get; set; }
        /// <summary>
        /// 身份认证
        /// </summary>
        public int Authentication { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public int CreateDate { get; set; }
        /// <summary>
        /// 默认数据库
        /// </summary>
        public string DefaultDatabase { get; set; }
    }
}
