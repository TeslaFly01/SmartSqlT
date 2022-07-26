using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HandyControl.Controls;
using HandyControl.Data;
using SmartSQL.Framework;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Framework.Util;
using SmartSQL.Views;
using SqlSugar;
using Window = System.Windows.Window;

namespace SmartSQL.UserControl.Connect
{
    /// <summary>
    /// SqlServerUC.xaml 的交互逻辑
    /// </summary>
    public partial class MySqlUC : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty ConnectConfigProperty = DependencyProperty.Register(
            "ConnectConfig", typeof(ConnectConfigs), typeof(MySqlUC), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 连接配置信息
        /// </summary>
        public ConnectConfigs ConnectConfig
        {
            get => (ConnectConfigs)GetValue(ConnectConfigProperty);
            set => SetValue(ConnectConfigProperty, value);
        }
        public MySqlUC()
        {
            InitializeComponent();
        }

        private void MySqlUC_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded || ConnectConfig == null)
            {
                return;
            }
            var connect = ConnectConfig;
            var pwd = EncryptHelper.Decode(connect.Password);
            var defaultBase = new List<DataBase> { new DataBase { DbName = connect.DefaultDatabase } };
            MySql_HidId.Text = connect.ID.ToString();
            MySql_TextConnectName.Text = connect.ConnectName;
            MySql_TextServerAddress.Text = connect.ServerAddress;
            MySql_TextServerPort.Value = connect.ServerPort;
            MySql_TextServerName.Text = connect.UserName;
            MySql_TextServerPassword.Password = pwd;
            MySql_ComboDefaultDatabase.ItemsSource = defaultBase;
            MySql_ComboDefaultDatabase.SelectedItem = defaultBase.First();
        }

        /// <summary>
        /// 重置表单
        /// </summary>
        public bool VerifyForm()
        {
            #region MyRegion
            var connectName = MySql_TextConnectName.Text.Trim();
            var serverAddress = MySql_TextServerAddress.Text.Trim();
            var serverPort = MySql_TextServerPort.Value;
            var userName = MySql_TextServerName.Text.Trim();
            var password = MySql_TextServerPassword.Password.Trim();
            var tipMsg = new StringBuilder();
            if (string.IsNullOrEmpty(connectName))
            {
                tipMsg.Append("- 请填写连接名称" + Environment.NewLine);
            }
            if (string.IsNullOrEmpty(serverAddress))
            {
                tipMsg.Append("- 请填写服务器地址" + Environment.NewLine);
            }
            if (serverPort < 1)
            {
                tipMsg.Append("- 请填写端口号" + Environment.NewLine);
            }
            if (string.IsNullOrEmpty(userName))
            {
                tipMsg.Append("- 请填写登录名" + Environment.NewLine);
            }
            if (string.IsNullOrEmpty(password))
            {
                tipMsg.Append("- 请填写密码");
            }
            if (tipMsg.ToString().Length > 0)
            {
                Growl.WarningGlobal(new GrowlInfo { Message = tipMsg.ToString(), WaitTime = 1, ShowDateTime = false });
                return false;
            }
            return true; 
            #endregion
        }
        
        /// <summary>
        /// 测试连接
        /// </summary>
        /// <param name="isTest"></param>
        public void TestConnect(bool isTest)
        {
            #region MyRegion
            if (!VerifyForm())
            {
                return;
            }
            var mainWindow = (ConnectManage)Window.GetWindow(this);
            if (mainWindow == null)
            {
                return;
            }
            mainWindow.LoadingG.Visibility = Visibility.Visible;
            var connectId = Convert.ToInt32(MySql_HidId.Text);
            var connectionString = $"server={MySql_TextServerAddress.Text.Trim()};" +
                               $"port={MySql_TextServerPort.Value};" +
                               $"uid={MySql_TextServerName.Text.Trim()};" +
                               $"pwd={MySql_TextServerPassword.Password.Trim()};" +
                               $"Allow User Variables=True;sslmode=none;";
            Task.Run(() =>
            {
                var exporter = ExporterFactory.CreateInstance(DbType.MySql, connectionString);
                var list = exporter.GetDatabases();
                Dispatcher.Invoke(() =>
                {
                    MySql_ComboDefaultDatabase.ItemsSource = list;
                    if (connectId < 1)
                    {
                        MySql_ComboDefaultDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.Equals("mysql"));
                    }
                    else
                    {
                        var sqLiteHelper = new SQLiteHelper();
                        var connect = sqLiteHelper.db.Table<ConnectConfigs>().FirstOrDefault(x => x.ID == connectId);
                        if (connect != null)
                        {
                            MySql_ComboDefaultDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.Equals(connect.DefaultDatabase));
                        }
                    }
                    mainWindow.LoadingG.Visibility = Visibility.Collapsed;
                    if (isTest)
                    {
                        Growl.SuccessGlobal(new GrowlInfo { Message = $"连接成功", WaitTime = 1, ShowDateTime = false });
                    }
                });
            }); 
            #endregion
        }
    }
}
