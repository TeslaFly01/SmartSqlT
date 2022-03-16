using System.Collections.Generic;
using Aspose.Words.Tables;
using SmartCode.DocUtils.Dtos;

namespace SmartCode.DocUtils.DBDoc
{
    public class WordDoc : Doc
    {
        public WordDoc(DBDto dto, string filter = "word files (.doc)|*.doc") : base(dto, filter)
        {
        }

        public override void Build(string filePath)
        {
            WordUtils.ExportWordByAsposeWords(filePath, this.Dto);
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

        /// <summary>
        /// 引用Aspose.Words.dll导出word数据库字典文档
        /// 注意：不依赖微软office办公软件
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="tables"></param>
        public static void ExportWordByAsposeWords(string fileName, DBDto dto)
        {
            var databaseName = dto.DBName;
            var tables = dto.Tables;

            Aspose.Words.Document doc = new Aspose.Words.Document();

            // TODO document properties
            doc.BuiltInDocumentProperties.Subject = "设计文档";
            doc.BuiltInDocumentProperties.ContentType = "数据库字典";
            doc.BuiltInDocumentProperties.Title = "数据库字典文档";
            doc.BuiltInDocumentProperties.Author = doc.BuiltInDocumentProperties.LastSavedBy = doc.BuiltInDocumentProperties.Manager = "zfluok";
            doc.BuiltInDocumentProperties.Company = "samrtsql.com";
            doc.BuiltInDocumentProperties.Version = doc.BuiltInDocumentProperties.RevisionNumber = 1;
            doc.BuiltInDocumentProperties.ContentStatus = "初稿";
            doc.BuiltInDocumentProperties.NameOfApplication = "SamrtSQL";
            doc.BuiltInDocumentProperties.LastSavedTime = doc.BuiltInDocumentProperties.CreatedTime = System.DateTime.Now;

            // TODO header and footer setting
            Aspose.Words.HeaderFooter header = new Aspose.Words.HeaderFooter(doc, Aspose.Words.HeaderFooterType.HeaderPrimary);
            doc.FirstSection.HeadersFooters.Add(header);
            // Add a paragraph with text to the header.
            header.AppendParagraph($"{databaseName}数据库字典文档").ParagraphFormat.Alignment =
                Aspose.Words.ParagraphAlignment.Right;

            Aspose.Words.DocumentBuilder builder = new Aspose.Words.DocumentBuilder(doc);

            // TODO 创建文档标题书签
            CreateBookmark(builder, Aspose.Words.ParagraphAlignment.Center, Aspose.Words.OutlineLevel.Level1, 25,
                asposeBookmark_prefix + "0", "数据库字典文档");
            builder.ParagraphFormat.OutlineLevel = Aspose.Words.OutlineLevel.BodyText;
            builder.Writeln("—— " + databaseName);

            // TODO 换行
            builder.InsertBreak(Aspose.Words.BreakType.ParagraphBreak);
            builder.InsertBreak(Aspose.Words.BreakType.ParagraphBreak);
            builder.InsertBreak(Aspose.Words.BreakType.ParagraphBreak);

            // TODO 数据库字典文档修订日志表
            CreateBookmark(builder, Aspose.Words.ParagraphAlignment.Center, Aspose.Words.OutlineLevel.Level2, 16,
                asposeBookmarkLog, AppConst.LOG_CHAPTER_NAME);
            CreateLogTable(builder);
            builder.InsertBreak(Aspose.Words.BreakType.PageBreak);

            // TODO 创建数据库字典文档数据库概况一览表
            CreateBookmark(builder, Aspose.Words.ParagraphAlignment.Center, Aspose.Words.OutlineLevel.Level2, 16,
                asposeBookmarkOverview, AppConst.TABLE_CHAPTER_NAME);
            CreateOverviewTable(builder, tables);
            builder.InsertBreak(Aspose.Words.BreakType.PageBreak);

            // TODO 创建书签
            CreateBookmark(builder, Aspose.Words.ParagraphAlignment.Left, Aspose.Words.OutlineLevel.Level2, 16,
                asposeBookmark_prefix + 0, AppConst.TABLE_STRUCTURE_CHAPTER_NAME);

            int i = 0; // 计数器
            // TODO 遍历数据库表集合
            foreach (var table in tables)
            {
                string bookmarkName = table.TableName + " " + (!string.IsNullOrWhiteSpace(table.Comment) ? table.Comment : "");

                // TODO 创建书签
                CreateBookmark(builder, Aspose.Words.ParagraphAlignment.Left, Aspose.Words.OutlineLevel.Level3, 16,
                    asposeBookmark_prefix + i, table.TableOrder + "、" + bookmarkName);

                // TODO 遍历数据库表字段集合
                // TODO 创建表格
                Aspose.Words.Tables.Table asposeTable = builder.StartTable();

                // 清除段落样式
                builder.ParagraphFormat.ClearFormatting();

                #region 表格列设置，列标题，列宽，字体等
                // Make the header row.
                builder.InsertCell();
                // Set the left indent for the table. Table wide formatting must be applied after 
                // at least one row is present in the table.
                asposeTable.Alignment = Aspose.Words.Tables.TableAlignment.Center;
                asposeTable.PreferredWidth = PreferredWidth.FromPercent(120);
                asposeTable.AllowAutoFit = false;
                // Set height and define the height rule for the header row.
                builder.RowFormat.Height = 40.0;
                builder.RowFormat.HeightRule = Aspose.Words.HeightRule.AtLeast;
                // Some special features for the header row.
                builder.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.FromArgb(198, 217, 241);
                builder.ParagraphFormat.Alignment = Aspose.Words.ParagraphAlignment.Center;
                builder.Font.Size = 14;
                builder.Font.Name = "Arial";
                builder.Font.Bold = true;
                builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(8);
                builder.Write("序号");

                // We don't need to specify the width of this cell because it's inherited from the previous cell.
                builder.InsertCell();
                builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(20);
                builder.Write("列名");

                builder.InsertCell();
                builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(12);
                builder.Write("数据类型");

                builder.InsertCell();
                builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(8);
                builder.Write("长度");

                //builder.InsertCell();
                //builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(8);
                //builder.Write("小数位");

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
                    // Set features for the other rows and cells.
                    builder.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.White;
                    builder.CellFormat.Width = 100.0;
                    builder.CellFormat.VerticalAlignment = Aspose.Words.Tables.CellVerticalAlignment.Center;
                    //builder.CellFormat.FitText = true;
                    // Reset height and define a different height rule for table body
                    builder.RowFormat.Height = 60.0;
                    builder.RowFormat.HeightRule = Aspose.Words.HeightRule.AtLeast;
                    builder.InsertCell();
                    // Reset font formatting.
                    builder.Font.Size = 12;
                    builder.Font.Bold = false;
                    builder.Write(column.ColumnOrder); // 序号

                    builder.InsertCell();
                    builder.Write(column.ColumnName); // 列名

                    builder.InsertCell();
                    builder.Write(column.ColumnTypeName); // 数据类型

                    builder.InsertCell();
                    builder.Write(column.Length); // 长度

                    //builder.InsertCell();
                    //builder.Write(column.Scale); // 小数位

                    builder.InsertCell();
                    builder.Write(column.IsPK); // 主键

                    if (!table.DBType.StartsWith("Oracle"))
                    {
                        builder.InsertCell();
                        builder.Write(column.IsIdentity); // 自增
                    }

                    builder.InsertCell();
                    builder.Write(column.CanNull); // 是否为空

                    builder.InsertCell();
                    builder.Font.Size = 10;
                    builder.Write(column.DefaultVal); // 默认值

                    builder.InsertCell();
                    builder.Font.Size = 10;
                    builder.Write(column.Comment); // 列说明

                    builder.EndRow();
                    #endregion
                }

                // TODO 表格创建完成，结束
                //asposeTable.PreferredWidth = Aspose.Words.Tables.PreferredWidth.Auto;
                //asposeTable.AutoFit(Aspose.Words.Tables.AutoFitBehavior.AutoFitToContents);
                builder.EndTable();

                i++;

                // TODO page breaks
                if (i < tables.Count)
                {
                    builder.InsertBreak(Aspose.Words.BreakType.PageBreak);
                }
            }

            // TODO 生成页码
            AutoGenPageNum(doc, builder);

            // TODO 添加水印
            //InsertWatermarkText(doc, "SmartSQL");

            doc.Save(fileName);
        }

        /// <summary>
        /// 生成页码
        /// </summary>
        /// <param name="builder"></param>
        public static void AutoGenPageNum(Aspose.Words.Document doc, Aspose.Words.DocumentBuilder builder)
        {
            Aspose.Words.HeaderFooter footer = new Aspose.Words.HeaderFooter(doc, Aspose.Words.HeaderFooterType.FooterPrimary);
            doc.FirstSection.HeadersFooters.Add(footer);
            // Add a paragraph with text to the footer.
            footer.AppendParagraph("").ParagraphFormat.Alignment = Aspose.Words.ParagraphAlignment.Center;
            // We want to insert a field like this: {PAGE} / {NUMPAGES}
            // TODO Go to the primary footer
            builder.MoveToHeaderFooter(Aspose.Words.HeaderFooterType.FooterPrimary);
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
        private static void CreateLogTable(Aspose.Words.DocumentBuilder builder)
        {
            // 清除段落样式
            builder.ParagraphFormat.ClearFormatting();

            // TODO 创建表格
            Aspose.Words.Tables.Table logTable = builder.StartTable();

            #region 表格列设置，列标题，列宽，字体等
            // Make the header row.
            builder.InsertCell();
            // Set the left indent for the table. Table wide formatting must be applied after 
            // at least one row is present in the table.
            logTable.Alignment = Aspose.Words.Tables.TableAlignment.Center;
            logTable.AllowAutoFit = true;
            // Set height and define the height rule for the header row.
            builder.RowFormat.Height = 40.0;
            builder.RowFormat.HeightRule = Aspose.Words.HeightRule.AtLeast;
            // Some special features for the header row.
            builder.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.FromArgb(198, 217, 241);
            builder.CellFormat.VerticalAlignment = Aspose.Words.Tables.CellVerticalAlignment.Center;
            builder.ParagraphFormat.Alignment = Aspose.Words.ParagraphAlignment.Center;
            builder.Font.Size = 14;
            builder.Font.Name = "Arial";
            builder.Font.Bold = true;
            builder.CellFormat.Width = 100.0;
            builder.Write("版本号");

            // We don't need to specify the width of this cell because it's inherited from the previous cell.
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
                builder.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.White;
                builder.CellFormat.Width = 100.0;
                builder.CellFormat.VerticalAlignment = Aspose.Words.Tables.CellVerticalAlignment.Center;
                // Reset height and define a different height rule for table body
                builder.RowFormat.Height = 40.0;
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
            // TODO 表格创建完成，结束
            builder.EndTable();
        }

        /// <summary>
        /// 创建数据库字典文档数据库概况一览表
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="tables"></param>
        private static void CreateOverviewTable(Aspose.Words.DocumentBuilder builder, List<TableDto> tables)
        {
            // 清除段落样式
            builder.ParagraphFormat.ClearFormatting();

            // TODO 创建表格
            Aspose.Words.Tables.Table overviewTable = builder.StartTable();

            #region 表格列设置，列标题，列宽，字体等
            // Make the header row.
            builder.InsertCell();
            // Set the left indent for the table. Table wide formatting must be applied after 
            // at least one row is present in the table.
            overviewTable.Alignment = Aspose.Words.Tables.TableAlignment.Center;
            overviewTable.AllowAutoFit = true;
            // Set height and define the height rule for the header row.
            builder.RowFormat.Height = 40.0;
            builder.RowFormat.HeightRule = Aspose.Words.HeightRule.AtLeast;
            // Some special features for the header row.
            builder.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.FromArgb(198, 217, 241);
            builder.CellFormat.VerticalAlignment = Aspose.Words.Tables.CellVerticalAlignment.Center;
            builder.ParagraphFormat.Alignment = Aspose.Words.ParagraphAlignment.Center;
            builder.Font.Size = 14;
            builder.Font.Name = "Arial";
            builder.Font.Bold = true;
            builder.CellFormat.Width = 100.0;
            builder.Write("序号");

            builder.InsertCell();
            builder.Write("表名");

            builder.InsertCell();
            builder.Write("注释/说明");

            builder.EndRow();
            #endregion

            // TODO 遍历数据库表集合
            foreach (var table in tables)
            {
                #region 遍历表格数据行写入
                // Set features for the other rows and cells.
                builder.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.White;
                builder.CellFormat.Width = 100.0;
                builder.CellFormat.VerticalAlignment = Aspose.Words.Tables.CellVerticalAlignment.Center;
                // Reset height and define a different height rule for table body
                builder.RowFormat.Height = 40.0;
                builder.InsertCell();
                // Reset font formatting.
                builder.Font.Size = 12;
                builder.Font.Bold = false;
                builder.Write(table.TableOrder); // 序号

                builder.InsertCell();
                builder.Write(table.TableName); // 表名

                builder.InsertCell();
                builder.Write((!string.IsNullOrWhiteSpace(table.Comment) ? table.Comment : "")); // 说明
                #endregion

                builder.EndRow();
            }
            // TODO 表格创建完成，结束
            builder.EndTable();
        }

        /// <summary>
        /// 创建书签
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="alignment"></param>
        /// <param name="outlineLevel"></param>
        /// <param name="fontSize"></param>
        /// <param name="bookmarkName"></param>
        /// <param name="bookmarkText"></param>
        private static void CreateBookmark(Aspose.Words.DocumentBuilder builder, Aspose.Words.ParagraphAlignment alignment,
            Aspose.Words.OutlineLevel outlineLevel, double fontSize, string bookmarkName, string bookmarkText)
        {
            // 清除段落样式
            builder.ParagraphFormat.ClearFormatting();

            // TODO 创建书签
            builder.StartBookmark(bookmarkName);
            builder.ParagraphFormat.Alignment = alignment;
            builder.ParagraphFormat.OutlineLevel = outlineLevel;
            builder.ParagraphFormat.SpaceBefore = builder.ParagraphFormat.SpaceAfter = 15;
            builder.Font.Size = fontSize;
            builder.Font.Name = "Arial";
            builder.Font.Bold = true;
            builder.Writeln(bookmarkText);
            builder.EndBookmark(bookmarkName);
        }

        /// <summary>
        /// Inserts a watermark into a document.
        /// </summary>
        /// <param name="doc">The input document.</param>
        /// <param name="watermarkText">Text of the watermark.</param>
        public static void InsertWatermarkText(Aspose.Words.Document doc, string watermarkText)
        {
            // Create a watermark shape. This will be a WordArt shape. 
            // You are free to try other shape types as watermarks.
            Aspose.Words.Drawing.Shape watermark = new Aspose.Words.Drawing.Shape(doc, Aspose.Words.Drawing.ShapeType.TextPlainText);
            // Set up the text of the watermark.
            watermark.TextPath.Text = watermarkText;
            watermark.TextPath.FontFamily = "Arial";
            watermark.Width = 500;
            watermark.Height = 100;
            // Text will be directed from the bottom-left to the top-right corner.
            watermark.Rotation = -40;
            // Remove the following two lines if you need a solid black text.
            watermark.Fill.Color = System.Drawing.Color.Gray; // Try LightGray to get more Word-style watermark
            watermark.StrokeColor = System.Drawing.Color.Gray; // Try LightGray to get more Word-style watermark
            // Place the watermark in the page center.
            watermark.RelativeHorizontalPosition = Aspose.Words.Drawing.RelativeHorizontalPosition.Page;
            watermark.RelativeVerticalPosition = Aspose.Words.Drawing.RelativeVerticalPosition.Page;
            watermark.WrapType = Aspose.Words.Drawing.WrapType.None;
            watermark.VerticalAlignment = Aspose.Words.Drawing.VerticalAlignment.Center;
            watermark.HorizontalAlignment = Aspose.Words.Drawing.HorizontalAlignment.Center;
            // Create a new paragraph and append the watermark to this paragraph.
            Aspose.Words.Paragraph watermarkPara = new Aspose.Words.Paragraph(doc);
            watermarkPara.AppendChild(watermark);
            // Insert the watermark into all headers of each document section.
            foreach (Aspose.Words.Section sect in doc.Sections)
            {
                // There could be up to three different headers in each section, since we want
                // the watermark to appear on all pages, insert into all headers.
                InsertWatermarkIntoHeader(watermarkPara, sect, Aspose.Words.HeaderFooterType.HeaderPrimary);
                InsertWatermarkIntoHeader(watermarkPara, sect, Aspose.Words.HeaderFooterType.HeaderFirst);
                InsertWatermarkIntoHeader(watermarkPara, sect, Aspose.Words.HeaderFooterType.HeaderEven);
            }
        }

        /// <summary>
        /// Inserts a watermark into a document header.
        /// </summary>
        /// <param name="watermarkPara"></param>
        /// <param name="sect"></param>
        /// <param name="headerType"></param>
        public static void InsertWatermarkIntoHeader(Aspose.Words.Paragraph watermarkPara, Aspose.Words.Section sect, Aspose.Words.HeaderFooterType headerType)
        {
            Aspose.Words.HeaderFooter header = sect.HeadersFooters[headerType];
            if (null == header)
            {
                // There is no header of the specified type in the current section, create it.
                header = new Aspose.Words.HeaderFooter(sect.Document, headerType);
                sect.HeadersFooters.Add(header);
            }
            // Insert a clone of the watermark into the header.
            header.AppendChild(watermarkPara.Clone(true));
        }

    }
}
