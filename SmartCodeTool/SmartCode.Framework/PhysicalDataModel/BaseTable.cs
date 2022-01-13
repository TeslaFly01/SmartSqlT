using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartCode.Framework.PhysicalDataModel
{
    using Util;

    public abstract class BaseTable : IMetaData
    {
        protected string _name;
        protected string _mataTypeName;

        protected BaseTable() { }

        protected BaseTable(string id, string displayName, string name)
            : this(id, displayName, name, string.Empty)
        {
        }

        protected BaseTable(string id, string displayName, string name, string comment)
        {
            Id = id;
            DisplayName = displayName;
            Name = name;
            Comment = comment;
        }

        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 所属架构
        /// </summary>
        public string SchemaName { get; set; }

        public string LowerCamelName
        {
            get
            {
                var name = this._name ?? string.Empty;
                return name.LowerCamelCaseName();
            }
        }
        /// <summary>
        /// 注释说明
        /// </summary>
        public string Comment { get; set; }

        public string MetaTypeName
        {
            get { return this._mataTypeName; }
        }
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
