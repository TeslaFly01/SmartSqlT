using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SmartCode.Framework.SqliteModel;
using SQLite;

namespace SmartCode.Framework
{
    public class SQLiteHelper
    {
        public static string BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,Environment.SpecialFolderOption.Create),
            "SmartSQL");
        public string connstr = Path.Combine(BasePath, "SmartSQL.db");//没有数据库会创建数据库
        public SQLiteConnection db;
        public SQLiteHelper()
        {
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }
            db = new SQLiteConnection(connstr);
            //表已存在不会重复创建
            db.CreateTable<ConnectConfigs>();
            db.CreateTable<ObjectGroup>();
            db.CreateTable<SObjects>();
            db.CreateTable<SystemSet>();
            Init();
        }

        private void Init()
        {
            var sysList = db.Table<SystemSet>().ToList();
            var initValue = new List<string> { "IsGroup", "IsMultipleTab" };
            initValue.ForEach(x =>
            {
                if (!sysList.Any(m => m.Name.Equals(x)))
                {
                    db.Insert(new SystemSet
                    {
                        Name = x,
                        Value = "false"
                    });
                }
            });
        }

        public int Add<T>(T model)
        {
            return db.Insert(model);
        }

        public int Update<T>(T model)
        {
            return db.Update(model);
        }

        public int Delete<T>(T model)
        {
            return db.Update(model);
        }
        public List<T> Query<T>(string sql) where T : new()
        {
            return db.Query<T>(sql);
        }
        public int Execute(string sql)
        {
            return db.Execute(sql);
        }

        public bool GetSys(string name)
        {
            var defaultV = false;
            var sqLiteHelper = new SQLiteHelper();
            var sysSet = sqLiteHelper.db.Table<SystemSet>().FirstOrDefault(x => x.Name.Equals(name));
            if (sysSet != null)
            {
                defaultV = Convert.ToBoolean(sysSet.Value);
            }
            return defaultV;
        }
    }
}
