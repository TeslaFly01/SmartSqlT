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
using System.Windows.Threading;

namespace SmartSQL.UserControl.Controls
{
    /// <summary>
    /// Loading.xaml 的交互逻辑
    /// </summary>
    public partial class UcToolCard : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(UcToolCard), new PropertyMetadata(default(string)));

        /// <summary>
        /// 当前选中对象
        /// </summary>
        public string Title
        {
            get => (string) GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }


        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(string), typeof(UcToolCard), new PropertyMetadata(default(string)));

        /// <summary>
        /// 当前选中对象
        /// </summary>
        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>
        /// 单击事件
        /// </summary>
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent("ClickCard", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UcToolCard));

        /// <summary>
        /// 点击卡片的操作.
        /// </summary>
        public event RoutedEventHandler ClickCard
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        public UcToolCard()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }
    }
}
