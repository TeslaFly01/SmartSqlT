using FreeSql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Framework.Util
{
    public sealed class FreeSqlHelper : SingletonConstructor<FreeSqlHelper>
    {
        //连接字符串作为 key，存储构建的 IFreeSql 对象的字典集合
        private readonly static ConcurrentDictionary<string, IFreeSql> _FreeDic = new ConcurrentDictionary<string, IFreeSql>();

        #region 构建 freesql 对象
        public IFreeSql FreeBuilder(DataType dbType, string connStr)
        {
            if (string.IsNullOrWhiteSpace(connStr))
            {
                return default;
            }
            bool isOk = _FreeDic.TryGetValue(connStr, out IFreeSql fsql);
            if (isOk)
            {
                return fsql;
            }
            var provider = typeof(FreeSql.SqlServer.SqlServerProvider<>);
            switch (dbType)
            {
                case DataType.Sqlite: provider= typeof(FreeSql.Sqlite.SqliteProvider<>); break;
                case DataType.MySql: provider =typeof(FreeSql.MySql.MySqlProvider<>); break;
                case DataType.SqlServer: provider= typeof(FreeSql.SqlServer.SqlServerProvider<>); break;
                case DataType.PostgreSQL: provider = typeof(FreeSql.PostgreSQL.PostgreSQLProvider<>); break;
                case DataType.Oracle: provider = typeof(FreeSql.Oracle.OracleProvider<>); break;
                case DataType.Dameng: provider= typeof(FreeSql.Dameng.DamengProvider<>); break;
            }
            fsql = new FreeSqlBuilder()
                            .UseConnectionString(dbType, connStr, provider)
                            .UseAutoSyncStructure(false) //自动同步实体结构到数据库
                            .Build(); //请务必定义成 Singleton 单例模式 

            bool isAdd = _FreeDic.TryAdd(connStr, fsql);
            if (isAdd)
            {
                return fsql;
            }
            else
            {
                fsql.Dispose();
                return _FreeDic[connStr];
            }
        }

        public (bool isOk, IFreeSql fsql) GetFreeSql(DataType dbType, string connStr)
        {
            bool isOk = _FreeDic.TryGetValue(connStr, out IFreeSql fsql);
            if (!isOk)
            {
                fsql = FreeBuilder(dbType, connStr);
                isOk = fsql != null;
            }
            return (isOk, fsql ?? default);
        }
        #endregion
    }
}
