using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;

namespace SmartSQL.Views
{
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            DataContext = this;
            CopyRight = DateTime.Now.Year == 2021 ? $"Copyright ©{DateTime.Now.Year} zfluok" : $"Copyright ©2021-{DateTime.Now.Year} zfluok";
            Version = $"Version {version.ToString()}";
        }

        public static readonly DependencyProperty CopyRightProperty = DependencyProperty.Register(
            "CopyRight", typeof(string), typeof(AboutWindow), new PropertyMetadata(default(string)));

        public string CopyRight
        {
            get => (string)GetValue(CopyRightProperty);
            set => SetValue(CopyRightProperty, value);
        }

        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
            "Version", typeof(string), typeof(AboutWindow), new PropertyMetadata(default(string)));

        public string Version
        {
            get => (string)GetValue(VersionProperty);
            set => SetValue(VersionProperty, value);
        }

        private void LinkContact_OnClick(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(link?.NavigateUri.AbsoluteUri);
        }
    }
}
