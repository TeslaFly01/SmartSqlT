using System.Windows;
using System.Windows.Media;
using SmartSQL.Framework;
using SmartSQL.Helper;
using SmartSQL.Views;
using HandyControl.Data;
using ICSharpCode.AvalonEdit;

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
            var parentWindow = (ToolBox)System.Windows.Window.GetWindow(this);
            parentWindow.UcBox.Content = new UcMainTools();
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
    }
}
