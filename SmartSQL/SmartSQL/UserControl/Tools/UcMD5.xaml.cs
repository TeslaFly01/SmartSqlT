using System.Windows;
using System.Windows.Controls;
using SmartSQL.Helper;
using SmartSQL.Views;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcMD5 : BaseUserControl
    {
        public UcMD5()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void BtnEncrypt_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextInput.Text))
            {
                Oops.Oh("请输入加密文本");
                return;
            }
            var textU32 = TextEncryptHelper.GetMD5_32(TextInput.Text);
            var textU16 = TextEncryptHelper.GetMD5_16(TextInput.Text);
            TextOutputU16.Text = textU16;
            TextOutputL16.Text = textU16.ToLower();
            TextOutputU32.Text = textU32;
            TextOutputL32.Text = textU32.ToLower();
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = (ToolBox)System.Windows.Window.GetWindow(this);
            parentWindow.UcBox.Content = new UcMainTools();
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            var btnCopy = (Button)sender;
            var copyText = string.Empty;
            switch (btnCopy.Tag)
            {
                case "L16":
                    copyText = TextOutputL16.Text; break;
                case "U16":
                    copyText = TextOutputU16.Text; break;
                case "L32":
                    copyText = TextOutputL32.Text; break;
                case "U32":
                    copyText = TextOutputU32.Text; break;
            }
            if (copyText != string.Empty)
            {
                Clipboard.SetDataObject(copyText);
                Oops.Success("文本已复制到剪切板");
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TextInput.Text = string.Empty;
        }
    }
}
