using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartCode.Framework.PhysicalDataModel
{
    public class Procedure : BaseTable
    {
        public Procedure()
            : this("", "", "")
        {
        }

        public Procedure(string id, string displayName, string name)
            : this(id, displayName, name,string.Empty)
        {
        }

        public Procedure(string id, string displayName, string name, string comment)
            : base(id, displayName, name, comment)
        {
            this._mataTypeName = "procedure";
        }
    }
}
