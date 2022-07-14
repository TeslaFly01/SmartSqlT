using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace SmartSQL.Views
{
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();

            var assemblyDescription = typeof(AboutWindow).Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), true)[0] as AssemblyDescriptionAttribute;

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            DataContext = this;
            Description = assemblyDescription.Description;
            CopyRight = DateTime.Now.Year == 2021 ? $"Copyright ©{DateTime.Now.Year} zfluok" : $"Copyright ©2021-{DateTime.Now.Year} zfluok";
            Version = $"v{version.ToString()}";
        }

        #region Description
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(string), typeof(AboutWindow), new PropertyMetadata(default(string)));
        /// <summary>
        /// 版权信息
        /// </summary>
        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        #endregion

        #region CopyRight
        public static readonly DependencyProperty CopyRightProperty = DependencyProperty.Register(
            "CopyRight", typeof(string), typeof(AboutWindow), new PropertyMetadata(default(string)));
        /// <summary>
        /// 版权信息
        /// </summary>
        public string CopyRight
        {
            get => (string)GetValue(CopyRightProperty);
            set => SetValue(CopyRightProperty, value);
        }
        #endregion

        #region Version
        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
            "Version", typeof(string), typeof(AboutWindow), new PropertyMetadata(default(string)));
        /// <summary>
        /// 版本号信息
        /// </summary>
        public string Version
        {
            get => (string)GetValue(VersionProperty);
            set => SetValue(VersionProperty, value);
        }
        #endregion
        
        private void SvgViewboxForkMe_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(new ProcessStartInfo("https://gitee.com/izhaofu/SmartSQL"));
        }
    }
}
