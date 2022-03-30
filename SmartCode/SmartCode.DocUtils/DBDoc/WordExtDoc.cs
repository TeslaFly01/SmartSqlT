using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZetaLongPaths;
using SmartCode.DocUtils.Dtos;
using SmartCode.DocUtils.Properties;

namespace SmartCode.DocUtils.DBDoc
{
    public class WordExtDoc : Doc
    {
        public WordExtDoc(DBDto dto, string filter = "docx files (*.docx)|*.docx") : base(dto, filter)
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
            var doc_tpl = File.ReadAllText(doc, Encoding.UTF8);
            var doc_doc = doc_tpl.RazorRender(this.Dto);
            
            using (FileStream sfs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(sfs))
                {
                    sw.Write(doc_doc);
                }
            }

            ZlpIOHelper.WriteAllText(filePath, doc_doc, Encoding.UTF8);
        }
    }
}
