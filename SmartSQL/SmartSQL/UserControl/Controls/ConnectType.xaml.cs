using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace SmartSQL.UserControl.Controls
{
    /// <summary>
    /// ConnectType.xaml 的交互逻辑
    /// </summary>
    public partial class ConnectType : System.Windows.Controls.UserControl
    {

        public static readonly DependencyProperty DataBaseNameProperty = DependencyProperty.Register(
            "DataBaseName", typeof(string), typeof(ConnectType), new PropertyMetadata(default(string)));
        /// <summary>
        /// 名称
        /// </summary>
        public string DataBaseName
        {
            get => (string)GetValue(DataBaseNameProperty);
            set => SetValue(DataBaseNameProperty, value);
        }


        public static readonly DependencyProperty DataBaseIconProperty = DependencyProperty.Register(
            "DataBaseIcon", typeof(string), typeof(ConnectType), new PropertyMetadata(default(string)));
        /// <summary>
        /// 图标
        /// </summary>
        public string DataBaseIcon
        {
            get => (string)GetValue(DataBaseIconProperty);
            set => SetValue(DataBaseIconProperty, value);
        }


        public new static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled", typeof(bool), typeof(ConnectType), new PropertyMetadata(default(bool)));
        /// <summary>
        /// 是否可用
        /// </summary>
        public new bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }


        /// <summary>
        /// 单击事件
        /// </summary>
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent("ClickCard", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ConnectType));

        /// <summary>
        /// 点击卡片的操作.
        /// </summary>
        public event RoutedEventHandler ClickCard
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }


        public ConnectType()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 鼠标左键单击卡片事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Card_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }

        private void ConnectType_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsEnabled)
            {
                DataBaseName = $"{DataBaseName}(开发中)";
            }
        }
    }
}
