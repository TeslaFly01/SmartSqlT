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
using SmartSQL.Framework;

namespace SmartSQL.DocUtils.DBDoc
{
    public class MarkDownDoc : Doc
    {
        public MarkDownDoc(DBDto dto, string filter = "markdown files (.md)|*.md") : base(dto, filter)
        {
        }

        public override bool Build(string filePath)
        {
            return BuildDoc(filePath);
        }

        private bool BuildDoc(string filePath)
        {
            #region MyRegion
            var sb = new StringBuilder();
            sb.AppendLine("### 📚 数据库表目录");
            var regPattern = @"(.+?\|\s+)([a-zA-Z][a-zA-Z0-9_]+)(\s+\|.+\n?)";
            var regPlacement = $"$1[$2](#$2)$3";
            //Extensions.MarkDown();
            var Objects = new List<TableDto>();
            Dto.Tables.ForEach(t =>
            {
                Objects.Add(t);
            });
            Dto.Views.ForEach(v =>
            {
                var oNum = Objects.Count + 1;
                Objects.Add(new TableDto
                {
                    TableOrder = oNum.ToString(),
                    TableName = v.ObjectName,
                    Comment = v.Comment
                });
            });
            Dto.Procs.ForEach(v =>
            {
                var oNum = Objects.Count + 1;
                Objects.Add(new TableDto
                {
                    TableOrder = oNum.ToString(),
                    TableName = v.ObjectName,
                    Comment = v.Comment
                });
            });
            var dirMD = Objects.MarkDown("Columns", "DBType", "Script");
            dirMD = Regex.Replace(dirMD, regPattern, regPlacement, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            sb.Append(dirMD);
            sb.AppendLine();
            int count = 0;
            int count_total = Dto.Tables.Count + Dto.Views.Count + Dto.Procs.Count;
            if (this.Dto.Tables.Any())
            {
                sb.Append("### 📒 表结构");
                foreach (var dto in this.Dto.Tables)
                {
                    sb.AppendLine();
                    sb.AppendLine($"#### 表名： {dto.TableName}");
                    sb.AppendLine($"说明： {dto.Comment}");

                    if (dto.DBType.StartsWith("Oracle"))
                    {
                        sb.Append(dto.Columns.MarkDown("IsIdentity"));
                    }
                    else
                    {
                        sb.Append(dto.Columns.MarkDown());
                    }

                    sb.AppendLine();
                    count++;
                    // 更新进度
                    base.OnProgress(new ChangeRefreshProgressArgs
                    {
                        BuildNum = count,
                        TotalNum = count_total,
                        BuildName = dto.TableName
                    });
                }
            }

            if (this.Dto.Views.Any())
            {
                sb.Append("### 📰 视图");
                foreach (var item in this.Dto.Views)
                {
                    sb.AppendLine();
                    sb.AppendLine($"#### 视图名： {item.ObjectName}");
                    sb.AppendLine($"说明： {item.Comment}");

                    sb.AppendLine("``` sql");
                    var fmtSql = item.Script.SqlFormat();
                    sb.Append(fmtSql);
                    sb.AppendLine("```");
                    sb.AppendLine();
                    count++;
                    // 更新进度
                    base.OnProgress(new ChangeRefreshProgressArgs
                    {
                        BuildNum = count,
                        TotalNum = count_total,
                        BuildName = item.ObjectName
                    });
                }
            }

            if (this.Dto.Procs.Any())
            {
                sb.Append("### 📜 存储过程");
                foreach (var item in this.Dto.Procs)
                {
                    sb.AppendLine();
                    sb.AppendLine($"#### 存储过程名： {item.ObjectName}");
                    sb.AppendLine($"说明： {item.Comment}");
                    
                    sb.AppendLine("``` sql");
                    var fmtSql = item.Script.SqlFormat();
                    sb.Append(fmtSql);
                    sb.AppendLine("```");
                    sb.AppendLine();
                    count++;
                    // 更新进度
                    base.OnProgress(new ChangeRefreshProgressArgs
                    {
                        BuildNum = count,
                        TotalNum = count_total,
                        BuildName = item.ObjectName
                    });
                }
            }
            var md = sb.ToString();

            ZlpIOHelper.WriteAllText(filePath, md, Encoding.UTF8);
            // 更新进度
            base.OnProgress(new ChangeRefreshProgressArgs
            {
                BuildNum = count,
                TotalNum = count_total,
                IsEnd = true
            });
            return true;
            #endregion
        }
    }
}
