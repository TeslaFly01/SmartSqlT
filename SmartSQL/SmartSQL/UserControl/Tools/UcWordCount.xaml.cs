using System;
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
    public partial class UcWordCount : BaseUserControl
    {
        public UcWordCount()
        {
            InitializeComponent();
            DataContext = this;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextEditor.TextArea.TextView.ElementGenerators.Add(new TruncateLongLines());
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
                    ///JsonDocument.Parse(clipboardText); 
                    isTryParse = true;
                }
                catch { }
                if (isTryParse)
                {
                    TextEditor.Text = clipboardText;
                }
            }
        }

        private void TextEditor_OnTextChanged(object sender, EventArgs e)
        {
            int iAllChr = 0; //字符总数：不计字符'\n'和'\r'
            int iChineseChr = 0; //中文字符计数
            int iChinesePnct = 0;//中文标点计数
            int iEnglishChr = 0; //英文字符计数
            int iEnglishPnct = 0;//中文标点计数
            int iNumber = 0;  //数字字符：0-9
            foreach (char ch in TextEditor.Text)
            {
                if (ch != '\n' && ch != '\r')
                {
                    iAllChr++;
                }
                if ("～！＠＃￥％…＆（）—＋－＝".IndexOf(ch) != -1 || "｛｝【】：“”；‘'《》，。、？｜＼".IndexOf(ch) != -1)
                {
                    iChinesePnct++;
                }
                if (ch >= 0x4e00 && ch <= 0x9fbb)
                {
                    iAllChr++;
                    iChineseChr++;
                }
                if ("`~!@#$%^&*()_+-={}[]:\";'<>,.?/\\|".IndexOf(ch) != -1)
                {
                    iEnglishPnct++;
                }
                if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z'))
                {
                    iEnglishChr++;
                }
                if (ch >= '0' && ch <= '9')
                {
                    iNumber++;
                }
            }

            ShieldHanZi.Status = iChineseChr;
            ShieldZiMu.Status = iEnglishChr;
            ShieldShuZi.Status = iNumber;
            ShieldBiaoDian.Status = iChinesePnct + iEnglishPnct;
            ShieldTotalZiShu.Status = iChineseChr + iEnglishChr;
            ShieldTotalZiFu.Status = iAllChr;
        }
    }
}
