using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartCode.Framework.SqliteModel;

namespace SmartCode.Framework.Exporter
{
    public class SqliteExporter
    {

        public SystemSet SystemSetByName(string name)
        {
            var sqLiteHelper = new SQLiteHelper();
            var system = sqLiteHelper.db.Table<SystemSet>().FirstOrDefault(x => x.Name.Equals(name));
            if (system == null)
            {
                system = new SystemSet
                {
                    Name = name,
                    Value = "false"
                };
                sqLiteHelper.Add(system);
            }
            return system;
        }
    }
}
