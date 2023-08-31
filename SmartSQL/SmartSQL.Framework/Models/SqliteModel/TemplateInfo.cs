using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace SmartSQL.Framework.SqliteModel
{
    public class TemplateInfo
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        /// <summary>
        /// 模板名称
        /// </summary>
        public string TempName { get; set; }
        /// <summary>
        /// 名称格式
        /// </summary>
        public string FileNameFormat { get; set; }
        /// <summary>
        /// 文件后缀名
        /// </summary>
        public string FileExt { get; set; }
        /// <summary>
        /// 模板内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime ChangeTime { get; set; }
    }
}
