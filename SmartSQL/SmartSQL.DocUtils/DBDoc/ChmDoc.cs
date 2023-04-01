using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using HandyControl.Controls;
using SmartSQL.DocUtils.Dtos;
using SmartSQL.DocUtils.Properties;
using ZetaLongPaths;

namespace SmartSQL.DocUtils.DBDoc
{
    public class ChmDoc : Doc
    {
        #region MyRegion
        /// <summary>
        /// C:\Users\用户名\AppData\Roaming\SmartSQL\TplFile\chm
        /// </summary>
        private static string CHMPath = Path.Combine(TplPath, "chm");
        /// <summary>
        /// hhc.exe文件路径
        /// </summary>
        private string HHCPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hhc.exe");
        /// <summary>
        /// GBK编码
        /// </summary>
        private Encoding Gbk => Encoding.GetEncoding("GB18030");

        private Encoding Utf8 => Encoding.UTF8;
        #endregion


        public ChmDoc(DBDto dto, string filter = "chm files (*.chm)|*.chm") : base(dto, filter)
        {

        }

        public override bool Build(string filePath)
        {
            return BuildDoc(filePath);
        }

        /// <summary>
        /// 生成文档
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool BuildDoc(string filePath)
        {
            #region MyRegion
            this.InitDirFiles();

            var hhc_tpl = Encoding.UTF8.GetString(Resources.hhc);

            var hhk_tpl = Encoding.UTF8.GetString(Resources.hhk);

            var hhp_tpl = Encoding.UTF8.GetString(Resources.hhp);
            //文档目录
            var list_tpl = Encoding.UTF8.GetString(Resources.list);
            //表结构
            var table_tpl = Encoding.UTF8.GetString(Resources.table);
            //视图、存储过程
            var sqlcode_tpl = Encoding.UTF8.GetString(Resources.sqlcode);


            var hhc = hhc_tpl.RazorRender(this.Dto).Replace("</LI>", "");

            var hhk = hhk_tpl.RazorRender(this.Dto).Replace("</LI>", "");

            var list = list_tpl.RazorRender(this.Dto);

            ZlpIOHelper.WriteAllText(Path.Combine(this.WorkTmpDir, "chm.hhc"), hhc, Gbk);

            ZlpIOHelper.WriteAllText(Path.Combine(this.WorkTmpDir, "chm.hhk"), hhk, Gbk);

            ZlpIOHelper.WriteAllText(Path.Combine(this.WorkTmpDir, "数据库目录.html"), list, Utf8);
            int count = 0;
            int count_total = Dto.Tables.Count + Dto.Views.Count + Dto.Procs.Count ;
            //生成表
            foreach (var tab in this.Dto.Tables)
            {
                var tablePath = Path.Combine(this.WorkTmpDir, "table", $"{tab.TableName} {tab.Comment}.html");
                var content = table_tpl.RazorRender(tab);
                ZlpIOHelper.WriteAllText(tablePath, content, Utf8);
                count++;
                // 更新进度
                base.OnProgress(new ChangeRefreshProgressArgs
                {
                    BuildNum = count,
                    TotalNum = count_total,
                    BuildName = tab.TableName
                });
            }

            //生成视图
            foreach (var item in Dto.Views)
            {
                var viewPath = Path.Combine(this.WorkTmpDir, "view", $"{item.ObjectName}.html");
                var content = sqlcode_tpl.RazorRender(
                     new SqlCode()
                     {
                         DBType = Dto.DBType,
                         CodeName = item.ObjectName,
                         Content = item.Script.Trim()
                     }
                     );
                ZlpIOHelper.WriteAllText(viewPath, content, Utf8);
                count++;
                // 更新进度
                base.OnProgress(new ChangeRefreshProgressArgs
                {
                    BuildNum = count,
                    TotalNum = count_total,
                    BuildName = item.ObjectName
                });
            }

            //生成存储过程
            foreach (var item in Dto.Procs)
            {
                var procPath = Path.Combine(this.WorkTmpDir, "proc", $"{item.ObjectName}.html");
                var content = sqlcode_tpl.RazorRender(
                    new SqlCode
                    {
                        DBType = Dto.DBType,
                        CodeName = item.ObjectName,
                        Content = item.Script.Trim()
                    }
                    );
                ZlpIOHelper.WriteAllText(procPath, content, Utf8);
                count++;
                // 更新进度
                base.OnProgress(new ChangeRefreshProgressArgs
                {
                    BuildNum = count,
                    TotalNum = count_total,
                    BuildName = item.ObjectName
                });
            }

            var hhp_Path = Path.Combine(this.WorkTmpDir, "chm.hhp");
            var hhp = hhp_tpl.RazorRender(new ChmHHP(filePath, WorkTmpDir));

            ZlpIOHelper.WriteAllText(hhp_Path, hhp, Gbk);

            //开始生成CHM文件
            StartRun(HHCPath, hhp_Path, Encoding.GetEncoding("gbk"));
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

        /// <summary>
        /// 初始化模板文件
        /// </summary>
        private void InitDirFiles()
        {
            #region MyRegion
            var dirNames = new string[] {
                "table",
                "view",
                "proc",
                "resources\\js"
            };

            foreach (var name in dirNames)
            {
                var tmpDir = Path.Combine(this.WorkTmpDir, name);

                if (ZlpIOHelper.DirectoryExists(tmpDir))
                {
                    ZlpIOHelper.DeleteDirectory(tmpDir, true);
                }
                ZlpIOHelper.CreateDirectory(tmpDir);
            }

            if (!Directory.Exists(CHMPath))
            {
                Directory.CreateDirectory(CHMPath);
            }
            if (!Directory.Exists(Path.Combine(CHMPath, "embed")))
            {
                Directory.CreateDirectory(Path.Combine(CHMPath, "embed"));
            }
            if (!Directory.Exists(Path.Combine(CHMPath, "js")))
            {
                Directory.CreateDirectory(Path.Combine(CHMPath, "js"));
            }
            var highlight = Path.Combine(CHMPath, "embed", "highlight.js");
            if (!File.Exists(highlight))
            {
                File.AppendAllText(highlight, Resources.highlight);
            }
            var sql_formatter = Path.Combine(CHMPath, "embed", "sql-formatter.js");
            if (!File.Exists(sql_formatter))
            {
                File.AppendAllText(sql_formatter, Resources.sql_formatter);
            }
            var jQuery = Path.Combine(CHMPath, "js", "jQuery.js");
            if (!File.Exists(jQuery))
            {
                File.AppendAllText(jQuery, Resources.jQuery);
            }
            var dir = Path.Combine(TplPath, "chm\\");

            var files = Directory.GetFiles(dir, "*.js", SearchOption.AllDirectories);

            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                ZlpIOHelper.CopyFile(filePath, Path.Combine(this.WorkTmpDir, "resources\\js\\", fileName), true);
            }
            #endregion
        }

        private string StartRun(string hhcPath, string arguments, Encoding encoding)
        {
            string resultLog = string.Empty;
            var startInfo = new ProcessStartInfo
            {
                FileName = hhcPath,  //调入HHC.EXE文件 
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardErrorEncoding = encoding,
                StandardOutputEncoding = encoding
            };
            using (var process = Process.Start(startInfo))
            {
                using (var reader = process.StandardOutput)
                {
                    resultLog = reader.ReadToEnd();
                }
                process.WaitForExit();
            }
            return resultLog.Trim();
        }

    }
}
