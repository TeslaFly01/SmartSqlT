using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Framework.PhysicalDataModel
{
    public partial class DataBase
    {
        public string DbName { get; set; }

        public string Schema
        {
            get
            {
                return DbName.Contains(":") ? DbName.Split(':')[1] : DbName;
            }
        }

        public bool IsSelected { get; set; }
    }
}
