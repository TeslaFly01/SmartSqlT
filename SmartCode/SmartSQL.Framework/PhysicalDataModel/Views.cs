using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartSQL.Framework.PhysicalDataModel
{
    public class Views : Dictionary<string, View>
    {
        public Views()
            : base()
        {
        }

        public Views(int capacity)
            : base(capacity)
        {
        }
    }
}
