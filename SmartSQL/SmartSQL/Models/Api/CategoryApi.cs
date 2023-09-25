using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Models.Api
{
    public class CategoryApi
    {
        public CategoryApi() { }

        public string categoryName { get; set; }

        public string icon { get; set; }

        public int count { get; set; }

        public bool isEnable { get; set; }

        public List<CategoryApiType> type { get; set; }

        public CategoryApiType SelectedType { get; set; }

        public List<SiteApi> sites { get; set; }
    }

    public class CategoryApiType
    {
        public string typeName { get; set; }

        public List<SiteApi> sites { get; set; }
    }
}
