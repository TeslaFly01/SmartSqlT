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
using ICSharpCode.AvalonEdit;

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
            var parentWindow = (MainWindow)System.Windows.Window.GetWindow(this);
            parentWindow.UcMainTools.Content = new UcMainTools();
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
