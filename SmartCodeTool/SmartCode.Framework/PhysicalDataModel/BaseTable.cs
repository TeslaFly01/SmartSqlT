using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartCode.Framework.PhysicalDataModel
{
    using Util;

    public abstract class BaseTable : IMetaData
    {
        protected string _id;
        protected string _displayName;
        protected string _name;
        protected string _originalName;
        protected string _comment;
        protected string _mataTypeName;
        protected Columns _columns;

        protected BaseTable() { }

        protected BaseTable(string id, string displayName, string name)
            : this(id, displayName, name, string.Empty)
        {
        }

        protected BaseTable(string id, string displayName, string name, string comment)
        {
            this._id = id;
            this._displayName = displayName;
            this._name = name;
            this._comment = comment;
        }

        public string Id
        {
            get { return this._id; }
            set { this._id = value; }
        }

        public string DisplayName
        {
            get { return this._displayName; }
            set { this._displayName = value; }
        }

        public string Name
        {
            get { return this._name; }
            set { this._name = value; }
        }

        public string OriginalName
        {
            get { return this._originalName ?? string.Empty; }
            set { this._originalName = value; }
        }

        public string LowerCamelName
        {
            get
            {
                var name = this._name ?? string.Empty;
                return name.LowerCamelCaseName();
            }
        }

        public string Comment
        {
            get { return this._comment; }
            set { this._comment = value; }
        }

        public string MetaTypeName
        {
            get { return this._mataTypeName; }
        }

        public Columns Columns
        {
            get { return this._columns; }
            set { this._columns = value; }
        }
    }
}
