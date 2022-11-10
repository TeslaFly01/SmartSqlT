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
    public partial class UcNPinYin : BaseUserControl
    {
        public UcNPinYin()
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

        ///// <summary>
        ///// 不指定格式
        ///// </summary>
        //None,
        ///// <summary>
        ///// 首字母大写，此选项对 a e o i u 几个独音无效
        ///// </summary>
        //CAPITALIZE_FIRST_LETTER = 1 << 1,
        ///// <summary>
        ///// 全小写
        ///// </summary>
        //LOWERCASE = 1 << 2,
        ///// <summary>
        ///// 全大写
        ///// </summary>
        //UPPERCASE = 1 << 3,
        ///// <summary>
        ///// 将 ü 输出为 u:
        ///// </summary>
        //WITH_U_AND_COLON = 1 << 4,
        ///// <summary>
        ///// 将 ü 输出为 v
        ///// </summary>
        //WITH_V = 1 << 5,
        ///// <summary>
        ///// 将 ü 输出为 ü
        ///// </summary>
        //WITH_U_UNICODE = 1 << 6,
        ///// <summary>
        ///// 将 ü 输出为 yu
        ///// </summary>
        //WITH_YU = 1 << 10,
        ///// <summary>
        ///// 带声调标志
        ///// </summary>
        //WITH_TONE_MARK = 1 << 7,
        ///// <summary>
        ///// 不带声调
        ///// </summary>
        //WITHOUT_TONE = 1 << 8,
        ///// <summary>
        ///// 带声调数字值
        ///// </summary>
        //WITH_TONE_NUMBER = 1 << 9,
        /// <summary>
        /// 开始转换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            var inputText = TextInput.Text;
            if (string.IsNullOrEmpty(inputText))
            {
                return;
            }
            //var hanziText = Convert.ToChar(inputText);
            //// 判断是否是汉字
            //if (PinyinUtil.IsHanzi(hanziText))
            //{
            //    return;
            //}
            // 设置拼音输出格式
            //PinyinFormat format = PinyinFormat.WITHOUT_TONE | PinyinFormat.LOWERCASE | PinyinFormat.WITH_TONE_MARK | PinyinFormat.WITH_U_UNICODE;
            PinyinFormat format = PinyinFormat.None;
            var selectPyType = (ComboBoxItem)ComPinYinType.SelectedItem;
            if (selectPyType.Content.Equals("小写"))
            {
                format |= PinyinFormat.LOWERCASE;
            }
            if (selectPyType.Content.Equals("大写"))
            {
                format |= PinyinFormat.UPPERCASE;
            }
            if (selectPyType.Content.Equals("首字母大写"))
            {
                format |= PinyinFormat.CAPITALIZE_FIRST_LETTER;
            }
            //标注声调
            if (CheckPyMark.IsChecked == true)
            {
                format |= PinyinFormat.WITH_TONE_MARK;
            }
            else
            {
                format |= PinyinFormat.WITHOUT_TONE;
            }

            // 取指定汉字的唯一或者第一个拼音
            var pinyinText = Pinyin4Net.GetPinyinArray(inputText, format);
            var tex = new StringBuilder();
            pinyinText.ForEach(py =>
            {
                if (py.Any())
                {
                    tex.Append(py.First() + " ");
                }
                else
                {
                    tex.Append(py.RawChar);
                }
            });
            TextOutput.Text = tex.ToString();
        }

        private void TextInput_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var inputTextNums = TextInput.Text.Length;
            var surplusTextNums = 2000 - inputTextNums;
            TextSurplusNum.Text = surplusTextNums.ToString();
            ProgressBarTextNum.Value = inputTextNums;
        }
    }
}
