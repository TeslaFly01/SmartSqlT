using System;
using System.IO;
using System.Linq;
using System.Text;
using SmartSQL.DocUtils;
using SmartSQL.DocUtils.Dtos;
using SmartSQL.DocUtils.Properties;
using ZetaLongPaths;

namespace SmartSQL.DocUtils.DBDoc
{
    /// <summary>
    /// 生成Excel文档
    /// </summary>
    public class ExcelDoc : Doc
    {
        public ExcelDoc(DBDto dto, string filter = "excel files (.xlsx)|*.xlsx") : base(dto, filter)
        {
        }

        public override bool Build(string filePath)
        {
            var excelTpl = Resources.excel;
            var excelContent = excelTpl.RazorRender(this.Dto);
            ZlpIOHelper.WriteAllText(filePath, excelContent, Encoding.UTF8);
            return true;
        }
    }
}
