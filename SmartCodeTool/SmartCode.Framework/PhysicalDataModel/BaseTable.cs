using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartCode.Framework.PhysicalDataModel
{
    using Util;

    public abstract class BaseTable : IMetaData
    {
        /// <summary>
        /// 对象ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 对象名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 对象显示名称（架构名 + . + 对象名称）
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 所属架构
        /// </summary>
        public string SchemaName { get; set; }
        /// <summary>
        /// 注释说明
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime ModifyDate { get; set; }
    }
}
