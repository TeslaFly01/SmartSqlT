using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
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
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Models;
using System.Runtime.CompilerServices;
using SmartSQL.Framework;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Annotations;
using SmartSQL.DocUtils;
using SmartSQL.Framework.Const;
using SmartSQL.Helper;
using SmartSQL.UserControl;
using SmartSQL.Views;
using ComboBox = System.Windows.Controls.ComboBox;
using FontAwesome = FontAwesome.WPF.FontAwesome;
using TabControl = System.Windows.Controls.TabControl;
using TabItem = System.Windows.Controls.TabItem;
using HandyControl.Data;
using ICSharpCode.AvalonEdit;
using HandyControl.Controls;
using System.Xml;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcSqlFormatter : BaseUserControl
    {
        public UcSqlFormatter()
        {
            InitializeComponent();
            DataContext = this;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextEditor.TextArea.TextView.ElementGenerators.Add(new TruncateLongLines());
            TextEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "SQL");
            TextEditor.TextArea.SelectionCornerRadius = 0;
            TextEditor.TextArea.SelectionBorder = null;
            TextEditor.TextArea.SelectionForeground = null;
            TextEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
        }

        /// <summary>
        /// 格式化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFormatter_Click(object sender, RoutedEventArgs e)
        {
            TextEditor.Text = TextEditor.Text.SqlFormat();
        }

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = (MainWindow)System.Windows.Window.GetWindow(this);
            parentWindow.UcMainTools.Content = new UcMainTools();
        }

        /// <summary>
        /// 复制SQL脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCopyScript_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextEditor.Text))
            {
                TextEditor.SelectAll();
                Clipboard.SetDataObject(TextEditor.Text);
                Oops.Success("文本已复制到剪切板");
            }
        }

        /// <summary>
        /// 清空输入框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TextEditor.Text = string.Empty;
        }

        /// <summary>
        /// 编辑器获取焦点自动粘贴剪切板文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextEditor_GotFocus(object sender, RoutedEventArgs e)
        {
            var clipboardText = Clipboard.GetText();
            if (string.IsNullOrEmpty(TextEditor.Text) && !string.IsNullOrEmpty(clipboardText))
            {
                TextEditor.Text = clipboardText;
            }
        }
    }
}
