using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Models
{
    public class ExecuteSQL
    {
        /// <summary>
        /// 执行SQL语句
        /// </summary>
        public string ExeSQL { get; set; }
        /// <summary>
        /// 是否查询
        /// </summary>
        public bool IsSelect { get; set; }
    }
}
