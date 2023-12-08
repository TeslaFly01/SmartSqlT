using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DatabaseModel;
using FreeSql.Internal.Model;
using JinianNet.JNTemplate;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.Util;
using SqlSugar;
using DbType = SqlSugar.DbType;

namespace SmartSQL.Framework.Exporter
{
    public class PostgreSqlExporter : Exporter, IExporter
    {
        private readonly SqlSugarClient _dbClient;
        private readonly IFreeSql _FreeSql;
        public PostgreSqlExporter(string connectionString) : base(connectionString)
        {
            _dbClient = SugarFactory.GetInstance(DbType.PostgreSQL, DbConnectString);
            _FreeSql= FreeSqlHelper.GetInstance().FreeBuilder(FreeSql.DataType.PostgreSQL, connectionString);
        }

        public PostgreSqlExporter(string connectionString, string dbName) : base(connectionString, dbName)
        {
            _dbClient = SugarFactory.GetInstance(DbType.PostgreSQL, DbConnectString);
            _FreeSql= FreeSqlHelper.GetInstance().FreeBuilder(FreeSql.DataType.PostgreSQL, connectionString);
        }

        public PostgreSqlExporter(Table table, List<Column> columns) : base(table, columns)
        {

        }

        public override Model Init()
        {
            var model = new Model { Database = "PostgreSql" };
            try
            {
                var list = _FreeSql.DbFirst.GetTablesByDatabase(DbName);
                model.Tables = this.GetTables(list);
                model.Views = this.GetViews(list);
                model.Procedures = this.GetProcedures(list);
                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override List<DataBase> GetDatabases(string defaultDb = "")
        {
            #region MyRegion
            var dbList = _FreeSql.DbFirst.GetDatabases();
            var list = new List<DataBase>();
            dbList.ForEach(db =>
            {
                var dBase = new DataBase
                {
                    DbName = db,
                    IsSelected = db == defaultDb
                };
                list.Add(dBase);
            });
            return list;
            //var dbClient = SugarFactory.GetInstance(DbType.PostgreSQL, DbConnectString);
            //var schemaSql = "SELECT nspname AS NspName FROM pg_catalog.pg_namespace order by nspname asc ";
            //var schemaList = dbClient.SqlQueryable<SchemaInfo>(schemaSql).ToList();
            ////var dataBaseList = _dbMaintenance.GetDataBaseList(dbClient);
            //var list = new List<DataBase>();
            //schemaList.ForEach(schema =>
            //{
            //    if (schema.Equals("pg_toast") || schema.Equals("pg_catalog") || schema.Equals("information_schema"))
            //    {
            //        return;
            //    }
            //    var dBase = new DataBase
            //    {
            //        DbName = $"{defaultDatabase}:{schema.NspName}",
            //        IsSelected = false
            //    };
            //    list.Add(dBase);
            //});
            //return list;
            #endregion
        }

        #region Private
        private Tables GetTables(List<FreeSql.DatabaseModel.DbTableInfo> tableList)
        {
            #region MyRegion
            var tables = new Tables();
            tableList.ForEach(tb =>
            {
                if (tb.Type != FreeSql.DatabaseModel.DbTableType.TABLE)
                {
                    return;
                }
                if (tables.ContainsKey(tb.Name))
                {
                    return;
                }
                var table = new Table
                {
                    Id = tb.Name,
                    Name = tb.Name,
                    DisplayName = tb.Schema + "." + tb.Name,
                    Comment = tb.Comment
                };
                tables.Add(tb.Name, table);
            });
            return tables;
            #endregion
        }

        private Views GetViews(List<FreeSql.DatabaseModel.DbTableInfo> viewList)
        {
            #region MyRegion
            var views = new Views();
            viewList.ForEach(v =>
            {
                if (v.Type != FreeSql.DatabaseModel.DbTableType.VIEW)
                {
                    return;
                }
                if (views.ContainsKey(v.Name))
                {
                    return;
                }
                var view = new View()
                {
                    Id = v.Name,
                    Name = v.Name,
                    DisplayName = v.Name,
                    Comment = v.Comment
                };
                views.Add(v.Name, view);
            });
            return views;
            #endregion
        }

        private Procedures GetProcedures(List<FreeSql.DatabaseModel.DbTableInfo> procList)
        {
            #region MyRegion
            var procDic = new Procedures();
            procList.ForEach(p =>
            {
                if (p.Type != FreeSql.DatabaseModel.DbTableType.StoreProcedure)
                {
                    return;
                }
                if (procDic.ContainsKey(p.Name))
                {
                    return;
                }
                var proc = new Procedure()
                {
                    Id = p.Name,
                    Name = p.Name,
                    DisplayName = $"{p.Schema}.{p.Name}",
                    Comment = p.Comment
                };
                procDic.Add(p.Name, proc);
            });
            return procDic;
            #endregion
        }
        #endregion

        public override Columns GetColumnInfoById(string objectId)
        {
            #region MyRegion
            var colList = _dbClient.DbMaintenance.GetColumnInfosByTableName(objectId);
            var columns = new Columns(500);
            //var schema = DbName.Contains(":") ? DbName.Split(':')[1] : DbName;
            //var sql = $@"select cast (pclass.oid as int4) as TableId,cast(ptables.tablename as varchar) as TableName,
            //                    pcolumn.column_name as DbColumnName,pcolumn.udt_name as DataType,
            //                    CASE WHEN pcolumn.numeric_scale > 0 THEN pcolumn.numeric_precision ELSE pcolumn.character_maximum_length END   as Length,
            //                    pcolumn.column_default as DefaultValue,
            //                    pcolumn.numeric_scale as DecimalDigits,
            //                    pcolumn.numeric_scale as Scale,
            //                    col_description(pclass.oid, pcolumn.ordinal_position) as ColumnDescription,
            //                    case when pkey.colname = pcolumn.column_name
            //                    then true else false end as IsPrimaryKey,
            //                    case when pcolumn.column_default like 'nextval%'
            //                    then true else false end as IsIdentity,
            //                    case when pcolumn.is_nullable = 'YES'
            //                    then true else false end as IsNullable
            //                     from(select * from pg_tables where upper(tablename) = upper('{objectId}') and schemaname = '{schema}') ptables inner join pg_class pclass
            //                     on ptables.tablename = pclass.relname inner join(SELECT*
            //                     FROM information_schema.columns ) pcolumn on pcolumn.table_name = ptables.tablename
            //                    left join(
            //                        select pg_class.relname, pg_attribute.attname as colname from
            //                        pg_constraint inner join pg_class on pg_constraint.conrelid = pg_class.oid
            //                       inner join pg_attribute on pg_attribute.attrelid = pg_class.oid and  pg_attribute.attnum = pg_constraint.conkey[1] inner join pg_type on pg_type.oid = pg_attribute.atttypid 
            //                        where pg_constraint.contype = 'p'
            //                    ) pkey on pcolumn.table_name = pkey.relname
            //                    order by ptables.tablename";
            //var colList = _dbClient.SqlQueryable<SqlSugar.DbColumnInfo>(sql).ToList();
            //var viewList = _dbMaintenance.GetColumnInfosByTableName(objectId);
            colList.ForEach(v =>
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
                if (!string.IsNullOrEmpty(v.DefaultValue))
                {
                    column.DefaultValue = v.DefaultValue.Replace("(", "").Replace(")", "");
                }
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
            var result = false;
            if (objectType == DbObjectType.Table)
            {
                result = _dbClient.DbMaintenance.AddTableRemark(objectName, remark);
            }
            if (objectType == DbObjectType.View)
            {
                throw new NotSupportedException();
            }
            if (objectType == DbObjectType.Proc)
            {
                throw new NotSupportedException();
            }
            return result;
        }

        /// <summary>
        /// 更新列注释
        /// </summary>
        /// <param name="columnInfo"></param>
        /// <param name="remark"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool UpdateColumnRemark(Column columnInfo, string remark, DbObjectType objectType = DbObjectType.Table)
        {
            var result = false;
            var dbName = DbName.Split(':')[1];
            if (objectType == DbObjectType.Table)
            {
                var dbColumn = new SqlSugar.DbColumnInfo()
                {
                    TableName = columnInfo.ObjectId,
                    DbColumnName = columnInfo.DisplayName,
                    ColumnDescription = remark,
                    PropertyName = dbName
                };
                result = _dbClient.DbMaintenance.AddColumnRemark(dbColumn);
            }
            if (objectType == DbObjectType.View)
            {
                throw new NotSupportedException();
            }
            if (objectType == DbObjectType.Proc)
            {
                throw new NotSupportedException();
            }
            return result;
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
            strSql.Append($"{tempSql} FROM \"{Table.DisplayName}\"");
            return strSql.ToString();
            #endregion
        }

        public override string InsertSql()
        {
            #region MyRegion
            var tempCols = Columns.Where(x => x.IsIdentity == false).ToList();
            var strSql = new StringBuilder($"INSERT INTO \"{Table.DisplayName}\" (");
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
            var strSql = new StringBuilder($"UPDATE \"{Table.DisplayName}\" SET ");
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
            var strSql = new StringBuilder($"DELETE FROM \"{Table.DisplayName}\" WHERE ");
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

    public class SchemaInfo
    {
        public string NspName { get; set; }
    }
}
