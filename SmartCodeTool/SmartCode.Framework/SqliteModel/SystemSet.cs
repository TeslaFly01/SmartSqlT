using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace SmartCode.Framework.SqliteModel
{
    public class SystemSet
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Type { get; set; }

        public string Value { get; set; }

    }
}
