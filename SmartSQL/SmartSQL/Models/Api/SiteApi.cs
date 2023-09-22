using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Models.Api
{
    public class SiteApi
    {
        public string siteName { get; set; }

        public string icon { get; set; }

        public string url { get; set; }

        public string description { get; set; }

        public string category { get; set; }

        public string type { get; set; }

        public bool isEnable { get; set; }
    }
}
