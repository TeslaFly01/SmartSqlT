using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSQL.DocUtils.DBDoc;
using SmartSQL.DocUtils.Dtos;

namespace SmartSQL.DocUtils
{
    public class DocFactory
    {
        public static Doc CreateInstance(DocType type, DBDto dto)
        {
            switch (type)
            {
                case DocType.chm:
                    return new ChmDoc(dto);
                case DocType.html:
                    return new HtmlDoc(dto);
                case DocType.word:
                    return new WordDoc(dto);
                case DocType.excel:
                    return new ExcelDoc(dto);
                case DocType.pdf:
                    return new PdfDoc(dto);
                case DocType.markdown:
                    return new MarkDownDoc(dto);
                case DocType.xml:
                    return new XmlDoc(dto);
                default:
                    return new ChmDoc(dto);
            }
        }
    }
}
