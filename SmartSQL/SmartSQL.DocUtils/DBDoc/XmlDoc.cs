using System.Xml;
using SmartSQL.DocUtils.Dtos;

namespace SmartSQL.DocUtils.DBDoc
{
    /// <summary>
    /// 生成Xml文档
    /// </summary>
    public class XmlDoc : Doc
    {
        public XmlDoc(DBDto dto, string filter = "xml files (.xml)|*.xml") : base(dto, filter)
        {
        }

        public override bool Build(string filePath)
        {
            return BuildDoc(filePath);
        }

        private bool BuildDoc(string filePath)
        {
            int count_total = Dto.Tables.Count + Dto.Views.Count + Dto.Procs.Count;
            // 更新进度
            base.OnProgress(new ChangeRefreshProgressArgs
            {
                Type = DocType.html,
                BuildNum = count_total,
                TotalNum = count_total,
                IsEnd = true
            });
            string xmlContent = this.Dto.SerializeXml();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);

            var root = xmlDoc.DocumentElement;
            root.SetAttribute("databaseName", this.Dto.DBName);
            root.SetAttribute("tableNum", this.Dto.Tables.Count + "");
            xmlDoc.Save(filePath);
            return true;
        }
    }
}
