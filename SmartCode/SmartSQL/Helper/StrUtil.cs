using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartCode.Framework.PhysicalDataModel;

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
    }
}
