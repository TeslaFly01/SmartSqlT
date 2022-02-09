using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace SmartCode.Framework
{
    public class SugarFactory
    {
        //创建SqlSugarClient 
        public static SqlSugarClient GetInstance()
        {
            //创建数据库对象
            SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = "server=10.136.0.114,1433;database=PSAData;uid=ipsa;pwd=ipsa@20200705;",//连接符字串  "server= IP ;uid= 账号 ;pwd= 密码 ;database= 数据库名 ";
                DbType = DbType.SqlServer,
                IsAutoCloseConnection = true
            });

            //添加Sql打印事件，开发中可以删掉这个代码
            db.Aop.OnLogExecuting = (sql, pars) =>
            {
                Console.WriteLine(sql);
            };
            return db;
        }
    }
}
