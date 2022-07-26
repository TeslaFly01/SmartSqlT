using System;
using System.Collections.Generic;
using System.Linq;
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
using SmartSQL.UserControl.Controls;
using SmartSQL.Views;

namespace SmartSQL.UserControl.Connect
{
    /// <summary>
    /// SqlServerUC.xaml 的交互逻辑
    /// </summary>
    public partial class ConnectMainUC : System.Windows.Controls.UserControl
    {
        public ConnectMainUC()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 选中连接类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectType_OnClickCard(object sender, RoutedEventArgs e)
        {
            var senderInfo = (ConnectType)sender;
            var mainWindow = (ConnectManageExt)Window.GetWindow(this);
            if (mainWindow == null)
            {
                return;
            }
            switch (senderInfo.DataBaseName)
            {
                case "SQLServer":
                    mainWindow.MainContent = new SqlServerUC();
                    break;
                case "MySQL":
                    mainWindow.MainContent = new MySqlUC();
                    break;
                case "PostgreSQL":
                    mainWindow.MainContent = new PostgreSqlUC();
                    break;
                default:
                    return;
            }
        }
    }
}
