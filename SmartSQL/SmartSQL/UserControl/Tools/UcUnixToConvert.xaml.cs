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

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcUnixToConvert : BaseUserControl
    {
        public UcUnixToConvert()
        {
            InitializeComponent();
            DataContext = this;
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
            TextUnixTime.Value = unixTime;
        }

        private void BtnGen_Click(object sender, RoutedEventArgs e)
        {
            var unixTime = Convert.ToInt64(TextUnixTime.Value);
            var datetime = DateTimeHelper.UnixToDateTime(unixTime);
            TextDateTime.Text = datetime.ToString("F");
        }

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = (MainWindow)System.Windows.Window.GetWindow(this);
            parentWindow.UcMainTools.Content = new UcMainTools();
        }
    }
}
