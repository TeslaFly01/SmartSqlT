using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SmartCode.Framework.Exporter
{
    using PhysicalDataModel;

    public interface IExporter
    {
        Model Export(string connectionString);
        List<DataBase> GetDatabases(string connectionString);

        Columns GetColumns(int objectId, string connectionString);

        string GetScripts(int objectId, string connectionString);

        DataSet GetDataSet(string connectionString, string tbName, string strWhere);

        bool UpdateComment(string connection,string type, string tableName,string schema, string comment, string columnName="");
    }
}
