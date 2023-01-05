using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace SmartSQL.DocUtils.Dtos
{
    /// <summary>
    /// 数据库Dto
    /// </summary>
    public class DBDto
    {
        public DBDto() { }

        public DBDto(string dbName, object tag = null)
        {
            this.DBName = dbName;
            this.Tag = tag;
        }
        /// <summary>
        /// 文档标题
        /// </summary>
        public string DocTitle { get; set; }
        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DBName { get; set; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DBType { get; set; }
        /// <summary>
        /// 表
        /// </summary>
        public List<TableDto> Tables { get; set; } = new List<TableDto>();
        /// <summary>
        /// 视图
        /// </summary>
        public List<ViewProDto> Views { get; set; } = new List<ViewProDto>();
        /// <summary>
        /// 存储过程
        /// </summary>
        public List<ViewProDto> Procs { get; set; } = new List<ViewProDto>();
        /// <summary>
        /// 其他一些参数数据，用法如 WinForm 控件的 Tag属性
        /// </summary>
        public object Tag { get; set; }
    }
}
