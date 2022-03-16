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
                model.Views = this.GetViews(connectionString);
                model.Procedures = this.GetProcedures(connectionString);
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
            var tables = new Tables();
            var dbMaintenancet = SugarFactory.GetDbMaintenance(DbType.MySql, connectionString);
            var tableList = dbMaintenancet.GetTableInfoList();
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
        }

        private Views GetViews(string connectionString)
        {
            Views views = new Views();
            var dbMaintenancet = SugarFactory.GetDbMaintenance(DbType.MySql, connectionString);
            var viewList = dbMaintenancet.GetViewInfoList();
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
        }

        private Procedures GetProcedures(string connectionString)
        {
            #region MyRegion
            Procedures procDic = new Procedures();

            var dbMaintenancet = SugarFactory.GetDbMaintenance(DbType.MySql, connectionString);
            var procInfoList = dbMaintenancet.GetProcInfoList();
            var dbName = dbMaintenancet.Context.Ado.Connection.Database;
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

        public override Columns GetColumns(string objectId, string connectionString)
        {
            var columns = new Columns(500);
            var dbMaintenancet = SugarFactory.GetDbMaintenance(DbType.MySql, connectionString);
            var viewList = dbMaintenancet.GetColumnInfosByTableName(objectId);
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

        public override string GetScripts(string objectId, string connectionString)
        {
            return "";
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT a.name,a.[type],b.[definition] ");
            sqlBuilder.Append("FROM sys.all_objects a, sys.sql_modules b ");
            sqlBuilder.AppendFormat("WHERE a.is_ms_shipped = 0 AND a.object_id = b.object_id AND a.object_id={0} ORDER BY a.[name] ASC ", objectId);

            return this.GetScripts1(connectionString, sqlBuilder.ToString());
        }

        private string GetScripts1(string connectionString, string sqlCmd)
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
