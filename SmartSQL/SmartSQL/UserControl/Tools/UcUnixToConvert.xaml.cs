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
using SmartSQL.UserControl.Tags;
using SmartSQL.Framework.Util;
using System.Dynamic;
using System.Windows.Threading;
using AduSkin.Controls.Metro;

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
