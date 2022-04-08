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
                model.Tables = this.GetTables();
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

        private Tables GetTables()
        {
            #region MyRegion
            var tables = new Tables();
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.PostgreSQL, DbConnectString);
            var tableList = dbMaintenance.GetTableInfoList(false);
            tableList.ForEach(tb =>
            {
                if (tables.ContainsKey(tb.Name))
                {
                    return;
                }
                var table = new Table
                {
                    Id = tb.Name,
                    Name = tb.Name,
                    DisplayName = tb.Name,
                    Comment = tb.Description,
                    CreateDate = tb.CreateDate,
                    ModifyDate = tb.ModifyDate
                };
                tables.Add(tb.Name, table);
            });
            return tables;
            #endregion
        }

        private Views GetViews()
        {
            #region MyRegion
            var views = new Views();
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.PostgreSQL, DbConnectString);
            var viewList = dbMaintenance.GetViewInfoList(false);
            viewList.ForEach(v =>
            {
                if (views.ContainsKey(v.Name))
                {
                    return;
                }
                var view = new View()
                {
                    Id = v.Name,
                    Name = v.Name,
                    DisplayName = v.Name,
                    Comment = v.Description,
                    CreateDate = v.CreateDate,
                    ModifyDate = v.ModifyDate
                };
                views.Add(v.Name, view);
            });
            return views;
            #endregion
        }

        private Procedures GetProcedures()
        {
            #region MyRegion
            var procDic = new Procedures();
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.PostgreSQL, DbConnectString);
            var procInfoList = dbMaintenance.GetProcInfoList(false);
            var dbName = dbMaintenance.Context.Ado.Connection.Database;
            var procList = procInfoList.Where(x => x.Schema == dbName).ToList();
            procList.ForEach(p =>
            {
                if (procDic.ContainsKey(p.Name))
                {
                    return;
                }
                var proc = new Procedure()
                {
                    Id = p.Name,
                    Name = p.Name,
                    DisplayName = p.Name,
                    Comment = p.Description,
                    CreateDate = p.CreateDate,
                    ModifyDate = p.ModifyDate
                };
                procDic.Add(p.Name, proc);
            });
            return procDic;
            #endregion
        }

        public override Columns GetColumnInfoById(string objectId)
        {
            var columns = new Columns(500);
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.PostgreSQL, DbConnectString);
            var viewList = dbMaintenance.GetColumnInfosByTableName(objectId);
            viewList.ForEach(v =>
            {
                if (columns.ContainsKey(v.DbColumnName))
                {
                    return;
                }
                var column = new Column(v.DbColumnName, v.DbColumnName, v.DbColumnName, v.DataType, v.ColumnDescription);
                column.Length = "";
                switch (v.DataType)
                {
                    case "char":
                    case "nchar":
                    case "time":
                    case "text":
                    case "binary":
                    case "varchar":
                    case "nvarchar":
                    case "varbinary":
                    case "datetime2":
                    case "datetimeoffset":
                        if (v.Length > 0)
                        {
                            column.Length = $"({v.Length})";
                        }
                        break;
                    case "numeric":
                    case "decimal":
                        column.Length = $"({v.Length},{v.Scale})"; break;
                }
                column.ObjectId = objectId.ToString();
                column.ObjectName = v.DbColumnName;
                column.IsIdentity = v.IsIdentity;
                column.IsNullable = v.IsNullable;
                column.DefaultValue = !string.IsNullOrEmpty(v.DefaultValue) && v.DefaultValue.Contains("((") ? v.DefaultValue.Replace("((", "").Replace("))", "") : v.DefaultValue;
                column.DataType = v.DataType;
                column.OriginalName = v.DbColumnName;
                column.Comment = v.ColumnDescription;
                column.IsPrimaryKey = v.IsPrimarykey;
                columns.Add(v.DbColumnName, column);
            });
            return columns;
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
