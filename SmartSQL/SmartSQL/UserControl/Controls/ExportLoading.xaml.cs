using SmartSQL.Annotations;
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
using System.Windows.Threading;

namespace SmartSQL.UserControl.Controls
{
    /// <summary>
    /// Loading.xaml 的交互逻辑
    /// </summary>
    public partial class ExportLoading : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static readonly DependencyProperty BackgroundOpacityProperty = DependencyProperty.Register(
            "BackgroundOpacity", typeof(double), typeof(ExportLoading), new PropertyMetadata(default(double)));

        public static readonly DependencyProperty ProgressTitleProperty = DependencyProperty.Register(
            "ProgressTitle", typeof(string), typeof(ExportLoading), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ProgressTitleNumProperty = DependencyProperty.Register(
            "ProgressTitleNum", typeof(string), typeof(ExportLoading), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ProgressNumProperty = DependencyProperty.Register(
            "ProgressNum", typeof(double), typeof(ExportLoading), new PropertyMetadata(default(double)));

        /// <summary>
        /// 背景透明度
        /// </summary>
        public double BackgroundOpacity
        {
            get => (double)GetValue(BackgroundOpacityProperty);
            set => SetValue(BackgroundOpacityProperty, value);
        }

        /// <summary>
        /// 进度标题
        /// </summary>
        public string ProgressTitle
        {
            get => (string)GetValue(ProgressTitleProperty);
            set => SetValue(ProgressTitleProperty, value);
        }

        /// <summary>
        /// 进度标题
        /// </summary>
        public string ProgressTitleNum
        {
            get => (string)GetValue(ProgressTitleNumProperty);
            set => SetValue(ProgressTitleNumProperty, value);
        }

        /// <summary>
        /// 进度值
        /// </summary>
        public double ProgressNum
        {
            get => (double)GetValue(ProgressNumProperty);
            set
            {
                SetValue(ProgressNumProperty, value);
                OnPropertyChanged(nameof(ProgressNum));
            }
        }

        public ExportLoading()
        {
            InitializeComponent();
            DataContext = this;
            animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, Dispatcher);
            animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 90);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressTitle = "正在为您准备导出文档，请耐心等候";
        }

        #region Data  
        private readonly DispatcherTimer animationTimer;
        #endregion

        #region Private Methods  
        private void Start()
        {
            animationTimer.Tick += HandleAnimationTick;
            animationTimer.Start();
        }

        private void Stop()
        {
            animationTimer.Stop();
            animationTimer.Tick -= HandleAnimationTick;
        }

        private void HandleAnimationTick(object sender, EventArgs e)
        {
            //SpinnerRotate.Angle = (SpinnerRotate.Angle + 36) % 360;
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            const double offset = Math.PI;
            const double step = Math.PI * 2 / 10.0;

            //SetPosition(C0, offset, 0.0, step);
            //SetPosition(C1, offset, 1.0, step);
            //SetPosition(C2, offset, 2.0, step);
            //SetPosition(C3, offset, 3.0, step);
            //SetPosition(C4, offset, 4.0, step);
            //SetPosition(C5, offset, 5.0, step);
            //SetPosition(C6, offset, 6.0, step);
            //SetPosition(C7, offset, 7.0, step);
            //SetPosition(C8, offset, 8.0, step);
        }

        private void SetPosition(Ellipse ellipse, double offset,
            double posOffSet, double step)
        {
            ellipse.SetValue(Canvas.LeftProperty, 50.0
                + Math.Sin(offset + posOffSet * step) * 50.0);

            ellipse.SetValue(Canvas.TopProperty, 50
                + Math.Cos(offset + posOffSet * step) * 50.0);
        }

        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void HandleVisibleChanged(object sender,
            DependencyPropertyChangedEventArgs e)
        {
            bool isVisible = (bool)e.NewValue;

            if (isVisible)
            {
                Start();
            }
            else
            {
                Stop();
                ProgressTitle="正在为您准备导出文档，请耐心等候";
                ProgressNum=0;
            }
        }
        #endregion

        private void ProgressBar_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //ProgressTitleText.Text = $"正在导出 {ProgressTitle}";
        }
    }
}
