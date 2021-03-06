using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using SqlSugar;
using DbType = SqlSugar.DbType;

namespace SmartSQL.Framework.Exporter
{
    using PhysicalDataModel;
    using Util;

    public class SqlServerExporter : Exporter, IExporter
    {
        public SqlServerExporter(string connectionString) : base(connectionString)
        {

        }
        public SqlServerExporter(string table, List<Column> columns) : base(table, columns)
        {

        }

        #region IExporter Members
        /// <summary>
        /// 初始化获取对象列表
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public override Model Init()
        {
            var model = new Model { Database = "SqlServer2008" };
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

        #endregion

        #region Private Members
        /// <summary>
        /// 获取数据库列表
        /// </summary>
        /// <returns></returns>
        public override List<DataBase> GetDatabases()
        {
            var sqlCmd = "SELECT name FROM sysdatabases ORDER BY name ASC";
            SqlDataReader dr = SqlHelper.ExecuteReader(DbConnectString, CommandType.Text, sqlCmd);
            var list = new List<DataBase>();
            while (dr.Read())
            {
                string displayName = dr.GetString(0);
                var dBase = new DataBase
                {
                    DbName = displayName,
                    IsSelected = false
                };
                list.Add(dBase);
            }
            return list;
        }

        /// <summary>
        /// 获取所有表
        /// </summary>
        /// <returns></returns>
        private Tables GetTables()
        {
            #region MyRegion
            Tables tables = new Tables(10);
            string sqlCmd = @"SELECT sy.[name],
                                     [object_id],
                                     CASE
			                              WHEN st.name='MS_Description' THEN ISNULL(st.value,'')
			                              ELSE '' 
	                                 END AS descript,
									 sy.create_date,
									 sy.modify_date,
	                                 s.name AS schemaName
                              FROM sys.tables sy
                              LEFT JOIN sys.extended_properties st ON st.major_id = sy.object_id
                              AND minor_id = 0
							  LEFT JOIN sys.schemas s ON s.schema_id=sy.schema_id
                              WHERE sy.type = 'U'
							  AND sy.name <> 'sysdiagrams'
                              ORDER BY sy.name ASC";
            SqlDataReader dr = SqlHelper.ExecuteReader(DbConnectString, CommandType.Text, sqlCmd);
            while (dr.Read())
            {
                try
                {
                    var name = dr.GetString(0);
                    var objectId = dr.GetInt32(1);
                    var comment = dr.IsDBNull(2) ? "" : dr.GetString(2);
                    var createDate = dr.GetDateTime(3);
                    var modifyDate = dr.GetDateTime(4);
                    var schemaName = dr.GetString(5);
                    var key = $"{schemaName}.{name}";
                    var table = new Table
                    {
                        Id = objectId.ToString(),
                        Name = name,
                        DisplayName = schemaName + "." + name,
                        SchemaName = schemaName,
                        Comment = comment,
                        CreateDate = createDate,
                        ModifyDate = modifyDate
                    };
                    if (!tables.ContainsKey(key))
                    {
                        tables.Add(key, table);
                    }
                }
                catch (Exception)
                {

                }
            }
            dr.Close();
            return tables;
            #endregion
        }

        /// <summary>
        /// 获取所有视图
        /// </summary>
        /// <returns></returns>
        private Views GetViews()
        {
            #region MyRegion
            var views = new Views(10);
            var sqlCmd = @"SELECT   a.name,
                                       a.object_id,
                                       b.descript,
                                       a.create_date,
                                       a.modify_date,
                                       s.name AS schemaName
                                FROM sys.views a
                                LEFT JOIN
                                (
                                    SELECT sy.name,
                                           sy.object_id,
                                           CASE
                                               WHEN st.name = 'MS_Description' THEN
                                                   ISNULL(st.value, '')
                                               ELSE
                                                   ''
                                           END AS descript,
                                           sy.create_date,
                                           sy.modify_date
                                    FROM sys.views sy
                                    LEFT JOIN sys.extended_properties st ON st.major_id = sy.object_id
                                                                            AND minor_id = 0
                                    WHERE sy.type = 'V'
                                          AND
                                          (
                                              st.name IS NULL
                                              OR st.name IN ( 'MS_Description' )
                                          )
                                    GROUP BY sy.name,
                                             sy.object_id,
                                             sy.create_date,
                                             sy.modify_date,
                                             CASE
                                                 WHEN st.name = 'MS_Description' THEN
                                                     ISNULL(st.value, '')
                                                 ELSE
                                                     ''
                                             END
                                ) b ON a.object_id = b.object_id
                                LEFT JOIN sys.schemas s ON s.schema_id = a.schema_id
                                ORDER BY a.name;";
            SqlDataReader dr = SqlHelper.ExecuteReader(DbConnectString, CommandType.Text, sqlCmd);
            while (dr.Read())
            {
                try
                {
                    var name = dr.GetString(0);
                    var objectId = dr.GetInt32(1);
                    var comment = dr.IsDBNull(2) ? "" : dr.GetString(2);
                    var createDate = dr.GetDateTime(3);
                    var modifyDate = dr.GetDateTime(4);
                    var schemaName = dr.IsDBNull(5) ? "" : dr.GetString(5);
                    var key = string.IsNullOrEmpty(schemaName) ? name : $"{schemaName}.{name}";
                    var view = new View
                    {
                        Id = objectId.ToString(),
                        Name = name,
                        DisplayName = schemaName + "." + name,
                        SchemaName = schemaName,
                        Comment = comment,
                        CreateDate = createDate,
                        ModifyDate = modifyDate
                    };
                    if (!views.ContainsKey(key))
                    {
                        views.Add(key, view);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            dr.Close();
            return views;
            #endregion
        }

        /// <summary>
        /// 获取所有存储过程
        /// </summary>
        /// <returns></returns>
        private Procedures GetProcedures()
        {
            #region MyRegion
            var procDic = new Procedures();
            var sqlCmd = @"SELECT   a.name,
                                       a.object_id,
                                       b.descript,
                                       a.create_date,
                                       a.modify_date,
                                       s.name AS schemaName
                                FROM sys.procedures a
                                LEFT JOIN sys.sql_modules m ON m.object_id = a.object_id
                                LEFT JOIN
                                (
                                    SELECT sy.name,
                                           sy.object_id,
                                           CASE
                                               WHEN st.name = 'MS_Description' THEN
                                                   ISNULL(st.value, '')
                                               ELSE
                                                   ''
                                           END AS descript,
                                           sy.create_date,
                                           sy.modify_date
                                    FROM sys.procedures sy
                                    LEFT JOIN sys.extended_properties st ON st.major_id = sy.object_id
                                                                            AND minor_id = 0
                                    WHERE sy.type = 'P'
                                          AND
                                          (
                                              st.name IS NULL
                                              OR st.name IN ( 'MS_Description' )
                                          )
                                    GROUP BY sy.name,
                                             sy.object_id,
                                             sy.create_date,
                                             sy.modify_date,
                                             CASE
                                                 WHEN st.name = 'MS_Description' THEN
                                                     ISNULL(st.value, '')
                                                 ELSE
                                                     ''
                                             END
                                ) b ON a.object_id = b.object_id
                                LEFT JOIN sys.schemas s ON s.schema_id = a.schema_id
                                WHERE a.is_ms_shipped = 0
                                      AND m.execute_as_principal_id IS NULL
                                      AND a.name <> 'sp_upgraddiagrams'
                                ORDER BY a.name;";
            SqlDataReader dr = SqlHelper.ExecuteReader(DbConnectString, CommandType.Text, sqlCmd);
            while (dr.Read())
            {
                string name = dr.GetString(0);
                int objectId = dr.GetInt32(1);
                string comment = dr.IsDBNull(2) ? "" : dr.GetString(2);
                DateTime createDate = dr.GetDateTime(3);
                DateTime modifyDate = dr.GetDateTime(4);
                var schemaName = dr.IsDBNull(5) ? "" : dr.GetString(5);
                var key = string.IsNullOrEmpty(schemaName) ? name : $"{schemaName}.{name}";

                var proc = new Procedure
                {
                    Id = objectId.ToString(),
                    Name = name,
                    DisplayName = schemaName + "." + name,
                    SchemaName = schemaName,
                    Comment = comment,
                    CreateDate = createDate,
                    ModifyDate = modifyDate
                };
                if (!procDic.ContainsKey(key))
                {
                    procDic.Add(key, proc);
                }
            }
            dr.Close();
            return procDic;
            #endregion
        }

        public override Columns GetColumnInfoById(string objectId)
        {
            #region MyRegion
            //var sql = $@"SELECT d.id as object_id,
            //                    d.name as object_name,
            //                    a.colorder AS column_id, 
            //                    a.name AS column_name, 
            //                    b.name AS type_name, 
            //                    COLUMNPROPERTY(a.id,a.name,'IsIdentity') AS is_identity , 
            //                    a.length AS byte_length, 
            //                    CASE WHEN b.name IN ('char','nchar','varchar','nvarchar','text','binary','varbinary','datetime2','datetimeoffset','time','numeric','decimal') THEN COLUMNPROPERTY(a.id,a.name,'PRECISION') ELSE 0 END AS length, 
            //                    isnull(COLUMNPROPERTY(a.id,a.name,'Scale'),0) AS dot_length, 
            //                    a.isnullable AS is_nullable, 
            //                    isnull(e.text,'') AS default_value, 
            //                    isnull(g.[value],'') AS description,
            //                    case when exists(SELECT 1 FROM sysobjects where xtype='PK' and name in (
            //                      SELECT name FROM sysindexes WHERE indid in(
            //                      SELECT indid FROM sysindexkeys WHERE id = a.id AND colid=a.colid 
            //                       ))) then 1 else 0 END AS is_primarykey 
            //                    FROM syscolumns a 
            //                    left join systypes b on a.xtype=b.xusertype 
            //                    inner join sysobjects d on a.id=d.id and d.name<>'dtproperties' 
            //                    left join syscomments e on a.cdefault=e.id 
            //                    left join sys.extended_properties g on a.id=g.major_id and a.colid=g.minor_id 
            //                    left join sys.extended_properties f on d.id=f.major_id and f.minor_id =0 
            //                    where d.id={ Convert.ToInt32(objectId)}
            //                    order by a.id,a.colorder";
            //return this.GetColumnsExt(DbConnectString, sql);
            #endregion

            #region MyRegion
            var columns = new Columns();
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.SqlServer, DbConnectString);
            var colList = dbMaintenance.GetColumnInfosByTableName(objectId);
            colList.ForEach(col =>
            {
                if (columns.ContainsKey(col.DbColumnName))
                {
                    return;
                }
                var column = new Column(col.DbColumnId.ToString(), col.DbColumnName, col.DbColumnName, col.DataType, col.ColumnDescription);
                column.Length = "";
                switch (col.DataType)
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
                        column.Length = $"({col.Length})"; break;
                    case "numeric":
                    case "decimal":
                        column.Length = $"({col.Length},{col.Scale})"; break;
                }

                column.ObjectId = objectId.ToString();
                column.ObjectName = col.TableName;
                column.IsIdentity = col.IsIdentity;
                column.IsNullable = col.IsNullable;
                column.DefaultValue = !string.IsNullOrEmpty(col.DefaultValue) && col.DefaultValue.Contains("((") ? col.DefaultValue.Replace("((", "").Replace("))", "") : col.DefaultValue;
                column.DataType = col.DataType;
                column.OriginalName = col.DbColumnName;
                column.Comment = col.ColumnDescription;
                column.IsPrimaryKey = col.IsPrimarykey;
                columns.Add(col.DbColumnName, column);
            });
            return columns; 
            #endregion
        }

        /// <summary>
        /// 获取对象定义脚本（视图、存储过程）
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override string GetScriptInfoById(string objectId, DbObjectType objectType)
        {
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.SqlServer, DbConnectString);
            var scriptInfo = dbMaintenance.GetScriptInfo(objectId, objectType);
            return scriptInfo.Definition;
        }

        public override string CreateTableSql()
        {
            #region MyRegion
            if (string.IsNullOrEmpty(TableName) || !Columns.Any())
            {
                return "";
            }
            var sb = new StringBuilder();
            sb.Append($"CREATE TABLE {TableName}(");
            var tempStr = new StringBuilder();
            Columns.ForEach(col =>
            {
                tempStr.Append(Environment.NewLine);
                tempStr.Append($"\t{col.DisplayName} {col.DataType}{col.Length} ");
                if (col.IsIdentity)
                {
                    tempStr.Append("IDENTITY(1,1) ");
                }
                var isNull = col.IsNullable ? "NULL," : "NOT NULL,";
                tempStr.Append(isNull);
            });
            sb.Append(tempStr.ToString().TrimEnd(','));
            var primaryKeyList = Columns.FindAll(x => x.IsPrimaryKey);
            if (primaryKeyList.Any())
            {
                sb.Append(Environment.NewLine);
                sb.Append($",\tPRIMARY KEY (");
                var sbPriKey = new StringBuilder();
                foreach (var column in primaryKeyList)
                {
                    sbPriKey.Append($"{column.DisplayName},");
                }
                sb.Append(sbPriKey.ToString().TrimEnd(','));
                sb.Append(")");
            }
            sb.Append(Environment.NewLine);
            sb.Append(")");
            return sb.ToString(); 
            #endregion
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
            #region MyRegion
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
            #endregion
        }

        /// <summary>
        /// 更新数据sql脚本
        /// </summary>
        /// <returns></returns>
        public override string UpdateSql()
        {
            #region MyRegion
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
            #endregion
        }

        /// <summary>
        /// 删除数据sql脚本
        /// </summary>
        /// <returns></returns>
        public override string DeleteSql()
        {
            #region MyRegion
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
            #endregion
        }

        /// <summary>
        /// 添加列sql脚本
        /// </summary>
        /// <returns></returns>
        public override string AddColumnSql()
        {
            var strSql = new StringBuilder();
            Columns.ForEach(col =>
            {
                strSql.Append($"ALTER TABLE {TableName} ADD {col.Name} {col.DataType.ToLower()} ");
                if (SqlServerDbTypeMapHelper.IsMulObj(col.DataType))
                {
                    strSql.Append($"{col.Length} ");
                }
                var isNull = col.IsNullable ? "NULL " : "NOT NULL ";
                strSql.Append(isNull);
                strSql.Append(Environment.NewLine);
                strSql.Append("GO");
                strSql.Append(Environment.NewLine);
                #region 字段注释
                if (!string.IsNullOrEmpty(col.Comment))
                {
                    strSql.Append($@"EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = '{col.Comment}',     
                                @level0type = 'SCHEMA',  
                                @level0name = 'dbo',
                                @level1type = 'TABLE',  
                                @level1name = '{col.ObjectName}',
                                @level2type = 'COLUMN',  
                                @level2name = '{col.DisplayName}' ");
                }
                #endregion
            });
            return strSql.ToString();
        }

        /// <summary>
        /// 修改列sql脚本
        /// </summary>
        /// <returns></returns>
        public override string AlterColumnSql()
        {
            var strSql = new StringBuilder();
            Columns.ForEach(col =>
            {
                strSql.Append($"ALTER TABLE {TableName} ALTER {col.Name} {col.DataType.ToLower()} ");
                if (SqlServerDbTypeMapHelper.IsMulObj(col.DataType))
                {
                    strSql.Append($"{col.Length} ");
                }
                var isNull = col.IsNullable ? "NULL " : "NOT NULL ";
                strSql.Append(isNull);
                strSql.Append(Environment.NewLine);
                strSql.Append("GO");
                strSql.Append(Environment.NewLine);
                #region 字段注释
                if (!string.IsNullOrEmpty(col.Comment))
                {
                    strSql.Append($@"EXEC sys.sp_updateextendedproperty @name = N'MS_Description',
                    @value = '{col.Comment}',
                    @level0type = 'SCHEMA',
                    @level0name = 'dbo',
                    @level1type = 'TABLE',
                    @level1name = '{col.ObjectName}',
                    @level2type = 'COLUMN',
                    @level2name = {col.DisplayName}");
                }
                #endregion
            });
            return strSql.ToString();
        }

        /// <summary>
        /// 删除列sql脚本
        /// </summary>
        /// <returns></returns>
        public override string DropColumnSql()
        {
            var strSql = new StringBuilder();
            Columns.ForEach(col =>
            {
                strSql.Append($"ALTER TABLE {TableName} DROP COLUMN {col.DisplayName}");
                #region 字段注释
                if (!string.IsNullOrEmpty(col.Comment))
                {
                    strSql.Append($@"EXEC sys.sp_dropextendedproperty @name = N'MS_Description',      
                                 @level0type = 'SCHEMA',  
                                 @level0name = 'dbo',
                                 @level1type = 'TABLE',  
                                 @level1name = '{col.ObjectName}',
                                 @level2type = 'COLUMN',  
                                 @level2name = '{col.DisplayName}' ");
                }
                #endregion
            });
            return strSql.ToString();
        }
        #endregion
    }
}
