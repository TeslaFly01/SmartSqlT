using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SmartSQL.DocUtils;
using ZetaLongPaths;
using SmartSQL.DocUtils.Dtos;

namespace SmartSQL.DocUtils.DBDoc
{
    public class MarkDownDoc : Doc
    {
        public MarkDownDoc(DBDto dto, string filter = "markdown files (.md)|*.md") : base(dto, filter)
        {
        }

        public override void Build(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# 数据库表目录");
            var dirMD = this.Dto.Tables.MarkDown("Columns", "DBType");
            dirMD = Regex.Replace(dirMD, @"(.+?\|\s+)([a-zA-Z][a-zA-Z0-9_]+)(\s+\|.+\n?)", $"$1[$2](#$2)$3", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            sb.Append(dirMD);
            sb.AppendLine();

            if (this.Dto.Tables.Any())
            {
                sb.Append("## 表结构");
                foreach (var dto in this.Dto.Tables)
                {
                    sb.AppendLine();
                    sb.AppendLine($"### <a name=\"{dto.TableName}\">{dto.TableName} {dto.Comment}</a>");

                    if (dto.DBType.StartsWith("Oracle"))
                    {
                        sb.Append(dto.Columns.MarkDown("IsIdentity"));
                    }
                    else
                    {
                        sb.Append(dto.Columns.MarkDown());
                    }

                    sb.AppendLine();
                }
            }

            if (this.Dto.Views.Any())
            {
                sb.Append("## 视图");
                foreach (var item in this.Dto.Views)
                {
                    sb.AppendLine();
                    sb.AppendLine($"### {item.ObjectName}");
                    sb.AppendLine("``` sql");
                    var fmtSql = JS.RunFmtSql(item.Script, this.Dto.DBType);
                    sb.Append(fmtSql);
                    sb.AppendLine("```");
                    sb.AppendLine();
                }
            }

            if (this.Dto.Procs.Any())
            {
                sb.Append("## 存储过程");
                foreach (var item in this.Dto.Procs)
                {
                    sb.AppendLine();
                    sb.AppendLine($"### {item.ObjectName}");
                    sb.AppendLine("``` sql");
                    var fmtSql = JS.RunFmtSql(item.Script, this.Dto.DBType);
                    sb.Append(fmtSql);
                    sb.AppendLine("```");
                    sb.AppendLine();
                }
            }
            var md = sb.ToString();

            ZlpIOHelper.WriteAllText(filePath, md, Encoding.UTF8);
        }
    }
}
