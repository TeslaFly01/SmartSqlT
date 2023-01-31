using System;
using System.Security.Policy;
using System.Text.Encodings.Web;
using System.Windows;
using SmartSQL.Helper;
using SmartSQL.Views;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcUrlEncode : BaseUserControl
    {
        public UcUrlEncode()
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
            var rText = System.Net.WebUtility.UrlEncode(inputText);
            TextOutput.Text = rText;
        }

        private void BtnDecode_Click(object sender, RoutedEventArgs e)
        {
            var inputText = TextInput.Text;
            if (inputText == string.Empty)
            {
                return;
            }
            try
            {
                var rText = System.Net.WebUtility.UrlDecode(inputText);
                TextOutput.Text = rText;
            }
            catch (Exception ex)
            {
                TextOutput.Text = ex.Message;
            }
        }
    }
}
