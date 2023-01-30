using System;
using System.Windows;
using System.Windows.Media;
using SmartSQL.Helper;
using SmartSQL.Views;
using HandyControl.Data;
using System.Text.Json;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcJsonFormatter : BaseUserControl
    {
        public UcJsonFormatter()
        {
            InitializeComponent();
            DataContext = this;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextEditor.TextArea.TextView.ElementGenerators.Add(new TruncateLongLines());
            TextEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "Json");
            TextEditor.TextArea.SelectionCornerRadius = 0;
            TextEditor.TextArea.SelectionBorder = null;
            TextEditor.TextArea.SelectionForeground = null;
            TextEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
        }

        /// <summary>
        /// 格式化Json
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFormatter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TextEditor.Text))
                {
                    Oops.Oh("请输入Json文本");
                    return;
                }
                TextEditor.Text = StrUtil.JsonFormatter(TextEditor.Text);
            }
            catch (Exception)
            {
                Oops.Oh("Json解析失败，请检查");
            }
        }

        /// <summary>
        /// 压缩Json
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCompress_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TextEditor.Text))
                {
                    Oops.Oh("请输入Json文本");
                    return;
                }
                TextEditor.Text = StrUtil.JsonCompress(TextEditor.Text);
            }
            catch (Exception)
            {
                Oops.Oh("Json解析失败，请检查");
            }
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
        /// 复制文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCopy_Click(object sender, RoutedEventArgs e)
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
                var isTryParse = false;
                try
                {
                    JsonDocument.Parse(clipboardText); 
                    isTryParse = true;
                }
                catch { }
                if (isTryParse)
                {
                    TextEditor.Text = clipboardText;
                }
            }
        }
    }
}
