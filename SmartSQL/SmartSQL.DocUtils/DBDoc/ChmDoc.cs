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
        private static string ChmPath = Path.Combine(TplPath, "chm");
        public ChmDoc(DBDto dto, string filter = "chm files (*.chm)|*.chm") : base(dto, filter)
        {

        }

        private Encoding Gbk => Encoding.GetEncoding("GB18030");

        private Encoding Utf8 => Encoding.UTF8;

        private string HHCPath
        {
            get
            {
                var hhcPath = string.Empty;
                var hhwDir = ConfigUtils.SearchInstallDir("HTML Help Workshop", "hhw.exe");
                if (!string.IsNullOrWhiteSpace(hhwDir) && ZlpIOHelper.DirectoryExists(hhwDir))
                {
                    hhcPath = Path.Combine(hhwDir, "hhc.exe");
                }
                return hhcPath;
            }
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
            #region 使用 HTML Help Workshop 的 hhc.exe 编译 ,先判断系统中是否已经安装有  HTML Help Workshop 

            if (this.HHCPath.IsNullOrWhiteSpace() || !File.Exists(HHCPath))
            {
                string htmlhelpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "htmlhelp.exe");

                if (File.Exists(htmlhelpPath))
                {
                    string htmlHelpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "htmlhelp.exe");
                    if (File.Exists(htmlHelpPath))
                    {
                        Growl.AskGlobal("导出CHM文档需安装 HTML Help Workshop ，是否立即安装？", isConfirm =>
                        {
                            if (isConfirm)
                            {
                                try
                                {
                                    var proc = Process.Start(htmlHelpPath);
                                }
                                catch { }
                            }
                            return true;
                        });
                    }
                }
                else
                {
                    Growl.AskGlobal("导出CHM文档需安装 HTML Help Workshop ，是否立即下载安装？", isConfirm =>
                    {
                        if (isConfirm)
                        {
                            try
                            {
                                var myProcess = new Process();
                                //"firefox.exe";// "iexplore.exe";  //chrome  //iexplore.exe //哪个浏览器打开
                                myProcess.StartInfo.FileName = "chrome.exe";
                                myProcess.StartInfo.Arguments = "https://gitee.com/izhaofu/SmartSQL/attach_files/1124263/download";
                                myProcess.Start();
                            }
                            catch { }
                        }
                        return true;
                    });
                }
                return false;
            }
            #endregion

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

            ZlpIOHelper.WriteAllText(Path.Combine(this.WorkTmpDir, "chm.hhc"), hhc,Gbk);

            ZlpIOHelper.WriteAllText(Path.Combine(this.WorkTmpDir, "chm.hhk"), hhk, Gbk);

            ZlpIOHelper.WriteAllText(Path.Combine(this.WorkTmpDir, "数据库目录.html"), list, Utf8);

            foreach (var tab in this.Dto.Tables)
            {
                var tab_path = Path.Combine(this.WorkTmpDir, "table", $"{tab.TableName} {tab.Comment}.html");
                var content = table_tpl.RazorRender(tab);
                ZlpIOHelper.WriteAllText(tab_path, content, Utf8);
            }


            foreach (var item in Dto.Views)
            {
                var vw_path = Path.Combine(this.WorkTmpDir, "view", $"{item.ObjectName}.html");
                var content = sqlcode_tpl.RazorRender(
                     new SqlCode()
                     {
                         DBType = Dto.DBType,
                         CodeName = item.ObjectName,
                         Content = item.Script.Trim()
                     }
                     );
                ZlpIOHelper.WriteAllText(vw_path, content, Utf8);
            }


            foreach (var item in Dto.Procs)
            {
                var proc_path = Path.Combine(this.WorkTmpDir, "proc", $"{item.ObjectName}.html");
                var content = sqlcode_tpl.RazorRender(
                    new SqlCode()
                    {
                        DBType = Dto.DBType,
                        CodeName = item.ObjectName,
                        Content = item.Script.Trim()
                    }
                    );
                ZlpIOHelper.WriteAllText(proc_path, content, Utf8);
            }

            var hhp_Path = Path.Combine(this.WorkTmpDir, "chm.hhp");
            var hhp = hhp_tpl.RazorRender(new ChmHHP(filePath, WorkTmpDir));

            ZlpIOHelper.WriteAllText(hhp_Path, hhp, Gbk);

            var res = StartRun(HHCPath, hhp_Path, Encoding.GetEncoding("gbk"));

            //LogManager.Info("生成chm信息", res);
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

            if (!Directory.Exists(ChmPath))
            {
                Directory.CreateDirectory(ChmPath);
            }
            if (!Directory.Exists(Path.Combine(ChmPath, "embed")))
            {
                Directory.CreateDirectory(Path.Combine(ChmPath, "embed"));
            }
            if (!Directory.Exists(Path.Combine(ChmPath, "js")))
            {
                Directory.CreateDirectory(Path.Combine(ChmPath, "js"));
            }
            var highlight = Path.Combine(ChmPath, "embed", "highlight.js");
            if (!File.Exists(highlight))
            {
                File.AppendAllText(highlight, Resources.highlight);
            }
            var sql_formatter = Path.Combine(ChmPath, "embed", "sql-formatter.js");
            if (!File.Exists(sql_formatter))
            {
                File.AppendAllText(sql_formatter, Resources.sql_formatter);
            }
            var jQuery = Path.Combine(ChmPath, "js", "jQuery.js");
            if (!File.Exists(jQuery))
            {
                File.AppendAllText(jQuery, Resources.jQuery);
            }
            //var hhc = Path.Combine(ChmPath, "hhc.cshtml");
            //var hhk = Path.Combine(ChmPath, "hhk.cshtml");
            //var hhp = Path.Combine(ChmPath, "hhp.cshtml");
            //var list = Path.Combine(ChmPath, "list.cshtml");
            //var sqlcode = Path.Combine(ChmPath, "sqlcode.cshtml");
            //var table = Path.Combine(ChmPath, "table.cshtml");
            //if (!File.Exists(hhc))
            //{
            //    File.WriteAllBytes(hhc, Resources.hhc);
            //}
            //if (!File.Exists(hhk))
            //{
            //    File.WriteAllBytes(hhk, Resources.hhk);
            //}
            //if (!File.Exists(hhp))
            //{
            //    File.WriteAllBytes(hhp, Resources.hhp);
            //}
            //if (!File.Exists(list))
            //{
            //    File.WriteAllBytes(list, Resources.list);
            //}
            //if (!File.Exists(sqlcode))
            //{
            //    File.WriteAllBytes(sqlcode, Resources.sqlcode);
            //}
            //if (!File.Exists(table))
            //{
            //    File.WriteAllBytes(table, Resources.table);
            //}
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
            string str = "";
            ProcessStartInfo startInfo = new ProcessStartInfo()
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

            using (Process process = Process.Start(startInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    str = reader.ReadToEnd();
                }
                process.WaitForExit();
            }
            return str.Trim();
        }

    }
}
