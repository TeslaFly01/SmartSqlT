using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using SqlSugar;
using DbType = SqlSugar.DbType;

namespace SmartCode.Framework.Exporter
{
    using PhysicalDataModel;
    using Util;

    public class SqlServerExporter : Exporter, IExporter
    {
        public SqlServerExporter(string connectionString) : base(connectionString)
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
                catch (Exception ex)
                {
                    throw;
                }
            }
            dr.Close();
            return views;
            #endregion
        }

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
            var sql = $@"SELECT d.id as object_id,
                                d.name as object_name,
                                a.colorder AS column_id, 
                                a.name AS column_name, 
                                b.name AS type_name, 
                                COLUMNPROPERTY(a.id,a.name,'IsIdentity') AS is_identity , 
                                a.length AS byte_length, 
                                CASE WHEN b.name IN ('char','nchar','varchar','nvarchar','text','binary','varbinary','datetime2','datetimeoffset','time','numeric','decimal') THEN COLUMNPROPERTY(a.id,a.name,'PRECISION') ELSE 0 END AS length, 
                                isnull(COLUMNPROPERTY(a.id,a.name,'Scale'),0) AS dot_length, 
                                a.isnullable AS is_nullable, 
                                isnull(e.text,'') AS default_value, 
                                isnull(g.[value],'') AS description,
                                case when exists(SELECT 1 FROM sysobjects where xtype='PK' and name in (
                                  SELECT name FROM sysindexes WHERE indid in(
                                  SELECT indid FROM sysindexkeys WHERE id = a.id AND colid=a.colid 
                                   ))) then 1 else 0 END AS is_primarykey 
                                FROM syscolumns a 
                                left join systypes b on a.xtype=b.xusertype 
                                inner join sysobjects d on a.id=d.id and d.name<>'dtproperties' 
                                left join syscomments e on a.cdefault=e.id 
                                left join sys.extended_properties g on a.id=g.major_id and a.colid=g.minor_id 
                                left join sys.extended_properties f on d.id=f.major_id and f.minor_id =0 
                                where d.id={ Convert.ToInt32(objectId)}
                                order by a.id,a.colorder";
            return this.GetColumnsExt(DbConnectString, sql);
            #endregion
        }

        private Columns GetColumnsExt(string connectionString, string sqlCmd)
        {
            #region MyRegion
            var columns = new Columns(500);
            var dr = SqlHelper.ExecuteReader(connectionString, CommandType.Text, sqlCmd);
            while (dr.Read())
            {
                int objectId = dr.IsDBNull(0) ? 0 : dr.GetInt32(0);
                string objectName = dr.IsDBNull(1) ? "" : dr.GetString(1);
                int id = dr.IsDBNull(2) ? 0 : dr.GetInt16(2);
                string displayName = dr.IsDBNull(3) ? string.Empty : dr.GetString(3);
                string name = dr.IsDBNull(3) ? string.Empty : dr.GetString(3);
                string dataType = dr.IsDBNull(4) ? string.Empty : dr.GetString(4);
                int identity = dr.IsDBNull(5) ? 0 : dr.GetInt32(5);
                int length = dr.IsDBNull(7) ? 0 : dr.GetInt32(7);
                int length_dot = dr.IsDBNull(8) ? 0 : dr.GetInt32(8);
                int isNullable = dr.IsDBNull(9) ? 0 : dr.GetInt32(9);
                string defaultValue = dr.IsDBNull(10) ? string.Empty : dr.GetString(10);
                string comment = dr.IsDBNull(11) ? string.Empty : dr.GetString(11);
                int isPrimaryKey = dr.IsDBNull(12) ? 0 : dr.GetInt32(12);

                Column column = new Column(id.ToString(), displayName, name, dataType, comment);
                column.Length = "";
                switch (dataType)
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
                        column.Length = $"({length})"; break;
                    case "numeric":
                    case "decimal":
                        column.Length = $"({length},{length_dot})"; break;
                }

                column.ObjectId = objectId.ToString();
                column.ObjectName = objectName;
                column.IsIdentity = identity == 1;
                column.IsNullable = isNullable == 1;
                column.DefaultValue = defaultValue.Contains("((") ? defaultValue.Replace("((", "").Replace("))", "") : defaultValue;
                column.DataType = dataType;
                column.OriginalName = name;
                column.Comment = comment;
                column.IsPrimaryKey = isPrimaryKey == 1;
                if (!columns.ContainsKey(id.ToString()))
                {
                    columns.Add(id.ToString(), column);
                }
            }
            dr.Close();
            return columns;
            #endregion
        }

        public override string GetScriptInfoById(string objectId, DbObjectType objectType)
        {
            var dbMaintenance = SugarFactory.GetDbMaintenance(DbType.SqlServer, DbConnectString);
            var scriptInfo = dbMaintenance.GetScriptInfo(objectId, objectType);
            return scriptInfo.Definition;
        }
        #endregion
    }
}
