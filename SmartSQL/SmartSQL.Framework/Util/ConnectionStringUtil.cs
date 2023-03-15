using Dm;
using MySql.Data.MySqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Data.SqlClient;
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
            var serAddress = $"{serverAddress},{port}";
            if (serverAddress.Equals(".") || serverAddress.Contains(@"\"))
            {
                serAddress = serverAddress;
            }
            //var connectString = $@"server={serAddress};
            //                     database={database};
            //                          uid={userName};
            //                          pwd={EncryptHelper.Decode(password)};";
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serAddress,
                InitialCatalog = database,
                UserID = userName,
                Password = EncryptHelper.Decode(password)
            };
            //return connectString;
            return sqlConnectionStringBuilder.ConnectionString;
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
            //var connectString = $@"server={serverAddress};
            //                         port={port};
            //                          uid={userName};
            //                          pwd={EncryptHelper.Decode(password)};                                
            //                 SslMode=Preferred;
            //         Allow User Variables=True;
            //      AllowPublicKeyRetrieval=true;";
            var mySqlConnectionStringBuild = new MySqlConnectionStringBuilder
            {
                Server = serverAddress,
                Port = Convert.ToUInt32(port),
                UserID = userName,
                Password = EncryptHelper.Decode(password),
                SslMode = MySqlSslMode.Preferred,
                AllowUserVariables = true,
                AllowPublicKeyRetrieval = true,
                CharacterSet = ""
            };
            if (!string.IsNullOrEmpty(database))
            {
                //connectString += $@"database ={database};";
                mySqlConnectionStringBuild.Database = database;
            }
            //return connectString;
            return mySqlConnectionStringBuild.ConnectionString;
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
        public static string PostgreSqlString(string serverAddress, int port, string database, string userName, string password)
        {
            database = database.Contains(":") ? database.Split(':')[0] : database;
            //var connectString = $@"HOST={serverAddress};
            //                       PORT={port};
            //                   DATABASE={database};
            //                    USER ID={userName};
            //                   PASSWORD={EncryptHelper.Decode(password)}";
            var npgSqlConnectionStringBuild = new NpgsqlConnectionStringBuilder
            {
                Host = serverAddress,
                Port = port,
                Database = database,
                Username = userName,
                Password = EncryptHelper.Decode(password)
            };
            //return connectString;
            return npgSqlConnectionStringBuild.ConnectionString;
        }

        /// <summary>
        /// Oracle数据库字符串
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="port"></param>
        /// <param name="serviceName"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string OracleString(string serverAddress, int port, string serviceName, string userName, string password)
        {
            //var connectString = $@"Data Source={serverAddress}:{port}/{serviceName};User ID={userName};Password={EncryptHelper.Decode(password)};Pooling=False;";
            var oracleConnectionStringBuilder = new OracleConnectionStringBuilder
            {
                DataSource = $"{serverAddress}:{port}/{serviceName}",
                UserID = userName,
                Password = EncryptHelper.Decode(password),
                Pooling = false
            };
            //return connectString;
            return oracleConnectionStringBuilder.ConnectionString;
        }

        /// <summary>
        /// 达梦数据库字符串
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="port"></param>
        /// <param name="serviceName"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string DmString(string serverAddress, int port, string serviceName, string userName, string password)
        {
            //var connectString = $@"HOST={serverAddress};PORT={port};DATABASE={serviceName};USER ID={userName};PASSWORD={EncryptHelper.Decode(password)};";
            var dmConnectionStringBuilder = new DmConnectionStringBuilder
            {
                Host = serverAddress,
                Port = port,
                Database = serviceName,
                User = userName,
                Password = EncryptHelper.Decode(password)
            };
            //return connectString;
            return dmConnectionStringBuilder.ConnectionString;
        }
    }
}
