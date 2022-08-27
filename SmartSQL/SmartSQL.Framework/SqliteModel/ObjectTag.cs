using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Framework.SqliteModel
{
    public class ObjectTag
    {
        [PrimaryKey, AutoIncrement]
        public int TagId { get; set; }

        public int ConnectId { get; set; }

        public string DataBaseName { get; set; }

        public string TagName { get; set; }
    }
}
