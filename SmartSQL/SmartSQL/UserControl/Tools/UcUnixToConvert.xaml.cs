using System;
using System.Windows;
using SmartSQL.Helper;
using SmartSQL.Views;
using System.Windows.Threading;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcUnixToConvert : BaseUserControl
    {
        DispatcherTimer timerHeartBeat = new DispatcherTimer();
        public UcUnixToConvert()
        {
            InitializeComponent();
            DataContext = this;
            timerHeartBeat.Tick += SendHeartBeatToServer;
            timerHeartBeat.Interval = TimeSpan.FromSeconds(1);
            timerHeartBeat.Start();
        }

        private void SendHeartBeatToServer(object sender, EventArgs e)
        {
            var unixTime = DateTimeHelper.DateTimeToUnix(DateTime.Now);
            TextNowUnixTime.Text = unixTime.ToString();
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNowStop_Click(object sender, RoutedEventArgs e)
        {
            BtnNowStart.IsEnabled = true;
            BtnNowRefresh.IsEnabled = true;
            BtnNowStop.IsEnabled = false;
            timerHeartBeat.Stop();
        }

        /// <summary>
        /// 开始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNowStart_Click(object sender, RoutedEventArgs e)
        {
            BtnNowStart.IsEnabled = false;
            BtnNowRefresh.IsEnabled = false;
            BtnNowStop.IsEnabled = true;
            timerHeartBeat.Start();
        }

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNowRefresh_Click(object sender, RoutedEventArgs e)
        {
            var unixTime = DateTimeHelper.DateTimeToUnix(DateTime.Now);
            TextNowUnixTime.Text = unixTime.ToString();
        }

        /// <summary>
        /// 初始化加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BaseUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var nowTime = DateTime.Now;
            var unixTime = DateTimeHelper.DateTimeToUnix(nowTime);
            //TextUnixTime.Value = unixTime;
            //TextDateTime.Text = nowTime.ToString("F");
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
        /// Unix时间转换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnUnixConvert_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextUnixTime.Text))
            {
                Oops.Oh("请输入Unix时间戳");
                return;
            }
            var unixTime = Convert.ToInt64(TextUnixTime.Text);
            var datetime = DateTimeHelper.UnixToDateTime(unixTime);
            TextUnixResult.Text = datetime.ToString("yyyy/MM/dd HH:mm:ss");
        }

        /// <summary>
        /// 北京时间转换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBjConvert_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBjTime.Text))
            {
                Oops.Oh("请输入北京时间");
                return;
            }
            if (!DateTime.TryParse(TextBjTime.Text, out var bjTime))
            {
                Oops.Oh("时间格式不正确");
                return; ;
            }
            var datetime = DateTimeHelper.DateTimeToUnix(bjTime);
            TextBjResult.Text = datetime.ToString();
        }
    }
}
