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

        public override string GetScriptInfoById(string objectId, DbObjectType objectType)
        {
            throw new NotImplementedException();
        }

        public override Model Init()
        {
            var model = new Model { Database = "Dm" };
            try
            {
                model.Tables = this.GetTables();
                model.Views = new Views();// this.GetViews();
                model.Procedures = new Procedures();//暂时不支持存储过程 this.GetProcedures();
                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Tables GetTables()
        {
            #region MyRegion
            //var tables = new Tables();
            //var tableList = _dbMaintenance.GetTableInfoList(false);
            //tableList.ForEach(tb =>
            //{
            //    if (tables.ContainsKey(tb.Name))
            //    {
            //        return;
            //    }
            //    var table = new Table
            //    {
            //        Id = tb.Name,
            //        Name = tb.Name,
            //        DisplayName = tb.Name,
            //        Comment = tb.Description,
            //        CreateDate = tb.CreateDate,
            //        ModifyDate = tb.ModifyDate
            //    };
            //    tables.Add(tb.Name, table);
            //});
            //return tables;



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
            #endregion
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
