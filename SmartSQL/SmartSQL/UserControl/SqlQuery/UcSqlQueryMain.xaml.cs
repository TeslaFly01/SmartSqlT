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
using SmartSQL.DocUtils;
using SmartSQL.Framework.Const;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ZXing.PDF417.Internal;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using TSQL.Statements;
using TSQL;
using System.Text;
using TSQL.Tokens;
using SqlSugar.Extensions;
using SmartSQL.Framework.Util;
using System.Data;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class UcSqlQueryMain : BaseUserControl
    {
        #region Filds
        public event ObjChangeRefreshHandler ObjChangeRefreshEvent;
        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(TreeNodeItem), typeof(UcSqlQueryMain), new PropertyMetadata(default(TreeNodeItem)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(UcSqlQueryMain), new PropertyMetadata(default(DataBase)));

        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcSqlQueryMain), new PropertyMetadata(default(ConnectConfigs)));

        /// <summary>
        /// 当前选中对象
        /// </summary>
        public TreeNodeItem SelectedObject
        {
            get => (TreeNodeItem)GetValue(SelectedObjectProperty);
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
        #endregion

        private List<DataBase> DataBaseList = new List<DataBase>();
        private Model model = new Model();

        public CompletionWindow completionWindow;

        public UcSqlQueryMain()
        {
            InitializeComponent();
            DataContext = this;
            TextEditor.TextArea.TextEntered+=TextArea_TextEntered;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "SQL");
            TextEditor.TextArea.SelectionCornerRadius = 0;
            TextEditor.TextArea.SelectionBorder = null;
            TextEditor.TextArea.SelectionForeground = null;
            TextEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            #region MyRegion
            if (completionWindow != null)
            {
                return;
            }
            Logger.Info($"ShowCompletion:1");
            var enterText = e.Text;
            // 这里可以加入更加智能的条件，例如判断前一个单词是不是SQL关键字。
            if (char.IsLetter(enterText[0]) || enterText == " ")
            {
                ShowCompletion(enterText, false);
            }
            #endregion
        }
        private string GetLastWord(string text)
        {
            var words = text.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length > 0 ? words[words.Length - 1] : string.Empty;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="enteredText"></param>
        /// <param name="controlSpace"></param>
        private void ShowCompletion(string enteredText, bool controlSpace)
        {
            #region MyRegion
            var textArea = TextEditor.TextArea;
            var caret = textArea.Caret;
            var document = textArea.Document;

            // 获取光标前的文本
            var lineText = document.GetText(document.GetLineByNumber(caret.Line));
            var textUpToCaret = lineText.Substring(0, caret.Column-1);

            // 最简单的上下文分析: 分析光标前的非空字符
            // 实践中这可能要复杂得多，可能涉及到语法和词法的分析。
            var lastWord = GetLastWord(textUpToCaret);

            completionWindow = new CompletionWindow(TextEditor.TextArea);
            completionWindow.BorderThickness = new Thickness(0);
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;

            // 根据用户已经键入的文字，筛选出匹配的 SQL 关键字
            foreach (var keyword in SysConst.Sys_SqlKeywords)
            {
                if (keyword.IndexOf(enteredText, StringComparison.OrdinalIgnoreCase)>=0 || enteredText ==" ")
                {
                    data.Add(new SqlCompletionData(keyword));
                }
            }
            foreach (var fun in SysConst.Sys_SqlFuns)
            {
                if (fun.IndexOf(enteredText, StringComparison.OrdinalIgnoreCase)>=0 || enteredText == " ")
                {
                    data.Add(new SqlCompletionData(fun, ObjType.Func));
                }
            }
            //数据库
            foreach (var db in DataBaseList)
            {
                if (db.DbName.IndexOf(enteredText, StringComparison.OrdinalIgnoreCase)>=0 || enteredText ==" ")
                {
                    data.Add(new SqlCompletionData(db.DbName, ObjType.Db));
                }
            }
            //表
            foreach (var table in model.Tables)
            {
                if (table.Value.Name.IndexOf(enteredText, StringComparison.OrdinalIgnoreCase)>=0  || enteredText ==" ")
                {
                    data.Add(new SqlCompletionData(table.Value.Name, ObjType.Table, table.Value.Comment));
                }
            }
            //视图
            foreach (var view in model.Views)
            {
                if (view.Value.Name.IndexOf(enteredText, StringComparison.OrdinalIgnoreCase)>=0  || enteredText ==" ")
                {
                    data.Add(new SqlCompletionData(view.Value.Name, ObjType.View, view.Value.Comment));
                }
            }
            //存储过程
            foreach (var proc in model.Procedures)
            {
                if (proc.Value.Name.IndexOf(enteredText, StringComparison.OrdinalIgnoreCase)>=0  || enteredText ==" ")
                {
                    data.Add(new SqlCompletionData(proc.Value.Name, ObjType.Proc, proc.Value.Comment));
                }
            }
            Logger.Info($"ShowCompletion:{data.Count}");
            if (data.Count == 0)
            {
                // 如果没有匹配项，则不需要显示无内容的智能提示窗口
                completionWindow.Close();
                return;
            }
            completionWindow.Show();
            completionWindow.Closed += delegate
            {
                completionWindow = null;
            };
            completionWindow.PreviewKeyDown += (sender, args) =>
            {
                if (args.Key == Key.Space && !IsFocused)
                {
                    completionWindow.Close();
                }
            };
            // 默认选择第一项
            completionWindow.CompletionList.SelectedItem = data[0];
            completionWindow.Width = 550;
            var listBox = completionWindow.CompletionList.ListBox;

            listBox.Margin=new Thickness(0);
            // 设置ListBox的边框样式
            listBox.BorderThickness = new Thickness(0); // 设置边框粗细
            // 创建新的样式以应用于每个ListBox项
            var style = new Style(typeof(ListBoxItem));
            style.BasedOn = listBox.ItemContainerStyle; // 从ListBox的ItemContainerStyle继承样式
            style.Setters.Add(new Setter(FrameworkElement.HeightProperty, 25.0)); // 设置ListBoxItem的高度

            // 创建触发器，以便在项被选中时更改背景色
            Trigger trigger = new Trigger { Property = ListBoxItem.IsSelectedProperty, Value = true };
            trigger.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#aee1ff"))));
            trigger.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"))));
            style.Triggers.Add(trigger);
            // 应用样式到ListBox的每个项
            listBox.ItemContainerStyle = style;
            #endregion
        }

        /// <summary>
        /// 页面加载完成后加载数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UcSqlQueryMain_Loaded(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var selectedConnection = SelectedConnection;
            var selectedDatabase = SelectedDataBase;
            if (selectedConnection == null || selectedDatabase == null)
            {
                return;
            }
            Task.Run(() =>
            {
                try
                {
                    var sqLiteHelper = SQLiteHelper.GetInstance();
                    var connectConfigs = sqLiteHelper.GetList<ConnectConfigs>();
                    var dbInstance = ExporterFactory.CreateInstance(selectedConnection.DbType, selectedConnection.DbDefaultConnectString, selectedConnection.DefaultDatabase);
                    var list = dbInstance.GetDatabases(selectedConnection.DefaultDatabase);
                    model = dbInstance.Init();
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        SelectConnets.ItemsSource = connectConfigs;
                        SelectConnets.SelectedItem = connectConfigs.FirstOrDefault(x => x.ID == selectedConnection.ID);
                        SelectDatabase.ItemsSource = list;
                        SelectDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName == selectedDatabase.DbName);
                        DataBaseList = list;
                    }));
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Oops.God($"连接失败 {selectedConnection.ConnectName}，原因：" + ex.ToMsg());
                    }));
                }
            });
            #endregion
        }

        /// <summary>
        /// 切换连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectConnets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded)
            {
                return;
            }
            var selectedConnection = (ConnectConfigs)SelectConnets.SelectedItem;
            var selectedDatabase = SelectedDataBase;
            if (selectedConnection == null)
            {
                return;
            }
            Task.Run(() =>
            {
                try
                {
                    var dbInstance = ExporterFactory.CreateInstance(selectedConnection.DbType, selectedConnection.DbDefaultConnectString, selectedConnection.DefaultDatabase);
                    var list = dbInstance.GetDatabases(selectedConnection.DefaultDatabase);
                    model = dbInstance.Init();
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        DataBaseList = list;
                        SelectDatabase.ItemsSource = list;
                        var dbName = selectedDatabase == null ? selectedConnection.DefaultDatabase : selectedDatabase.DbName;
                        SelectDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName == dbName);
                    }));
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Oops.God($"连接失败 {selectedConnection.ConnectName}，原因：" + ex.ToMsg());
                    }));
                }
            });
            #endregion
        }

        /// <summary>
        /// 切换数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded)
            {
                return;
            }
            var selectedConnection = (ConnectConfigs)SelectConnets.SelectedItem;
            var selectedDatabase = (DataBase)SelectDatabase.SelectedItem;
            if (selectedConnection == null || selectedDatabase == null)
            {
                return;
            }
            Task.Run(() =>
            {
                try
                {
                    var dbInstance = ExporterFactory.CreateInstance(selectedConnection.DbType, selectedConnection.SelectedDbConnectString(selectedDatabase.DbName), selectedDatabase.DbName);
                    model = dbInstance.Init();
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Oops.God($"连接失败 {selectedConnection.ConnectName}，原因：" + ex.ToMsg());
                    }));
                }
            });
            #endregion
        }

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExecute_Click(object sender, RoutedEventArgs e)
        {
            var script = TextEditor.Text;
            if (string.IsNullOrEmpty(script))
            {
                return;
            }
            var selectedText = TextEditor.SelectedText;
            if (!string.IsNullOrEmpty(selectedText))
            {
                script = selectedText;
            }
            PageInfo.PageIndex = 1;
            ExecuteSql(script);
        }

        /// <summary>
        /// 页码更新触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageInfo_PageUpdated(object sender, FunctionEventArgs<int> e)
        {
            var script = TextEditor.Text;
            if (string.IsNullOrEmpty(script))
            {
                return;
            }
            var selectedText = TextEditor.SelectedText;
            if (!string.IsNullOrEmpty(selectedText))
            {
                script = selectedText;
            }
            ExecuteSql(script);
        }

        /// <summary>
        /// 快捷键：F5 | Ctrl + Enter 
        /// 触发执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UcSqlQueryMain_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            #region MyRegion
            // F5 或者 Ctrl + Enter 执行
            if (e.Key == Key.F5 || ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))  && e.Key==Key.Enter))
            {
                #region MyRegion
                var script = TextEditor.Text;
                if (string.IsNullOrEmpty(script))
                {
                    return;
                }
                var selectedText = TextEditor.SelectedText;
                if (!string.IsNullOrEmpty(selectedText))
                {
                    script = selectedText;
                }
                PageInfo.PageIndex = 1;
                ExecuteSql(script);
                // 在这里触发相关事件
                #endregion
            }
            // Ctrl + S 另存为
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.S)
            {
                #region MyRegion
                var saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "SQL 文件(*.sql)|*.sql|所有文件(*.*)|*.*|文本文件(*.txt)|*.txt"; // 设置保存文件的筛选器
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var fileName = saveFileDialog.FileName;
                    TextEditor.Save(fileName);
                }
                #endregion
            }
            e.Handled = true; // 防止触发其他按键事件 
            #endregion
        }

        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="script"></param>
        private void ExecuteSql(string script)
        {
            #region MyRegion
            TabResult.SelectedIndex = 1;
            var selConnection = (ConnectConfigs)SelectConnets.SelectedItem;
            var selDatabase = (DataBase)SelectDatabase.SelectedItem;
            var pageIndex = PageInfo.PageIndex -1;
            var pageSize = Convert.ToInt32(((ComboBoxItem)PageSize.SelectedItem).Content);
            var sqLiteInstance = SQLiteHelper.GetInstance();
            var sw = new Stopwatch();
            LoadingG.Visibility= Visibility.Visible;
            Task.Run(() =>
            {
                try
                {
                    sw.Start(); // 开始计时
                    var sqlList = SqlParse(script);
                    var execSql = sqlList[sqlList.Count -1];
                    var selectSql = execSql.ExeSQL;
                    var orderBySql = string.Empty;

                    // 判断是否包含 ORDER BY 子句
                    int orderByIndex = execSql.ExeSQL.IndexOf("ORDER BY", StringComparison.OrdinalIgnoreCase);
                    if (orderByIndex != -1)
                    {
                        selectSql = execSql.ExeSQL.Substring(0, orderByIndex).Trim();
                        orderBySql = execSql.ExeSQL.Substring(orderByIndex).Trim();
                    }
                    int backRows = 0;
                    //历史查询
                    var sqlHistory = new SqlQueryHistory();
                    sqlHistory.ConnectName = selConnection.ConnectName;
                    sqlHistory.DataBaseName = selDatabase.DbName;
                    sqlHistory.QuerySql = selectSql;
                    sqlHistory.QueryTime = DateTime.Now;
                    var dataView = new DataView();
                    var totalPages = 0;
                    var dbInstance = ExporterFactory.CreateInstance(selConnection.DbType, selConnection.SelectedDbConnectString(selDatabase.DbName), selDatabase.DbName);
                    if (execSql.IsSelect)
                    {
                        var result = dbInstance.GetDataTable(selectSql, orderBySql, pageIndex, pageSize);

                        //编写获取数据并显示在界面的代码
                        dataView = result.Item1.DefaultView;
                        //总页数
                        totalPages = (int)Math.Ceiling((double)result.Item2 / pageSize);
                        //受影响行
                        backRows = result.Item2;
                    }
                    else
                    {
                        //受影响行
                        backRows = dbInstance.ExecuteSQL(execSql.ExeSQL);
                    }
                    // 停止计时
                    sw.Stop();
                    // 获取经过的时间
                    TimeSpan elapsedTime = sw.Elapsed;
                    //历史查询
                    sqlHistory.BackRows = backRows;
                    sqlHistory.TimeConsuming = elapsedTime.TotalMilliseconds;

                    sqLiteInstance.Add(sqlHistory);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TabResult.SelectedIndex = 1;
                        DgResult.ItemsSource = null;
                        DgResult.ItemsSource = dataView;
                        PageInfo.MaxPageCount = totalPages;
                        NoDataTextExt.Visibility = Visibility.Hidden;
                        TbLogIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.CheckCircle;
                        TbLogIcon.Foreground = new SolidColorBrush(Colors.Green);
                        TbLogMsg.Text = $"执行成功，耗时：{elapsedTime.TotalMilliseconds.ToString("N2")} ms，共 {backRows} 条数据受影响";
                        TbLogMsg.Foreground = new SolidColorBrush(Colors.Black);
                        if (dataView.Count < 1)
                        {
                            NoDataTextExt.Visibility = Visibility.Visible;
                        }
                    }));
                }
                catch (Exception ex)
                {
                    // 停止计时
                    sw.Stop();
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //Oops.God($"执行失败，原因：" + ex.ToMsg());
                        TabLog.IsSelected = true;
                        TbLogIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.TimesCircle;
                        TbLogIcon.Foreground = new SolidColorBrush(Colors.Red);
                        TbLogMsg.Text = ex.Message;
                        TbLogMsg.Foreground = new SolidColorBrush(Colors.Red);
                    }));
                }
                finally
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TbLogIcon.Visibility = Visibility.Visible;
                        LoadingG.Visibility = Visibility.Hidden;
                    }));
                }
            });
            #endregion
        }

        private void DgResult_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Visibility =  e.PropertyName == "RowIndex" ? Visibility.Collapsed : Visibility.Visible;
        }

        //SQL解析
        private List<ExecuteSQL> SqlParse(string sql)
        {
            #region MyRegion
            var exeSQLList = new List<ExecuteSQL>();
            // 初始化解析器
            var reader = new TSQLStatementReader(sql);
            // 遍历所有 SQL 语句
            while (reader.MoveNext())
            {
                var exeSQL = new ExecuteSQL();
                var statement = reader.Current;
                exeSQL.IsSelect = statement.AsSelect !=null;

                var sb = new StringBuilder();
                int lastPosition = -1;
                foreach (TSQLToken token in statement.Tokens)
                {
                    // 如果当前 token 的开始位置大于上一个 token 的结束位置，则添加空格
                    if (lastPosition >= 0 && token.BeginPosition > lastPosition + 1)
                    {
                        sb.Append(' ', token.BeginPosition - lastPosition - 1);
                    }
                    sb.Append(token.Text);
                    lastPosition = token.EndPosition;
                }
                exeSQL.ExeSQL = sb.ToString();
                exeSQLList.Add(exeSQL);
            }
            return exeSQLList;
            #endregion
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            CMeneExport.IsOpen = true;
        }

        /// <summary>
        /// 切换结果面板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded)
            {
                return;
            }
            var selTab = (HandyControl.Controls.TabItem)TabResult.SelectedItem;
            if (selTab.Header.ToString() == "查询历史")
            {
                LoadLogInfo();
            }
            #endregion
        }

        /// <summary>
        /// 日志页码更新触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageInfoLog_PageUpdated(object sender, FunctionEventArgs<int> e)
        {
            LoadLogInfo();
        }

        /// <summary>
        /// 加载日志数据
        /// </summary>
        private void LoadLogInfo()
        {
            #region MyRegion
            LoadingLog.Visibility= Visibility.Visible;
            var pageIndex = PageInfoLog.PageIndex;
            var pageSize = Convert.ToInt32(((ComboBoxItem)PageSizeLog.SelectedItem).Content);
            var sqLiteInstance = SQLiteHelper.GetInstance();
            Task.Run(() =>
            {
                var result = sqLiteInstance.GetPageList<SqlQueryHistory>(pageIndex, pageSize, x => x.QueryTime, OrderByType.Desc);
                //总页数
                int totalPages = (int)Math.Ceiling((double)result.Item2 / pageSize);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DgHistory.ItemsSource = result.Item1;
                    PageInfoLog.MaxPageCount = totalPages;
                    NoDataLog.Visibility = Visibility.Visible;
                    if (result.Item2 > 0)
                    {
                        NoDataLog.Visibility = Visibility.Collapsed;
                    }
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        LoadingLog.Visibility = Visibility.Hidden;
                    }));
                }));

            });
            #endregion
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuCopy_Click(object sender, RoutedEventArgs e)
        {
            TextEditor.Copy();
        }

        /// <summary>
        /// 粘贴
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuPaste_Click(object sender, RoutedEventArgs e)
        {
            TextEditor.Paste();
        }

        /// <summary>
        /// 格式化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuFormat_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 另存为
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "SQL 文件(*.sql)|*.sql|所有文件(*.*)|*.*|文本文件(*.txt)|*.txt"; // 设置保存文件的筛选器
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var fileName = saveFileDialog.FileName;
                TextEditor.Save(fileName);
            }
        }
    }
}
