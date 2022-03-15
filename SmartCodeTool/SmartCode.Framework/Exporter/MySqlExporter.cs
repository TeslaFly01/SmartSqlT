using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartCode.Framework.PhysicalDataModel;
using DbType = SqlSugar.DbType;

namespace SmartCode.Framework.Exporter
{
    public class MySqlExporter : Exporter, IExporter
    {
        public MySqlExporter(string connectionString) : base(connectionString)
        {

        }

        public override Model Export(string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");

            var model = new Model { Database = "MySql" };
            try
            {
                model.Tables = this.GetTables(connectionString);
                //model.Views = this.GetViews(connectionString);
                //model.Procedures = this.GetProcedures(connectionString);
                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override List<DataBase> GetDatabases(string connectionString)
        {
            var dbMaintenancet = SugarFactory.GetDbMaintenance(DbType.MySql, connectionString);
            var dbClient = SugarFactory.GetInstance(DbType.MySql, connectionString);
            var dataBaseList = dbMaintenancet.GetDataBaseList(dbClient);
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
        }

        private Tables GetTables(string connectionString)
        {
            Tables tables = new Tables(10);

            var dbMaintenancet = SugarFactory.GetDbMaintenance(DbType.MySql, connectionString);
            var dbClient = SugarFactory.GetInstance(DbType.MySql, connectionString);
            var tableList = dbMaintenancet.GetTableInfoList();
            tableList.ForEach(tb =>
            {
                var table = new Table
                {
                    Name = tb.Name,
                    DisplayName = tb.Name,
                    Comment = tb.Description,
                    CreateDate = tb.CreateDate,
                    ModifyDate = tb.ModifyDate
                };
                if (!tables.ContainsKey(tb.Name))
                {
                    tables.Add(tb.Name, table);
                }
            });
            return tables;
        }

        private Views GetViews(string connectionString)
        {
            Views views = new Views(10);

            string sqlCmd = @"SELECT   a.name,
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
            SqlDataReader dr = SqlHelper.ExecuteReader(connectionString, CommandType.Text, sqlCmd);
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
        }

        private Procedures GetProcedures(string connectionString)
        {
            Procedures procDic = new Procedures();

            string sqlCmd = @"SELECT   a.name,
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
            SqlDataReader dr = SqlHelper.ExecuteReader(connectionString, CommandType.Text, sqlCmd);
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
        }

        public override Columns GetColumns(int objectId, string connectionString)
        {
            var sql = $@"SELECT  
                                --表名=case when a.colorder=1 then d.name else '' end, 
                                --表说明=case when a.colorder=1 then isnull(f.value,'') else '' end,
                                d.id as object_id,
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
                                where d.id={objectId}
                                order by a.id,a.colorder";

            return this.GetColumnsExt(connectionString, sql);
        }

        private Columns GetColumnsExt(string connectionString, string sqlCmd)
        {
            Columns columns = new Columns(500);
            SqlDataReader dr = SqlHelper.ExecuteReader(connectionString, CommandType.Text, sqlCmd);
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
        }

        public override string GetScripts(int objectId, string connectionString)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT a.name,a.[type],b.[definition] ");
            sqlBuilder.Append("FROM sys.all_objects a, sys.sql_modules b ");
            sqlBuilder.AppendFormat("WHERE a.is_ms_shipped = 0 AND a.object_id = b.object_id AND a.object_id={0} ORDER BY a.[name] ASC ", objectId);

            return this.GetScripts(connectionString, sqlBuilder.ToString());
        }

        public Columns GetPrimaryKeys(int objectId, string connectionString, Columns columns)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("select syscolumns.name from syscolumns,sysobjects,sysindexes,sysindexkeys ");
            sqlBuilder.AppendFormat("where syscolumns.id ={0} ", objectId);
            sqlBuilder.Append("and sysobjects.xtype = 'PK' and sysobjects.parent_obj = syscolumns.id ");
            sqlBuilder.Append("and sysindexes.id = syscolumns.id and sysobjects.name = sysindexes.name and ");
            sqlBuilder.Append("sysindexkeys.id = syscolumns.id and sysindexkeys.indid = sysindexes.indid and syscolumns.colid = sysindexkeys.colid");

            Columns primaryKeys = new Columns(4);
            SqlDataReader dr = SqlHelper.ExecuteReader(connectionString, CommandType.Text, sqlBuilder.ToString());
            while (dr.Read())
            {
                string name = dr.IsDBNull(0) ? string.Empty : dr.GetString(0);
                if (columns.ContainsKey(name)) primaryKeys.Add(name, columns[name]);
            }
            dr.Close();

            return primaryKeys;
        }

        private string GetScripts(string connectionString, string sqlCmd)
        {
            string script = string.Empty;
            SqlDataReader dr = SqlHelper.ExecuteReader(connectionString, CommandType.Text, sqlCmd);
            while (dr.Read())
            {
                string displayName = dr.IsDBNull(2) ? string.Empty : dr.GetString(2);
                script = displayName;
            }
            dr.Close();

            return script;
        }

        public override DataSet GetDataSet(string connectionString, string tbName, string strWhere)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append($"select top 200 * from {tbName} where 1=1 ");
            if (!string.IsNullOrEmpty(strWhere))
            {
                sqlBuilder.Append($" and {strWhere}");
            }
            var dataSet = SqlHelper.ExecuteDataset(connectionString, CommandType.Text, sqlBuilder.ToString());
            return dataSet;
        }

        /// <summary>
        /// 修改字段备注说明
        /// </summary>
        /// <param name="connection">连接</param>
        /// <param name="objectName">对象名</param>
        /// <param name="schema">架构名</param>
        /// <param name="comment">描述</param>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public override bool UpdateComment(string connection, string type, string objectName, string schema, string comment, string columnName)
        {
            var sb = new StringBuilder();
            sb.Append(
                $"EXEC sp_updateextendedproperty @name= N'MS_Description',@value= N'{comment}',@level0type= N'SCHEMA', @level0name=N'{schema}',@level1type=N'{type}', @level1name=N'{objectName}'");
            if (!string.IsNullOrEmpty(columnName))
            {
                sb.Append($",@level2type=N'column',@level2name= N'{columnName}'");
            }
            try
            {
                SqlHelper.ExecuteNonQuery(connection, CommandType.Text, sb.ToString());
            }
            catch (Exception ex)
            {
                try
                {
                    sb.Clear();
                    sb.Append($"EXEC sp_addextendedproperty @name= N'MS_Description',@value= N'{comment}',@level0type= N'SCHEMA', @level0name=N'{schema}',@level1type=N'{type}', @level1name=N'{objectName}'");
                    if (!string.IsNullOrEmpty(columnName))
                    {
                        sb.Append($",@level2type=N'column',@level2name= N'{columnName}'");
                    }
                    SqlHelper.ExecuteNonQuery(connection, CommandType.Text, sb.ToString());
                }
                catch (Exception ex1)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
