using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartCode.Framework.PhysicalDataModel
{
    /// <summary>
    /// Represent the Physical Data Model of Database.
    /// </summary>
    public class Model
    {
        protected string _database;
        protected string _databaseName;
        protected string _schema;
        protected Tables _tables;
        protected Views _views;
        protected Procedures _procs;

        public Model() { }

        public Model(Tables tables)
        {
            this._tables = tables;
        }

        public Model(Views views)
        {
            this._views = views;
        }

        public Model(Tables tables, Views views)
            : this(tables)
        {
            this._views = views;
        }

        public Tables Tables
        {
            get { return this._tables; }
            set { this._tables = value; }
        }

        public Views Views
        {
            get { return this._views; }
            set { this._views = value; }
        }

        public Procedures Procedures
        {
            get { return this._procs; }
            set { this._procs = value; }
        }

        public string Database
        {
            get { return this._database; }
            set { this._database = value; }
        }

        public string DatabaseName
        {
            get { return this._databaseName; }
            set { this._databaseName = value; }
        }

        public string Schema
        {
            get { return this._schema; }
            set { this._schema = value; }
        }
    }
}
