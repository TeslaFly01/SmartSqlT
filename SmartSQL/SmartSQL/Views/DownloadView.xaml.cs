using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SmartSQL.Views
{
    public partial class DownloadView
    {
        public DownloadView()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            //容器Grid
            Grid grid = this.Owner.Content as Grid;
            //父级窗体原来的内容
            UIElement original = VisualTreeHelper.GetChild(grid, 0) as UIElement;
            //将父级窗体原来的内容在容器Grid中移除
            grid.Children.Remove(original);
            //赋给父级窗体
            this.Owner.Content = original;
            //var mainWindow = (GenCode)Window.GetWindow(this);
            this.Close();
        }
    }
}
