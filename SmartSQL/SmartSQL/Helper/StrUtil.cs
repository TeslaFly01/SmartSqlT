using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Xml;
using SmartSQL.Framework.PhysicalDataModel;

namespace SmartSQL.Helper
{
    public static class StrUtil
    {
        public static void CreateClass(string filePath, string objectName, List<Column> columns)
        {
            using (var aFile = new FileStream(filePath, FileMode.Create))
            {
                using (var sw = new StreamWriter(aFile, Encoding.UTF8))
                {
                    sw.WriteLine("using System;");
                    sw.WriteLine("using System.Collections.Generic;");
                    sw.WriteLine("\t\t");
                    sw.WriteLine("namespace TestClass");
                    sw.WriteLine("{");
                    sw.WriteLine("\tpublic class {0}", objectName);
                    sw.WriteLine("\t{");
                    sw.WriteLine("\t\tpublic {0}()", objectName);
                    sw.WriteLine("\t\t{");
                    sw.WriteLine("\t\t}");
                    sw.WriteLine("\t\t");
                    foreach (var column in columns)
                    {
                        sw.WriteLine("\t\t///<summary>");
                        sw.WriteLine("\t\t///{0}", column.Comment);
                        sw.WriteLine("\t\t///</summary>");
                        sw.WriteLine("\t\tpublic {0} {1} {{ get; set; }}", column.CSharpType, column.DisplayName);
                        sw.WriteLine("\t\t");
                    }
                    sw.WriteLine("\t}");
                    sw.WriteLine("}");
                }
            }
        }

        /// <summary>
        /// Json格式化
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string JsonFormatter(string str)
        {
            var jsonDocument = JsonDocument.Parse(str);
            var formatJson = JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions()
            {
                // 整齐打印
                WriteIndented = true,

                //重新编码，解决中文乱码问题
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            });
            return formatJson;
        }

        /// <summary>
        /// 生成随机密码
        /// </summary>
        /// <param name="genlen"></param>
        /// <param name="usenumbers"></param>
        /// <param name="uselowalphabets"></param>
        /// <param name="usehighalphabets"></param>
        /// <param name="usesymbols"></param>
        /// <returns></returns>
        public static string GeneratePassword(int genlen = 21, bool usenumbers = true, bool uselowalphabets = true, bool usehighalphabets = true, bool usesymbols = true)
        {

            var upperCase = new char[]
                {
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
                'V', 'W', 'X', 'Y', 'Z'
                };

            var lowerCase = new char[]
                {
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u',
                'v', 'w', 'x', 'y', 'z'
                };

            var numerals = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            var symbols = new char[]
                {
                '~', '`', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '{', '[', '}', ']', '-', '_', '=', '+', ':',
                ';', '|', '/', '?', ',', '<', '.', '>'
                };

            char[] total = (new char[0])
                            .Concat(usehighalphabets ? upperCase : new char[0])
                            .Concat(uselowalphabets ? lowerCase : new char[0])
                            .Concat(usenumbers ? numerals : new char[0])
                            .Concat(usesymbols ? symbols : new char[0])
                            .ToArray();

            var rnd = new Random();

            var chars = Enumerable
                .Repeat<int>(0, genlen)
                .Select(i => total[rnd.Next(total.Length)])
                .ToArray();

            return new string(chars);
        }
    }
}
