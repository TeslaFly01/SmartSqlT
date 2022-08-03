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
                model.Tables = this.GetTables();
                model.Views = this.GetViews();
                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override List<DataBase> GetDatabases()
        {
            return new List<DataBase>
            {
                new DataBase
                {
                    DbName = "default",
                    IsSelected = true
                }
            };
        }

        public override Columns GetColumnInfoById(string objectId)
        {
            var columns = new Columns(500);
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.Sqlite, DbConnectString);
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
                        column.Length = v.Length == 2147483647 ? "" : $"({v.Length})"; break;
                    case "numeric":
                    case "decimal":
                        column.Length = $"({v.Length},{v.Scale})"; break;
                }

                 column.ObjectId = objectId.ToString();
                column.ObjectName = v.DbColumnName;
                column.IsIdentity = v.IsIdentity;
                column.IsNullable = v.IsNullable;
                column.DefaultValue = !string.IsNullOrEmpty(v.DefaultValue) && v.DefaultValue.Contains("((") ? v.DefaultValue.Replace("((", "").Replace("))", "") : v.DefaultValue;
                column.DataType = v.DataType.ToUpper();
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

        /// <summary>
        /// 更新列注释
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public override bool UpdateColumnRemark(string tableName, string columnName, string remark)
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

        private Tables GetTables()
        {
            #region MyRegion
            var tables = new Tables();
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.Sqlite, DbConnectString);
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
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.Sqlite, DbConnectString);
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
    }
}
