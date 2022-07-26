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
    public partial class SqlServerUC : System.Windows.Controls.UserControl
    {

        public static readonly DependencyProperty ConnectConfigProperty = DependencyProperty.Register(
            "ConnectConfig", typeof(ConnectConfigs), typeof(SqlServerUC), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 连接配置信息
        /// </summary>
        public ConnectConfigs ConnectConfig
        {
            get => (ConnectConfigs)GetValue(ConnectConfigProperty);
            set => SetValue(ConnectConfigProperty, value);
        }

        public SqlServerUC()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 首次加载页面数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SqlServerUC_OnLoaded(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded || ConnectConfig == null)
            {
                return;
            }
            var connect = ConnectConfig;
            var pwd = EncryptHelper.Decode(connect.Password);
            var defaultBase = new List<DataBase> { new DataBase { DbName = connect.DefaultDatabase } };
            MsSql_HidId.Text = connect.ID.ToString();
            MsSql_TextConnectName.Text = connect.ConnectName;
            MsSql_TextServerAddress.Text = connect.ServerAddress;
            MsSql_TextServerPort.Value = connect.ServerPort;
            MsSql_TextServerName.Text = connect.UserName;
            MsSql_ComboAuthentication.SelectedItem = connect.Authentication == 1 ? SQLServer : Windows;
            MsSql_TextServerPassword.Password = pwd;
            MsSql_ComboDefaultDatabase.ItemsSource = defaultBase;
            MsSql_ComboDefaultDatabase.SelectedItem = defaultBase.First();
            #endregion
        }

        /// <summary>
        /// 重置表单
        /// </summary>
        public bool VerifyForm()
        {
            #region MyRegion
            var connectName = MsSql_TextConnectName.Text.Trim();
            var serverAddress = MsSql_TextServerAddress.Text.Trim();
            var serverPort = MsSql_TextServerPort.Value;
            var userName = MsSql_TextServerName.Text.Trim();
            var password = MsSql_TextServerPassword.Password.Trim();
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

        public void TestConnect(bool isTest)
        {
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
            var connectId = Convert.ToInt32(MsSql_HidId.Text);
            var serAddr = MsSql_TextServerAddress.Text.Trim().Equals(".")
                ? $"{MsSql_TextServerAddress.Text.Trim()}"
                : $"{MsSql_TextServerAddress.Text.Trim()},{MsSql_TextServerPort.Value}";
            var connectionString = $"server={serAddr};" +
                                "database=master;" +
                                $"uid={MsSql_TextServerName.Text.Trim()};" +
                                $"pwd={MsSql_TextServerPassword.Password.Trim()};";
            Task.Run(() =>
            {
                var exporter = ExporterFactory.CreateInstance(DbType.SqlServer, connectionString);
                var list = exporter.GetDatabases();
                Dispatcher.Invoke(() =>
                {
                    MsSql_ComboDefaultDatabase.ItemsSource = list;
                    if (connectId < 1)
                    {
                        MsSql_ComboDefaultDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.Equals("master"));
                    }
                    else
                    {
                        var sqLiteHelper = new SQLiteHelper();
                        var connect = sqLiteHelper.db.Table<ConnectConfigs>().FirstOrDefault(x => x.ID == connectId);
                        if (connect != null)
                        {
                            MsSql_ComboDefaultDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.Equals(connect.DefaultDatabase));
                        }
                    }
                    mainWindow.LoadingG.Visibility = Visibility.Collapsed;
                    if (isTest)
                    {
                        Growl.SuccessGlobal(new GrowlInfo { Message = $"连接成功", WaitTime = 1, ShowDateTime = false });
                    }
                });
            });

        }
    }
}
