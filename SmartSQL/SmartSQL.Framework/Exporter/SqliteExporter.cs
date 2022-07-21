using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSQL.Framework.PhysicalDataModel;
using SqlSugar;
using DbType = SqlSugar.DbType;

namespace SmartSQL.Framework.Exporter
{
    using PhysicalDataModel;
    using Util;
    /// <summary>
    /// Sqlite数据库
    /// </summary>
    public class SqliteExporter : Exporter, IExporter
    {
        public SqliteExporter(string connectionString) : base(connectionString)
        {
        }

        public SqliteExporter(string tableName, List<Column> columns) : base(tableName, columns)
        {
        }

        public override Model Init()
        {
            var model = new Model { Database = "Sqlite" };
            try
            {
                //model.Tables = this.GetTables();
                //model.Views = this.GetViews();
                //model.Procedures = this.GetProcedures();
                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override List<DataBase> GetDatabases()
        {
            #region MyRegion
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.Sqlite, DbConnectString);
            var dbClient = SugarFactory.GetInstance(DbType.Sqlite, DbConnectString);
            var dataBaseList = dbMaintenance.GetDataBaseList(dbClient);
            var list = new List<DataBase>();
            dataBaseList.ForEach(dbName =>
            {
                var dBase = new DataBase
                {
                    DbName = dbName,
                    IsSelected = false
                };
                list.Add(dBase);
            });
            return list;
            #endregion
        }

        public override Columns GetColumnInfoById(string objectId)
        {
            throw new NotImplementedException();
        }

        public override string GetScriptInfoById(string objectId, DbObjectType objectType)
        {
            throw new NotImplementedException();
        }

        public override string CreateTableSql()
        {
            throw new NotImplementedException();
        }

        public override string SelectSql()
        {
            throw new NotImplementedException();
        }

        public override string InsertSql()
        {
            throw new NotImplementedException();
        }

        public override string UpdateSql()
        {
            throw new NotImplementedException();
        }

        public override string DeleteSql()
        {
            throw new NotImplementedException();
        }

        public override string AddColumnSql()
        {
            throw new NotImplementedException();
        }

        public override string AlterColumnSql()
        {
            throw new NotImplementedException();
        }

        public override string DropColumnSql()
        {
            throw new NotImplementedException();
        }
    }
}
