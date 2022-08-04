using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace SmartSQL.Framework.Util
{
    public class SqliteDbTypeMapHelper
    {

        public static string MapCsharpType(string dbtype, bool isNullable)
        {
            if (string.IsNullOrEmpty(dbtype)) return dbtype;
            dbtype = dbtype.ToLower();
            var csharpType = "object";
            switch (dbtype)
            {
                case "integer":
                case "int":
                case "int32":
                case "integer32":
                case "number": csharpType = isNullable ? "int?" : "int"; break;
                case "varchar":
                case "varchar2":
                case "nvarchar":
                case "nvarchar2":
                case "text":
                case "ntext":
                case "blob_text":
                case "char":
                case "nchar":
                case "num":
                case "currency":
                case "datetext":
                case "word":
                case "graphic": csharpType = "string"; break;
                case "tinyint":
                case "unsignedinteger8": csharpType = isNullable ? "byte?" : "byte"; break;
                case "smallint":
                case "int16": csharpType = isNullable ? "short?" : "short"; break;
                case "bigint":
                case "int64":
                case "long":
                case "integer64": csharpType = isNullable ? "long?" : "long"; break;
                case "bit":
                case "bool":
                case "boolean": csharpType = isNullable ? "bool?" : "bool"; break;
                case "real":
                case "double": csharpType = isNullable ? "double?" : "double"; break;
                case "float": csharpType = isNullable ? "float?" : "float"; break;
                case "decimal":
                case "dec":
                case "numeric":
                case "money":
                case "smallmoney": csharpType = isNullable ? "decimal?" : "decimal"; break;
                case "datetime":
                case "timestamp":
                case "date":
                case "time": csharpType = isNullable ? "DateTime?" : "DateTime"; break;
                case "blob":
                case "clob":
                case "raw":
                case "oleobject":
                case "binary":
                case "photo":
                case "picture": csharpType = isNullable ? "byteArray?" : "byteArray"; break;
                case "uniqueidentifier": csharpType = isNullable ? "Guid?" : "Guid"; break;
            }
            return csharpType;
        }
    }
}
