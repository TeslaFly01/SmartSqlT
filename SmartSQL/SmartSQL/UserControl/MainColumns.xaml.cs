using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HandyControl.Controls;
using HandyControl.Data;
using SmartSQL.Framework;
using SmartSQL.Framework.Exporter;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Helper;
using SmartSQL.Models;
using SmartSQL.Views;
using SqlSugar;
using UserControlE = System.Windows.Controls.UserControl;
using PathF = System.IO.Path;
using TextBox = System.Windows.Controls.TextBox;
using MessageBox = HandyControl.Controls.MessageBox;
using DbType = SqlSugar.DbType;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class MainColumns : BaseUserControl
    {
        #region Filds
        public event ObjChangeRefreshHandler ObjChangeRefreshEvent;
        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(PropertyNodeItem), typeof(MainColumns), new PropertyMetadata(default(PropertyNodeItem)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(MainColumns), new PropertyMetadata(default(DataBase)));

        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(MainColumns), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty SourceColunmDataProperty = DependencyProperty.Register(
            "SourceColunmData", typeof(List<Column>), typeof(MainColumns), new PropertyMetadata(default(List<Column>)));

        public static readonly DependencyProperty ObjectColumnsProperty = DependencyProperty.Register(
            "ObjectColumns", typeof(List<Column>), typeof(MainColumns), new PropertyMetadata(default(List<Column>)));

        /// <summary>
        /// 当前选中对象
        /// </summary>
        public PropertyNodeItem SelectedObject
        {
            get => (PropertyNodeItem)GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }
        /// <summary>
        /// 当前选中数据库
        /// </summary>
        public DataBase SelectedDataBase
        {
            get => (DataBase)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }
        /// <summary>
        /// 当前数据连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }
        /// <summary>
        /// 当前选中表列数据
        /// </summary>
        public List<Column> SourceColunmData
        {
            get => (List<Column>)GetValue(SourceColunmDataProperty);
            set
            {
                SetValue(SourceColunmDataProperty, value);
                OnPropertyChanged(nameof(SourceColunmData));
            }
        }

        public List<Column> ObjectColumns
        {
            get => (List<Column>)GetValue(ObjectColumnsProperty);
            set
            {
                SetValue(ObjectColumnsProperty, value);
                OnPropertyChanged(nameof(ObjectColumns));
            }
        }
        #endregion

        public MainColumns()
        {
            InitializeComponent();
            DataContext = this;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextSqlEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "SQL");
            TextSqlEditor.TextArea.SelectionCornerRadius = 0;
            TextSqlEditor.TextArea.SelectionBorder = null;
            TextSqlEditor.TextArea.SelectionForeground = null;
            TextSqlEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
        }

        public void LoadPageData()
        {
            NoDataText.Visibility = Visibility.Collapsed;
            var selectedObject = SelectedObject;
            var selectedConnection = SelectedConnection;
            var dbConnectionString = selectedConnection.SelectedDbConnectString(SelectedDataBase.DbName);
            if (selectedObject.Type == ObjType.Table || selectedObject.Type == ObjType.View)
            {
                SearchColumns.Text = string.Empty;
                var isView = selectedObject.Type == ObjType.View;
                TabTable.SelectedItem = isView ? TabSql : TabStruct;
                LoadingG.Visibility = TabStruct.Visibility = Visibility.Visible;
                TabData.Visibility = Visibility.Collapsed;
                TabSql.Visibility = isView ? Visibility.Visible : Visibility.Collapsed;
                TabCode.Visibility = isView ? Visibility.Collapsed : Visibility.Visible;
                BtnCreateSqlScript.IsEnabled = !isView;
                var objName = isView ? "视图" : "数据表";
                TabStruct.Header = objName;
                TabData.Header = objName;
                var dbInstance = ExporterFactory.CreateInstance(selectedConnection.DbType, dbConnectionString);
                Task.Run(() =>
                {
                    var tableColumns = dbInstance.GetColumnInfoById(selectedObject.ObejcetId);
                    var list = tableColumns.Values.ToList();
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        SourceColunmData = list;
                        ObjectColumns = list;
                        ColList = list;
                        LoadingG.Visibility = Visibility.Collapsed;
                        if (!list.Any())
                        {
                            NoDataText.Visibility = Visibility.Visible;
                        }
                    }));
                    if (selectedObject.Type == ObjType.View)
                    {
                        var script = dbInstance.GetScriptInfoById(selectedObject.ObejcetId, DbObjectType.View);
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            TextSqlEditor.SelectedText = string.Empty;
                            TextSqlEditor.Text = script;
                        }));
                    }
                });
                //if (TabData.IsSelected)
                //{
                //    SearchTableExt2.Text = "";
                //    //BindDataSet(exporter, objects, string.Empty);
                //    BindColumnDataSet(exporter, selectedObjct);
                //}
            }
            else
            {
                TabStruct.Visibility = Visibility.Collapsed;
                TabData.Visibility = Visibility.Collapsed;
                TabCode.Visibility = Visibility.Collapsed;
                TabSql.Visibility = Visibility.Visible;
                TabTable.SelectedItem = TabSql;
                var dbInstance = ExporterFactory.CreateInstance(selectedConnection.DbType, dbConnectionString);
                Task.Run(() =>
                {
                    var script = dbInstance.GetScriptInfoById(selectedObject.ObejcetId, DbObjectType.Proc);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TextSqlEditor.SelectedText = string.Empty;
                        TextSqlEditor.Text = script;
                    }));
                });
            }
        }


        /// <summary>
        /// 表结构绑定
        /// </summary>
        /// <param name="exporter"></param>
        /// <param name="objects"></param>
        private void BindColumnDataSet(IExporter exporter, PropertyNodeItem objects)
        {
            //Task.Run(() =>
            //{
            //    TableColumns = exporter.GetColumnsExt(Convert.ToInt32(objects.ObejcetId), ConnectionString);
            //    var list = TableColumns.Values.ToList();
            //    this.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        CbTableColumnData.ItemsSource = list;
            //    }));
            //});
        }

        //表数据选项卡选中时加载数据
        private void TabTable_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion
            if (SelectedObject == null || SelectedObject.ObejcetId.Equals("0") || SelectedObject.TextColor.Equals("Red"))
            {
                return;
            }
            var selectedItem = (System.Windows.Controls.TabItem)((System.Windows.Controls.TabControl)sender).SelectedItem;
            if (selectedItem.Name.Equals("TabData"))
            {
                var connectionString = SelectedConnection.DbMasterConnectString;
                var exporter = ExporterFactory.CreateInstance(SelectedConnection.DbType, connectionString);
                if (TabData.IsSelected)
                {
                    SearchTableExt2.Text = "";
                    BindDataSet(exporter, SelectedObject, string.Empty);
                    BindColumnDataSet(exporter, SelectedObject);
                }
            }
            if (selectedItem == TabCode)
            {
                UTabCode.SelectedDataBase = SelectedDataBase.DbName;
                UTabCode.SelectedConnection = SelectedConnection;
                UTabCode.SelectedObject = SelectedObject;
                UTabCode.SelectedTableColunms = SourceColunmData;
            }
            #endregion
        }

        private List<Column> ColList;
        /// <summary>
        /// 表数据检索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchColumns_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            #region MyRegion
            NoDataText.Visibility = Visibility.Collapsed;
            var searchText = SearchColumns.Text.Trim();
            var searchData = ColList;
            if (!string.IsNullOrEmpty(searchText))
            {
                searchData = ColList.Where(x => x.DisplayName.ToLower().Contains(searchText.ToLower()) || (!string.IsNullOrEmpty(x.Comment) && x.Comment.ToLower().Contains(searchText.ToLower()))).ToList();
                if (!searchData.Any())
                {
                    NoDataText.Visibility = Visibility.Visible;
                }
            }
            ObjectColumns = searchData;
            #endregion
        }

        private string _cellEditValue = string.Empty;
        /// <summary>
        /// 单元格开始编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TableGrid_OnBeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (((DataGrid)sender).SelectedCells.Any())
            {
                //获取选中单元格(仅限单选)
                var selectedCell = ((DataGrid)sender).SelectedCells[0];
                //获取选中单元格数据
                var selectedData = selectedCell.Column.GetCellContent(selectedCell.Item);
                if (selectedData is TextBlock)
                {
                    var selectedText = (TextBlock)selectedData;
                    _cellEditValue = selectedText.Text;
                }
            }
        }

        /// <summary>
        /// 单元格修改提交
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TableGrid_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            #region MyRegion
            if (e.EditingElement != null && e.EditingElement is TextBox)
            {
                string newValue = (e.EditingElement as TextBox).Text;
                if (!_cellEditValue.Equals(newValue))
                {
                    if (SelectedObject.Type == ObjType.View)
                    {
                        ((TextBox)e.EditingElement).Text = _cellEditValue;
                        return;
                    }
                    var selectItem = (Column)e.Row.Item;
                    var msgResult = MessageBox.Show($"确认修改{SelectedObject.DisplayName}的备注为{newValue}？", "温馨提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (msgResult != MessageBoxResult.OK)
                    {
                        ((TextBox)e.EditingElement).Text = _cellEditValue;
                        return;
                    }
                    var dbConnectionString = SelectedConnection.SelectedDbConnectString(SelectedDataBase.DbName);
                    var db = SugarFactory.GetDbMaintenance(SelectedConnection.DbType, dbConnectionString);
                    if (db.IsAnyColumnRemark(selectItem.Name, SelectedObject.Name))
                    {
                        db.DeleteColumnRemark(selectItem.Name, SelectedObject.Name);
                    }
                    var flag = db.AddColumnRemark(selectItem.Name, SelectedObject.Name, newValue);
                    if (flag)
                        Growl.SuccessGlobal(new GrowlInfo { Message = $"修改成功", ShowDateTime = false });
                    else
                        Growl.ErrorGlobal(new GrowlInfo { Message = $"修改失败", ShowDateTime = false });
                }
            }
            #endregion
        }

        /// <summary>
        /// 复制选中单元格的文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TableGrid_OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (((DataGrid)sender).SelectedCells.Any())
            {
                //获取选中单元格(仅限单选)
                var selectedCell = ((DataGrid)sender).SelectedCells[0];
                //获取选中单元格数据
                var selectedData = selectedCell.Column.GetCellContent(selectedCell.Item);
                if (selectedData is TextBlock selectedText)
                {
                    //Clipboard.SetDataObject(selectedText.Text);
                }
            }
        }

        private void Handled_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        private void SearchTableExt2_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            #region MyRegion
            //if (e.Key == Key.Enter)
            //{
            //    var searchText = SearchTableExt2.Text;
            //    if (CbTableColumnData.SelectedItem == null)
            //    {
            //        return;
            //    }
            //    var objects = TreeViewTables.SelectedItem as PropertyNodeItem;
            //    if (objects == null || objects.ObejcetId.Equals("0") || objects.TextColor.Equals("Red"))
            //    {
            //        return;
            //    }
            //    var selectColumn = (Column)CbTableColumnData.SelectedItem;
            //    var strWhere = "";
            //    if (!string.IsNullOrEmpty(searchText))
            //    {
            //        strWhere = $" {selectColumn.DisplayName} like '{searchText}%'";
            //    }
            //    IExporter exporter = new SqlServer2008Exporter();
            //    BindDataSet(exporter, objects, strWhere);
            //}
            #endregion
        }

        /// <summary>
        /// 分页页码变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pagination_OnPageUpdated(object sender, FunctionEventArgs<int> e)
        {
            //var pageIndex = PaginationTable.PageIndex;
        }

        /// <summary>
        /// 设置分组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSetGroup_OnClick(object sender, RoutedEventArgs e)
        {
            //选中的表对象
            if (SelectedObject == null || SelectedObject.ObejcetId.Equals("0") || SelectedObject.TextColor.Equals("Red"))
            {
                Growl.WarningGlobal(new GrowlInfo { Message = $"请选择对应的数据表", ShowDateTime = false, WaitTime = 1 });
                return;
            }
            var mainWindow = System.Windows.Window.GetWindow(this);
            var group = new SetObjectGroup();
            group.ObjChangeRefreshEvent += ObjChangeRefreshEvent;
            group.Connection = SelectedConnection;
            group.SelectedDataBase = SelectedDataBase.DbName;
            group.SelectedObjects = new List<PropertyNodeItem>() { SelectedObject };
            group.Owner = mainWindow;
            group.ShowDialog();
        }

        /// <summary>
        /// 新建字段脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCreateSqlScript_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var selectRows = GetTableGridSelectRows();
            if (!selectRows.Any())
            {
                if (SelectedObject == null || SelectedObject.ObejcetId.Equals("0") || SelectedObject.TextColor.Equals("Red"))
                {
                    Growl.WarningGlobal(new GrowlInfo { Message = $"请选择对应的数据表", ShowDateTime = false, WaitTime = 1 });
                    return;
                }
                selectRows = SourceColunmData;
            }
            var mainWindow = System.Windows.Window.GetWindow(this);
            var scriptW = new ScriptWindow();
            scriptW.SelectedConnection = SelectedConnection;
            scriptW.SelectedObject = SelectedObject;
            scriptW.SelectedColumns = selectRows;
            scriptW.Owner = mainWindow;
            scriptW.ShowDialog();
            #endregion
        }

        /// <summary>
        /// 获取表结构选中行
        /// </summary>
        /// <returns></returns>
        public List<Column> GetTableGridSelectRows()
        {
            var count = TableGrid.SelectedCells.Count;
            var list = new List<Column>(count);
            for (int i = 0; i < count; i++)
            {
                var col = (Column)TableGrid.SelectedCells[i].Item;
                list.Add(col);
            }
            return list.Distinct().ToList();
        }

        /// <summary>BtnSetGroup_OnClick
        /// 生成C#实体类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCreateEntity_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            string baseDirectoryPath = PathF.Combine(AppDomain.CurrentDomain.BaseDirectory, "EntityTemp");
            if (!Directory.Exists(baseDirectoryPath))
            {
                Directory.CreateDirectory(baseDirectoryPath);
            }
            if (SelectedObject == null || SelectedObject.ObejcetId.Equals("0") || SelectedObject.TextColor.Equals("Red"))
            {
                Growl.WarningGlobal(new GrowlInfo { Message = $"请选择需要生成实体的数据表", ShowDateTime = false, WaitTime = 1 });
                return;
            }
            var filePath = string.Format($"{baseDirectoryPath}\\{SelectedObject.Name}.cs");
            StrUtil.CreateClass(filePath, SelectedObject.Name, SourceColunmData);

            Growl.SuccessGlobal(new GrowlInfo { Message = "实体生成成功", WaitTime = 1, ShowDateTime = false });
            Process.Start(baseDirectoryPath);
            #endregion
        }

        /// <summary>
        /// 表数据绑定
        /// </summary>
        /// <param name="exporter"></param>
        /// <param name="objects"></param>
        /// <param name="strWhere"></param>
        private void BindDataSet(IExporter exporter, PropertyNodeItem objects, string strWhere)
        {
            //LoadingLineTableData.Visibility = Visibility.Visible;
            //NoDataTextExt.Visibility = Visibility.Collapsed;
            //var connectionString = SelectedConnection.DbMasterConnectString;
            //Task.Run(() =>
            //{
            //    DataSet dataSet = exporter.GetDataSet(connectionString, objects.DisplayName, strWhere);
            //    this.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        TableDataGrid.ItemsSource = null;
            //        //编写获取数据并显示在界面的代码
            //        var dataView = dataSet.Tables[0].DefaultView;
            //        TableDataGrid.ItemsSource = dataView;
            //        LoadingLineTableData.Visibility = Visibility.Hidden;
            //        if (dataView.Count < 1)
            //        {
            //            NoDataTextExt.Visibility = Visibility.Visible;
            //        }
            //    }));
            //});
        }

        /// <summary>
        /// 复制脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCopyScript_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextSqlEditor.Text))
            {
                TextSqlEditor.SelectAll();
                Clipboard.SetDataObject(TextSqlEditor.Text);
                Growl.SuccessGlobal(new GrowlInfo { Message = "脚本已复制到剪切板.", WaitTime = 1, ShowDateTime = false });
            }
        }

        /// <summary>
        /// 格式化SQL代码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFormat_OnClick(object sender, RoutedEventArgs e)
        {
            TextSqlEditor.Text = TextSqlEditor.Text.SqlFormat();
        }
    }
}
