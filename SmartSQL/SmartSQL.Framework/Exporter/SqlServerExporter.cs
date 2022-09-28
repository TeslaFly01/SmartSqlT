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
        private readonly IDbMaintenance _dbMaintenance;
        public SqlServerExporter(string connectionString) : base(connectionString)
        {
            _dbMaintenance = SugarFactory.GetDbMaintenance(DbType.SqlServer, DbConnectString);
        }
        public SqlServerExporter(string connectionString,string dbName) : base(connectionString, dbName)
        {
            _dbMaintenance = SugarFactory.GetDbMaintenance(DbType.SqlServer, DbConnectString);
        }
        public SqlServerExporter(string table, List<Column> columns) : base(table, columns)
        {

        }

        /// <summary>
        /// 初始化获取对象列表
        /// </summary>
        /// <returns></returns>
        public override Model Init()
        {
            #region MyRegion
            var model = new Model { Database = "SqlServer" };
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
            #endregion
        }

        /// <summary>
        /// 获取数据库列表
        /// </summary>
        /// <returns></returns>
        public override List<DataBase> GetDatabases(string defaultDatabase = "")
        {
            #region MyRegion
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
            #endregion
        }

        #region 获取数据库对象私有方法
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
        /// 根据表名获取表信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private Tables GetTableByName(string tableName)
        {
            #region MyRegion
            Tables tables = new Tables(10);
            string sqlCmd = $@"SELECT sy.[name],
                                     [object_id],
                                     CASE
                                         WHEN st.name = 'MS_Description' THEN
                                             ISNULL(st.value, '')
                                         ELSE
                                             ''
                                     END AS descript,
                                     sy.create_date,
                                     sy.modify_date,
                                     s.name AS schemaName
                              FROM sys.tables sy
                              LEFT JOIN sys.extended_properties st ON st.major_id = sy.object_id
                                                                      AND minor_id = 0
                              LEFT JOIN sys.schemas s ON s.schema_id = sy.schema_id
                              WHERE sy.type = 'U'
                                    AND sy.name = '{tableName}'
                                    AND sy.name <> 'sysdiagrams'
                              ORDER BY sy.name ASC;";
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
        #endregion

        public override Columns GetColumnInfoById(string objectId)
        {
            #region MyRegion
            var columns = new Columns();
            int objId = 0;
            if (!int.TryParse(objectId, out objId))
            {
                var tableInfo = GetTableByName(objectId);
                if (!tableInfo.Values.Any())
                {
                    return columns;
                }
                objectId = tableInfo.Values.First().Id;
            }
            var colList = _dbMaintenance.GetColumnInfosByTableName(objectId);
            colList.ForEach(col =>
            {
                if (columns.ContainsKey(col.DbColumnName))
                {
                    return;
                }
                var column = new Column(col.DbColumnId.ToString(), col.DbColumnName, col.DbColumnName, col.DataType, col.ColumnDescription);
                column.LengthName = "";
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
                        column.LengthName = $"({col.Length})"; break;
                    case "numeric":
                    case "decimal":
                        column.LengthName = $"({col.Length},{col.Scale})"; break;
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
                column.CSharpType = SqlServerDbTypeMapHelper.MapCsharpType(col.DataType, col.IsNullable);
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
            var scriptInfo = _dbMaintenance.GetScriptInfo(objectId, objectType);
            return scriptInfo.Definition;
        }

        /// <summary>
        /// 更新对象备注
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
                result = _dbMaintenance.AddViewRemark(objectName, remark);
            }
            if (objectType == DbObjectType.Proc)
            {
                result = _dbMaintenance.AddProcRemark(objectName, remark);
            }
            return result;
        }

        /// <summary>
        /// 更新列注释
        /// </summary>
        /// <param name="columnInfo"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public override bool UpdateColumnRemark(Column columnInfo, string remark)
        {
            var columnName = columnInfo.Name;
            var tableName = columnInfo.ObjectName;
            if (_dbMaintenance.IsAnyColumnRemark(columnName, tableName))
            {
                _dbMaintenance.DeleteColumnRemark(columnName, tableName);
            }
            return _dbMaintenance.AddColumnRemark(columnName, tableName, remark);
        }

        #region 获取sql脚本
        /// <summary>
        /// 创建表结构sql脚本
        /// </summary>
        /// <returns></returns>
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
                tempStr.Append($"\t{col.DisplayName} {col.DataType}{col.LengthName} ");
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
            #region MyRegion
            var strSql = new StringBuilder("SELECT ");
            var tempCol = new StringBuilder();
            Columns.ForEach(col =>
            {
                tempCol.Append($"{col.Name},");
            });
            var tempSql = tempCol.ToString().TrimEnd(',');
            strSql.Append($"{tempSql} FROM {TableName}");
            return strSql.ToString();
            #endregion
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
            #region MyRegion
            var strSql = new StringBuilder();
            Columns.ForEach(col =>
            {
                strSql.Append($"ALTER TABLE {TableName} ADD {col.Name} {col.DataType.ToLower()} ");
                if (SqlServerDbTypeMapHelper.IsMulObj(col.DataType))
                {
                    strSql.Append($"{col.LengthName} ");
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
            #endregion
        }

        /// <summary>
        /// 修改列sql脚本
        /// </summary>
        /// <returns></returns>
        public override string AlterColumnSql()
        {
            #region MyRegion
            var strSql = new StringBuilder();
            Columns.ForEach(col =>
            {
                strSql.Append($"ALTER TABLE {TableName} ALTER {col.Name} {col.DataType.ToLower()} ");
                if (SqlServerDbTypeMapHelper.IsMulObj(col.DataType))
                {
                    strSql.Append($"{col.LengthName} ");
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
            #endregion
        }

        /// <summary>
        /// 删除列sql脚本
        /// </summary>
        /// <returns></returns>
        public override string DropColumnSql()
        {
            #region MyRegion
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
            #endregion
        }
        #endregion
    }
}
