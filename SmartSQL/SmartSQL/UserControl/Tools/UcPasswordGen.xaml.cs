using System;
using System.Collections.Generic;
using System.Windows;
using SmartSQL.Models;
using SmartSQL.Helper;
using SmartSQL.Views;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcPasswordGen : BaseUserControl
    {
        /// <summary>
        /// 结果数据列表
        /// </summary>
        public static readonly DependencyProperty PasswordListProperty = DependencyProperty.Register(
            "PasswordList", typeof(List<PasswordResultDTO>), typeof(UcPasswordGen), new PropertyMetadata(default(List<PasswordResultDTO>)));
        public List<PasswordResultDTO> PasswordList
        {
            get => (List<PasswordResultDTO>)GetValue(PasswordListProperty);
            set
            {
                SetValue(PasswordListProperty, value);
                OnPropertyChanged(nameof(PasswordList));
            }
        }

        public UcPasswordGen()
        {
            InitializeComponent();
            DataContext = this;
            TableGrid.PreviewMouseWheel += (sender, e) =>
            {
                var eventArg = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = System.Windows.UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                this.TableGrid.RaiseEvent(eventArg);
            };
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = (ToolBox)System.Windows.Window.GetWindow(this);
            parentWindow.UcBox.Content = new UcMainTools();
        }

        private void BtnGen_Click(object sender, RoutedEventArgs e)
        {
            var pwdNums = Convert.ToInt32(TextPwdNums.Value);
            var pwdLens = Convert.ToInt32(TextPwdLens.Value);
            var isNums = CheckNums.IsChecked;
            var isLowerCase = CheckLowerCase.IsChecked;
            var isUpperCase = CheckUpperCase.IsChecked;
            var isSymbol = CheckSymbol.IsChecked;
            var pwds = new List<PasswordResultDTO>();

            var strPwds = StrUtil.GeneratePassword(pwdLens, pwdNums, isNums.Value, isLowerCase.Value, isUpperCase.Value, isSymbol.Value);
            strPwds.ForEach(pwd =>
            {
                pwds.Add(new PasswordResultDTO
                {
                    Password = pwd,
                    PasswordStrength = StrUtil.PasswordStrength(pwd)
                });
            });
            PasswordList = pwds;
        }

        /// <summary>
        /// 复制行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCopyRow_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = (PasswordResultDTO)TableGrid.CurrentItem;
            if (selectedItem != null)
            {
                Clipboard.SetDataObject(selectedItem.Password);
                Oops.Success("文本已复制到剪切板");
            }
        }
    }
}
