using System.Windows;
using SmartSQL.Helper;
using SmartSQL.Views;
using SmartSQL.UserControl.Controls;

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
                case "qrCode":
                    toolBox.UcBox.Content = new UcQrCode();
                    title = "二维码生成";
                    break;
                case "base64ToImg":
                    toolBox.UcBox.Content = new UcBase64ToImg();
                    title = "图片转base64";
                    break;
                case "rgbToHex":
                    toolBox.UcBox.Content = new UcRgbToHex();
                    title = "RGB颜色转换";
                    break;
                case "storyOfWords":
                    toolBox.UcBox.Content = new UcStoryOfWords();
                    title = "文字物语";
                    break;
                case "urlEncode":
                    toolBox.UcBox.Content = new UcUrlEncode();
                    title = "Url编码/解码";
                    break;
                case "mimeType":
                    toolBox.UcBox.Content = new UcMimeType();
                    title = "MimeType对照表";
                    break;
                case "unicode":
                    toolBox.UcBox.Content = new UcUnicode();
                    title = "Unicode中文互转";
                    break;
                case "hexEncode":
                    toolBox.UcBox.Content = new UcHex();
                    title = "Hex编码/解码";
                    break;
                case "linuxCommand":
                    toolBox.UcBox.Content = new UcLinuxCommand();
                    title = "Linux命令大全";
                    break;
                case "fanti":
                    toolBox.UcBox.Content = new UcFanTi();
                    title = "繁简转换";
                    break;
                case "rmb":
                    toolBox.UcBox.Content = new UcRMB();
                    title = "人民币大写转换";
                    break;
                case "ico":
                    toolBox.UcBox.Content = new UcIcoToConvert();
                    title = "Ico图标生成";
                    break;
                case "icon":
                    toolBox.UcBox.Content = new UcIcon();
                    title = "Icon图标";
                    break;
                case "json2csharp":
                    toolBox.UcBox.Content = new UcJson2CSharp();
                    title = "Json转C#实体";
                    break;
                case "barCode":
                    toolBox.UcBox.Content = new UcQrCode();
                    title = "条形码生成";
                    break;
                default:
                    Oops.Oh("敬请期待"); return;
            }
            toolBox.Title = $"{title} - 开发工具箱";
            toolBox.Show();
        }
    }
}
