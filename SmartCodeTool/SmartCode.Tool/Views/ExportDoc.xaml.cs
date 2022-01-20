using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using SmartCode.DocUtils;
using SmartCode.DocUtils.Dtos;
using SmartCode.Framework;
using SmartCode.Framework.Exporter;
using SmartCode.Framework.PhysicalDataModel;
using SmartCode.Framework.SqliteModel;
using SmartCode.Tool.Models;

namespace SmartCode.Tool.Views
{
    /// <summary>
    /// ExportDoc.xaml 的交互逻辑
    /// </summary>
    public partial class ExportDoc
    {
        #region DependencyProperty
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "Connection", typeof(ConnectConfigs), typeof(ExportDoc), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 当前连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(ExportDoc), new PropertyMetadata(default(DataBase)));
        /// <summary>
        /// 当前数据库
        /// </summary>
        public DataBase SelectedDataBase
        {
            get => (DataBase)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }

        public static readonly DependencyProperty ExportDataProperty = DependencyProperty.Register(
            "ExportData", typeof(List<PropertyNodeItem>), typeof(ExportDoc), new PropertyMetadata(default(List<PropertyNodeItem>)));
        /// <summary>
        /// 导出目标数据
        /// </summary>
        public List<PropertyNodeItem> ExportData
        {
            get => (List<PropertyNodeItem>)GetValue(ExportDataProperty);
            set => SetValue(ExportDataProperty, value);
        }

        public static readonly DependencyProperty ExportTypeProperty = DependencyProperty.Register(
            "ExportType", typeof(ExportEnum), typeof(ExportDoc), new PropertyMetadata(default(ExportEnum)));
        /// <summary>
        /// 导出类型
        /// </summary>
        public ExportEnum ExportType
        {
            get => (ExportEnum)GetValue(ExportTypeProperty);
            set => SetValue(ExportTypeProperty, value);
        }
        #endregion

        public ExportDoc()
        {
            InitializeComponent();
            TxtPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private void ExportDoc_OnLoaded(object sender, RoutedEventArgs e)
        {
            Title = $"{SelectedDataBase.DbName} - {Title}";
            TxtFileName.Text = SelectedDataBase.DbName + "数据库结构信息";
            CheckAll.IsChecked = ExportType == ExportEnum.All;
            CheckPart.IsChecked = ExportType == ExportEnum.Partial;
        }

        private void BtnLookPath_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                TxtPath.Text = dialog.FileName;
            }
        }

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExport_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedConnection = SelectedConnection;
            var selectedDatabase = SelectedDataBase;
            var exportData = ExportData;
            var floderPath = TxtPath.Text;
            var doctype = DocumentType();
            if (string.IsNullOrEmpty(doctype))
            {
                Growl.WarningGlobal(new GrowlInfo { Message = $"请选择输出文档类型", WaitTime = 1, ShowDateTime = false });
                return;
            }
            if (string.IsNullOrEmpty(TxtFileName.Text))
            {
                TxtFileName.Text = $"{SelectedDataBase.DbName}数据库结构信息";
            }
            //文件扩展名
            var fileNameE = LblFileExtend.Content;
            //文件名
            var fileName = TxtFileName.Text.Trim() + fileNameE;
            LoadingG.Visibility = Visibility.Visible;
            var dbDto = new DBDto(selectedDatabase.DbName);
            Task.Run(() =>
            {
                dbDto.DBType = "SqlServer";
                dbDto.Tables = Trans2Table(exportData, selectedConnection, selectedDatabase);
                dbDto.Procs = Trans2Dictionary(exportData, selectedConnection, selectedDatabase, "Proc");
                dbDto.Views = Trans2Dictionary(exportData, selectedConnection, selectedDatabase, "View");
                //xml生成目前有问题
                var doc = DocFactory.CreateInstance((DocType)(Enum.Parse(typeof(DocType), doctype)), dbDto);
                var filePath = Path.Combine(floderPath, fileName);
                doc.Build(filePath);
                Dispatcher.Invoke(() =>
                {
                    LoadingG.Visibility = Visibility.Collapsed;
                    Growl.SuccessGlobal("导出成功.");
                });
            });
        }

        private List<ViewProDto> Trans2Dictionary(List<PropertyNodeItem> treeViewData, ConnectConfigs selectedConnection, DataBase selectedDatabase, string type)
        {
            var viewPro = new List<ViewProDto>();
            foreach (var group in treeViewData)
            {
                if (group.Name.Equals("treeTable"))
                {
                    continue;
                }
                if (group.Type == "Type")
                {
                    foreach (var item in group.Children)
                    {
                        if (item.Type == type)
                        {
                            var objectId = Convert.ToInt32(item.ObejcetId);
                            var exporter = ExporterFactory.CreateInstance(DataBaseType.SqlServer, selectedConnection.DbMasterConnectString.Replace("master", selectedDatabase.DbName));
                            var script = exporter.GetScripts(objectId, selectedConnection.DbMasterConnectString.Replace("master", selectedDatabase.DbName));

                            viewPro.Add(new ViewProDto
                            {
                                ObjectName = item.DisplayName,
                                Comment = item.Comment,
                                Script = script
                            });
                        }
                    }
                }
                else
                {
                    if (group.Type == type)
                    {
                        var objectId = Convert.ToInt32(group.ObejcetId);
                        var exporter = ExporterFactory.CreateInstance(DataBaseType.SqlServer,
                            selectedConnection.DbMasterConnectString.Replace("master", selectedDatabase.DbName));
                        var script = exporter.GetScripts(objectId, selectedConnection.DbMasterConnectString.Replace("master", selectedDatabase.DbName));

                        viewPro.Add(new ViewProDto
                        {
                            ObjectName = group.DisplayName,
                            Comment = group.Comment,
                            Script = script
                        });
                    }
                }
            }
            return viewPro;
        }

        private List<TableDto> Trans2Table(List<PropertyNodeItem> treeViewData, ConnectConfigs selectedConnection, DataBase selectedDatabase)
        {
            List<TableDto> tables = new List<TableDto>();
            var groupNo = 1;
            foreach (var group in treeViewData)
            {
                if (group.Type == "Type" && group.Name.Equals("treeTable"))
                {
                    int orderNo = 1;
                    foreach (var node in group.Children)
                    {
                        TableDto tbDto = new TableDto();
                        tbDto.TableOrder = orderNo.ToString();
                        tbDto.TableName = node.DisplayName;
                        tbDto.Comment = node.Comment;
                        tbDto.DBType = nameof(DataBaseType.SqlServer);

                        var lst_col_dto = new List<ColumnDto>();
                        var objectId = Convert.ToInt32(node.ObejcetId);
                        var exporter = ExporterFactory.CreateInstance(DataBaseType.SqlServer,
                            selectedConnection.DbMasterConnectString.Replace("master", selectedDatabase.DbName));
                        var columns = exporter.GetColumns(objectId, selectedConnection.DbMasterConnectString.Replace("master", selectedDatabase.DbName));
                        foreach (var col in columns)
                        {
                            ColumnDto colDto = new ColumnDto();
                            colDto.ColumnOrder = col.Key.ToString();
                            colDto.ColumnName = col.Value.DisplayName;
                            // 数据类型
                            colDto.ColumnTypeName = col.Value.DataType;
                            // 长度
                            colDto.Length = col.Value.Length;
                            // 小数位
                            //colDto.Scale = "";//(col.Scale.HasValue ? col.Scale.Value.ToString() : "");
                            // 主键
                            colDto.IsPK = (col.Value.IsPrimaryKey ? "√" : "");
                            // 自增
                            colDto.IsIdentity = (col.Value.IsAutoIncremented ? "√" : "");
                            // 允许空
                            colDto.CanNull = (col.Value.IsNullable ? "√" : "");
                            // 默认值
                            colDto.DefaultVal = (!string.IsNullOrWhiteSpace(col.Value.DefaultValue) ? col.Value.DefaultValue : "");
                            // 列注释（说明）
                            colDto.Comment = (!string.IsNullOrWhiteSpace(col.Value.Comment) ? col.Value.Comment : "");

                            lst_col_dto.Add(colDto);
                        }
                        tbDto.Columns = lst_col_dto;
                        tables.Add(tbDto);
                        orderNo++;
                    }
                }
                if (group.Type == "Table")
                {
                    TableDto tbDto = new TableDto();
                    tbDto.TableOrder = groupNo.ToString();
                    tbDto.TableName = group.DisplayName;
                    tbDto.Comment = group.Comment;
                    tbDto.DBType = "SqlServer";

                    var lst_col_dto = new List<ColumnDto>();
                    var objectId = Convert.ToInt32(group.ObejcetId);
                    var exporter = ExporterFactory.CreateInstance(DataBaseType.SqlServer,
                        selectedConnection.DbMasterConnectString.Replace("master", selectedDatabase.DbName));
                var columns = exporter.GetColumns(objectId, selectedConnection.DbMasterConnectString.Replace("master", selectedDatabase.DbName));
                    foreach (var col in columns)
                    {
                        ColumnDto colDto = new ColumnDto();
                        colDto.ColumnOrder = col.Key.ToString();
                        colDto.ColumnName = col.Value.DisplayName;
                        // 数据类型
                        colDto.ColumnTypeName = col.Value.DataType;
                        // 长度
                        colDto.Length = col.Value.Length;
                        // 小数位
                        //colDto.Scale = "";//(col.Scale.HasValue ? col.Scale.Value.ToString() : "");
                        // 主键
                        colDto.IsPK = (col.Value.IsPrimaryKey ? "√" : "");
                        // 自增
                        colDto.IsIdentity = (col.Value.IsAutoIncremented ? "√" : "");
                        // 允许空
                        colDto.CanNull = (col.Value.IsNullable ? "√" : "");
                        // 默认值
                        colDto.DefaultVal = (!string.IsNullOrWhiteSpace(col.Value.DefaultValue) ? col.Value.DefaultValue : "");
                        // 列注释（说明）
                        colDto.Comment = (!string.IsNullOrWhiteSpace(col.Value.Comment) ? col.Value.Comment : "");

                        lst_col_dto.Add(colDto);
                    }
                    tbDto.Columns = lst_col_dto;
                    tables.Add(tbDto);
                    groupNo++;
                }
            }
            return tables;
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 导出文档类型单选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Toggle_OnChecked(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton)sender;
            foreach (ToggleButton toggle in ToggleWarpPanel.Children)
            {
                if (toggle.Name != button.Name)
                {
                    toggle.IsChecked = false;
                }
            }
            if (IsLoaded)
            {
                var docType = (DocType)(Enum.Parse(typeof(DocType), button.Content.ToString().ToLower()));
                var fileExtend = FileExtend(docType);
                LblFileExtend.Content = "." + fileExtend;
            }
        }

        private string DocumentType()
        {
            var type = string.Empty;
            foreach (ToggleButton button in ToggleWarpPanel.Children)
            {
                if (button.IsChecked == true)
                {
                    type = button.Content.ToString().ToLower();
                }
            }
            return type;
        }

        private string FileExtend(DocType docType)
        {
            switch (docType)
            {
                case DocType.word: return "docx";
                case DocType.chm: return "chm";
                case DocType.excel: return "xlsx";
                case DocType.html: return "html";
                case DocType.markdown: return "md";
                case DocType.pdf: return "pdf";
                case DocType.xml: return "xml";
                default: return "chm";
            }
        }
    }
}
