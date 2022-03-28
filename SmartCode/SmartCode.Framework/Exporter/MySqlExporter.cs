using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartCode.Framework.PhysicalDataModel;
using SqlSugar;
using DbType = SqlSugar.DbType;

namespace SmartCode.Framework.Exporter
{
    public class MySqlExporter : Exporter, IExporter
    {
        public MySqlExporter(string connectionString) : base(connectionString)
        {

        }
        public MySqlExporter(string tableName, List<Column> columns) : base(tableName, columns)
        {

        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        /// <returns></returns>
        public override Model Init()
        {
            var model = new Model { Database = "MySql" };
            try
            {
                model.Tables = this.GetTables();
                model.Views = this.GetViews();
                model.Procedures = this.GetProcedures();
                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取数据库列表
        /// </summary>
        /// <returns></returns>
        public override List<DataBase> GetDatabases()
        {
            #region MyRegion
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.MySql, DbConnectString);
            var dbClient = SugarFactory.GetInstance(DbType.MySql, DbConnectString);
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

        #region private
        private Tables GetTables()
        {
            #region MyRegion
            var tables = new Tables();
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.MySql, DbConnectString);
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
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.MySql, DbConnectString);
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
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.MySql, DbConnectString);
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
        #endregion

        /// <summary>
        /// 根据对象ID获取列信息
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public override Columns GetColumnInfoById(string objectId)
        {
            var columns = new Columns(500);
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.MySql, DbConnectString);
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

        /// <summary>
        /// 根据对象ID获取脚本信息
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override string GetScriptInfoById(string objectId, DbObjectType objectType)
        {
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.MySql, DbConnectString);
            var scriptInfo = dbMaintenance.GetScriptInfo(objectId, objectType);
            return scriptInfo.Definition;
        }

        public override string CreateTableSql()
        {
            if (string.IsNullOrEmpty(TableName) || !Columns.Any())
            {
                return "";
            }
            var sb = new StringBuilder();
            sb.Append($"CREATE TABLE `{TableName}` (");
            sb.Append(Environment.NewLine);
            Columns.ForEach(col =>
            {
                sb.Append($" `{col.DisplayName}` {col.DataType}{col.Length} ");
                if (col.IsIdentity)
                {
                    sb.Append("IDENTITY(1,1) ");
                }
                var isNull = col.IsNullable ? "NULL," : "NOT NULL,";
                sb.Append(isNull);
                sb.Append(Environment.NewLine);
            });
            var primaryKeyList = Columns.FindAll(x => x.IsPrimaryKey);
            if (primaryKeyList.Any())
            {
                sb.Append($"\tPRIMARY KEY (");
                var sbPriKey = new StringBuilder();
                foreach (var column in primaryKeyList)
                {
                    sbPriKey.Append($"`{column.DisplayName}`,");
                }
                sb.Append(sbPriKey.ToString().TrimEnd(','));
                sb.Append(")");
                sb.Append(Environment.NewLine);
            }
            sb.Append(")");
            return sb.ToString();
        }

        /// <summary>
        /// 查询数据sql脚本
        /// </summary>
        /// <returns></returns>
        public override string SelectSql()
        {
            var strSql = new StringBuilder("SELECT ");
            var tempCol = new StringBuilder();
            Columns.ForEach(col =>
            {
                tempCol.Append($"{col.Name},");
            });
            var tempSql = tempCol.ToString().TrimEnd(',');
            strSql.Append($"{tempSql} FROM {TableName}");
            return strSql.ToString();
        }

        /// <summary>
        /// 插入数据sql脚本
        /// </summary>
        /// <returns></returns>
        public override string InsertSql()
        {
            var tempCols = Columns.Where(x => x.IsIdentity == false).ToList();
            var strSql = new StringBuilder($"INSERT INTO {TableName} (");
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
        }

        /// <summary>
        /// 更新数据sql脚本
        /// </summary>
        /// <returns></returns>
        public override string UpdateSql()
        {
            var tempCols = Columns.Where(x => x.IsIdentity == false).ToList();
            var strSql = new StringBuilder($"UPDATE {TableName} SET ");
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
        }

        /// <summary>
        /// 删除数据sql脚本
        /// </summary>
        /// <returns></returns>
        public override string DeleteSql()
        {
            var strSql = new StringBuilder($"DELETE FROM {TableName} WHERE ");
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
        }

        /// <summary>
        /// 添加列sql脚本
        /// </summary>
        /// <returns></returns>
        public override string AddColumnSql()
        {
            return "";
        }

        /// <summary>
        /// 修改列sql脚本
        /// </summary>
        /// <returns></returns>
        public override string AlterColumnSql()
        {
            return "";
        }

        /// <summary>
        /// 删除列sql脚本
        /// </summary>
        /// <returns></returns>
        public override string DropColumnSql()
        {
            return "";
        }
    }
}
