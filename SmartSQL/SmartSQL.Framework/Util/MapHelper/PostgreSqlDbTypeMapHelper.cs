using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace SmartSQL.Framework.Util
{
    public class PostgreSqlDbTypeMapHelper
    {

        public static string MapCsharpType(string dbtype, bool isNullable)
        {
            if (string.IsNullOrEmpty(dbtype)) return dbtype;
            dbtype = dbtype.ToLower();
            var csharpType = "object";
            switch (dbtype)
            {
                case "int2":
                case "smallint": csharpType = isNullable ? "short?" : "short"; break;
                case "int4":
                case "double precision":
                case "integer": csharpType = isNullable ? "int?" : "int"; break;
                case "int8":
                case "bigint": csharpType = isNullable ? "long?" : "long"; break;
                case "float4":
                case "real": csharpType = isNullable ? "float?" : "float"; break;
                case "float8": csharpType = isNullable ? "double?" : "double"; break;
                case "numeric":
                case "decimal":
                case "path":
                case "point":
                case "interval":
                case "lseg":
                case "macaddr":
                case "money": 
                case "polygon": csharpType = isNullable ? "decimal?" : "decimal"; break;
                case "boolean":
                case "bool":
                case "box":
                case "bytea": csharpType = isNullable ? "bool?" : "bool"; break;
                case "varchar":
                case "character varying":
                case "geometry":
                case "name":
                case "text":
                case "char":
                case "character":
                case "cidr":
                case "circle":
                case "tsquery":
                case "tsvector":
                case "xml":
                case "json":
                case "txid_snapshot": csharpType = "string"; break;
                case "uuid": csharpType = isNullable ? "Guid?" : "Guid"; break;
                case "timestamp":
                case "timestamp with time zone":
                case "timestamptz":
                case "timestamp without time zone": 
                case "date":
                case "time":
                case "time with time zone":
                case "timetz":
                case "time without time zone": csharpType = isNullable ? "DateTime?" : "DateTime"; break;
                case "bit":
                case "bit varying": csharpType = isNullable ? "byteArray?" : "byteArray"; break;
                case "varbit": csharpType = isNullable ? "byte?" : "byte"; break;
                case "public.geometry":
                case "inet": csharpType = "object"; break;
            }
            return csharpType;
        }
    }
}
