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
                toolBox= (ToolBox)parentWindow;
            }
            switch (selCard.Tag)
            {
                case "sqlformatter":
                    toolBox.UcBox.Content=new UcSqlFormatter();
                    //parentWindow.UcMainTools.Content = new UcSqlFormatter();
                    break;
                case "jsonformatter":
                    toolBox.UcBox.Content=new UcJsonFormatter();
                    //parentWindow.UcMainTools.Content = new UcJsonFormatter();
                    break;
                case "md5":
                    toolBox.UcBox.Content=new UcMD5();
                    //parentWindow.UcMainTools.Content = new UcMD5();
                    break;
                case "passwordGen":
                    toolBox.UcBox.Content=new UcPasswordGen();
                    //parentWindow.UcMainTools.Content = new UcPasswordGen(); 
                    break;
                case "uuidGen":
                    toolBox.UcBox.Content=new UcUUIDGen();
                    //parentWindow.UcMainTools.Content = new UcUUIDGen(); 
                    break;
                case "unixConvert":
                    toolBox.UcBox.Content=new UcUnixToConvert();
                    //parentWindow.UcMainTools.Content = new UcUnixToConvert(); 
                    break;
                case "base64":
                    toolBox.UcBox.Content=new UcBase64();
                    //parentWindow.UcMainTools.Content = new UcBase64(); 
                    break;
                case "nPinYin":
                    toolBox.UcBox.Content=new UcNPinYin();
                    //parentWindow.UcMainTools.Content = new UcNPinYin(); 
                    break;
                case "textInsert":
                    toolBox.UcBox.Content=new UcTextInsert();
                    //parentWindow.UcMainTools.Content = new UcTextInsert(); 
                    break;
                case "wordCount":
                    toolBox.UcBox.Content=new UcMD5();
                    //parentWindow.UcMainTools.Content = new UcWordCount(); 
                    break;
                default:
                    Oops.Oh("敬请期待"); return;
            }
            toolBox.Show();
        }
    }
}
