using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using SmartSQL.Helper;
using SmartSQL.Views;
using hyjiacan.py4n;
using ToolGood.Words;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcRMB : BaseUserControl
    {
        public UcRMB()
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

        /// <summary>
        /// 清理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TextInput.Text = string.Empty;
            TextOutput.Text = string.Empty;
        }

        private void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            var inputText = TextInput.Text;
            if (string.IsNullOrEmpty(inputText))
            {
                return;
            }
            var inputMoney = 0d;
            if (!double.TryParse(inputText, out inputMoney))
            {
                Oops.Oh("包含无效的字符");
                return;
            }
            TextOutput.Text = WordsHelper.ToChineseRMB(inputMoney);
        }
    }
}
