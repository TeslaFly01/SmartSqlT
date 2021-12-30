using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using SmartCode.DocUtils.Dtos;
using ZetaLongPaths;

namespace SmartCode.DocUtils.DBDoc
{
    public class ChmDoc : Doc
    {
        public ChmDoc(DBDto dto, string filter = "chm files (*.chm)|*.chm") : base(dto, filter)
        {

        }

        private Encoding CurrEncoding
        {
            get
            {
                return Encoding.GetEncoding("gbk");
            }
        }

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

        void InitDirFiles()
        {
            var dirNames = new string[] {
                "表结构",
                "视图",
                "存储过程",
                //"函数", 
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


            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TplFile\\chm\\");

            var files = Directory.GetFiles(dir, "*.js", SearchOption.AllDirectories);

            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                ZlpIOHelper.CopyFile(filePath, Path.Combine(this.WorkTmpDir, "resources\\js\\", fileName), true);
            }
        }

        public override void Build(string filePath)
        {
            #region 使用 HTML Help Workshop 的 hhc.exe 编译 ,先判断系统中是否已经安装有  HTML Help Workshop 

            if (this.HHCPath.IsNullOrWhiteSpace())
            {
                string htmlhelpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "htmlhelp.exe");

                if (File.Exists(htmlhelpPath))
                {
                    var result = new HandyControl.Data.MessageBoxInfo
                    {
                        Message = "导出CHM文档需安装 HTML Help Workshop ，是否现在安装？",
                        Caption = "提示",
                        Button = MessageBoxButton.OKCancel,
                        Icon = Geometry.Empty,
                    };
                    if (result.DefaultResult == MessageBoxResult.OK)
                    {
                        var proc = Process.Start(htmlhelpPath);
                    }
                }
                return;
            }

            #endregion

            this.InitDirFiles();

            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TplFile\\chm");

            var hhc_tpl = File.ReadAllText(Path.Combine(dir, "hhc.cshtml"), CurrEncoding);
            var hhk_tpl = File.ReadAllText(Path.Combine(dir, "hhk.cshtml"), CurrEncoding);
            var hhp_tpl = File.ReadAllText(Path.Combine(dir, "hhp.cshtml"), CurrEncoding);
            var list_tpl = File.ReadAllText(Path.Combine(dir, "list.cshtml"), CurrEncoding);
            var table_tpl = File.ReadAllText(Path.Combine(dir, "table.cshtml"), CurrEncoding);
            var sqlcode_tpl = File.ReadAllText(Path.Combine(dir, "sqlcode.cshtml"), CurrEncoding);

            var hhc = hhc_tpl.RazorRender(this.Dto).Replace("</LI>", "");

            var hhk = hhk_tpl.RazorRender(this.Dto).Replace("</LI>", "");

            ZlpIOHelper.WriteAllText(Path.Combine(this.WorkTmpDir, "chm.hhc"), hhc, CurrEncoding);

            ZlpIOHelper.WriteAllText(Path.Combine(this.WorkTmpDir, "chm.hhk"), hhk, CurrEncoding);

            ZlpIOHelper.WriteAllText(Path.Combine(this.WorkTmpDir, "数据库目录.html"), list_tpl.RazorRender(this.Dto), CurrEncoding);

            foreach (var tab in this.Dto.Tables)
            {
                var tab_path = Path.Combine(this.WorkTmpDir, "表结构", $"{tab.TableName} {tab.Comment}.html");
                var content = table_tpl.RazorRender(tab);
                ZlpIOHelper.WriteAllText(tab_path, content, CurrEncoding);
            }


            foreach (var item in Dto.Views)
            {
                var vw_path = Path.Combine(this.WorkTmpDir, "视图", $"{item.Key}.html");
                var content = sqlcode_tpl.RazorRender(
                     new SqlCode() { DBType = Dto.DBType, CodeName = item.Key, Content = item.Value.Trim() }
                     );
                ZlpIOHelper.WriteAllText(vw_path, content, CurrEncoding);
            }


            foreach (var item in Dto.Procs)
            {
                var proc_path = Path.Combine(this.WorkTmpDir, "存储过程", $"{item.Key}.html");
                var content = sqlcode_tpl.RazorRender(
                    new SqlCode() { DBType = Dto.DBType, CodeName = item.Key, Content = item.Value.Trim() }
                    );
                ZlpIOHelper.WriteAllText(proc_path, content, CurrEncoding);
            }

            var hhp_Path = Path.Combine(this.WorkTmpDir, "chm.hhp");

            ZlpIOHelper.WriteAllText(hhp_Path, hhp_tpl.RazorRender(new ChmHHP(filePath, this.WorkTmpDir)), CurrEncoding);

            string res = StartRun(HHCPath, hhp_Path, Encoding.GetEncoding("gbk"));
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log")))
            {
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log"));
            }
            ZlpIOHelper.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log", "chm.log"), res);
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
