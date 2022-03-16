using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using SmartCode.Annotations;
using SmartCode.Models;

namespace SmartCode.UserControl.Controls
{
    /// <summary>
    /// NoDataArea.xaml 的交互逻辑
    /// </summary>
    public partial class NoDataArea : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public static readonly DependencyProperty TipTextProperty = DependencyProperty.Register(
            "TipText", typeof(string), typeof(NoDataArea), new PropertyMetadata(default(string)));
        /// <summary>
        /// 提示文字
        /// </summary>
        public string TipText
        {
            get => (string)GetValue(TipTextProperty);
            set
            {
                SetValue(TipTextProperty, value);
                OnPropertyChanged(nameof(TipText));
            }
        }

        public static readonly DependencyProperty ShowTypeProperty = DependencyProperty.Register(
            "ShowType", typeof(ShowType), typeof(NoDataArea), new PropertyMetadata(default(ShowType)));
        /// <summary>
        /// 提示类型
        /// </summary>
        public ShowType ShowType
        {
            get => (ShowType)GetValue(ShowTypeProperty);
            set => SetValue(ShowTypeProperty, value);
        }

        public NoDataArea()
        {
            InitializeComponent();
            DataContext = this;
            if (string.IsNullOrEmpty(TipText))
            {
                TipText = "暂无数据";
            }
        }

        private void NoDataArea_OnLoaded(object sender, RoutedEventArgs e)
        {
            TxtNoData.Visibility = ShowType == ShowType.Txt ? Visibility.Visible : Visibility.Collapsed;
            ImgNoData.Visibility = ShowType == ShowType.Img ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
