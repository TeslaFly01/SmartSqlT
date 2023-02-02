using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using SmartSQL.Helper;
using SmartSQL.Views;
using HandyControl.Data;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcJWT : BaseUserControl
    {
        public UcJWT()
        {
            InitializeComponent();
            DataContext = this;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextInput.TextArea.TextView.ElementGenerators.Add(new TruncateLongLines());
            TextInput.TextArea.SelectionCornerRadius = 0;
            TextInput.TextArea.SelectionBorder = null;
            TextInput.TextArea.SelectionForeground = null;
            TextInput.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
            TextHeader.TextArea.TextView.ElementGenerators.Add(new TruncateLongLines());
            TextHeader.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "Json");
            TextHeader.TextArea.SelectionCornerRadius = 0;
            TextHeader.TextArea.SelectionBorder = null;
            TextHeader.TextArea.SelectionForeground = null;
            TextHeader.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
            TextPayLoad.TextArea.TextView.ElementGenerators.Add(new TruncateLongLines());
            TextPayLoad.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "Json");
            TextPayLoad.TextArea.SelectionCornerRadius = 0;
            TextPayLoad.TextArea.SelectionBorder = null;
            TextPayLoad.TextArea.SelectionForeground = null;
            TextPayLoad.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
        }

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = (ToolBox)Window.GetWindow(this);
            parentWindow.UcBox.Content = new UcMainTools();
        }

        /// <summary>
        /// 清空输入框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TextInput.Text = string.Empty;
            TextHeader.Text = string.Empty;
            TextPayLoad.Text = string.Empty;
        }

        private void TextInput_OnTextChanged(object sender, EventArgs e)
        {
            try
            {
                var text = TextInput.Text;
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }
                if (!text.Contains("."))
                {
                    Oops.Oh("JWT格式有误");
                    return;
                }
                if (text.StartsWith("Bearer "))
                {
                    text = text.Replace("Bearer ", "");
                }
                var texts = text.Split('.');
                if (texts.Count() > 3)
                {
                    Oops.Oh("JWT格式有误");
                    return;
                }
                var header = StrUtil.Base46_Decode(BaseUrl(texts[0]));
                var payLoad = StrUtil.Base46_Decode(BaseUrl(texts[1]));
                var status = StrUtil.Base46_Decode(BaseUrl(texts[2]));
                TextHeader.Text = StrUtil.JsonFormatter(header);
                TextPayLoad.Text = StrUtil.JsonFormatter(payLoad);
            }
            catch (Exception)
            {
                Oops.God("JWT格式有误");
            }
        }

        private string BaseUrl(string text)
        {
            char[] padding = { '=' };
            text = text.TrimEnd(padding).Replace('-', '+').Replace('_', '/').Replace(" ", "+");
            //base字符串必须被4整除，不足的在末尾填充'='号
            if (text.Length % 4 > 0)
            {
                text = text.PadRight(text.Length + 4 - text.Length % 4, '=');
            }
            return text;
        }
    }
}
