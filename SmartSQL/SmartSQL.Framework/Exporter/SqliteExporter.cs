using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeRedis;
using FreeSql.Internal.Model;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.Util;
using SqlSugar;
using DbType = SqlSugar.DbType;

namespace SmartSQL.Framework.Exporter
{
    /// <summary>
    /// Sqlite数据库
    /// </summary>
    public class SqliteExporter : Exporter, IExporter
    {
        private readonly SqlSugarClient _dbClient;
        private readonly IFreeSql _FreeSql;
        public SqliteExporter(string connectionString) : base(connectionString)
        {
            _dbClient = SugarFactory.GetInstance(DbType.Sqlite, DbConnectString);
            _FreeSql= FreeSqlHelper.GetInstance().FreeBuilder(FreeSql.DataType.Sqlite, connectionString);
        }

        public SqliteExporter(string connectionString, string dbName) : base(connectionString, dbName)
        {
            _dbClient = SugarFactory.GetInstance(DbType.Sqlite, DbConnectString);
            _FreeSql= FreeSqlHelper.GetInstance().FreeBuilder(FreeSql.DataType.Sqlite, connectionString);
        }

        public SqliteExporter(Table table, List<Column> columns) : base(table, columns)
        {
        }

        public override Model Init()
        {
            var model = new Model { Database = "Sqlite" };
            try
            {
                model.Tables = this.GetTables();
                model.Views = this.GetViews();
                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override List<DataBase> GetDatabases(string defaultDatabase = "")
        {
            #region MyRegion
            return new List<DataBase>
            {
                new DataBase
                {
                    DbName = "default",
                    IsSelected = true
                }
            };
            #endregion
        }

        public override Columns GetColumnInfoById(string objectId)
        {
            #region MyRegion
            var columns = new Columns(500);
            var viewList = _dbClient.DbMaintenance.GetColumnInfosByTableName(objectId, false);
            viewList.ForEach(v =>
            {
                if (columns.ContainsKey(v.DbColumnName))
                {
                    return;
                }
                var column = new Column(v.DbColumnName, v.DbColumnName, v.DbColumnName, v.DataType, v.ColumnDescription);
                column.LengthName = "";
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
                        column.LengthName = v.Length == 2147483647 ? "" : $"({v.Length})"; break;
                    case "numeric":
                    case "decimal":
                        column.LengthName = $"({v.Length},{v.Scale})"; break;
                }

                column.ObjectId = objectId.ToString();
                column.ObjectName = v.DbColumnName;
                column.IsIdentity = v.IsIdentity;
                column.IsNullable = v.IsNullable;
                if (!string.IsNullOrEmpty(v.DefaultValue))
                {
                    column.DefaultValue = v.DefaultValue.Replace("(", "").Replace(")", "");
                }
                column.DataType = v.DataType.ToUpper();
                column.OriginalName = v.DbColumnName;
                column.Comment = v.ColumnDescription;
                column.IsPrimaryKey = v.IsPrimarykey;
                column.CSharpType = SqliteDbTypeMapHelper.MapCsharpType(v.DataType, v.IsNullable);
                columns.Add(v.DbColumnName, column);
            });
            return columns;
            #endregion
        }

        public override string GetScriptInfoById(string objectId, DbObjectType objectType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 更新表/视图/存储过程对象注释
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public override bool UpdateObjectRemark(string objectName, string remark, DbObjectType objectType = DbObjectType.Table)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 更新列注释
        /// </summary>
        /// <param name="columnInfo"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public override bool UpdateColumnRemark(Column columnInfo, string remark, DbObjectType objectType = DbObjectType.Table)
        {
            throw new NotImplementedException();
        }

        public override string CreateTableSql()
        {
            return "";
        }

        public override string SelectSql()
        {
            return "";
        }

        public override string InsertSql()
        {
            return "";
        }

        public override string UpdateSql()
        {
            return "";
        }

        public override string DeleteSql()
        {
            return "";
        }

        public override string AddColumnSql()
        {
            return "";
        }

        public override string AlterColumnSql()
        {
            return "";
        }

        public override string DropColumnSql()
        {
            return "";
        }

        private Tables GetTables()
        {
            #region MyRegion
            var tables = new Tables();
            var tableList = _dbClient.DbMaintenance.GetTableInfoList(false);
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
            var viewList = _dbClient.DbMaintenance.GetViewInfoList(false);
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

        public override (DataTable, int) GetDataTable(string sql, string orderBySql, int pageIndex, int pageSize)
        {
            #region MyRegion
            var pageInfo = new BasePagingInfo
            {
                PageNumber = pageIndex,
                PageSize = pageSize
            };
            var result = _FreeSql.Select<object>().WithSql(sql).Page(pageInfo).OrderBy(orderBySql).ToDataTable();
            return (result, Convert.ToInt32(pageInfo.Count));
            #endregion
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public override int ExecuteSQL(string sql)
        {
            return _dbClient.Ado.ExecuteCommand(sql);
        }

        public override RedisClient.DatabaseHook GetDB()
        {
            throw new NotImplementedException();
        }

        public override RedisServerInfo GetInfo()
        {
            throw new NotImplementedException();
        }
    }
}
