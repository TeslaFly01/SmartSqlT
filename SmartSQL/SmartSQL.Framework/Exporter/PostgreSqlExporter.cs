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
        public PostgreSqlExporter(string connectionString, string dbName) : base(connectionString, dbName)
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
            var schemaSql = "SELECT nspname AS NspName FROM pg_catalog.pg_namespace order by nspname asc ";
            var schemaList = dbClient.SqlQueryable<SchemaInfo>(schemaSql).ToList();
            //var dataBaseList = _dbMaintenance.GetDataBaseList(dbClient);
            var list = new List<DataBase>();
            schemaList.ForEach(schema =>
            {
                if (schema.Equals("pg_toast") || schema.Equals("pg_catalog") || schema.Equals("information_schema"))
                {
                    return;
                }
                var dBase = new DataBase
                {
                    DbName = $"{defaultDatabase}:{schema.NspName}",
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
            var schema = DbName.Contains(":") ? DbName.Split(':')[1] : DbName;
            var tbSql = $@"SELECT DISTINCT
                                table_name AS Name,
                                obj_description(oid, 'pg_class') AS Description
                           FROM
                                information_schema.tables t,pg_class p
                           WHERE
                                    table_schema = '{schema}'
                           AND t.table_name = p.relname";
            var dbClient = SugarFactory.GetInstance(DbType.PostgreSQL, DbConnectString);
            var tableList = dbClient.SqlQueryable<DbTableInfo>(tbSql).ToList();
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
            var schema = DbName.Contains(":") ? DbName.Split(':')[1] : DbName;
            var dbClient = SugarFactory.GetInstance(DbType.PostgreSQL, DbConnectString);
            var sql = $@"select cast (pclass.oid as int4) as TableId,cast(ptables.tablename as varchar) as TableName,
                                pcolumn.column_name as DbColumnName,pcolumn.udt_name as DataType,
                                CASE WHEN pcolumn.numeric_scale > 0 THEN pcolumn.numeric_precision ELSE pcolumn.character_maximum_length END   as Length,
                                pcolumn.column_default as DefaultValue,
                                pcolumn.numeric_scale as DecimalDigits,
                                pcolumn.numeric_scale as Scale,
                                col_description(pclass.oid, pcolumn.ordinal_position) as ColumnDescription,
                                case when pkey.colname = pcolumn.column_name
                                then true else false end as IsPrimaryKey,
                                case when pcolumn.column_default like 'nextval%'
                                then true else false end as IsIdentity,
                                case when pcolumn.is_nullable = 'YES'
                                then true else false end as IsNullable
                                 from(select * from pg_tables where upper(tablename) = upper('{objectId}') and schemaname = '{schema}') ptables inner join pg_class pclass
                                 on ptables.tablename = pclass.relname inner join(SELECT*
                                 FROM information_schema.columns ) pcolumn on pcolumn.table_name = ptables.tablename
                                left join(
                                    select pg_class.relname, pg_attribute.attname as colname from
                                    pg_constraint inner join pg_class on pg_constraint.conrelid = pg_class.oid
                                   inner join pg_attribute on pg_attribute.attrelid = pg_class.oid and  pg_attribute.attnum = pg_constraint.conkey[1] inner join pg_type on pg_type.oid = pg_attribute.atttypid 
                                    where pg_constraint.contype = 'p'
                                ) pkey on pcolumn.table_name = pkey.relname
                                order by ptables.tablename";
            var colList = dbClient.SqlQueryable<DbColumnInfo>(sql).ToList();
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
                result = _dbMaintenance.AddTableRemark(objectName, remark);
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
                var dbColumn = new DbColumnInfo()
                {
                    TableName = columnInfo.ObjectId,
                    DbColumnName = columnInfo.DisplayName,
                    ColumnDescription = remark,
                    PropertyName = dbName
                };
                result = _dbMaintenance.AddColumnRemark(dbColumn);
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

    public class SchemaInfo
    {
        public string NspName { get; set; }
    }
}
