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
        public TemplateInfo()
        {
            Type = 2;
            TypeNo = 0;
            IsDel = false;
            ChangeTime = DateTime.Now;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        /// <summary>
        /// 模板名称
        /// </summary>
        public string TempName { get; set; }
        /// <summary>
        /// 模板类型（1.系统模板，2.用户模板）
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 系统模板类型编号
        /// </summary>
        public int TypeNo { get; set; }
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
        /// 是否删除
        /// </summary>
        public bool IsDel { get; set; }
        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime ChangeTime { get; set; }
    }
}
