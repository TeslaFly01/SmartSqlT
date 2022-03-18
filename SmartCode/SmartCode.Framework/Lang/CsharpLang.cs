using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartCode.Framework.PhysicalDataModel;

namespace SmartCode.Framework.Lang
{
    public class CsharpLang : Lang
    {
        public override string BuildEntity(string tableName, List<Column> columns)
        {
            var sb = new StringBuilder();
            sb.Append("using System;");
            sb.Append(Environment.NewLine);
            sb.Append("using System.Collections.Generic;");
            sb.Append(Environment.NewLine);
            sb.Append("namespace Test");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append($"\tpublic class {tableName}");
            sb.Append(Environment.NewLine);
            sb.Append("\t{");
            sb.Append(Environment.NewLine);
            sb.Append($"\t\tpublic {tableName}()");
            sb.Append(Environment.NewLine);
            sb.Append("\t\t{");
            sb.Append(Environment.NewLine);
            sb.Append("\t\t}");
            sb.Append(Environment.NewLine);
            foreach (var column in columns)
            {
                sb.Append("\t\t///<summary>");
                sb.Append(Environment.NewLine);
                sb.Append($"\t\t///{column.Comment}");
                sb.Append(Environment.NewLine);
                sb.Append("\t\t///</summary>");
                sb.Append(Environment.NewLine);
                sb.Append($"\t\tpublic {column.CSharpType} {column.DisplayName} {{ get; set; }}");
                sb.Append("\t\t");
                sb.Append(Environment.NewLine);
            }
            sb.Append("\t}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
