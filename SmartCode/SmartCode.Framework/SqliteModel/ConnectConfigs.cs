using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSQL.Framework.Util;
using SQLite;
using DbType = SqlSugar.DbType;

namespace SmartSQL.Framework.SqliteModel
{
    public class ConnectConfigs
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        /// <summary>
        /// 连接名称
        /// </summary>
        public string ConnectName { get; set; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public DbType DbType { get; set; }
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
        public DateTime CreateDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 默认数据库
        /// </summary>
        public string DefaultDatabase { get; set; }

        [Ignore]
        public string Icon
        {
            get
            {
                switch (DbType)
                {
                    case DbType.MySql: return "/SmartCode;component/Resources/Img/mysql.png";
                    default: return "/SmartCode;component/Resources/Img/mssql.png";
                }
            }
        }

        /// <summary>
        /// Master数据库连接，查询系统库相关信息（不映射数据库)
        /// </summary>
        [Ignore]
        public string DbMasterConnectString =>
            DbType == DbType.SqlServer ?
                $"server={ServerAddress},{ServerPort};database=master;uid={UserName};pwd={EncryptHelper.Decode(Password)};"
                : $"Server={ServerAddress};uid={UserName};pwd={EncryptHelper.Decode(Password)};database={DefaultDatabase};Allow User Variables=True;";
        /// <summary>
        /// 默认数据库连接，查询默认数据库相关信息（不映射数据库)
        /// </summary>
        [Ignore]
        public string DbDefaultConnectString => DbType == DbType.SqlServer
            ? $"server={ServerAddress},{ServerPort};database={DefaultDatabase};uid={UserName};pwd={EncryptHelper.Decode(Password)};"
            : $"Server={ServerAddress};uid={UserName};pwd={EncryptHelper.Decode(Password)};database={DefaultDatabase};Allow User Variables=True;";

        public string SelectedDbConnectString(string selectedDatabase)
        {
            return DbType == DbType.SqlServer
                ? $"server={ServerAddress},{ServerPort};database={selectedDatabase};uid={UserName};pwd={EncryptHelper.Decode(Password)};"
                : $"Server={ServerAddress};port={ServerPort};uid={UserName};pwd={EncryptHelper.Decode(Password)};database={selectedDatabase};Allow User Variables=True;";
        }
    }
}
