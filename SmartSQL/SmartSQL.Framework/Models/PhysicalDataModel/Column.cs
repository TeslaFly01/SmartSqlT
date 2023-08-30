using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartSQL.Framework.PhysicalDataModel
{
    using Util;

    public class Column : IMetaData
    {
        protected string _id;
        protected string _displayName;
        protected string _name;
        protected string _originalName;
        protected string _comment;
        protected string _dataType;
        protected string _defaultValue;
        protected string _languageType = string.Empty;
        protected string _languageTypeAlias = string.Empty;
        protected string _languageDefaultValue = string.Empty;
        protected string _lengthName;
        protected int _ordinal = -1;
        protected bool _isNullable;
        protected bool _hasDefault;
        protected bool _isPrimaryKey;
        protected bool _isIdentity;

        public Column()
        {
            new Column();
        }

        public Column(string id, string displayName, string name)
        {
            this._id = id;
            this._displayName = displayName;
            this._name = name;
        }

        public Column(string id, string displayName, string name, string dataType)
            : this(id, displayName, name)
        {
            this._dataType = dataType;
        }

        public Column(string id, string displayName, string name, string dataType, string comment)
            : this(id, displayName, name, dataType)
        {
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

        public string LowerCamelName
        {
            get
            {
                var name = this._name ?? string.Empty;
                return name.LowerCamelCaseName();
            }
        }

        public string OriginalName
        {
            get { return this._originalName ?? string.Empty; }
            set { this._originalName = value; }
        }

        public string Comment
        {
            get { return this._comment; }
            set { this._comment = value; }
        }

        public string DataType
        {
            get { return this._dataType; }
            set { this._dataType = value; }
        }

        public int Length { get; set; }

        public string LengthName
        {
            get { return this._lengthName; }
            set { this._lengthName = value; }
        }

        public bool IsIdentity
        {
            get { return this._isIdentity; }
            set { this._isIdentity = value; }
        }

        public bool IsNullable
        {
            get { return this._isNullable; }
            set { this._isNullable = value; }
        }

        public string DefaultValue
        {
            get { return this._defaultValue; }
            set { this._defaultValue = value; }
        }

        public string LanguageType
        {
            get { return this._languageType ?? string.Empty; }
            set { this._languageType = value; }
        }

        public string LanguageTypeAlias
        {
            get { return this._languageTypeAlias ?? string.Empty; }
            set { this._languageTypeAlias = value; }
        }

        public string LanguageTypeAliasAbbr
        {
            get
            {
                var name = this._languageTypeAlias ?? string.Empty;
                return name.Split('.').LastOrDefault();
            }
        }

        public string LanguageDefaultValue
        {
            get { return this._languageDefaultValue ?? string.Empty; }
            set { this._languageDefaultValue = value; }
        }

        public int Ordinal
        {
            get { return this._ordinal; }
            set { this._ordinal = value; }
        }

        public bool HasDefault
        {
            get { return this._hasDefault; }
            set { this._hasDefault = value; }
        }

        public bool IsPrimaryKey
        {
            get { return this._isPrimaryKey; }
            set { this._isPrimaryKey = value; }
        }

        public string ObjectId { get; set; }

        public string ObjectName { get; set; }

        public string MetaTypeName
        {
            get { return "column"; }
        }

        public string CSharpType { get; set; }
    }
}
