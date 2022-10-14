using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.Serialization.Json;
using SmartSQL.Framework.Const;
using SmartSQL.Framework.SqliteModel;
using SQLite;

namespace SmartSQL.Framework
{
    public class SQLiteHelper
    {
        public static string BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create),
            "SmartSQL");
        public string connstr = Path.Combine(BasePath, "SmartSQL.db");//没有数据库会创建数据库
        public SQLiteConnection db;

        private static SQLiteHelper _instance;

        private static readonly object obj = new object();
        public static SQLiteHelper GetInstance()
        {
            if (_instance == null)
            {
                lock (obj)  //加锁防止多线程
                {
                    if (_instance == null)
                    {
                        _instance = new SQLiteHelper();
                    }
                }
            }
            return _instance;
        }

        public SQLiteHelper()
        {
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }
            db = new SQLiteConnection(connstr);
            //表已存在不会重复创建
            db.CreateTable<ConnectConfigs>();
            db.CreateTable<GroupInfo>();
            db.CreateTable<TagInfo>();
            db.CreateTable<GroupObjects>();
            db.CreateTable<TagObjects>();
            db.CreateTable<SystemSet>();
            Init();
        }

        public List<SystemSet> InitValue = new List<SystemSet>
        {
            new SystemSet{Name = SysConst.Sys_IsGroup,Type = 1,Value = "false"},
            new SystemSet{Name = SysConst.Sys_IsMultipleTab,Type = 1,Value = "false"},
            new SystemSet{Name = SysConst.Sys_IsLikeSearch,Type = 1,Value = "false"},
            new SystemSet{Name = SysConst.Sys_LeftMenuType,Type = 2,Value = "1"},
            new SystemSet{Name = SysConst.Sys_SelectedConnection,Type = 3,Value = ""},
            new SystemSet{Name = SysConst.Sys_SelectedDataBase,Type = 3,Value = ""},
        };
        private void Init()
        {
            var sysList = db.Table<SystemSet>().ToList();
            InitValue.ForEach(x =>
            {
                if (!sysList.Any(m => m.Name.Equals(x.Name)))
                {
                    db.Insert(x);
                }
            });
        }

        public int Add<T>(T model)
        {
            return db.Insert(model);
        }

        public int Add<T>(List<T> model)
        {
            return db.InsertAll(model);
        }

        public int Update<T>(T model)
        {
            return db.Update(model);
        }

        public int Delete<T>(T model)
        {
            return db.Update(model);
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predExpr) where T : new()
        {
            return db.Table<T>().FirstOrDefault(predExpr);
        }

        public List<T> ToList<T>() where T : new()
        {
            return db.Table<T>().ToList();
        }

        public List<T> ToList<T>(Expression<Func<T, bool>> predExpr) where T : new()
        {
            return db.Table<T>().Where(predExpr).ToList();
        }

        public List<T> Query<T>(string sql) where T : new()
        {
            return db.Query<T>(sql);
        }

        public bool IsAny<T>(Expression<Func<T, bool>> predExpr) where T : new()
        {
            return db.Table<T>().Count(predExpr) > 0;
        }

        public int Execute(string sql)
        {
            return db.Execute(sql);
        }

        public void SetSysValue(string name, object value)
        {
            var sqLiteHelper = new SQLiteHelper();
            var sysSet = sqLiteHelper.db.Table<SystemSet>().First(x => x.Name == name);
            if (sysSet == null)
            {
                return;
            }
            sysSet.Value = value.ToString();
            sqLiteHelper.Update(sysSet);
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
