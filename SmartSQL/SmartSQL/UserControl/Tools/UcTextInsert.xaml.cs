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
using hyjiacan.py4n;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcTextInsert : BaseUserControl
    {
        public UcTextInsert()
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

        /// <summary>
        /// 清除文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TextInput.Text = string.Empty;
            TextOutput.Text = string.Empty;
            TextInputStart.Text = string.Empty;
            TextInputEnd.Text = string.Empty;
        }

        /// <summary>
        /// 文本变更事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextInput_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var inputTextNums = TextInput.Text.Length;
            var surplusTextNums = 5000 - inputTextNums;
            TextSurplusNum.Text = surplusTextNums.ToString();
            ProgressBarTextNum.Value = inputTextNums;

            if (!string.IsNullOrEmpty(TextInput.Text))
            {
                var outText = new StringBuilder();
                var inputText = this.TextInput.Text.Trim();

                string[] lineSplit = inputText.Split(new[] { "\n" }, StringSplitOptions.None);
                var count = 0;
                foreach (var item in lineSplit)
                {
                    var itemValue = item;
                    if (string.IsNullOrEmpty(item))
                    {
                        continue;
                    }
                    if (item.Contains("\r"))
                    {
                        itemValue = item.Replace("\r", "");
                    }
                    count++;
                    if (string.IsNullOrEmpty(TextInputStart.Text.Trim()) && string.IsNullOrEmpty(TextInputEnd.Text.Trim()))
                    {
                        outText.Append("'" + itemValue + "',\n");
                    }
                    else
                    {
                        outText.Append(TextInputStart.Text.Trim() + itemValue + TextInputEnd.Text.Trim() + "\n");
                    }
                }
                var result = lineSplit.Length > 1 ? outText.ToString().TrimEnd(new char[] { ',', '\n' }) : outText.ToString();
                TextOutput.Text = result;
                TextOutput.SelectAll();
            }
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCopy_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextOutput.Text))
            {
                return;
            }
            TextOutput.SelectAll();
            Clipboard.SetDataObject(TextOutput.Text);
            Oops.Success("文本已复制到剪切板");
        }
    }
}
