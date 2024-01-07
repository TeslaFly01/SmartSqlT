using SmartSQL.Annotations;
using SmartSQL.Framework;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Models;
using SmartSQL.Views.Category;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.WindowsAPICodePack.Dialogs;
using SmartSQL.DocUtils;
using SmartSQL.DocUtils.Dtos;
using SmartSQL.Framework.Const;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Helper;
using SmartSQL.Views;
using Path = System.Windows.Shapes.Path;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace SmartSQL.UserControl.GenCodes
{
    /// <summary>
    /// TagObjects.xaml 的交互逻辑
    /// </summary>
    public partial class UcGenTemplate : BaseUserControl
    {
        public CompletionWindow completionWindow;

        #region PropertyFiled
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcGenTemplate), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 当前选中连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }


        public static readonly DependencyProperty DataListProperty = DependencyProperty.Register(
            "DataList", typeof(List<TemplateInfo>), typeof(UcGenTemplate), new PropertyMetadata(default(List<TemplateInfo>)));
        public List<TemplateInfo> DataList
        {
            get => (List<TemplateInfo>)GetValue(DataListProperty);
            set
            {
                SetValue(DataListProperty, value);
                OnPropertyChanged(nameof(DataList));
            }
        }
        #endregion

        public UcGenTemplate()
        {
            InitializeComponent();
            DataContext = this;

            TextContent.TextArea.TextEntered += TextContent_TextEntered;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextContent.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "C#");
            TextContent.TextArea.SelectionCornerRadius = 0;
            TextContent.TextArea.SelectionBorder = null;
            TextContent.TextArea.SelectionForeground = null;
            TextContent.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            LoadMenu();
        }

        private void TextContent_TextEntered(object sender, TextCompositionEventArgs e)
        {
            #region MyRegion
            if (completionWindow != null)
            {
                return;
            }
            Logger.Info($"ShowCompletion:1");
            var enterText = e.Text;
            // 这里可以加入更加智能的条件，例如判断前一个单词是不是SQL关键字。
            if (char.IsLetter(enterText[0]) || enterText == " " || enterText == "." || enterText == "@")
            {
                ShowCompletion(enterText, false);
            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enteredText"></param>
        /// <param name="controlSpace"></param>
        private void ShowCompletion(string enteredText, bool controlSpace)
        {
            #region MyRegion
            var textArea = TextContent.TextArea;
            var caret = textArea.Caret;
            var document = textArea.Document;

            // 获取光标前的文本
            var lineText = document.GetText(document.GetLineByNumber(caret.Line));
            var textUpToCaret = lineText.Substring(0, caret.Column-1);

            // 最简单的上下文分析: 分析光标前的非空字符
            // 实践中这可能要复杂得多，可能涉及到语法和词法的分析。

            completionWindow = new CompletionWindow(TextContent.TextArea);
            completionWindow.BorderThickness = new Thickness(0);
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;

            // 根据用户已经键入的文字，筛选出匹配的 SQL 关键字
            foreach (var dic in SysConst.Sys_GencodeKeywords)
            {
                var keyweod = dic.Key;
                var keyDesc = dic.Value;
                if (keyweod.IndexOf(enteredText, StringComparison.OrdinalIgnoreCase)>=0 || enteredText == " " || enteredText == "." || enteredText == "@")
                {
                    data.Add(new CsharpCompletionData(keyweod, null, keyDesc));
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
        /// 加载菜单
        /// </summary>
        /// <param name="searchName"></param>
        private void LoadMenu(string searchName = "")
        {
            #region MyRegion
            Task.Run(() =>
            {
                var sqLiteHelper = new SQLiteHelper();
                var datalist = sqLiteHelper.db.Table<TemplateInfo>().Where(x => x.TempName.Contains(searchName) && x.IsDel == false).ToList();
                Dispatcher.Invoke(() =>
                {
                    DataList = datalist;
                    NoDataText.Visibility = datalist.Any() ? Visibility.Collapsed : Visibility.Visible;
                });
            });
            #endregion
        }

        /// <summary>
        /// 重置系统模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void MenuReset_OnClick(object sender, RoutedEventArgs e)
        {
            var sqlLiteInstance = SQLiteHelper.GetInstance();
            sqlLiteInstance.Init(false);
            LoadMenu();
        }

        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuDelete_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (!(ListTemplate.SelectedItem is TemplateInfo selectedTemp))
            {
                Oops.Oh("请选择需要删除的模板.");
                return;
            }
            var sqlLiteInstance = SQLiteHelper.GetInstance();
            Task.Run(() =>
            {
                if (selectedTemp.Type == 1)
                {
                    selectedTemp.IsDel = true;
                    sqlLiteInstance.db.Update(selectedTemp);
                }
                else
                {
                    sqlLiteInstance.db.Delete<TemplateInfo>(selectedTemp.Id);
                }
                LoadMenu();
            });
            #endregion
        }

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var tempId = Convert.ToInt32(HidId.Text);
            var tempName = TextTempName.Text.Trim();
            var tempContent = TextContent.Text.Trim();
            var tempFileFormat = TextFileFormat.Text.Trim();
            var tempFileExt = TextFileExt.Text.Trim();
            var sqLiteHelper = new SQLiteHelper();
            if (string.IsNullOrEmpty(tempName))
            {
                Oops.Oh("模板名称为空");
                return;
            }
            if (!string.IsNullOrEmpty(tempFileFormat) && !tempFileFormat.Contains("{0}"))
            {
                Oops.Oh("名称格式有误，必须包含占位符：{0}");
                return;
            }
            if (string.IsNullOrEmpty(tempFileExt))
            {
                Oops.Oh("文件后缀为空");
                return;
            }
            if (string.IsNullOrEmpty(tempContent))
            {
                Oops.Oh("模板内容为空");
                return;
            }
            if (sqLiteHelper.IsAny<TemplateInfo>(x => x.Id != tempId && x.TempName == tempName))
            {
                Oops.Oh("模板名称出现重复");
                return;
            }
            var isSuccess = false;
            var temp = sqLiteHelper.FirstOrDefault<TemplateInfo>(x => x.Id == tempId);
            if (tempId > 0)
            {
                temp.Id = tempId;
                temp.TempName = tempName;
                temp.FileNameFormat = tempFileFormat ?? "{0}";
                temp.FileExt = tempFileExt;
                temp.Content = tempContent;
                isSuccess = sqLiteHelper.db.Update(temp) > 0;
            }
            else
            {
                temp = new TemplateInfo
                {
                    Id = tempId,
                    TempName = tempName,
                    FileNameFormat = tempFileFormat ?? "{0}",
                    FileExt = tempFileExt,
                    Content = tempContent
                };
                isSuccess = sqLiteHelper.db.Insert(temp) > 0;
                TextTempName.Text = string.Empty;
                TextContent.Text = string.Empty;
            }
            if (isSuccess)
            {
                Oops.Success("保存成功");
            }
            LoadMenu();
            #endregion
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = (GenCode)System.Windows.Window.GetWindow(this);
            mainWindow?.Close();
        }

        private void ListTemplate_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion
            var listBox = (ListBox)sender;
            if (listBox.SelectedItems.Count > 0)
            {
                var tempInfo = (TemplateInfo)listBox.SelectedItems[0];
                HidId.Text = tempInfo.Id.ToString();
                TextTempName.Text = tempInfo.TempName;
                TextFileFormat.Text = tempInfo.FileNameFormat;
                TextFileExt.Text = tempInfo.FileExt;
                TextContent.Text = tempInfo.Content;
            }
            #endregion
        }

        private void SearchMenu_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchName = SearchMenu.Text.Trim();
            LoadMenu(searchName);
        }

        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            HidId.Text = "0";
            TextTempName.Text = "";
            TextContent.Text = "";
            TextFileFormat.Text = "";
            TextFileExt.Text = "";
            ListTemplate.SelectedItem = null;
        }

        private void ListTemplate_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuDelete.Visibility = Visibility.Visible;
            if (!(ListTemplate.SelectedItem is TemplateInfo selectedTemp))
            {
                MenuDelete.Visibility = Visibility.Collapsed;
            }
        }
    }
}
