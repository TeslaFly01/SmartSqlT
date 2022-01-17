using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SmartCode.Framework.Exporter
{
    using PhysicalDataModel;

    public abstract class BaseExporter : IExporter
    {
        public abstract Model Export(string connectionString);
        public abstract List<DataBase> GetDatabases(string connectionString);

        public abstract Columns GetColumns(int objectId, string connectionString);

        public abstract string GetScripts(int objectId, string connectionString);

        public abstract DataSet GetDataSet(string connectionString, string tbName, string strWhere);

        public abstract bool UpdateComment(string connection,string  type, string tableName,string schema, string columnName, string comment);
    }
}
