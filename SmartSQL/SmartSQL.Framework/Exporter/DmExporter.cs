using SmartSQL.Framework.PhysicalDataModel;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Framework.Exporter
{
    /// <summary>
    /// 达梦数据库
    /// </summary>
    public class DmExporter : Exporter, IExporter
    {
        private readonly IDbMaintenance _dbMaintenance;

        public DmExporter(string connectionString) : base(connectionString)
        {

        }

        public DmExporter(string connectionString, string dbName) : base(connectionString, dbName)
        {
            _dbMaintenance = SugarFactory.GetDbMaintenance(DbType.Dm, DbConnectString);
        }

        public DmExporter(string tableName, List<Column> columns) : base(tableName, columns)
        {

        }

        public override Model Init()
        {
            #region MyRegion
            var model = new Model { Database = "Dm" };
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
            #endregion
        }

        /// <summary>
        /// 获取所有模式
        /// </summary>
        /// <param name="defaultDatabase"></param>
        /// <returns></returns>
        public override List<DataBase> GetDatabases(string defaultDatabase = "")
        {
            #region MyRegion
            var dbClient = SugarFactory.GetInstance(DbType.Dm, DbConnectString);
            var dataBaseList = dbClient.Ado.SqlQuery<string>("select distinct object_name from all_objects where object_type = 'SCH';");
            var dbList = new List<DataBase>();
            dataBaseList.ForEach(x =>
            {
                dbList.Add(new DataBase
                {
                    DbName = x,
                    IsSelected = x == defaultDatabase
                });
            });
            return dbList;
            #endregion
        }

        /// <summary>
        /// 获取所有表
        /// </summary>
        /// <returns></returns>
        private Tables GetTables()
        {
            #region MyRegion
            var tables = new Tables();
            var tbSql = $@"SELECT
                                    a.table_name AS NAME,
                                    b.comments   AS Description
                            FROM
                                    dba_tables AS a
                            LEFT OUTER JOIN USER_TAB_COMMENTS b
                            ON
                                    a.table_name = b.table_name
                            WHERE
                                    a.table_name!='HELP'
                                AND a.table_name NOT LIKE '%$%'
                                AND a.table_name NOT LIKE 'LOGMNRC_%'
                                AND a.table_name!='LOGMNRP_CTAS_PART_MAP'
                                AND a.table_name!='LOGMNR_LOGMNR_BUILDLOG'
                                AND a.table_name!='SQLPLUS_PRODUCT_PROFILE'
                                AND a.Owner = '{DbName}'";
            var dbClient = SugarFactory.GetInstance(DbType.Dm, DbConnectString);
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

        /// <summary>
        /// 获取所有视图
        /// </summary>
        /// <returns></returns>
        private Views GetViews()
        {
            #region MyRegion
            var views = new Views();
            var tbSql = $@"select
                                    a.Object_Name as Name       ,
                                    b.Comments    as Description,
                                    a.Created     as CreateDate ,
                                    a.Created     as ModifyDate
                            from
                                    dba_objects         as a
                            left join USER_TAB_COMMENTS as b
                            on
                                    a.Object_Name=b.table_name
                            where
                                    OWNER      = '{DbName}'
                                and Object_type='VIEW'";
            var dbClient = SugarFactory.GetInstance(DbType.Dm, DbConnectString);
            var viewList = dbClient.SqlQueryable<DbTableInfo>(tbSql).ToList();
            viewList.ForEach(v =>
            {
                if (views.ContainsKey(v.Name))
                {
                    return;
                }
                var view = new View
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

        /// <summary>
        /// 获取所有存储过程
        /// </summary>
        /// <returns></returns>
        private Procedures GetProcedures()
        {
            return new Procedures();
        }

        /// <summary>
        /// 获取对象所有列信息
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
                var dataType = v.DataType.ToLower();
                switch (dataType)
                {
                    case "char":
                    case "character":
                    case "varchar":
                    case "varchar2":
                    case "time":
                    case "text":
                    case "binary":
                    case "nvarchar":
                    case "varbinary":
                    case "datetime2":
                    case "datetimeoffset":
                        column.LengthName = $"({v.Length})"; break;
                    case "numeric":
                    case "decimal":
                    case "number":
                    case "dec":
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
                //  column.CSharpType = OracleDbTypeMapHelper.MapCsharpType(v.DataType, v.IsNullable);
                columns.Add(v.DbColumnName, column);
            });
            return columns;
            #endregion
        }

        /// <summary>
        /// 获取对象脚本信息
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override string GetScriptInfoById(string objectId, DbObjectType objectType)
        {
            #region MyRegion
            var sql = $@"select
                                view_name as Name,
                                'View' as Type,
                                text as definition
                        from
                                dba_Views
                        where
                                owner = '{DbName}'
                            and view_name = '{objectId}'";
            var list = new List<ScriptInfo>();

            var dbClient = SugarFactory.GetInstance(DbType.Dm, DbConnectString);
            list = dbClient.SqlQueryable<ScriptInfo>(sql).ToList();
            if (list.Any())
            {
                return list.First().Definition;
            }
            return new ScriptInfo().Definition;
            #endregion
        }

        public override string AddColumnSql()
        {
            throw new NotImplementedException();
        }

        public override string AlterColumnSql()
        {
            throw new NotImplementedException();
        }

        public override string CreateTableSql()
        {
            throw new NotImplementedException();
        }

        public override string DeleteSql()
        {
            throw new NotImplementedException();
        }

        public override string DropColumnSql()
        {
            throw new NotImplementedException();
        }

        public override string InsertSql()
        {
            throw new NotImplementedException();
        }

        public override string SelectSql()
        {
            throw new NotImplementedException();
        }

        public override bool UpdateColumnRemark(Column columnInfo, string remark)
        {
            throw new NotImplementedException();
        }

        public override bool UpdateObjectRemark(string objectName, string remark, DbObjectType objectType = DbObjectType.Table)
        {
            throw new NotImplementedException();
        }

        public override string UpdateSql()
        {
            throw new NotImplementedException();
        }
    }
}
