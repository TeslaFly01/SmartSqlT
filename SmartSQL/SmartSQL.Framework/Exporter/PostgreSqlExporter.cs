using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSQL.Framework.PhysicalDataModel;
using SqlSugar;

namespace SmartSQL.Framework.Exporter
{
    public class PostgreSqlExporter : Exporter, IExporter
    {
        public PostgreSqlExporter(string connectionString) : base(connectionString)
        {

        }

        public PostgreSqlExporter(string tableName, List<Column> columns) : base(tableName, columns)
        {

        }

        public override Model Init()
        {
            var model = new Model { Database = "PostgreSql" };
            try
            {
                model.Tables = new Tables();// this.GetTables();
                model.Views = new Views();// this.GetViews();
                model.Procedures = new Procedures();// this.GetProcedures();
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
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.PostgreSQL, DbConnectString);
            var dbClient = SugarFactory.GetInstance(DbType.PostgreSQL, DbConnectString);
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
