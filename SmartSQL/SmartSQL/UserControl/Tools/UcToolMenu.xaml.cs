using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
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
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Models;
using System.Runtime.CompilerServices;
using SmartSQL.Framework;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Annotations;
using SmartSQL.DocUtils;
using SmartSQL.Framework.Const;
using SmartSQL.Helper;
using SmartSQL.UserControl;
using SmartSQL.Views;
using ComboBox = System.Windows.Controls.ComboBox;
using FontAwesome = FontAwesome.WPF.FontAwesome;
using TabControl = System.Windows.Controls.TabControl;
using TabItem = System.Windows.Controls.TabItem;
using SmartSQL.Views.Category;
using SmartSQL.UserControl.Controls;
using Org.BouncyCastle.Tls;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcToolMenu : BaseUserControl
    {
        public UcToolMenu()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UcToolCard_ClickCard(object sender, RoutedEventArgs e)
        {
            var selCard = (UcToolCard)sender;
            var toolBox = new ToolBox();
            var parentWindow = Window.GetWindow(this);
            if (parentWindow is ToolBox)
            {
                toolBox = (ToolBox)parentWindow;
            }
            var title = "";
            switch (selCard.Tag)
            {
                case "sqlformatter":
                    toolBox.UcBox.Content = new UcSqlFormatter();
                    title = "SQL格式化";
                    break;
                case "jsonformatter":
                    toolBox.UcBox.Content = new UcJsonFormatter();
                    title = "Json格式化";
                    break;
                case "md5":
                    toolBox.UcBox.Content = new UcMD5();
                    title = "MD5文本加密";
                    break;
                case "passwordGen":
                    toolBox.UcBox.Content = new UcPasswordGen();
                    title = "密码生成器";
                    break;
                case "uuidGen":
                    toolBox.UcBox.Content = new UcUUIDGen();
                    title = "UUID生成器";
                    break;
                case "unixConvert":
                    toolBox.UcBox.Content = new UcUnixToConvert();
                    title = "Unix时间戳转换";
                    break;
                case "base64":
                    toolBox.UcBox.Content = new UcBase64();
                    title = "Base64编码/解码";
                    break;
                case "nPinYin":
                    toolBox.UcBox.Content = new UcNPinYin();
                    title = "汉字转拼音";
                    break;
                case "textInsert":
                    toolBox.UcBox.Content = new UcTextInsert();
                    title = "文本两端插入字符";
                    break;
                case "wordCount":
                    toolBox.UcBox.Content = new UcWordCount();
                    title = "字数统计";
                    break;
                case "jwt":
                    toolBox.UcBox.Content = new UcJWT();
                    title = "JWT解码器";
                    break;
                case "dateDiff":
                    toolBox.UcBox.Content = new UcDateDiff();
                    title = "时间差计算";
                    break;
                default:
                    Oops.Oh("敬请期待"); return;
            }
            toolBox.Title = $"{title} - 开发工具箱";
            toolBox.Show();
        }
    }
}
