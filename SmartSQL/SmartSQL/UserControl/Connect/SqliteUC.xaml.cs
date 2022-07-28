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
using SmartSQL.Views;

namespace SmartSQL.UserControl.Connect
{
    /// <summary>
    /// SqliteUC.xaml 的交互逻辑
    /// </summary>
    public partial class SqliteUC : System.Windows.Controls.UserControl
    {
        public event ConnectChangeRefreshHandlerExt ChangeRefreshEvent;
        public SqliteUC()
        {
            InitializeComponent();
        }
    }
}
