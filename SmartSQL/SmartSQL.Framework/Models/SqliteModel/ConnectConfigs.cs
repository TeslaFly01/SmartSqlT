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
    /// <summary>
    /// 连接信息
    /// </summary>
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
                    case DbType.SqlServer: return "/SmartSQL;component/Resources/svg/sqlserver.svg";
                    case DbType.MySql: return "/SmartSQL;component/Resources/svg/mysql.svg";
                    case DbType.PostgreSQL: return "/SmartSQL;component/Resources/svg/postgresql.svg";
                    case DbType.Sqlite: return "/SmartSQL;component/Resources/svg/sqlite@64.svg";
                    case DbType.Oracle: return "/SmartSQL;component/Resources/svg/database/icon_oracle_64.svg";
                    case DbType.Dm: return "/SmartSQL;component/Resources/svg/dameng.svg";
                    default: return "";
                }
            }
        }

        /// <summary>
        /// Master数据库连接，查询系统库相关信息（不映射数据库)
        /// </summary>
        [Ignore]
        public string DbMasterConnectString
        {
            get
            {
                var connectString = string.Empty;
                switch (DbType)
                {
                    case DbType.SqlServer:
                        connectString = ConnectionStringUtil.SqlServerString(ServerAddress, ServerPort, "master",
                            UserName, Password);
                        break;
                    case DbType.MySql:
                        connectString = ConnectionStringUtil.MySqlString(ServerAddress, ServerPort, DefaultDatabase,
                            UserName, Password);
                        break;
                    case DbType.PostgreSQL:
                        connectString = ConnectionStringUtil.PostgreSqlString(ServerAddress, ServerPort, DefaultDatabase, UserName, Password);
                        break;
                    case DbType.Sqlite:
                        connectString = $@"DataSource={ServerAddress}";
                        break;
                    case DbType.Oracle:
                        connectString = ConnectionStringUtil.OracleString(ServerAddress, ServerPort, DefaultDatabase, UserName, Password);
                        break;
                    case DbType.Dm:
                        connectString = ConnectionStringUtil.DmString(ServerAddress, ServerPort, DefaultDatabase, UserName, Password);
                        break;
                }
                return connectString;
            }
        }
        /// <summary>
        /// 默认数据库连接，查询默认数据库相关信息（不映射数据库)
        /// </summary>
        [Ignore]
        public string DbDefaultConnectString
        {
            get
            {
                var connectString = string.Empty;
                switch (DbType)
                {
                    case DbType.SqlServer:
                        connectString = ConnectionStringUtil.SqlServerString(ServerAddress, ServerPort, DefaultDatabase,
                            UserName, Password);
                        break;
                    case DbType.MySql:
                        connectString = ConnectionStringUtil.MySqlString(ServerAddress, ServerPort, DefaultDatabase,
                            UserName, Password);
                        break;
                    case DbType.PostgreSQL:
                        connectString = ConnectionStringUtil.PostgreSqlString(ServerAddress, ServerPort, DefaultDatabase, UserName, Password);
                        break;
                    case DbType.Sqlite:
                        connectString = $"DataSource={ServerAddress}";
                        break;
                    case DbType.Oracle:
                        connectString = ConnectionStringUtil.OracleString(ServerAddress, ServerPort, DefaultDatabase, UserName, Password);
                        break;
                    case DbType.Dm:
                        connectString = ConnectionStringUtil.DmString(ServerAddress, ServerPort, DefaultDatabase, UserName, Password);
                        break;
                }
                return connectString;
            }
        }

        /// <summary>
        /// 当前选中数据库连接
        /// </summary>
        /// <param name="selectedDatabase"></param>
        /// <returns></returns>
        public string SelectedDbConnectString(string selectedDatabase)
        {
            var connectString = string.Empty;
            switch (DbType)
            {
                case DbType.SqlServer:
                    connectString = ConnectionStringUtil.SqlServerString(ServerAddress, ServerPort, selectedDatabase,
                        UserName, Password);
                    break;
                case DbType.MySql:
                    connectString = ConnectionStringUtil.MySqlString(ServerAddress, ServerPort, selectedDatabase,
                        UserName, Password);
                    break;
                case DbType.PostgreSQL:
                    connectString = ConnectionStringUtil.PostgreSqlString(ServerAddress, ServerPort, selectedDatabase, UserName, Password);
                    break;
                case DbType.Sqlite:
                    connectString = $"DataSource={ServerAddress}";
                    break;
                case DbType.Oracle:
                    connectString = ConnectionStringUtil.OracleString(ServerAddress, ServerPort, selectedDatabase, UserName, Password);
                    break;
                case DbType.Dm:
                    connectString = ConnectionStringUtil.DmString(ServerAddress, ServerPort, selectedDatabase, UserName, Password);
                    break;
            }
            return connectString;
        }
    }
}
