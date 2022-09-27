using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.Util;
using SqlSugar;

namespace SmartSQL.Framework.Exporter
{
    public class PostgreSqlExporter : Exporter, IExporter
    {
        private readonly IDbMaintenance _dbMaintenance;
        public PostgreSqlExporter(string connectionString) : base(connectionString)
        {
            _dbMaintenance = SugarFactory.GetDbMaintenance(DbType.PostgreSQL, DbConnectString);
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
                model.Views = this.GetViews();
                model.Procedures = new Procedures();// this.GetProcedures();
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
            var dbClient = SugarFactory.GetInstance(DbType.PostgreSQL, DbConnectString);
            var dataBaseList = _dbMaintenance.GetDataBaseList(dbClient);
            var list = new List<DataBase>();
            dataBaseList.ForEach(dbName =>
            {
                if (dbName.Equals("template0") || dbName.Equals("template1"))
                {
                    return;
                }
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
            try
            {
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
            }
            catch (Exception ex)
            {

            }
            return views;
            #endregion
        }

        private Procedures GetProcedures()
        {
            #region MyRegion
            var procDic = new Procedures();
            try
            {
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
            }
            catch (Exception ex)
            {
                
            }
            return procDic;
            #endregion
        }
        #endregion

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
                        if (v.Length > 0)
                        {
                            column.LengthName = $"({v.Length})";
                        }
                        break;
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
                column.CSharpType = PostgreSqlDbTypeMapHelper.MapCsharpType(v.DataType, v.IsNullable);
                columns.Add(v.DbColumnName, column);
            });
            return columns;
            #endregion
        }

        public override string GetScriptInfoById(string objectId, DbObjectType objectType)
        {
            return "";
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
            #region MyRegion
            var strSql = new StringBuilder("SELECT ");
            var tempCol = new StringBuilder();
            Columns.ForEach(col =>
            {
                tempCol.Append($"\"{col.Name}\",");
            });
            var tempSql = tempCol.ToString().TrimEnd(',');
            strSql.Append($"{tempSql} FROM \"{TableName}\"");
            return strSql.ToString();
            #endregion
        }

        public override string InsertSql()
        {
            #region MyRegion
            var tempCols = Columns.Where(x => x.IsIdentity == false).ToList();
            var strSql = new StringBuilder($"INSERT INTO \"{TableName}\" (");
            var tempCol = new StringBuilder();
            tempCols.ForEach(col =>
            {
                tempCol.Append($"\"{col.Name}\",");
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
            var strSql = new StringBuilder($"UPDATE \"{TableName}\" SET ");
            var tempCol = new StringBuilder();
            tempCols.ForEach(col =>
            {
                tempCol.Append($"\"{col.Name}\"=");
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
                    tempCol.Append($"\"{col.Name}\"=");
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
            var strSql = new StringBuilder($"DELETE FROM \"{TableName}\" WHERE ");
            var tempCol = new StringBuilder();
            var j = 0;
            Columns.ForEach(col =>
            {
                if (j == 0)
                {
                    tempCol.Append($"\"{col.Name}\"=");
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
