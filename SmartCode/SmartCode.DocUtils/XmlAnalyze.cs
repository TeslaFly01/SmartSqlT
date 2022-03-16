using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace SmartCode.DocUtils
{
    /// <summary>
    /// 解析VS生成的 XML文档文件
    /// </summary>
    public class XmlAnalyze
    {
        public Dictionary<KeyValuePair<string, string>, List<KeyValuePair<string, string>>> Data { get; set; } = new Dictionary<KeyValuePair<string, string>, List<KeyValuePair<string, string>>>();

        public List<string> EntityNames { get; set; } = new List<string>();

        public Dictionary<string, string> EntityComments { get; set; } = new Dictionary<string, string>();

        private Dictionary<string, string> EntityXPaths { get; set; } = new Dictionary<string, string>();

        public XmlAnalyze(string path)
        {
            var content = File.ReadAllText(path, Encoding.UTF8);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);
            var rootNode = doc.DocumentElement;

            var nodeList = rootNode.SelectNodes("//members/member[starts-with(@name,'T:')]");

            foreach (XmlNode node in nodeList)
            {
                var value = node.Attributes["name"]?.Value;
                var entityName = value?.Split('.')?.LastOrDefault();

                //实体名 必须由 字母数字或下划线组成
                if (Regex.IsMatch(entityName, @"^[a-z\d_]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase))
                {
                    var comment = node?.InnerText?.Trim();
                    EntityNames.Add(entityName);
                    EntityComments.Add(entityName, comment);

                    var xpath = value?.Replace("T:", "P:") + ".";
                    EntityXPaths.Add(entityName, xpath);
                }
            }

            foreach (var item in EntityXPaths)
            {
                var nodes = rootNode.SelectNodes($"//members/member[starts-with(@name,'{item.Value}')]");

                var ecKey = new KeyValuePair<string, string>(item.Key, EntityComments[item.Key]);

                var lstKV = new List<KeyValuePair<string, string>>();

                foreach (XmlNode node in nodes)
                {
                    var value = node.Attributes["name"]?.Value;
                    var propName = value?.Split('.')?.LastOrDefault();
                    var comment = node.InnerText?.Trim();

                    lstKV.Add(new KeyValuePair<string, string>(propName, comment));
                }
                Data.Add(ecKey, lstKV);
            }
        }
    }
}
