using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JinianNet.JNTemplate;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.Properties;

namespace SmartSQL.Framework.Lang
{
    public class CsharpLang : Lang
    {
        public CsharpLang(string tbName, string tbComment, List<Column> columns) : base(tbName, tbComment, columns)
        {

        }

        public override string BuildEntity()
        {
            var excelTpl = Encoding.UTF8.GetString(Resource.Csharp);
            var template = Engine.CreateTemplate(excelTpl);
            template.Set("ClassName", TableName);
            template.Set("ClassComment", TableComment);
            template.Set("FieldList", Columns);
            return template.Render();
        }
    }
}
