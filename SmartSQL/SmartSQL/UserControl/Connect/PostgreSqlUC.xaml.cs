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
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Framework.Util;

namespace SmartSQL.UserControl.Connect
{
    /// <summary>
    /// SqlServerUC.xaml 的交互逻辑
    /// </summary>
    public partial class PostgreSqlUC : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty ConnectConfigProperty = DependencyProperty.Register(
            "ConnectConfig", typeof(ConnectConfigs), typeof(PostgreSqlUC), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 连接配置信息
        /// </summary>
        public ConnectConfigs ConnectConfig
        {
            get => (ConnectConfigs)GetValue(ConnectConfigProperty);
            set => SetValue(ConnectConfigProperty, value);
        }
        public PostgreSqlUC()
        {
            InitializeComponent();
        }

        private void PostgreSqlUC_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            var connect = ConnectConfig;
            var pwd = EncryptHelper.Decode(connect.Password);
            var defaultBase = new List<DataBase> { new DataBase { DbName = connect.DefaultDatabase } };
            PostgreSql_HidId.Text = connect.ID.ToString();
            PostgreSql_TextConnectName.Text = connect.ConnectName;
            PostgreSql_TextServerAddress.Text = connect.ServerAddress;
            PostgreSql_TextServerPort.Value = connect.ServerPort;
            PostgreSql_TextServerName.Text = connect.UserName;
            PostgreSql_TextServerPassword.Password = pwd;
            PostgreSql_ComboDefaultDatabase.ItemsSource = defaultBase;
            PostgreSql_ComboDefaultDatabase.SelectedItem = defaultBase.First();
        }

        /// <summary>
        /// 重置表单
        /// </summary>
        public bool VerifyForm()
        {
            var connectName = PostgreSql_TextConnectName.Text.Trim();
            var serverAddress = PostgreSql_TextServerAddress.Text.Trim();
            var serverPort = PostgreSql_TextServerPort.Value;
            var userName = PostgreSql_TextServerName.Text.Trim();
            var password = PostgreSql_TextServerPassword.Password.Trim();
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
        }
    }
}
