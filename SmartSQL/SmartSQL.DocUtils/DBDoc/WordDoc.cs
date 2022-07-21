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

        public override bool Build(string filePath)
        {
            return BuildDoc(filePath);
        }

        private bool BuildDoc(string filePath)
        {
            //var docPath = Path.Combine(TplPath, "doc");
            //if (!Directory.Exists(docPath))
            //{
            //    Directory.CreateDirectory(docPath);
            //}
            //var doc = Path.Combine(docPath, "doc.xml");
            //if (!File.Exists(doc))
            //{
            //    File.WriteAllText(doc, Resources.doc);
            //}
            var docTpl = File.ReadAllText(Resources.doc, Encoding.UTF8);
            var docContent = docTpl.RazorRender(this.Dto);
            using (var sfs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (var sw = new StreamWriter(sfs))
                {
                    sw.Write(docContent);
                }
            }
            ZlpIOHelper.WriteAllText(filePath, docContent, Encoding.UTF8);
            return true;
        }
    }
}
