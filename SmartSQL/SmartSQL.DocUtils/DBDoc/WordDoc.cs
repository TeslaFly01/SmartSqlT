using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZetaLongPaths;
using SmartSQL.DocUtils.Dtos;
using Aspose.Words.Tables;
using Aspose.Words;
using Aspose.Words.Drawing;

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
            WordUtils.ExportWordByAsposeWords(filePath, this.Dto);
            return true;
        }
    }

    /// <summary>
    /// Word处理工具类
    /// </summary>
    internal static class WordUtils
    {
        private static string asposeBookmark_prefix = "AsposeBookmark";
        private static string asposeBookmarkLog = "asposeBookmarkLog";
        private static string asposeBookmarkOverview = "asposeBookmarkOverview";
        private static Color headerColor = Color.FromArgb(231, 230, 230);

        /// <summary>
        /// 引用Aspose.Words.dll导出word数据库字典文档
        /// 注意：不依赖微软office办公软件
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="tables"></param>
        public static void ExportWordByAsposeWords(string fileName, DBDto dto)
        {
            var tables = dto.Tables;
            var title = dto.DBName + "数据库设计文档";

            Document doc = new Document();

            // TODO document properties
            doc.BuiltInDocumentProperties.Subject = "设计文档";
            doc.BuiltInDocumentProperties.ContentType = "数据库字典";
            doc.BuiltInDocumentProperties.Title = title;
            doc.BuiltInDocumentProperties.Author = "SmartSQL";
            doc.BuiltInDocumentProperties.Manager = "SmartSQL";
            doc.BuiltInDocumentProperties.Company = "SmartSQL";
            doc.BuiltInDocumentProperties.LastSavedBy = "SmartSQL";
            doc.BuiltInDocumentProperties.Version = 1;
            doc.BuiltInDocumentProperties.RevisionNumber = 1;
            doc.BuiltInDocumentProperties.ContentStatus = "初稿";
            doc.BuiltInDocumentProperties.NameOfApplication = "SmartSQL";
            doc.BuiltInDocumentProperties.LastSavedTime = DateTime.Now;
            doc.BuiltInDocumentProperties.CreatedTime = DateTime.Now;

            // header and footer setting
            HeaderFooter header = new HeaderFooter(doc, HeaderFooterType.HeaderPrimary);
            doc.FirstSection.HeadersFooters.Add(header);
            header.AppendParagraph("SmartSQL数据库文档工具").ParagraphFormat.Alignment = ParagraphAlignment.Right;

            var builder = new DocumentBuilder(doc);

            // 文档标题 - 书签
            CreateBookmark(builder, ParagraphAlignment.Center, OutlineLevel.Level1, "宋体", 19, asposeBookmark_prefix + "0", title);
            builder.ParagraphFormat.OutlineLevel = OutlineLevel.BodyText;

            // 换行
            builder.InsertBreak(BreakType.ParagraphBreak);
            builder.InsertBreak(BreakType.ParagraphBreak);
            builder.InsertBreak(BreakType.ParagraphBreak);

            // 修订日志 - 书签
            CreateBookmark(builder, ParagraphAlignment.Center, OutlineLevel.Level2, "", 13, asposeBookmarkLog, AppConst.LOG_CHAPTER_NAME);
            CreateLogTable(builder);
            builder.InsertBreak(BreakType.PageBreak);

            // 数据库表目录 - 书签
            CreateBookmark(builder, ParagraphAlignment.Center, OutlineLevel.Level2, "", 13, asposeBookmarkOverview, AppConst.TABLE_CHAPTER_NAME);
            CreateOverviewTable(builder, tables);
            builder.InsertBreak(BreakType.PageBreak);

            // 数据库表结构 - 书签
            CreateBookmark(builder, ParagraphAlignment.Center, OutlineLevel.Level2, "", 13, asposeBookmark_prefix + 0, AppConst.TABLE_STRUCTURE_CHAPTER_NAME);

            int i = 0; // 计数器
            // 遍历数据库表集合
            foreach (var table in tables)
            {
                string bookmarkName = table.TableName + " " + (!string.IsNullOrWhiteSpace(table.Comment) ? table.Comment : "");

                // 创建书签
                CreateBookmark(builder, ParagraphAlignment.Left, OutlineLevel.Level3, "", 12, asposeBookmark_prefix + i, table.TableOrder + "." + bookmarkName);

                // 遍历数据库表字段集合
                // 创建表格
                Table asposeTable = builder.StartTable();

                // 清除段落样式
                builder.ParagraphFormat.ClearFormatting();

                #region 表格列设置，列标题，列宽，字体等
                builder.InsertCell();
                asposeTable.Alignment = TableAlignment.Center;
                asposeTable.PreferredWidth = PreferredWidth.FromPercent(120);
                asposeTable.AllowAutoFit = false;
                builder.RowFormat.Height = 22;
                builder.RowFormat.HeightRule = HeightRule.AtLeast;
                builder.CellFormat.Shading.BackgroundPatternColor = headerColor;
                builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
                builder.Font.Size = 9;
                builder.Font.Name = "宋体";
                builder.Font.Bold = true;
                builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(8);
                builder.Write("序号");

                builder.InsertCell();
                builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(20);
                builder.Write("列名");

                builder.InsertCell();
                builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(12);
                builder.Write("数据类型");

                builder.InsertCell();
                builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(8);
                builder.Write("长度");

                builder.InsertCell();
                builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(8);
                builder.Write("主键");

                if (!table.DBType.StartsWith("Oracle"))
                {
                    builder.InsertCell();
                    builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(8);
                    builder.Write("自增");
                }

                builder.InsertCell();
                builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(8);
                builder.Write("允许空");

                builder.InsertCell();
                builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(10);
                builder.Write("默认值");

                builder.InsertCell();
                builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(30);
                builder.Write("列说明");
                builder.EndRow();
                #endregion

                foreach (var column in table.Columns)
                {
                    #region 遍历表格数据行写入
                    builder.CellFormat.Shading.BackgroundPatternColor = Color.White;
                    builder.CellFormat.Width = 100.0;
                    builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
                    builder.RowFormat.Height = 22;
                    builder.RowFormat.HeightRule = HeightRule.AtLeast;
                    builder.InsertCell();
                    builder.Font.Size = 9;
                    builder.Font.Name = "宋体";
                    builder.Font.Bold = false;
                    builder.Write(column.ColumnOrder);      // 序号

                    builder.InsertCell();
                    builder.Write(column.ColumnName);       // 列名

                    builder.InsertCell();
                    builder.Write(column.ColumnTypeName);   // 数据类型

                    builder.InsertCell();
                    builder.Write(column.Length);           // 长度

                    builder.InsertCell();
                    builder.Write(column.IsPK);             // 主键

                    if (!table.DBType.StartsWith("Oracle"))
                    {
                        builder.InsertCell();
                        builder.Write(column.IsIdentity);   // 自增
                    }

                    builder.InsertCell();
                    builder.Write(column.CanNull);          // 是否为空

                    builder.InsertCell();
                    builder.Font.Size = 10;
                    builder.Write(column.DefaultVal);       // 默认值

                    builder.InsertCell();
                    builder.Font.Size = 10;
                    builder.Write(column.Comment);          // 列说明

                    builder.EndRow();
                    #endregion
                }

                // TODO 表格创建完成，结束
                builder.EndTable();

                i++;

                // TODO page breaks
                if (i < tables.Count)
                {
                    builder.InsertBreak(BreakType.PageBreak);
                }
            }

            // 生成页码
            AutoGenPageNum(doc, builder);

            // 添加水印
            //InsertWatermarkText(doc, "SmartSQL");

            doc.Save(fileName);
        }

        /// <summary>
        /// 生成页码
        /// </summary>
        /// <param name="builder"></param>
        public static void AutoGenPageNum(Document doc, DocumentBuilder builder)
        {
            HeaderFooter footer = new HeaderFooter(doc, HeaderFooterType.FooterPrimary);
            doc.FirstSection.HeadersFooters.Add(footer);
            // Add a paragraph with text to the footer.
            footer.AppendParagraph("").ParagraphFormat.Alignment = ParagraphAlignment.Center;

            // We want to insert a field like this: {PAGE} / {NUMPAGES}
            // TODO Go to the primary footer
            builder.MoveToHeaderFooter(HeaderFooterType.FooterPrimary);
            // TODO Add fields for current page number
            builder.InsertField("PAGE");
            // TODO Add any custom text formatter
            builder.Write(" / ");
            // TODO Add field for total page numbers in document
            builder.InsertField("NUMPAGES");
            // Finally update the outer field to recalcaluate the final value. 
            // Doing this will automatically update the inner fields at the same time.
            // field.Update();
        }

        /// <summary>
        /// 创建数据库字典文档修订日志表
        /// </summary>
        /// <param name="builder"></param>
        private static void CreateLogTable(DocumentBuilder builder)
        {
            // 清除段落样式
            builder.ParagraphFormat.ClearFormatting();

            // 创建表格
            Table logTable = builder.StartTable();

            #region 表格列设置，列标题，列宽，字体等
            // Make the header row.
            builder.InsertCell();
            logTable.Alignment = TableAlignment.Center;
            logTable.AllowAutoFit = true;
            builder.RowFormat.Height = 22;
            builder.RowFormat.HeightRule = HeightRule.AtLeast;
            builder.CellFormat.Shading.BackgroundPatternColor = headerColor;
            builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            builder.Font.Size = 9;
            builder.Font.Name = "宋体";
            builder.Font.Bold = true;
            builder.CellFormat.Width = 150.0;
            builder.Write("版本号");

            builder.InsertCell();
            builder.Write("修订日期");

            builder.InsertCell();
            builder.Write("修订内容");

            builder.InsertCell();
            builder.Write("修订人");

            builder.InsertCell();
            builder.Write("审核人");

            builder.EndRow();
            #endregion

            for (var i = 0; i < 5; i++)
            {
                #region 遍历表格数据行写入
                // Set features for the other rows and cells.
                builder.CellFormat.Shading.BackgroundPatternColor = Color.White;
                builder.CellFormat.Width = 150.0;
                builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
                // Reset height and define a different height rule for table body
                builder.RowFormat.Height = 22;
                builder.InsertCell();
                // Reset font formatting.
                builder.Font.Size = 12;
                builder.Font.Bold = false;
                builder.Write(""); // 版本号

                builder.InsertCell();
                builder.Write(""); // 修订日期

                builder.InsertCell();
                builder.Write(""); // 修订内容

                builder.InsertCell();
                builder.Write(""); // 修订人

                builder.InsertCell();
                builder.Write(""); // 审核人

                builder.EndRow();
                #endregion
            }
            // 表格创建完成，结束
            builder.EndTable();
        }

        /// <summary>
        /// 创建数据库字典目录
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="tables"></param>
        private static void CreateOverviewTable(DocumentBuilder builder, List<TableDto> tables)
        {
            // 清除段落样式
            builder.ParagraphFormat.ClearFormatting();

            // 创建表格
            Table overviewTable = builder.StartTable();

            #region 表格列设置，列标题，列宽，字体等
            builder.InsertCell();
            overviewTable.Alignment = TableAlignment.Center;
            overviewTable.AllowAutoFit = false;
            overviewTable.PreferredWidth = PreferredWidth.FromPercent(120);
            builder.RowFormat.Height = 22;
            builder.RowFormat.HeightRule = HeightRule.AtLeast;
            builder.CellFormat.Shading.BackgroundPatternColor = headerColor;
            builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            builder.Font.Size = 9;
            builder.Font.Name = "宋体";
            builder.Font.Bold = true;
            builder.CellFormat.Width = 50;
            builder.Write("序号");

            builder.InsertCell();
            builder.CellFormat.Width = 150;
            builder.Write("表名");

            builder.InsertCell();
            builder.CellFormat.Width = 150;
            builder.Write("注释/说明");

            builder.EndRow();
            #endregion

            // 遍历数据库表集合
            foreach (var table in tables)
            {
                #region 遍历表格数据行写入
                builder.RowFormat.Height = 22;

                builder.InsertCell();
                builder.CellFormat.ClearFormatting();
                builder.CellFormat.Shading.BackgroundPatternColor = Color.White;
                builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
                builder.CellFormat.Width = 50;
                builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;

                builder.Font.Name = "宋体";
                builder.Font.Size = 9;
                builder.Font.Bold = false;
                builder.Write(table.TableOrder); // 序号

                builder.InsertCell();
                //清除以前操作中的单元格格式。
                builder.CellFormat.ClearFormatting();
                builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
                builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
                builder.CellFormat.Width = 150;
                builder.Write(table.TableName); // 表名

                builder.InsertCell();
                builder.CellFormat.ClearFormatting();
                builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
                builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
                builder.CellFormat.Width = 150;
                builder.Write(!string.IsNullOrWhiteSpace(table.Comment) ? table.Comment : ""); // 说明
                #endregion

                builder.EndRow();
            }
            // 表格创建完成，结束
            builder.EndTable();
        }

        /// <summary>
        /// 创建书签
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="alignment"></param>
        /// <param name="outlineLevel"></param>
        /// <param name="fontName"></param>
        /// <param name="fontSize"></param>
        /// <param name="bookmarkName"></param>
        /// <param name="bookmarkText"></param>
        private static void CreateBookmark(DocumentBuilder builder, ParagraphAlignment alignment,
            OutlineLevel outlineLevel, string fontName, double fontSize, string bookmarkName, string bookmarkText)
        {
            if (string.IsNullOrEmpty(fontName))
            {
                fontName = "Arial";
            }
            // 清除段落样式
            builder.ParagraphFormat.ClearFormatting();

            // TODO 创建书签
            builder.StartBookmark(bookmarkName);
            builder.ParagraphFormat.Alignment = alignment;
            builder.ParagraphFormat.OutlineLevel = outlineLevel;
            builder.ParagraphFormat.SpaceBefore = builder.ParagraphFormat.SpaceAfter = 15;
            builder.Font.Size = fontSize;
            builder.Font.Name = fontName;
            builder.Font.Bold = true;
            builder.Writeln(bookmarkText);
            builder.EndBookmark(bookmarkName);
        }

        /// <summary>
        /// Inserts a watermark into a document.
        /// </summary>
        /// <param name="doc">The input document.</param>
        /// <param name="watermarkText">Text of the watermark.</param>
        public static void InsertWatermarkText(Document doc, string watermarkText)
        {
            Shape watermark = new Shape(doc, ShapeType.TextPlainText);
            watermark.TextPath.Text = watermarkText;
            watermark.TextPath.FontFamily = "Arial";
            watermark.Width = 500;
            watermark.Height = 100;
            watermark.Rotation = -40;
            watermark.Fill.Color = Color.Gray;
            watermark.StrokeColor = Color.Gray;
            watermark.RelativeHorizontalPosition = RelativeHorizontalPosition.Page;
            watermark.RelativeVerticalPosition = RelativeVerticalPosition.Page;
            watermark.WrapType = WrapType.None;
            watermark.VerticalAlignment = VerticalAlignment.Center;
            watermark.HorizontalAlignment = HorizontalAlignment.Center;
            Paragraph watermarkPara = new Paragraph(doc);
            watermarkPara.AppendChild(watermark);
            foreach (Section sect in doc.Sections)
            {
                InsertWatermarkIntoHeader(watermarkPara, sect, HeaderFooterType.HeaderPrimary);
                InsertWatermarkIntoHeader(watermarkPara, sect, HeaderFooterType.HeaderFirst);
                InsertWatermarkIntoHeader(watermarkPara, sect, HeaderFooterType.HeaderEven);
            }
        }

        /// <summary>
        /// Inserts a watermark into a document header.
        /// </summary>
        /// <param name="watermarkPara"></param>
        /// <param name="sect"></param>
        /// <param name="headerType"></param>
        public static void InsertWatermarkIntoHeader(Paragraph watermarkPara, Section sect, HeaderFooterType headerType)
        {
            HeaderFooter header = sect.HeadersFooters[headerType];
            if (null == header)
            {
                // There is no header of the specified type in the current section, create it.
                header = new HeaderFooter(sect.Document, headerType);
                sect.HeadersFooters.Add(header);
            }
            // Insert a clone of the watermark into the header.
            header.AppendChild(watermarkPara.Clone(true));
        }

    }
}
