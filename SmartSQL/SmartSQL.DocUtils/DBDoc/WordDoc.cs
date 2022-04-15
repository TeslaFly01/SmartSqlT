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
    public class WordDoc : Doc
    {
        public WordDoc(DBDto dto, string filter = "docx files (*.docx)|*.docx") : base(dto, filter)
        {
        }

        public override void Build(string filePath)
        {
            var docPath = Path.Combine(TplPath, "doc");
            if (!Directory.Exists(docPath))
            {
                Directory.CreateDirectory(docPath);
            }
            var doc = Path.Combine(docPath, "doc.xml");
            if (!File.Exists(doc))
            {
                File.WriteAllBytes(doc, Resources.doc);
            }
            var docTpl = File.ReadAllText(doc, Encoding.UTF8);
            var docResult = docTpl.RazorRender(this.Dto);
            using (var sfs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (var sw = new StreamWriter(sfs))
                {
                    sw.Write(docResult);
                }
            }
            ZlpIOHelper.WriteAllText(filePath, docResult, Encoding.UTF8);
        }
    }
}
