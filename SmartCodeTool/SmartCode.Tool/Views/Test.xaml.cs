using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SmartCode.Tool.ViewModels;
using unvell.ReoGrid;
using unvell.ReoGrid.DataFormat;
using unvell.ReoGrid.Graphics;

namespace SmartCode.Tool.Views
{
    /// <summary>
    /// Test.xaml 的交互逻辑
    /// </summary>
    public partial class Test
    {
        public Test()
        {
            InitializeComponent();
            DataContext = Workspace.This;
        }

        private void BtnGet_OnClick(object sender, RoutedEventArgs e)
        {
            //var wsheet = ReoGridExcel.Worksheets[0];
            //for (int i = 0; i < wsheet.RowCount; i++)
            //{
            //    var no = wsheet.Cells[i, 0].GetData<string>();
            //    if (string.IsNullOrEmpty(no))
            //    {
            //        continue;
            //    }
            //    var prjNo = wsheet.Cells[i, 1].GetData<string>();
            //    var preFix = wsheet.Cells[i, 2].GetData<string>();
            //    var postDate = Convert.ToDateTime(wsheet.Cells[i, 3].GetData<string>());
            //    var specifiedPeriod = wsheet.Cells[i, 4].GetData<int>();
            //    var revenueWithTax = wsheet.Cells[i, 5].GetData<decimal>();
            //    var revenue = wsheet.Cells[i, 6].GetData<decimal>();
            //    var taxMoney = wsheet.Cells[i, 7].GetData<decimal>();
            //    var taxYard = wsheet.Cells[i, 8].GetData<string>();
            //    var remark = wsheet.Cells[i, 9].GetData<string>();
            //    var refDoc = wsheet.Cells[i, 10].GetData<string>();

            //}
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            //ReoGridExcel.SetSettings(unvell.ReoGrid.WorkbookSettings.View_ShowSheetTabControl, false);
            //var wsheet1 = ReoGridExcel.Worksheets[0];
            //ReoGridExcel.SetSettings(WorkbookSettings.View_ShowScrolls,false);
            //wsheet1.SetSettings(WorksheetSettings.View_ShowColumnHeader,false);
            //wsheet1.SetSettings(WorksheetSettings.View_ShowRowHeader, false);
            //wsheet1.SetRows(1);
            //wsheet1.CellDataChanged += Wsheet1_CellDataChanged;
            //wsheet1.ColumnHeaders[0].Text = "序号";
            //wsheet1.ColumnHeaders[1].Text = "项目编号";
            //wsheet1.ColumnHeaders[2].Text = "前缀";
            //wsheet1.ColumnHeaders[3].Text = "过账日期";
            //wsheet1.ColumnHeaders[4].Text = "指定期间";
            //wsheet1.ColumnHeaders[5].Text = "收入含税_AR";
            //wsheet1.ColumnHeaders[6].Text = "收入金额";
            //wsheet1.ColumnHeaders[7].Text = "税额";
            //wsheet1.ColumnHeaders[8].Text = "税码";
            //wsheet1.ColumnHeaders[9].Text = "备注";
            //wsheet1.ColumnHeaders[10].Text = "凭证编号";
            //wsheet1.SetCols(11);
            //wsheet1.SetRangeStyles("A1:K200", new WorksheetRangeStyle
            //{
            //    Flag = PlainStyleFlag.HorizontalAlign,
            //    HAlign = ReoGridHorAlign.Center
            //});
            //wsheet1.SetRangeStyles("F1:H200", new WorksheetRangeStyle
            //{
            //    Flag = PlainStyleFlag.HorizontalAlign,
            //    HAlign = ReoGridHorAlign.Right
            //});
            //wsheet1.SetRangeDataFormat("F1:H200", CellDataFormatFlag.Number, new NumberDataFormatter.NumberFormatArgs()
            //{
            //    DecimalPlaces = 4
            //});
            //wsheet1.SetColumnsWidth(1, 1, 120);
            //wsheet1.SetColumnsWidth(3, 1, 90);
            //wsheet1.SetColumnsWidth(5, 3, 90);
            //wsheet1.SetColumnsWidth(10, 1, 90);
        }

        private void Wsheet1_CellDataChanged(object sender, unvell.ReoGrid.Events.CellEventArgs e)
        {
            //ReoGridExcel.SetSettings(unvell.ReoGrid.WorkbookSettings.View_ShowSheetTabControl, false);
            //var wsheet = ReoGridExcel.Worksheets[0];
            //int total = 0;
            //for (int i = 0; i < wsheet.RowCount; i++)
            //{
            //    var no = wsheet.Cells[i, 0].GetData<string>();
            //    var prjNo = wsheet.Cells[i, 1].GetData<string>();
            //    wsheet.Cells[i,1].Style.BackColor=SolidColor.Red;
            //    if (string.IsNullOrEmpty(no) || string.IsNullOrEmpty(prjNo))
            //    {
            //        continue;
            //    }
            //    total++;
            //}
            //wsheet.SetRows(total + 1);
        }
    }
}
