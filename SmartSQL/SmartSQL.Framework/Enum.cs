using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Framework
{
    ///// <summary>
    ///// 数据库类型
    ///// </summary>
    //public enum DBType
    //{
    //    SqlServer,
    //    MySQL,
    //    Oracle,
    //    PostgreSQL,
    //    DB2,
    //    SQLite
    //}

    /// <summary>
    /// 脚本类型
    /// </summary>
    public enum ScriptType
    {
        /// <summary>
        /// 表
        /// </summary>
        Table = 1,
        /// <summary>
        /// 视图
        /// </summary>
        View = 2,
        /// <summary>
        /// 存储过程
        /// </summary>
        Proc = 3
    }

    public enum LangType
    {
        Csharp = 1,

        Java = 2
    }
}
