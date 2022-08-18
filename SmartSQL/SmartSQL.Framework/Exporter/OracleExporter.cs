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
        private readonly IDbMaintenance _dbMaintenance;
        public OracleExporter(string connectionString) : base(connectionString)
        {
            _dbMaintenance = SugarFactory.GetDbMaintenance(DbType.Oracle, DbConnectString);
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
            var model = new Model { Database = "Oracle" };
            try
            {
                model.Tables = this.GetTables();
                model.Views = this.GetViews();
                model.Procedures = new Procedures();//暂时不支持存储过程 this.GetProcedures();
                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Private
        private Tables GetTables()
        {
            #region MyRegion
            var tables = new Tables();
            var tableList = _dbMaintenance.GetTableInfoList(false);
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
            var viewList = _dbMaintenance.GetViewInfoList(false);
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
            var procInfoList = _dbMaintenance.GetProcInfoList(false);
            var dbName = _dbMaintenance.Context.Ado.Connection.Database;
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
        #endregion

        public override List<DataBase> GetDatabases(string defaultDatabase = "")
        {
            #region MyRegion
           _dbMaintenance.GetTableInfoList(false);
            return new List<DataBase>
            {
                new DataBase
                {
                    DbName = defaultDatabase,
                    IsSelected = true
                }
            };
            #endregion
        }

        /// <summary>
        /// 根据对象ID获取列信息
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public override Columns GetColumnInfoById(string objectId)
        {
            #region MyRegion
            var columns = new Columns(500);
            var viewList = _dbMaintenance.GetColumnInfosByTableName(objectId);
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
                        column.LengthName = $"({v.Length})"; break;
                    case "numeric":
                    case "decimal":
                        column.LengthName = $"({v.Length},{v.Scale})"; break;
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
            #endregion
        }

        public override string GetScriptInfoById(string objectId, DbObjectType objectType)
        {
            #region MyRegion
            var scriptInfo = _dbMaintenance.GetScriptInfo(objectId, objectType);
            return scriptInfo.Definition;
            #endregion
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
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
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
    }
}
