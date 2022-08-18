using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace SmartSQL.Framework.Util
{
    public class MySqlDbTypeMapHelper
    {
        public static string MapCsharpType(string dbtype, bool isNullable)
        {
            if (string.IsNullOrEmpty(dbtype)) return dbtype;
            dbtype = dbtype.ToLower();
            var csharpType = "object";
            switch (dbtype)
            {
                case "int": 
                case "mediumint":
                case "integer": csharpType = isNullable ? "int?" : "int"; break;
                case "varchar": 
                case "text":
                case "char": 
                case "enum": 
                case "mediumtext":
                case "tinytext": 
                case "longtext": csharpType = "string"; break;
                case "tinyint": csharpType = isNullable ? "byte?" : "byte"; break;
                case "smallint": csharpType = isNullable ? "short?" : "short"; break;
                case "bigint": csharpType = isNullable ? "long?" : "long"; break;
                case "bit": csharpType = isNullable ? "bool?" : "bool"; break;
                case "real": 
                case "double": csharpType = isNullable ? "double?" : "double"; break;
                case "float": csharpType = isNullable ? "float?" : "float"; break;
                case "decimal": 
                case "numeric": csharpType = isNullable ? "decimal?" : "decimal"; break;
                case "year": csharpType = isNullable ? "int?" : "int"; break;
                case "datetime": 
                case "timestamp": 
                case "date":
                case "time": csharpType = isNullable ? "DateTime?" : "DateTime"; break;
                case "blob":
                case "longblob":
                case "tinyblob":
                case "varbinary":
                case "binary":
                case "multipoint":
                case "geometry":
                case "multilinestring":
                case "polygon":
                case "mediumblob": csharpType = "byteArray"; break;
            }
            return csharpType;
        }
    }
}
