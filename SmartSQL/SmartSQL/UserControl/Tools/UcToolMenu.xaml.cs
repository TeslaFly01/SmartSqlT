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
            var parentWindow = (MainWindow)Window.GetWindow(this);
            switch (selCard.Tag)
            {
                case "sqlformatter":
                    parentWindow.UcMainTools.Content = new UcSqlFormatter(); break;
                case "jsonformatter":
                    parentWindow.UcMainTools.Content = new UcJsonFormatter(); break;
                case "md5":
                    parentWindow.UcMainTools.Content = new UcMD5(); break;
                case "passwordGen":
                    parentWindow.UcMainTools.Content = new UcPasswordGen(); break;
                case "uuidGen":
                    parentWindow.UcMainTools.Content = new UcUUIDGen(); break;
                case "unixConvert":
                    parentWindow.UcMainTools.Content = new UcUnixToConvert(); break;
                case "base64":
                    parentWindow.UcMainTools.Content = new UcBase64(); break;
                case "nPinYin":
                    parentWindow.UcMainTools.Content = new UcNPinYin(); break;
                case "textInsert":
                    parentWindow.UcMainTools.Content = new UcTextInsert(); break;
                default:
                    Oops.Oh("敬请期待"); return;
            }
        }
    }
}
