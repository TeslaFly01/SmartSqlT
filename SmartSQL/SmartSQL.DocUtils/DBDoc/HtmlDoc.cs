using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSQL.DocUtils;
using ZetaLongPaths;
using SmartSQL.DocUtils.Dtos;
using SmartSQL.DocUtils.Properties;

namespace SmartSQL.DocUtils.DBDoc
{
    public class HtmlDoc : Doc
    {
        public HtmlDoc(DBDto dto, string filter = "html files (*.html)|*.html") : base(dto, filter)
        {
        }

        public override bool Build(string filePath)
        {
            return BuildDoc(filePath);
        }

        private bool BuildDoc(string filePath)
        {
            var htmlPath = Path.Combine(TplPath, "html");
            if (!Directory.Exists(htmlPath))
            {
                Directory.CreateDirectory(htmlPath);
            }
            var html = Path.Combine(htmlPath, "html.cshtml");
            if (!File.Exists(html))
            {
                File.WriteAllBytes(html, Resources.html);
            }
            var html_tpl = File.ReadAllText(html, Encoding.UTF8);
            var html_doc = html_tpl.RazorRender(this.Dto);
            ZlpIOHelper.WriteAllText(filePath, html_doc, Encoding.UTF8);
            return true;
        }
    }
}
