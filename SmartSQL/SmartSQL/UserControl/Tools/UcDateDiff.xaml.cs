using System;
using System.Collections.Generic;
using System.Windows;
using SmartSQL.Helper;
using SmartSQL.Views;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcDateDiff : BaseUserControl
    {

        private readonly List<int> HTimes = new List<int>
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            21, 22, 23
        };

        private readonly List<int> MTimes = new List<int>
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            21, 22, 23, 24, 25, 26, 27, 28, 29, 30,
            31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
            41, 42, 43, 44, 45, 46, 47, 48, 49, 50,
            51, 52, 53, 54, 55, 56, 57, 58, 59
        };

        public UcDateDiff()
        {
            InitializeComponent();
            DataContext = this;
            ComStartHTimes.ItemsSource = HTimes;
            ComStartMTimes.ItemsSource = MTimes;
            ComStartSTimes.ItemsSource = MTimes;
            ComEndHTimes.ItemsSource = HTimes;
            ComEndMTimes.ItemsSource = MTimes;
            ComEndSTimes.ItemsSource = MTimes;
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
        /// 计算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCalculate_OnClick(object sender, RoutedEventArgs e)
        {
            if (StartDate.SelectedDate == null)
            {
                Oops.Oh("请填写起始时间");
                return;
            }
            if (EndDate.SelectedDate == null)
            {
                Oops.Oh("请填写终止时间");
                return;
            }
            var startDate = StartDate.SelectedDate.Value;
            var startDateTime = new DateTime(startDate.Year, startDate.Month, startDate.Day,
                ComStartHTimes.SelectedIndex, ComStartMTimes.SelectedIndex, ComStartSTimes.SelectedIndex);
            var endDate = EndDate.SelectedDate.Value;
            var endDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, ComEndHTimes.SelectedIndex,
                ComEndMTimes.SelectedIndex, ComEndSTimes.SelectedIndex);
            var diffDate = endDateTime - startDateTime;
            TextCalculateResult.Text =
                $"起止时间相差：{Math.Abs(diffDate.Days)}天 {Math.Abs(diffDate.Hours)}小时 {Math.Abs(diffDate.Minutes)}分钟 {Math.Abs(diffDate.Seconds)}秒";
            BoxResult.Visibility = Visibility.Visible;
        }
    }
}
