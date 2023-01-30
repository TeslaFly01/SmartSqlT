using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RestSharp;
using SmartSQL.Helper;
using SmartSQL.Views;
using Brush = System.Windows.Media.Brush;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcStoryOfWords : BaseUserControl
    {
        public UcStoryOfWords()
        {
            InitializeComponent();
            DataContext = this;
        }

        private readonly List<string> Colors = new List<string>
        {
            "#F44336","#E91E63","#9C27B0","#673AB7","#3F51B5","#2196F3","#03A9F4","#00BCD4","#009688",
            "#4CAF50","#8BC34A","#CDDC39","#FFEB3B","#FFC107","#FF9800","#FF5722","#795548","#9E9E9E"
        };

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

        private void UcStoryOfWords_OnLoaded(object sender, RoutedEventArgs e)
        {
            CreateText();
        }

        /// <summary>
        /// 换一句
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReplace_OnClick(object sender, RoutedEventArgs e)
        {
            CreateText();
        }

        private void CreateText()
        {
            Task.Run(() =>
            {
                RestClient client = new RestClient("https://v1.hitokoto.cn/?encode=json");
                RestRequest request = new RestRequest();
                var result = client.Execute(request);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var hitokotoResult = JsonSerializer.Deserialize<Hitokoto>(result.Content);
                    Dispatcher.Invoke(() =>
                    {
                        TextContent.Text = hitokotoResult.hitokoto;
                        TextAuthor.Text = $"——{hitokotoResult.from_who}「{hitokotoResult.from}」";
                        //创建随机数，并在集合总数中随机取出一个
                        int r = new Random().Next(Colors.Count);
                        var backgroudColor = Colors[r];
                        BrushConverter brushConverter = new BrushConverter();
                        Brush brush = (Brush)brushConverter.ConvertFromString(backgroudColor);
                        //Box.Background = brush;
                    });
                }
            });
        }
    }

    public class Hitokoto
    {
        public int id { get; set; }
        public string uuid { get; set; }
        public string hitokoto { get; set; }
        public string type { get; set; }
        public string from { get; set; }
        public string from_who { get; set; }
        public string creator { get; set; }
        public int creator_uid { get; set; }
        public int reviewer { get; set; }
        public string commit_from { get; set; }
        public string created_at { get; set; }
        public int length { get; set; }
    }
}
