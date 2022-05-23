using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SmartSQL.DocUtils.Dtos
{
    /// <summary>
    /// 数据库视图存储过程Dto
    /// </summary>
    public class ViewProDto
    {
        /// <summary>
        /// 序号
        /// </summary>
        [Display(Name = "序号")]
        public string TableOrder { get; set; }

        /// <summary>
        /// 表名
        /// </summary>
        [Display(Name = "对象名")]
        public string ObjectName { get; set; }

        /// <summary>
        /// 注释
        /// </summary>
        [Display(Name = "备注说明")]
        public string Comment { get; set; }

        /// <summary>
        /// 对象脚本
        /// </summary>
        //[XmlIgnore]
        public string Script { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        [Display(Name = "数据库类型")]
        public string DBType { get; set; }

        /// <summary>
        /// 表格列集合
        /// </summary>
        [Display(Name = "列数据")]
        public List<ColumnDto> Columns { get; set; }
    }
}
