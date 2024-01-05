using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    public partial class SwitchButton : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(SwitchButton), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(string), typeof(SwitchButton), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            "IsChecked", typeof(bool), typeof(SwitchButton), new PropertyMetadata(default(bool)));

        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get => (string) GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        /// <summary>
        /// 描述
        /// </summary>
        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        /// <summary>
        /// 单击事件
        /// </summary>
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent("ClickCard", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SwitchButton));

        /// <summary>
        /// 点击卡片的操作.
        /// </summary>
        public event RoutedEventHandler ClickCard
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        public SwitchButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var border = (Border)sender;
            // 查找Border内的所有复选框
            ToggleButton togButton = FindVisualChildren<ToggleButton>(border).First();
            togButton.IsChecked=!IsChecked;
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }

        // 查找Visual子元素的泛型方法
        public static List<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            List<T> result = new List<T>();
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        result.Add((T)child);
                    }

                    result.AddRange(FindVisualChildren<T>(child));
                }
            }
            return result;
        }
    }
}
