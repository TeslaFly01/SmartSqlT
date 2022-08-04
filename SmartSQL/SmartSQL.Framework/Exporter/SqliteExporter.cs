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
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.Sqlite, DbConnectString);
            var viewList = dbMaintenance.GetColumnInfosByTableName(objectId);
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
                column.DefaultValue = !string.IsNullOrEmpty(v.DefaultValue) && v.DefaultValue.Contains("((") ? v.DefaultValue.Replace("((", "").Replace("))", "") : v.DefaultValue;
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
        public override bool UpdateObjectRemark(string objectName, string remark)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 更新列注释
        /// </summary>
        /// <param name="columnInfo"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public override bool UpdateColumnRemark(Column columnInfo, string remark)
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
