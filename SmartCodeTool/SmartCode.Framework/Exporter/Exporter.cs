using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SmartCode.Framework.Exporter
{
    using PhysicalDataModel;

    public abstract class Exporter : IExporter
    {
        public Exporter(string connectionString)
        {
            DBConnectString = connectionString;
        }
        /// <summary>
        /// 数据库连接
        /// </summary>
        public string DBConnectString { get; private set; }

        public abstract Model Export(string connectionString);
        public abstract List<DataBase> GetDatabases(string connectionString);

        public abstract Columns GetColumns(string objectId, string connectionString);

        public abstract string GetScripts(string objectId, string connectionString);

        public abstract DataSet GetDataSet(string connectionString, string tbName, string strWhere);

        public abstract bool UpdateComment(string connection, string type, string tableName, string schema, string columnName, string comment);
    }
}
