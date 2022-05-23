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
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Models;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainConnect.xaml 的交互逻辑
    /// </summary>
    public partial class MainConnect :  BaseUserControl
    {
        public static readonly DependencyProperty ConnectConfigsProperty = DependencyProperty.Register(
            "ConnectConfigs", typeof(List<ConnectConfigs>), typeof(MainConnect), new PropertyMetadata(default(List<ConnectConfigs>)));

        /// <summary>
        /// 当前选中对象
        /// </summary>
        public List<ConnectConfigs> ConnectConfigs
        {
            get => (List<ConnectConfigs>)GetValue(ConnectConfigsProperty);
            set => SetValue(ConnectConfigsProperty, value);
        }

        public MainConnect()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void LoadPage()
        {

        }
    }
}
