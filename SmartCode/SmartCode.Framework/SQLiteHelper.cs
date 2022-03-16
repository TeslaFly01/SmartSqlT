using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Json;
using SmartCode.Framework.SqliteModel;
using SQLite;

namespace SmartCode.Framework
{
    public class SQLiteHelper
    {
        public static string BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create),
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
            var initValue = new List<string> { "IsGroup", "IsMultipleTab", "LeftMenuType" };
            initValue.ForEach(x =>
            {
                if (!sysList.Any(m => m.Name.Equals(x)))
                {
                    var defaultValue = x == "LeftMenuType" ? "1" : "false";
                    var defaultType = x == "LeftMenuType" ? 2 : 1;
                    db.Insert(new SystemSet
                    {
                        Name = x,
                        Type = defaultType,
                        Value = defaultValue
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

        public bool GetSysBool(string name)
        {
            var sqLiteHelper = new SQLiteHelper();
            var type = SysDataType.BOOL.GetHashCode();
            var sysSet = sqLiteHelper.db.Table<SystemSet>().FirstOrDefault(x => x.Name.Equals(name) && x.Type == type);
            if (sysSet == null)
            {
                return false;
            }
            return Convert.ToBoolean(sysSet.Value);
        }

        public int GetSysInt(string name)
        {
            var sqLiteHelper = new SQLiteHelper();
            var type = SysDataType.INT.GetHashCode();
            var sysSet = sqLiteHelper.db.Table<SystemSet>().FirstOrDefault(x => x.Name.Equals(name) && x.Type == type);
            if (sysSet == null)
            {
                return 0;
            }
            return Convert.ToInt32(sysSet.Value);
        }

        public string GetSysString(string name)
        {
            var sqLiteHelper = new SQLiteHelper();
            var type = SysDataType.STRING.GetHashCode();
            var sysSet = sqLiteHelper.db.Table<SystemSet>().FirstOrDefault(x => x.Name.Equals(name) && x.Type == type);
            if (sysSet == null)
            {
                return "";
            }
            return sysSet.Value;
        }

        //public T GetSysJson<T>(string name)
        //{
        //    var sqLiteHelper = new SQLiteHelper();
        //    var sysSet = sqLiteHelper.db.Table<SystemSet>().FirstOrDefault(x => x.Name.Equals(name) && x.Type == SysDataType.JSON.GetHashCode());
        //    if (sysSet == null)
        //    {
        //        return default(T);
        //    }
        //    return sysSet.Value;
        //}
    }

    /// <summary>
    /// 系统设置数据类型
    /// </summary>
    public enum SysDataType
    {
        BOOL = 1,
        INT = 2,
        STRING = 3,
        JSON = 4
    }
}
