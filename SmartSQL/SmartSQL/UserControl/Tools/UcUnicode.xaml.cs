using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using SmartSQL.Helper;
using SmartSQL.Views;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcUnicode : BaseUserControl
    {
        public UcUnicode()
        {
            InitializeComponent();
            DataContext = this;
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

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TextInput.Text = string.Empty;
            TextOutput.Text = string.Empty;
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnEncode_Click(object sender, RoutedEventArgs e)
        {
            var inputText = TextInput.Text;
            if (inputText == string.Empty)
            {
                return;
            }
            var rText = GB2312UnicodeConverter.ToGB2312(inputText);
            if (string.IsNullOrEmpty(rText))
            {
                rText = inputText;
            }
            TextOutput.Text = rText;
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDecode_Click(object sender, RoutedEventArgs e)
        {
            var inputText = TextInput.Text;
            if (inputText == string.Empty)
            {
                return;
            }
            try
            {
                var rText = GB2312UnicodeConverter.ToUnicode(inputText);
                TextOutput.Text = rText;
            }
            catch (Exception ex)
            {
                TextOutput.Text = ex.Message;
            }
        }

        private void BtnExchange_Click(object sender, RoutedEventArgs e)
        {
            var inputText = TextInput.Text;
            var outputText = TextOutput.Text;
            if (inputText == string.Empty && outputText == string.Empty)
            {
                return;
            }
            TextInput.Text = outputText;
            TextOutput.Text = inputText;
        }
    }

    public class GB2312UnicodeConverter
    {
        /// <summary>
        /// 汉字转换为Unicode编码
        /// </summary>
        /// <param name="str">要编码的汉字字符串</param>
        /// <returns>Unicode编码的的字符串</returns>
        public static string ToUnicode(string str)
        {
            byte[] bts = Encoding.Unicode.GetBytes(str);
            string r = "";
            for (int i = 0; i < bts.Length; i += 2) r += "\\u" + bts[i + 1].ToString("x").PadLeft(2, '0') + bts[i].ToString("x").PadLeft(2, '0');
            return r;
        }
        /// <summary>
        /// 将Unicode编码转换为汉字字符串
        /// </summary>
        /// <param name="str">Unicode编码字符串</param>
        /// <returns>汉字字符串</returns>
        public static string ToGB2312(string str)
        {
            string r = "";
            MatchCollection mc = Regex.Matches(str, @"\\u([\w]{2})([\w]{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            byte[] bts = new byte[2];
            foreach (Match m in mc)
            {
                bts[0] = (byte)int.Parse(m.Groups[2].Value, NumberStyles.HexNumber);
                bts[1] = (byte)int.Parse(m.Groups[1].Value, NumberStyles.HexNumber);
                r += Encoding.Unicode.GetString(bts);
            }
            return r;
        }
    }
}
