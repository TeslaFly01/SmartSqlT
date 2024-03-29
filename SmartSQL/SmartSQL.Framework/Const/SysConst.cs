using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Framework.Const
{
    public static class SysConst
    {
        public static readonly string Sys_IsGroup = "IsGroup";
        public static readonly string Sys_IsMultipleTab = "IsMultipleTab";
        public static readonly string Sys_IsLikeSearch = "IsLikeSearch";
        public static readonly string Sys_IsContainsObjName = "IsContainsObjName";
        public static readonly string Sys_LeftMenuType = "LeftMenuType";
        public static readonly string Sys_SelectedConnection = "SelectedConnection";
        public static readonly string Sys_SelectedDataBase = "SelectedDataBase";
        public static readonly string Sys_IsShowSaveWin = "IsShowSaveWin";

        public static readonly string Sys_GROUPICON = "pack://application:,,,/Resources/svg/floder.svg";
        public static readonly string Sys_TABLEICON = "pack://application:,,,/Resources/svg/table.svg";
        public static readonly string Sys_VIEWICON = "pack://application:,,,/Resources/svg/view.svg";
        public static readonly string Sys_PROCICON = "pack://application:,,,/Resources/svg/proc.svg";
        public static readonly string Sys_TAGICON = "pack://application:,,,/Resources/svg/tag.svg";
        public static readonly string Sys_SQLICON = "pack://application:,,,/Resources/svg/sqlQuery.svg";
        public static readonly string Sys_KEYICON = "pack://application:,,,/Resources/svg/icons/icon-key.svg";
        public static readonly string Sys_FLODERICON = "pack://application:,,,/Resources/svg/floder.svg";

        public static readonly string Sys_TableIcon = "pack://application:,,,/SmartSQL;component/Resources/Img/icon/icon_table.png";
        public static readonly string Sys_ViewIcon = "pack://application:,,,/SmartSQL;component/Resources/Img/icon/icon_view.png";
        public static readonly string Sys_ProcIcon = "pack://application:,,,/SmartSQL;component/Resources/Img/icon/icon_proc.png";
        public static readonly string Sys_DatabaseIcon = "pack://application:,,,/SmartSQL;component/Resources/Img/icon/icon_database.png";

        /// <summary>
        /// SQL常用关键字
        /// </summary>
        public static readonly List<string> Sys_SqlKeywords = new List<string> {
            "ADD",        "ALL",        "ALTER",      "AND",       "ANY",       "AS",        "ASC",
            "BACKUP",     "BETWEEN",    "BY",         "CASE",      "CHECK",     "COLUMN",    "CONSTRAINT",
            "CREATE",     "DATABASE",   "DEFAULT",    "DELETE",    "DESC",      "DISTINCT",
            "DROP",       "ELSE",       "EXCEPT",     "EXISTS",    "FOREIGN",   "FROM",      "FULL",
            "GROUP",      "HAVING",     "IN",         "INDEX",     "INNER",     "INSERT",
            "INTO",       "IS",         "JOIN",       "LEFT",      "LIKE",      "LIMIT",
            "NOT",        "NULL",       "OR",         "ORDER",     "OUTER",     "PRIMARY",
            "REFERENCES", "RIGHT",      "SELECT",     "SET",       "TABLE",     "TOP",
            "TRUNCATE",   "UNION",      "UNIQUE",     "UPDATE",    "VALUES",    "VIEW",
            "WHERE",      "WITH"
            };

        /// <summary>
        /// SQL常用函数
        /// </summary>
        public static readonly List<string> Sys_SqlFuns = new List<string>
        {
            "COUNT",   "SUM",   "AVG",  "MIN",  "MAX",  "UPPER",    "LOWER",    "SUBSTRING",    "CONCAT",   "DATEPART"
        };

        public static readonly Dictionary<string,string> Sys_GencodeKeywords = new Dictionary<string,string>
        {
            ["Model"] = "",
            ["ClassName"] = "类名",
            ["TableName"] = "表名",
            ["Description"] = "表描述",
            ["Properties"] = "列集合",
            ["PropertyName"] = "类中属性名",
            ["Type"] = "类中的属性类型",
            ["DbCloumnName"] = "表字段名",
            ["IsPrimaryKey"] = "是否主键",
            ["IsIdentity"] = "是否自增",
            ["IsNullable"] = "是否为空"
        };
    }
}
