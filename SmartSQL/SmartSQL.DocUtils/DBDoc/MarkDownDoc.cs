﻿using System;
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

        public override bool Build(string filePath)
        {
            return BuildDoc(filePath);
        }

        private bool BuildDoc(string filePath)
        {
            #region MyRegion
            var sb = new StringBuilder();
            sb.AppendLine("# 📚 数据库表目录");
            var dirMD = this.Dto.Tables.MarkDown("Columns", "DBType");
            dirMD = Regex.Replace(dirMD, @"(.+?\|\s+)([a-zA-Z][a-zA-Z0-9_]+)(\s+\|.+\n?)", $"$1[$2](#$2)$3", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            sb.Append(dirMD);
            sb.AppendLine();
            int count = 0;
            int count_total = Dto.Tables.Count + Dto.Views.Count + Dto.Procs.Count;
            if (this.Dto.Tables.Any())
            {
                sb.Append("## 📒 表结构");
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
                sb.Append("## 📰 视图");
                foreach (var item in this.Dto.Views)
                {
                    sb.AppendLine();
                    sb.AppendLine($"### <a name=\"{item.ObjectName}\">{item.ObjectName} {item.Comment}</a>");
                    sb.AppendLine("``` sql");
                    var fmtSql = JS.RunFmtSql(item.Script, this.Dto.DBType);
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
                sb.Append("## 📜 存储过程");
                foreach (var item in this.Dto.Procs)
                {
                    sb.AppendLine();
                    sb.AppendLine($"### <a name=\"{item.ObjectName}\">{item.ObjectName} {item.Comment}</a>");
                    sb.AppendLine("``` sql");
                    var fmtSql = JS.RunFmtSql(item.Script, this.Dto.DBType);
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
