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

        public override void Build(string filePath)
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
                new System.IO.FileStream(fileName, System.IO.FileMode.Create));
            pdfDocument.Open(); // 打开文档

            //  标题
            Paragraph title = new Paragraph("数据库字典文档\n\n", BaseFont(fontPath, 30, Font.BOLD));
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
            CreateOverviewTable(pdfDocument, pdfFont, tables);
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
                pdfTable.AddCell(CreatePdfPCell("序号", pdfFont));
                pdfTable.AddCell(CreatePdfPCell("列名", pdfFont));
                pdfTable.AddCell(CreatePdfPCell("数据类型", pdfFont));
                pdfTable.AddCell(CreatePdfPCell("长度", pdfFont));
                pdfTable.AddCell(CreatePdfPCell("主键", pdfFont));

                if (!table.DBType.StartsWith("Oracle"))
                {
                    pdfTable.AddCell(CreatePdfPCell("自增", pdfFont));
                }

                pdfTable.AddCell(CreatePdfPCell("允许空", pdfFont));
                pdfTable.AddCell(CreatePdfPCell("默认值", pdfFont));
                pdfTable.AddCell(CreatePdfPCell("列说明", pdfFont));
                //  添加数据行,循环数据库表字段
                foreach (var column in table.Columns)
                {
                    pdfTable.AddCell(CreatePdfPCell(column.ColumnOrder, pdfFont));
                    pdfTable.AddCell(CreatePdfPCell(column.ColumnName, pdfFont));
                    pdfTable.AddCell(CreatePdfPCell(column.ColumnTypeName, pdfFont));
                    pdfTable.AddCell(CreatePdfPCell(column.Length, pdfFont));
                    pdfTable.AddCell(CreatePdfPCell(column.IsPK, pdfFont));

                    if (!table.DBType.StartsWith("Oracle"))
                    {
                        pdfTable.AddCell(CreatePdfPCell(column.IsIdentity, pdfFont));
                    }

                    pdfTable.AddCell(CreatePdfPCell(column.CanNull, pdfFont));
                    pdfTable.AddCell(CreatePdfPCell(column.DefaultVal, pdfFont));
                    pdfTable.AddCell(CreatePdfPCell(column.Comment, pdfFont));
                }

                //  设置表格居中
                pdfTable.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfTable.TotalWidth = 520F;
                pdfTable.LockedWidth = true;

                if (!table.DBType.StartsWith("Oracle"))
                {
                    pdfTable.SetWidths(new float[] { 50F, 60F, 60F, 50F, 50F, 50F, 50F, 50F, 50F });
                }
                else
                {
                    pdfTable.SetWidths(new float[] { 50F, 80F, 70F, 50F, 50F, 50F, 50F, 70F });
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

            //  添加列标题
            pdfTable.AddCell(CreatePdfPCell("版本号", pdfFont));
            pdfTable.AddCell(CreatePdfPCell("修订日期", pdfFont));
            pdfTable.AddCell(CreatePdfPCell("修订内容", pdfFont));
            pdfTable.AddCell(CreatePdfPCell("修订人", pdfFont));
            pdfTable.AddCell(CreatePdfPCell("审核人", pdfFont));
            for (var i = 0; i < 16; i++)
            {
                //  添加数据行,循环数据库表字段
                pdfTable.AddCell(CreatePdfPCell("", pdfFont));
                pdfTable.AddCell(CreatePdfPCell("", pdfFont));
                pdfTable.AddCell(CreatePdfPCell("", pdfFont));
                pdfTable.AddCell(CreatePdfPCell("", pdfFont));
                pdfTable.AddCell(CreatePdfPCell("", pdfFont));
            }

            //  设置表格居中
            pdfTable.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfTable.TotalWidth = 540F;
            pdfTable.LockedWidth = true;
            pdfTable.SetWidths(new float[] { 80F, 100F, 200F, 80F, 80F });

            //  添加表格
            pdfDocument.Add(pdfTable);
        }

        /// <summary>
        /// create overview table
        /// </summary>
        /// <param name="pdfDocument"></param>
        /// <param name="pdfFont"></param>
        /// <param name="tables"></param>
        private static void CreateOverviewTable(Document pdfDocument, Font pdfFont, List<TableDto> tables)
        {
            //  创建表格
            PdfPTable pdfTable = new PdfPTable(3);

            //  添加列标题
            pdfTable.AddCell(CreatePdfPCell("序号", pdfFont));
            pdfTable.AddCell(CreatePdfPCell("表名", pdfFont));
            pdfTable.AddCell(CreatePdfPCell("注释/说明", pdfFont));
            foreach (var table in tables)
            {
                //  添加数据行,循环数据库表字段
                pdfTable.AddCell(CreatePdfPCell(table.TableOrder, pdfFont));
                pdfTable.AddCell(CreatePdfPCell(table.TableName, pdfFont));
                pdfTable.AddCell(CreatePdfPCell((!string.IsNullOrWhiteSpace(table.Comment) ? table.Comment : ""), pdfFont));
            }

            //  设置表格居中
            pdfTable.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfTable.TotalWidth = 330F;
            pdfTable.LockedWidth = true;
            pdfTable.SetWidths(new float[] { 60F, 120F, 150F });

            //  添加表格
            pdfDocument.Add(pdfTable);
        }

        /// <summary>
        /// 创建pdf表格单元格
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pdfFont"></param>
        /// <returns></returns>
        private static PdfPCell CreatePdfPCell(string text, Font pdfFont)
        {
            Phrase phrase = new Phrase(text, pdfFont);
            PdfPCell pdfPCell = new PdfPCell(phrase);

            //  单元格垂直居中显示
            pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;

            pdfPCell.MinimumHeight = 30;

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
