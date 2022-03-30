using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace SmartSQL.Framework
{
    public class SugarFactory
    {
        //创建SqlSugarClient 
        public static SqlSugarClient GetInstance(DbType dbType, string connectionString)
        {
            //创建数据库对象
            SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
            {
                //连接符字串  "server= IP ;uid= 账号 ;pwd= 密码 ;database= 数据库名 ";
                ConnectionString = connectionString,
                DbType = dbType,
                IsAutoCloseConnection = true,
                MoreSettings = new ConnMoreSettings
                {
                    IsWithNoLockQuery = true
                }
            });
            //添加Sql打印事件，开发中可以删掉这个代码
            db.Aop.OnLogExecuting = (sql, pars) =>
            {
                Console.WriteLine(sql);
            };
            return db;
        }

        /// <summary>
        /// 创建库表管理实例
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static IDbMaintenance GetDbMaintenance(DbType dbType, string connectionString)
        {
            var instance = GetInstance(dbType, connectionString);
            return instance.DbMaintenance;
        }
    }
}
