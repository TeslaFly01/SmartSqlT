using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text.Encodings.Web;
using System.Windows;
using SmartSQL.Helper;
using SmartSQL.Properties;
using SmartSQL.Views;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcLinuxCommand : BaseUserControl
    {
        public UcLinuxCommand()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void BaseUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var linuxJson = Resource.linux;
            var linuxCommands = System.Text.Json.JsonSerializer.Deserialize<List<LinuxCommand>>(linuxJson);
            TableGrid.ItemsSource = linuxCommands;
        }

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = (ToolBox)Window.GetWindow(this);
            parentWindow.UcBox.Content = new UcMainTools();
        }

        /// <summary>
        /// 实时搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            NoDataText.Visibility = Visibility.Collapsed;
            var linuxJson = Resource.linux;
            var linuxCommands = System.Text.Json.JsonSerializer.Deserialize<List<LinuxCommand>>(linuxJson);
            var searchText = SearchText.Text;
            if (string.IsNullOrEmpty(searchText))
            {
                TableGrid.ItemsSource = linuxCommands;
                return;
            }
            var mimeTypes = linuxCommands.Where(x => x.n.Contains(searchText) || x.d.Contains(searchText));
            TableGrid.ItemsSource = mimeTypes;
            if (!mimeTypes.Any())
            {
                NoDataText.Visibility = Visibility.Visible;
            }
        }
    }

    public class LinuxCommand
    {
        public string n { get; set; }
        public string p { get; set; }
        public string d { get; set; }
    }
}
