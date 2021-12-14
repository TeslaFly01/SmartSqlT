using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartCode.Tool.Models;
using System.Configuration;

namespace SmartCode.Tool
{
    public class DataBasesConfig : NotifyPropertyBase,IConfigurationSectionHandler
    {
        public string DbName { get; set; }

        public string DbConnectString { get; set; }
        public object Create(object parent, object configContext, XmlNode section)
        {
            List<DataBasesConfig> config = new List<DataBasesConfig>();
            foreach (XmlNode xNode in section.ChildNodes)
            {
                if (xNode != null)
                {
                    var key = xNode.Attributes["key"];
                    var attribute = xNode.Attributes["value"];
                    if (attribute != null)
                    {
                        config.Add(new DataBasesConfig
                        {
                            DbName = key.Value,
                            DbConnectString = attribute.Value
                        });
                    }
                }
            }
            return config;
        }
    }
}
