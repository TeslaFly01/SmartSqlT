using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.Internal.Model;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.Util;
using SqlSugar;
using DbType = SqlSugar.DbType;

namespace SmartSQL.Framework.Exporter
{
    public class OracleExporter : Exporter, IExporter
    {
        private readonly SqlSugarClient _dbClient;
        private readonly IFreeSql _FreeSql;
        public OracleExporter(string connectionString) : base(connectionString)
        {
            _dbClient = SugarFactory.GetInstance(DbType.Oracle, DbConnectString);
            _FreeSql= FreeSqlHelper.GetInstance().FreeBuilder(FreeSql.DataType.Oracle, connectionString);
        }

        public OracleExporter(string connectionString, string dbName) : base(connectionString, dbName)
        {
            _dbClient = SugarFactory.GetInstance(DbType.Oracle, DbConnectString);
            _FreeSql= FreeSqlHelper.GetInstance().FreeBuilder(FreeSql.DataType.Oracle, connectionString);
        }

        public OracleExporter(Table table, List<Column> columns) : base(table, columns)
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

        private Procedures GetProcedures()
        {
            #region MyRegion
            var procDic = new Procedures();
            var procInfoList = _dbClient.DbMaintenance.GetProcInfoList(false);
            var dbName = _dbClient.DbMaintenance.Context.Ado.Connection.Database;
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
            var dbClient = SugarFactory.GetInstance(DbType.Oracle, DbConnectString);
            var dataBaseList = dbClient.Ado.SqlQuery<string>("SELECT USERNAME FROM ALL_USERS ORDER BY USERNAME");
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
            var viewList = _dbClient.DbMaintenance.GetColumnInfosByTableName(objectId, false);
            viewList.ForEach(v =>
            {
                if (columns.ContainsKey(v.DbColumnName))
                {
                    return;
                }
                var column = new Column(v.DbColumnName, v.DbColumnName, v.DbColumnName, v.DataType, v.ColumnDescription);
                column.LengthName = "";
                var dataType = v.DataType.ToLower();
                switch (dataType)
                {
                    case "char":
                    case "nchar":
                    case "time":
                    case "text":
                    case "string":
                    case "binary":
                    case "varchar":
                    case "varchar2":
                    case "nvarchar":
                    case "nvarchar2":
                    case "varbinary":
                    case "datetime2":
                    case "datetimeoffset":
                        column.LengthName = $"({v.Length})"; break;
                    case "number":
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
                column.DataType = v.OracleDataType;
                column.OriginalName = v.DbColumnName;
                column.Comment = v.ColumnDescription;
                column.IsPrimaryKey = v.IsPrimarykey;
                column.CSharpType = OracleDbTypeMapHelper.MapCsharpType(v.OracleDataType, v.IsNullable);
                columns.Add(v.DbColumnName, column);
            });
            return columns;
            #endregion
        }

        public override string GetScriptInfoById(string objectId, DbObjectType objectType)
        {
            #region MyRegion
            var scriptInfo = _dbClient.DbMaintenance.GetScriptInfo(objectId, objectType);
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
        public override bool UpdateColumnRemark(Column columnInfo, string remark, DbObjectType objectType = DbObjectType.Table)
        {
            throw new NotImplementedException();
        }

        public override (DataTable, int) GetDataTable(string sql,string orderBySql, int pageIndex, int pageSize)
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
        public override string CreateTableSql()
        {
            return "";
        }

        public override string SelectSql()
        {
            #region MyRegion
            var strSql = new StringBuilder("SELECT ");
            var tempCol = new StringBuilder();
            Columns.ForEach(col =>
            {
                tempCol.Append($"{col.Name},");
            });
            var tempSql = tempCol.ToString().TrimEnd(',');
            strSql.Append($"{tempSql} FROM {Table.DisplayName}");
            return strSql.ToString();
            #endregion
        }

        public override string InsertSql()
        {
            #region MyRegion
            var tempCols = Columns.Where(x => x.IsIdentity == false).ToList();
            var strSql = new StringBuilder($"INSERT INTO {Table.DisplayName} (");
            var tempCol = new StringBuilder();
            tempCols.ForEach(col =>
            {
                tempCol.Append($"{col.Name},");
            });
            strSql.Append($"{tempCol.ToString().TrimEnd(',')}) VALUES (");
            tempCol.Clear();
            tempCols.ForEach(col =>
            {
                if (col.DataType != "int" || col.DataType != "bigint")
                {
                    tempCol.Append("''");
                }
                tempCol.Append($",");
            });
            strSql.Append($"{tempCol.ToString().TrimEnd(',')})");
            return strSql.ToString();
            #endregion
        }

        public override string UpdateSql()
        {
            #region MyRegion
            var tempCols = Columns.Where(x => x.IsIdentity == false).ToList();
            var strSql = new StringBuilder($"UPDATE {Table.DisplayName} SET ");
            var tempCol = new StringBuilder();
            tempCols.ForEach(col =>
            {
                tempCol.Append($"{col.Name}=");
                if (col.DataType == "int" || col.DataType == "bigint")
                {
                    tempCol.Append("0");
                }
                else
                {
                    tempCol.Append("''");
                }
                tempCol.Append($",");
            });
            strSql.Append($"{tempCol.ToString().TrimEnd(',')} WHERE ");
            tempCol.Clear();
            var j = 0;
            Columns.ForEach(col =>
            {
                if (j == 0)
                {
                    tempCol.Append($"{col.Name}=");
                    if (col.DataType == "int" || col.DataType == "bigint")
                    {
                        tempCol.Append("0");
                    }
                    else
                    {
                        tempCol.Append("''");
                    }
                }
                j++;
            });
            strSql.Append(tempCol);
            return strSql.ToString();
            #endregion
        }

        public override string DeleteSql()
        {
            #region MyRegion
            var strSql = new StringBuilder($"DELETE FROM {Table.DisplayName} WHERE ");
            var tempCol = new StringBuilder();
            var j = 0;
            Columns.ForEach(col =>
            {
                if (j == 0)
                {
                    tempCol.Append($"{col.Name}=");
                    if (col.DataType == "int" || col.DataType == "bigint")
                    {
                        tempCol.Append("0");
                    }
                    else
                    {
                        tempCol.Append("''");
                    }
                }
                j++;
            });
            strSql.Append(tempCol);
            return strSql.ToString();
            #endregion
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
