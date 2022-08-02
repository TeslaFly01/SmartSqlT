using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Framework.Util
{
    public static class ConnectionStringUtil
    {
        /// <summary>
        /// SQLServer连接字符串
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="port"></param>
        /// <param name="database"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string SqlServerString(string serverAddress, int port, string database, string userName, string password)
        {
            var serAddress = serverAddress.Equals(".") ? "." : $"{serverAddress},{port}";
            var connectString =
                $@"server={serAddress};database={database};uid={userName};pwd={EncryptHelper.Decode(password)};";
            return connectString;
        }

        /// <summary>
        /// MySql数据库字符串
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="port"></param>
        /// <param name="database"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string MySqlString(string serverAddress, int port, string database, string userName, string password)
        {
            var connectString =
                 $@"server={serverAddress};port={port};uid={userName};pwd={EncryptHelper.Decode(password)};database={database};Allow User Variables=True;sslmode=none;";
            return connectString;
        }

        /// <summary>
        /// PostgreSQL数据库字符串
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="port"></param>
        /// <param name="database"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string PostgreSqlString(string serverAddress,int port,string database,string userName,string password)
        {
            var connectString = $@"HOST={serverAddress};" +
                            $"PORT={port};" +
                            $"DATABASE={database};" +
                            $"USER ID={userName};" +
                            $"PASSWORD={EncryptHelper.Decode(password)}";
            return connectString;
        }
    }
}
