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
using SmartSQL.Annotations;
using SmartSQL.Models;

namespace SmartSQL.UserControl.Controls
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
            TxtNoData.Visibility = Visibility.Visible;
            ImgNoData.Visibility = Visibility.Visible;
            switch (ShowType)
            {
                case ShowType.Txt:
                    ImgNoData.Visibility = Visibility.Collapsed;
                    break;
                case ShowType.Img:
                    TxtNoData.Visibility = Visibility.Collapsed;
                    break;
                default:
                    TxtNoData.FontWeight = FontWeights.Normal;
                    TxtNoData.Margin = new Thickness(0, 75, 0, 0);
                    ImgNoData.Margin = new Thickness(0, 0, 0, 45);
                    break;
            }
        }
    }
}
