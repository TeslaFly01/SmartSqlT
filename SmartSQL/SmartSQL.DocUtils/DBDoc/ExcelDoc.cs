using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSQL.DocUtils;
using SmartSQL.DocUtils.Dtos;

namespace SmartSQL.DocUtils.DBDoc
{
    public class ExcelDoc : Doc
    {
        public ExcelDoc(DBDto dto, string filter = "excel files (.xlsx)|*.xlsx") : base(dto, filter)
        {
        }

        public override bool Build(string filePath)
        {
            ExcelUtils.ExportExcelByEpplus(filePath, this.Dto);
            return true;
        }
    }

    /// <summary>
    /// Excel处理工具类
    /// </summary>
    internal static class ExcelUtils
    {

        /// <summary>
        /// 引用EPPlus.dll导出excel数据库字典文档
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="databaseName"></param>
        /// <param name="tables"></param>
        public static void ExportExcelByEpplus(string fileName, DBDto dto)
        {
            var tables = dto.Tables;

            System.IO.FileInfo xlsFileInfo = new System.IO.FileInfo(fileName);

            if (xlsFileInfo.Exists)
            {
                //  注意此处：存在Excel文档即删除再创建一个
                xlsFileInfo.Delete();
                xlsFileInfo = new System.IO.FileInfo(fileName);
            }
            //  创建并添加Excel文档信息
            using (OfficeOpenXml.ExcelPackage epck = new OfficeOpenXml.ExcelPackage(xlsFileInfo))
            {
                //  创建overview sheet
                CreateLogSheet(epck, AppConst.LOG_CHAPTER_NAME, tables);

                //  创建overview sheet
                CreateOverviewSheet(epck, AppConst.TABLE_CHAPTER_NAME, tables);

                //  创建tables sheet
                CreateTableSheet(epck, AppConst.TABLE_STRUCTURE_CHAPTER_NAME, tables);

                epck.Save(); // 保存excel
                epck.Dispose();
            }
        }

        /// <summary>
        /// 创建修订日志sheet
        /// </summary>
        /// <param name="epck"></param>
        /// <param name="sheetName"></param>
        /// <param name="tables"></param>
        private static void CreateLogSheet(OfficeOpenXml.ExcelPackage epck, string sheetName, List<TableDto> tables)
        {
            OfficeOpenXml.ExcelWorksheet overviewTbWorksheet = epck.Workbook.Worksheets.Add(sheetName);

            int row = 1;

            overviewTbWorksheet.Cells[row, 1, row, 5].Merge = true;
            //overviewTbWorksheet.Cells[row, 1].Value = "总表数量";
            //overviewTbWorksheet.Cells[row, 2].Value = tables.Count + "";
            //overviewTbWorksheet.Cells[row, 4].Value = "密码等级";
            //overviewTbWorksheet.Cells[row, 5].Value = "秘密";

            row++; // 行号+1

            overviewTbWorksheet.Cells[row, 1].Value = "版本号";
            overviewTbWorksheet.Cells[row, 2].Value = "修订日期";
            overviewTbWorksheet.Cells[row, 3].Value = "修订内容";
            overviewTbWorksheet.Cells[row, 4].Value = "修订人";
            overviewTbWorksheet.Cells[row, 5].Value = "审核人";
            overviewTbWorksheet.Cells[row, 1, row, 5].Style.Font.Bold = true;
            overviewTbWorksheet.Cells[row, 1, row, 5].Style.Font.Size = 10;
            overviewTbWorksheet.Row(1).Height = 20; // 行高

            //  循环日志记录
            row++; // 行号+1
            for (var i = 0; i < 16; i++)
            {
                //  添加列标题
                overviewTbWorksheet.Cells[row, 1].Value = "";
                overviewTbWorksheet.Cells[row, 2].Value = "";
                overviewTbWorksheet.Cells[row, 3].Value = "";
                overviewTbWorksheet.Cells[row, 4].Value = "";
                overviewTbWorksheet.Cells[row, 5].Value = "";

                overviewTbWorksheet.Row(row).Height = 20; // 行高

                row++; // 行号+1
            }
            //  水平居中
            overviewTbWorksheet.Cells[1, 1, row - 1, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            //  垂直居中
            overviewTbWorksheet.Cells[1, 1, row - 1, 5].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            //  上下左右边框线
            overviewTbWorksheet.Cells[1, 1, row - 1, 5].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            overviewTbWorksheet.Cells[1, 1, row - 1, 5].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            overviewTbWorksheet.Cells[1, 1, row - 1, 5].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            overviewTbWorksheet.Cells[1, 1, row - 1, 5].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            overviewTbWorksheet.Column(1).Width = 25;
            overviewTbWorksheet.Column(2).Width = 25;
            overviewTbWorksheet.Column(3).Width = 50;
            overviewTbWorksheet.Column(4).Width = 25;
            overviewTbWorksheet.Column(5).Width = 25;
        }

        /// <summary>
        /// 创建表目录sheet
        /// </summary>
        /// <param name="epck"></param>
        /// <param name="sheetName"></param>
        /// <param name="tables"></param>
        private static void CreateOverviewSheet(OfficeOpenXml.ExcelPackage epck, string sheetName, List<TableDto> tables)
        {
            OfficeOpenXml.ExcelWorksheet overviewTbWorksheet = epck.Workbook.Worksheets.Add(sheetName);

            int row = 1;
            overviewTbWorksheet.Cells[row, 1].Value = "序号";
            overviewTbWorksheet.Cells[row, 2].Value = "表名";
            overviewTbWorksheet.Cells[row, 3].Value = "注释/说明";
            overviewTbWorksheet.Cells[row, 1, row, 3].Style.Font.Bold = true;
            overviewTbWorksheet.Cells[row, 1, row, 3].Style.Font.Size = 16;
            overviewTbWorksheet.Row(1).Height = 30; // 行高

            //  循环数据库表名
            row++;
            foreach (var table in tables)
            {
                //  数据库名称
                //  添加列标题
                overviewTbWorksheet.Cells[row, 1].Value = table.TableOrder;
                overviewTbWorksheet.Cells[row, 2].Value = table.TableName;
                overviewTbWorksheet.Cells[row, 3].Value = (!string.IsNullOrWhiteSpace(table.Comment) ? table.Comment : "");

                overviewTbWorksheet.Row(row).Height = 30; // 行高

                row++; // 行号+1
            }
            //  水平居中
            overviewTbWorksheet.Cells[1, 1, row - 1, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            //  垂直居中
            overviewTbWorksheet.Cells[1, 1, row - 1, 3].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            //  上下左右边框线
            overviewTbWorksheet.Cells[1, 1, row - 1, 3].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            overviewTbWorksheet.Cells[1, 1, row - 1, 3].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            overviewTbWorksheet.Cells[1, 1, row - 1, 3].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            overviewTbWorksheet.Cells[1, 1, row - 1, 3].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            overviewTbWorksheet.Column(1).Width = 10;
            overviewTbWorksheet.Column(2).Width = 50;
            overviewTbWorksheet.Column(3).Width = 50;
        }

        /// <summary>
        /// 创建表结构sheet
        /// </summary>
        /// <param name="epck"></param>
        /// <param name="sheetName"></param>
        /// <param name="tables"></param>
        private static void CreateTableSheet(OfficeOpenXml.ExcelPackage epck, string sheetName, List<TableDto> tables)
        {
            OfficeOpenXml.ExcelWorksheet tbWorksheet = epck.Workbook.Worksheets.Add(sheetName);
            int rowNum = 1, fromRow = 0, count = 0; // 行号计数器
                                                    //  循环数据库表名
            foreach (var table in tables)
            {
                var lstName = new List<string>
                {
                    "序号","列名","数据类型","长度","主键","自增","允许空","默认值","列说明"
                };

                //oracle不显示 列是否自增
                if (table.DBType.StartsWith("Oracle"))
                {
                    lstName.Remove("自增");
                }

                var spColCount = lstName.Count;

                //  数据库名称
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Merge = true;
                tbWorksheet.Cells[rowNum, 1].Value = table.TableName + " " + (!string.IsNullOrWhiteSpace(table.Comment) ? table.Comment : "");
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Font.Bold = true;
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Font.Size = 16;
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                //  注意：保存起始行号
                fromRow = rowNum;

                rowNum++; // 行号+1

                // tbWorksheet.Cells[int FromRow, int FromCol, int ToRow, int ToCol]
                //  列标题字体为粗体
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Font.Bold = true;

               

                //  添加列标题
                for (int j = 0; j < lstName.Count; j++)
                {
                    tbWorksheet.Cells[rowNum, j + 1].Value = lstName[j];
                }

                rowNum++; // 行号+1

                //  添加数据行,遍历数据库表字段
                foreach (var column in table.Columns)
                {
                    tbWorksheet.Cells[rowNum, 1].Value = column.ColumnOrder;
                    tbWorksheet.Cells[rowNum, 2].Value = column.ColumnName;
                    tbWorksheet.Cells[rowNum, 3].Value = column.ColumnTypeName;
                    tbWorksheet.Cells[rowNum, 4].Value = column.Length;
                    tbWorksheet.Cells[rowNum, 5].Value = column.IsPK;

                    //oracle不显示 列是否自增
                    if (table.DBType.StartsWith("Oracle"))
                    {
                        tbWorksheet.Cells[rowNum, 6].Value = column.CanNull;
                        tbWorksheet.Cells[rowNum, 7].Value = column.DefaultVal;
                        tbWorksheet.Cells[rowNum, 8].Value = column.Comment;
                    }
                    else
                    {
                        tbWorksheet.Cells[rowNum, 6].Value = column.IsIdentity;
                        tbWorksheet.Cells[rowNum, 7].Value = column.CanNull;
                        tbWorksheet.Cells[rowNum, 8].Value = column.DefaultVal;
                        tbWorksheet.Cells[rowNum, 9].Value = column.Comment;
                    }
                    rowNum++; // 行号+1
                }

                //  水平居中
                tbWorksheet.Cells[fromRow, 1, rowNum - 1, spColCount].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                //  垂直居中
                tbWorksheet.Cells[fromRow, 1, rowNum - 1, spColCount].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                //  上下左右边框线
                tbWorksheet.Cells[fromRow, 1, rowNum - 1, spColCount].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                tbWorksheet.Cells[fromRow, 1, rowNum - 1, spColCount].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                tbWorksheet.Cells[fromRow, 1, rowNum - 1, spColCount].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                tbWorksheet.Cells[fromRow, 1, rowNum - 1, spColCount].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                //  处理空白行，分割用
                if (count < tables.Count - 1)
                {
                    //tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    //tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Merge = true;
                    tbWorksheet.Cells[rowNum, 1, rowNum, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    tbWorksheet.Cells[rowNum, 1, rowNum, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DodgerBlue);
                }

                rowNum++; // 行号+1

                count++; // 计数器+1
            }

            //  设置表格样式
            tbWorksheet.Cells.Style.WrapText = true; // 自动换行
            tbWorksheet.Cells.Style.ShrinkToFit = true; // 单元格自动适应大小
        }

    }
}
