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
        /// <param name="genNum">生成个数</param>
        /// <param name="usenumbers"></param>
        /// <param name="uselowalphabets"></param>
        /// <param name="usehighalphabets"></param>
        /// <param name="usesymbols"></param>
        /// <returns></returns>
        public static List<string> GeneratePassword(int genlen = 21, int genNum = 1, bool usenumbers = true, bool uselowalphabets = true, bool usehighalphabets = true, bool usesymbols = true)
        {
            #region MyRegion
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
            var listGen = new List<string>();
            var rnd = new Random();
            for (int i = 0; i < genNum; i++)
            {
                var chars = Enumerable
                    .Repeat<int>(0, genlen)
                    .Select(j => total[rnd.Next(total.Length)])
                    .ToArray();
                listGen.Add(new string(chars));
            }
            return listGen;
            #endregion
        }

        /// <summary>
        /// 密码强度计算
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static int PasswordStrength(string password)
        {
            //空字符串强度值为0
            if (password == "") return 0;
            //字符统计
            int iNum = 0, iLtt = 0, iSym = 0;
            foreach (char c in password)
            {
                if (c >= '0' && c <= '9') iNum++;
                else if (c >= 'a' && c <= 'z') iLtt++;
                else if (c >= 'A' && c <= 'Z') iLtt++;
                else iSym++;
            }
            if (iLtt == 0 && iSym == 0) return 1; //纯数字密码
            if (iNum == 0 && iLtt == 0) return 1; //纯符号密码
            if (iNum == 0 && iSym == 0) return 1; //纯字母密码
            if (password.Length <= 6) return 1; //长度不大于6的密码
            if (iLtt == 0) return 2; //数字和符号构成的密码
            if (iSym == 0) return 2; //数字和字母构成的密码
            if (iNum == 0) return 2; //字母和符号构成的密码
            if (password.Length <= 10) return 2; //长度不大于10的密码
            return 3; //由数字、字母、符号构成的密码
        }

        /// <summary>
        /// Base64编码
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Base46_Encode(string text)
        {
            byte[] bytes = Encoding.Default.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Base64解码
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Base46_Decode(string text)
        {
            byte[] outputb = Convert.FromBase64String(text);
            return Encoding.Default.GetString(outputb);
        }
    }
}
