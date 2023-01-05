using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSQL.DocUtils.Dtos;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SmartSQL.DocUtils.Properties;
using ZetaLongPaths;

namespace SmartSQL.DocUtils.DBDoc
{
    public class PdfDoc : Doc
    {
        public PdfDoc(DBDto dto, string filter = "html files (.pdf)|*.pdf") : base(dto, filter)
        {

        }

        private static string TTF_Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TplFile\\pdf\\msyh.ttf");

        public override bool Build(string filePath)
        {
            return BuildDoc(filePath);
        }

        private bool BuildDoc(string filePath)
        {
            var pdfPath = Path.Combine(TplPath, "pdf");
            if (!Directory.Exists(pdfPath))
            {
                Directory.CreateDirectory(pdfPath);
            }
            var pdf = Path.Combine(pdfPath, "msyh.ttf");
            if (!File.Exists(pdf))
            {
                File.WriteAllBytes(pdf, Resources.msyh);
            }
            PdfUtils.ExportPdfByITextSharp(filePath, pdf, this.Dto);
            return true;
        }
    }

    /// <summary>
    /// Pdf处理工具类
    /// </summary>
    internal static class PdfUtils
    {

        /// <summary>
        /// 引用iTextSharp.dll导出pdf数据库字典文档
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fontPath"></param>
        /// <param name="dto"></param>
        public static void ExportPdfByITextSharp(string fileName, string fontPath, DBDto dto)
        {
            var databaseName = dto.DBName;
            var tables = dto.Tables;
            //  创建并添加文档信息
            Document pdfDocument = new Document();
            pdfDocument.AddTitle(fileName);

            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDocument,
                new FileStream(fileName, FileMode.Create));
            pdfDocument.Open(); // 打开文档

            //  标题
            Paragraph title = new Paragraph($"{dto.DocTitle}\n\n", BaseFont(fontPath, 30, Font.BOLD));
            title.Alignment = Element.ALIGN_CENTER;
            pdfDocument.Add(title);
            Paragraph subTitle = new Paragraph(" —— " + databaseName, BaseFont(fontPath, 20, Font.NORMAL));
            subTitle.Alignment = Element.ALIGN_CENTER;
            pdfDocument.Add(subTitle);

            //  PDF换页
            pdfDocument.NewPage();

            //  创建添加书签章节
            int chapterNum = 1;
            //  全局字体设置，处理iTextSharp中文不识别显示问题
            Font pdfFont = BaseFont(fontPath, 12, Font.NORMAL);
            Font tableHeader = BaseFont(fontPath, 9, Font.BOLD);
            Font tableContent = BaseFont(fontPath, 9, Font.NORMAL);

            var hBackColor = BaseColor.LIGHT_GRAY;
            var cBackColor = BaseColor.WHITE;

            //  log table
            Chapter logChapter = new Chapter(new Paragraph(AppConst.LOG_CHAPTER_NAME, pdfFont), chapterNum);
            pdfDocument.Add(logChapter);
            pdfDocument.Add(new Paragraph("\n", pdfFont)); // 换行
            CreateLogTable(pdfDocument, pdfFont, tables);
            //  PDF换页
            pdfDocument.NewPage();

            //  overview table
            Chapter dirChapter = new Chapter(new Paragraph(AppConst.TABLE_CHAPTER_NAME, pdfFont), (++chapterNum));
            pdfDocument.Add(dirChapter);
            pdfDocument.Add(new Paragraph("\n", pdfFont)); // 换行
            CreateOverviewTable(pdfDocument, tableHeader, tableContent, tables);
            //  PDF换页
            pdfDocument.NewPage();

            //  table structure
            //  添加书签章节
            Chapter tableChapter = new Chapter(new Paragraph(AppConst.TABLE_STRUCTURE_CHAPTER_NAME, pdfFont), (++chapterNum));
            tableChapter.BookmarkOpen = true;
            pdfDocument.Add(tableChapter);
            pdfDocument.Add(new Paragraph("\n", pdfFont)); // 换行

            foreach (var table in tables)
            {
                string docTableName = table.TableName + " " + (!string.IsNullOrWhiteSpace(table.Comment) ? table.Comment : "");
                //  添加书签章节
                Section selection = tableChapter.AddSection(20f, new Paragraph(docTableName, pdfFont), chapterNum);
                pdfDocument.Add(selection);
                pdfDocument.Add(new Paragraph("\n", pdfFont)); // 换行

                //  遍历数据库表
                //  创建表格
                PdfPTable pdfTable = null;
                if (!table.DBType.StartsWith("Oracle"))
                {
                    pdfTable = new PdfPTable(9);
                }
                else
                {
                    pdfTable = new PdfPTable(8);
                }

                //  添加列标题
                pdfTable.AddCell(CreatePdfPCell("序号", tableHeader, hBackColor));
                pdfTable.AddCell(CreatePdfPCell("列名", tableHeader, hBackColor));
                pdfTable.AddCell(CreatePdfPCell("数据类型", tableHeader, hBackColor));
                pdfTable.AddCell(CreatePdfPCell("长度", tableHeader, hBackColor));
                pdfTable.AddCell(CreatePdfPCell("主键", tableHeader, hBackColor));

                if (!table.DBType.StartsWith("Oracle"))
                {
                    pdfTable.AddCell(CreatePdfPCell("自增", tableHeader, hBackColor));
                }

                pdfTable.AddCell(CreatePdfPCell("允许空", tableHeader, hBackColor));
                pdfTable.AddCell(CreatePdfPCell("默认值", tableHeader, hBackColor));
                pdfTable.AddCell(CreatePdfPCell("列说明", tableHeader, hBackColor));
                //  添加数据行,循环数据库表字段
                foreach (var column in table.Columns)
                {
                    pdfTable.AddCell(CreatePdfPCell(column.ColumnOrder, tableContent, cBackColor));
                    pdfTable.AddCell(CreatePdfPCell(column.ColumnName, tableContent, cBackColor, Element.ALIGN_LEFT));
                    pdfTable.AddCell(CreatePdfPCell(column.ColumnTypeName, tableContent, cBackColor));
                    pdfTable.AddCell(CreatePdfPCell(column.Length, tableContent, cBackColor));
                    pdfTable.AddCell(CreatePdfPCell(column.IsPK, tableContent, cBackColor));
                    if (!table.DBType.StartsWith("Oracle"))
                    {
                        pdfTable.AddCell(CreatePdfPCell(column.IsIdentity, tableContent, cBackColor));
                    }
                    pdfTable.AddCell(CreatePdfPCell(column.CanNull, tableContent, cBackColor));
                    pdfTable.AddCell(CreatePdfPCell(column.DefaultVal, tableContent, cBackColor));
                    pdfTable.AddCell(CreatePdfPCell(column.Comment, tableContent, cBackColor, Element.ALIGN_LEFT));
                }

                //  设置表格居中
                pdfTable.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfTable.TotalWidth = 520F;
                pdfTable.LockedWidth = true;

                if (!table.DBType.StartsWith("Oracle"))
                {
                    pdfTable.SetWidths(new float[] { 40F, 90F, 60F, 40F, 40F, 40F, 40F, 70F, 100F });
                }
                else
                {
                    pdfTable.SetWidths(new float[] { 40F, 90F, 70F, 50F, 50F, 50F, 50F, 90F });
                }


                //  添加表格
                pdfDocument.Add(pdfTable);

                //  PDF换页
                pdfDocument.NewPage();
            }


            if (dto.Views.Count > 0)
            {
                Chapter viewChapter = new Chapter(new Paragraph("视图", pdfFont), (++chapterNum));
                viewChapter.BookmarkOpen = true;
                pdfDocument.Add(viewChapter);
                // 换行
                pdfDocument.Add(new Paragraph("\n", pdfFont));

                foreach (var item in dto.Views)
                {
                    Section selection = viewChapter.AddSection(20f, new Paragraph(item.ObjectName, pdfFont), chapterNum);
                    pdfDocument.Add(selection);
                    // 换行
                    pdfDocument.Add(new Paragraph("\n", pdfFont));

                    Paragraph pgh = new Paragraph(item.Script.Replace("`", ""), pdfFont);
                    pdfDocument.Add(pgh);

                    // 换行
                    pdfDocument.Add(new Paragraph("\n", pdfFont));
                }
                pdfDocument.NewPage();
            }


            if (dto.Procs.Count > 0)
            {
                Chapter procChapter = new Chapter(new Paragraph("存储过程", pdfFont), (++chapterNum));
                procChapter.BookmarkOpen = true;
                pdfDocument.Add(procChapter);
                // 换行
                pdfDocument.Add(new Paragraph("\n", pdfFont));

                foreach (var item in dto.Procs)
                {
                    Section selection = procChapter.AddSection(20f, new Paragraph(item.ObjectName, pdfFont), chapterNum);
                    pdfDocument.Add(selection);
                    // 换行
                    pdfDocument.Add(new Paragraph("\n", pdfFont));

                    Paragraph pgh = new Paragraph(item.Script.Replace("`", ""), pdfFont);
                    pdfDocument.Add(pgh);

                    // 换行
                    pdfDocument.Add(new Paragraph("\n", pdfFont));

                }
                pdfDocument.NewPage();
            }

            //  关闭释放PDF文档资源
            pdfDocument.Close();
        }

        /// <summary>
        /// create log table
        /// </summary>
        /// <param name="pdfDocument"></param>
        /// <param name="pdfFont"></param>
        /// <param name="tables"></param>
        private static void CreateLogTable(Document pdfDocument, Font pdfFont, List<TableDto> tables)
        {
            //  创建表格
            PdfPTable pdfTable = new PdfPTable(5);
            var hBackColor = BaseColor.LIGHT_GRAY;
            var cBackColor = BaseColor.WHITE;

            //  添加列标题
            pdfTable.AddCell(CreatePdfPCell("版本号", pdfFont, hBackColor));
            pdfTable.AddCell(CreatePdfPCell("修订日期", pdfFont, hBackColor));
            pdfTable.AddCell(CreatePdfPCell("修订内容", pdfFont, hBackColor));
            pdfTable.AddCell(CreatePdfPCell("修订人", pdfFont, hBackColor));
            pdfTable.AddCell(CreatePdfPCell("审核人", pdfFont, hBackColor));
            for (var i = 0; i < 16; i++)
            {
                //  添加数据行,循环数据库表字段
                pdfTable.AddCell(CreatePdfPCell("", pdfFont, cBackColor));
                pdfTable.AddCell(CreatePdfPCell("", pdfFont, cBackColor));
                pdfTable.AddCell(CreatePdfPCell("", pdfFont, cBackColor));
                pdfTable.AddCell(CreatePdfPCell("", pdfFont, cBackColor));
                pdfTable.AddCell(CreatePdfPCell("", pdfFont, cBackColor));
            }

            //  设置表格居中
            pdfTable.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfTable.TotalWidth = 520F;
            pdfTable.LockedWidth = true;
            pdfTable.SetWidths(new float[] { 80F, 100F, 180F, 80F, 80F });

            //  添加表格
            pdfDocument.Add(pdfTable);
        }

        /// <summary>
        /// create overview table
        /// </summary>
        /// <param name="pdfDocument"></param>
        /// <param name="pdfFont"></param>
        /// <param name="tables"></param>
        private static void CreateOverviewTable(Document pdfDocument, Font headerFont, Font contentFont, List<TableDto> tables)
        {
            //  创建表格
            PdfPTable pdfTable = new PdfPTable(3);
            var headerBackColor = BaseColor.LIGHT_GRAY;
            var contentBackColor = BaseColor.WHITE;

            //  添加列标题
            pdfTable.AddCell(CreatePdfPCell("序号", headerFont, headerBackColor));
            pdfTable.AddCell(CreatePdfPCell("表名", headerFont, headerBackColor));
            pdfTable.AddCell(CreatePdfPCell("说明", headerFont, headerBackColor));
            foreach (var table in tables)
            {
                var comment = !string.IsNullOrWhiteSpace(table.Comment) ? table.Comment : "";
                //  添加数据行,循环数据库表字段
                pdfTable.AddCell(CreatePdfPCell(table.TableOrder, contentFont, contentBackColor));
                pdfTable.AddCell(CreatePdfPCell(table.TableName, contentFont, contentBackColor, Element.ALIGN_LEFT));
                pdfTable.AddCell(CreatePdfPCell(comment, contentFont, contentBackColor, Element.ALIGN_LEFT));
            }

            //  设置表格居中
            pdfTable.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfTable.TotalWidth = 520F;
            pdfTable.LockedWidth = true;
            pdfTable.SetWidths(new float[] { 60F, 230F, 230F });

            //  添加表格
            pdfDocument.Add(pdfTable);
        }

        /// <summary>
        /// 创建pdf表格单元格
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pdfFont"></param>
        /// <returns></returns>
        private static PdfPCell CreatePdfPCell(string text, Font pdfFont, BaseColor backColor, int hAlign = Element.ALIGN_CENTER)
        {
            Phrase phrase = new Phrase(text, pdfFont);
            PdfPCell pdfPCell = new PdfPCell(phrase);

            //  单元格垂直居中显示
            pdfPCell.HorizontalAlignment = hAlign;
            pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            pdfPCell.BackgroundColor = backColor;
            pdfPCell.MinimumHeight = 25;

            return pdfPCell;
        }

        /// <summary>
        /// iTextSharp字体设置
        /// </summary>
        /// <param name="fontPath"></param>
        /// <param name="fontSize"></param>
        /// <param name="fontStyle"></param>
        private static Font BaseFont(string fontPath, float fontSize, int fontStyle)
        {
            BaseFont chinese = iTextSharp.text.pdf.BaseFont.CreateFont(fontPath, iTextSharp.text.pdf.BaseFont.IDENTITY_H, true);
            Font pdfFont = new Font(chinese, fontSize, fontStyle);
            return pdfFont;
        }

    }
}
