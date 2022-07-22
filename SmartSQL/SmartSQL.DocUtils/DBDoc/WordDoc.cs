using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZetaLongPaths;
using SmartSQL.DocUtils.Dtos;
using SmartSQL.DocUtils.Properties;

namespace SmartSQL.DocUtils.DBDoc
{
    /// <summary>
    /// 生成Word文档
    /// </summary>
    public class WordDoc : Doc
    {
        public WordDoc(DBDto dto, string filter = "docx files (*.docx)|*.docx") : base(dto, filter)
        {
        }

        public override bool Build(string filePath)
        {
            var docTpl = Resources.doc;
            var docContent = docTpl.RazorRender(this.Dto);
            ZlpIOHelper.WriteAllText(filePath, docContent, Encoding.UTF8);
            return true;
        }
    }
}
