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
using System.Text.RegularExpressions;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class UcRedisFrom : BaseUserControl
    {
        #region Filds
        public event ObjChangeRefreshHandler ObjChangeRefreshEvent;
        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(TreeNodeItem), typeof(UcRedisFrom), new PropertyMetadata(default(TreeNodeItem)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(UcRedisFrom), new PropertyMetadata(default(DataBase)));

        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcRedisFrom), new PropertyMetadata(default(ConnectConfigs)));

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

        public UcRedisFrom()
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
            var sqlScript = TextEditor.Text;
            if (string.IsNullOrEmpty(sqlScript))
            {
                return;
            }
            if (string.IsNullOrEmpty(TextEditor.SelectedText))
            {
                TextEditor.Text = sqlScript.SqlFormat();
                return;
            }
            var oldSqlScript = TextEditor.SelectedText;
            sqlScript = TextEditor.SelectedText;
            TextEditor.Text =  TextEditor.Text.Replace(oldSqlScript, sqlScript.SqlFormat());
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
