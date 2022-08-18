using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Framework.Util
{
    public class OracleDbTypeMapHelper
    {
        public static string MapCsharpType(string dbtype, bool isNullable)
        {
            if (string.IsNullOrEmpty(dbtype)) return dbtype;
            dbtype = dbtype.ToLower();
            string csharpType = "object";
            switch (dbtype)
            {
                case "int":
                case "integer":
                case "interval year to  month":
                case "interval day to  second":
                case "number": csharpType = "int"; break;
                case "decimal": csharpType = "decimal"; break;
                case "varchar":
                case "varchar2":
                case "nvarchar2":
                case "char":
                case "nchar":
                case "clob":
                case "long":
                case "nclob":
                case "rowid": csharpType = "string"; break;
                case "date":
                case "timestamp":
                case "timestamp with local time zone":
                case "timestamp with time zone": csharpType = "DateTime"; break;
                default: csharpType = "object"; break;
            }
            return csharpType;
        }
    }
}