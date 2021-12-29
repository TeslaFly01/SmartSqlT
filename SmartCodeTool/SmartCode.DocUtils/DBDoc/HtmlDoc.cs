using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartCode.DocUtils;
using ZetaLongPaths;
using SmartCode.DocUtils.Dtos;

namespace SmartCode.DocUtils.DBDoc
{
    public class HtmlDoc : Doc
    {
        public HtmlDoc(DBDto dto, string filter = "html files (*.html)|*.html") : base(dto, filter)
        {
        }

        public override void Build(string filePath)
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TplFile\\html");
            var html_tpl = File.ReadAllText(Path.Combine(dir, "html.cshtml"), Encoding.UTF8);
            var html_doc = html_tpl.RazorRender(this.Dto);
            ZlpIOHelper.WriteAllText(filePath, html_doc, Encoding.UTF8);
        }
    }
}
