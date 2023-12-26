using FreeRedis;
using Newtonsoft.Json;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.Util;
using SqlSugar;
using SqlSugar.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SmartSQL.Framework.Exporter
{
    public class RedisExporter : Exporter, IExporter
    {
        private readonly string DbPrex = "DB";
        public static RedisClient cli;
        public RedisExporter(string connectionString) : base(connectionString)
        {
            var _cliLazy = new Lazy<RedisClient>(() =>
            {
                var r = new RedisClient(connectionString); //redis 6.0
                r.Serialize = obj => JsonConvert.SerializeObject(obj);
                r.Deserialize = (json, type) => JsonConvert.DeserializeObject(json, type);
                return r;
            });
            cli= _cliLazy.Value;
        }

        public RedisExporter(string connectionString, string dbName) : base(connectionString, dbName)
        {
            var _cliLazy = new Lazy<RedisClient>(() =>
            {
                var r = new RedisClient(connectionString); //redis 6.0
                r.Serialize = obj => JsonConvert.SerializeObject(obj);
                r.Deserialize = (json, type) => JsonConvert.DeserializeObject(json, type);
                return r;
            });
            cli= _cliLazy.Value;
        }

        public RedisExporter(Table table, List<Column> columns) : base(table, columns)
        {

        }

        #region MyRegion

        public override Model Init()
        {
            var model = new Model();
            var dbIndex = Convert.ToInt32(DbName.Replace(DbPrex, ""));

            //var ff = cli.Info();
            //var gg = RedisServerInfoHelper.ParseServerInfo(ff);
            using (var db = cli.GetDatabase(dbIndex))
            {
                var tables = new Tables(10);
                var keys = db.Keys("*");
                foreach (var key in keys)
                {
                    var table = new Table
                    {
                        Id = key,
                        Name = key,
                        DisplayName = key,
                    };
                    if (!tables.ContainsKey(key))
                    {
                        tables.Add(key, table);
                    }
                }
                model.Tables= tables;
            }
            return model;
        }

        /// <summary>
        /// 获取所有数据库
        /// </summary>
        /// <param name="defaultDatabase"></param>
        /// <returns></returns>
        public override List<DataBase> GetDatabases(string defaultDatabase = "")
        {
            var dbList = new List<DataBase>();
            for (int i = 0; i < 16; i++)
            {
                dbList.Add(new DataBase
                {
                    DbName = $"{DbPrex}{i}",
                    IsSelected = false
                });
            }
            return dbList;
        }

        public override RedisServerInfo GetInfo()
        {
            var dbIndex = Convert.ToInt32(DbName.Replace(DbPrex, ""));
            using (var db = cli.GetDatabase(dbIndex))
            {
                var infoString = db.Info();
                return RedisServerInfoHelper.ParseServerInfo(infoString);
            }
        }

        public override RedisClient.DatabaseHook GetDB()
        {
            var dbIndex = Convert.ToInt32(DbName.Replace(DbPrex, ""));
            using (var db = cli.GetDatabase(dbIndex))
            {
                return db;
            }
        }

        public override string AddColumnSql()
        {
            throw new NotImplementedException();
        }

        public override string AlterColumnSql()
        {
            throw new NotImplementedException();
        }

        public override string CreateTableSql()
        {
            throw new NotImplementedException();
        }

        public override string DeleteSql()
        {
            throw new NotImplementedException();
        }

        public override string DropColumnSql()
        {
            throw new NotImplementedException();
        }

        public override int ExecuteSQL(string sql)
        {
            throw new NotImplementedException();
        }

        public override Columns GetColumnInfoById(string objectId)
        {
            throw new NotImplementedException();
        }

        public override (System.Data.DataTable, int) GetDataTable(string sql, string orderBySql, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public override string GetScriptInfoById(string objectId, DbObjectType objectType)
        {
            throw new NotImplementedException();
        }

        public override string InsertSql()
        {
            throw new NotImplementedException();
        }

        public override string SelectSql()
        {
            throw new NotImplementedException();
        }

        public override bool UpdateColumnRemark(Column columnInfo, string remark, DbObjectType objectType = DbObjectType.Table)
        {
            throw new NotImplementedException();
        }

        public override bool UpdateObjectRemark(string objectName, string remark, DbObjectType objectType = DbObjectType.Table)
        {
            throw new NotImplementedException();
        }

        public override string UpdateSql()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
