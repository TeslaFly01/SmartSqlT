using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSQL.Framework.PhysicalDataModel;
using SqlSugar;

namespace SmartSQL.Framework.Exporter
{
    public class OracleExporter : Exporter, IExporter
    {
        public OracleExporter(string connectionString) : base(connectionString)
        {

        }
        public OracleExporter(string tableName, List<Column> columns) : base(tableName, columns)
        {

        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override Model Init()
        {
            var model = new Model { Database = "MySql" };
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
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.Oracle, DbConnectString);
            var dbClient = SugarFactory.GetInstance(DbType.Oracle, DbConnectString);
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

        /// <summary>
        /// 根据对象ID获取列信息
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public override Columns GetColumnInfoById(string objectId)
        {
            var columns = new Columns(500);
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.Oracle, DbConnectString);
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
                        column.Length = $"({v.Length})"; break;
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
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.Oracle, DbConnectString);
            var scriptInfo = dbMaintenance.GetScriptInfo(objectId, objectType);
            return scriptInfo.Definition;
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
    }
}
