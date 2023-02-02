using System;
using System.Text;
using System.Windows;
using SimpleBase;
using SmartSQL.Helper;
using SmartSQL.Views;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcHex : BaseUserControl
    {
        public UcHex()
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
            var parentWindow = (ToolBox)System.Windows.Window.GetWindow(this);
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
            byte[] myBuffer = Encoding.Default.GetBytes(inputText);
            string result = Base16.EncodeLower(myBuffer); // encode to lowercase
            TextOutput.Text = result;
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
                var myBuffer =  Base16.Decode(inputText);
                TextOutput.Text = Encoding.Default.GetString(myBuffer.ToArray());
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

        /// <summary>
        /// 字符串转16进制
        /// </summary>
        /// <param name="_str">字符串</param>
        /// <param name="encode">编码格式</param>
        /// <returns></returns>
        private static string StrToHex(string Text)
        {
            byte[] buffer = Encoding.Default.GetBytes(Text);
            string result = string.Empty;
            foreach (char c in buffer)
            {
                result += Convert.ToString(c, 16);
            }
            return result.ToUpper();
        }

        /// <summary>
        /// 16进制转字符串
        /// </summary>
        /// <param name="hex">16进制字符</param>
        /// <param name="encode">编码格式</param>
        /// <returns></returns>
        private static string HexToStr(string hex)
        {
            byte[] buffer = new byte[hex.Length / 2];
            string result = string.Empty;
            for (int i = 0; i < hex.Length / 2; i++)
            {
                result = hex.Substring(i * 2, 2);
                buffer[i] = Convert.ToByte(result, 16);
            }
            return Encoding.Default.GetString(buffer);
        }
    }
}
