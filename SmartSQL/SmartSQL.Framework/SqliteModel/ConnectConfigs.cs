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
                    case DbType.SqlServer: return "/SmartSQL;component/Resources/svg/sqlserver.svg";
                    case DbType.MySql: return "/SmartSQL;component/Resources/svg/mysql.svg";
                    case DbType.PostgreSQL: return "/SmartSQL;component/Resources/svg/postgresql.svg";
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
                        var serAddress = ServerAddress.Equals(".") ? "." : $"{ServerAddress},{ServerPort}";
                        connectString =
                            $"server={serAddress};database=master;uid={UserName};pwd={EncryptHelper.Decode(Password)};";
                        break;
                    case DbType.MySql:
                        connectString =
                            $"Server={ServerAddress};uid={UserName};pwd={EncryptHelper.Decode(Password)};database={DefaultDatabase};Allow User Variables=True;";
                        break;
                    case DbType.PostgreSQL:
                        connectString = $"HOST={ServerAddress};" +
                                        $"PORT={ServerPort};" +
                                        $"DATABASE={DefaultDatabase};" +
                                        $"USER ID={UserName};" +
                                        $"PASSWORD={EncryptHelper.Decode(Password)}";
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
                        var serAddress = ServerAddress.Equals(".") ? "." : $"{ServerAddress},{ServerPort}";
                        connectString =
                            $"server={serAddress};database={DefaultDatabase};uid={UserName};pwd={EncryptHelper.Decode(Password)};";
                        break;
                    case DbType.MySql:
                        connectString = $"Server={ServerAddress};uid={UserName};pwd={EncryptHelper.Decode(Password)};database={DefaultDatabase};Allow User Variables=True;";
                        break;
                    case DbType.PostgreSQL:
                        connectString = $"HOST={ServerAddress};" +
                                        $"PORT={ServerPort};" +
                                        $"DATABASE={DefaultDatabase};" +
                                        $"USER ID={UserName};" +
                                        $"PASSWORD={EncryptHelper.Decode(Password)}";
                        break;
                }
                return connectString;
            }
        }

        public string SelectedDbConnectString(string selectedDatabase)
        {
            var connectString = string.Empty;
            switch (DbType)
            {
                case DbType.SqlServer:
                    var serAddress = ServerAddress.Equals(".") ? "." : $"{ServerAddress},{ServerPort}";
                    connectString =
                        $"server={serAddress};database={selectedDatabase};uid={UserName};pwd={EncryptHelper.Decode(Password)};";
                    break;
                case DbType.MySql:
                    connectString = $"Server={ServerAddress};port={ServerPort};uid={UserName};pwd={EncryptHelper.Decode(Password)};database={selectedDatabase};Allow User Variables=True;";
                    break;
                case DbType.PostgreSQL:
                    connectString = $"HOST={ServerAddress};" +
                                    $"PORT={ServerPort};" +
                                    $"DATABASE={selectedDatabase};" +
                                    $"USER ID={UserName};" +
                                    $"PASSWORD={EncryptHelper.Decode(Password)}";
                    break;
            }
            return connectString;
        }
    }
}
