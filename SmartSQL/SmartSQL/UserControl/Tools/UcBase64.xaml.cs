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
    public partial class UcBase64 : BaseUserControl
    {
        public UcBase64()
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
            var rText = StrUtil.Base46_Encode(inputText);
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
                var rText = StrUtil.Base46_Decode(inputText);
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
}
