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
using SmartSQL.DocUtils;
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
        /// 初始化加载页面
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
            #endregion
        }

        /// <summary>
        /// 保存连接
        /// </summary>
        public void SaveForm(bool isConnect)
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

            var connectId = Convert.ToInt32(MsSql_HidId.Text);
            var connectName = MsSql_TextConnectName.Text.Trim();
            var serverAddress = MsSql_TextServerAddress.Text.Trim();
            var serverPort = MsSql_TextServerPort.Value;
            var server = MsSql_TextServerAddress.Text.Trim().Equals(".")
                ? $"."
                : $"{MsSql_TextServerAddress.Text.Trim()},{MsSql_TextServerPort.Value}";
            var authentication = MsSql_ComboAuthentication.SelectedValue == SQLServer ? 1 : 0;
            var userName = MsSql_TextServerName.Text.Trim();
            var password = MsSql_TextServerPassword.Password.Trim();
            var defaultDataBase = (DataBase)MsSql_ComboDefaultDatabase.SelectedItem;
            var connectionString = $"server={server};" +
                               $"database=master;uid={userName};" +
                               $"pwd={password};";
            var sqLiteHelper = new SQLiteHelper();
            ConnectConfigs connectConfig;
            mainWindow.LoadingG.Visibility = Visibility.Visible;
            Task.Run(() =>
            {
                try
                {
                    if (isConnect)
                    {
                        var exporter = ExporterFactory.CreateInstance(DbType.SqlServer, connectionString);
                        exporter.GetDatabases();
                    }
                    Dispatcher.Invoke(() =>
                    {
                        mainWindow.LoadingG.Visibility = Visibility.Collapsed;
                        if (isConnect)
                        {
                            Growl.SuccessGlobal(new GrowlInfo { Message = $"连接成功", WaitTime = 1, ShowDateTime = false });
                        }
                        if (connectId > 0)
                        {
                            connectConfig = sqLiteHelper.db.Table<ConnectConfigs>().FirstOrDefault(x => x.ID == connectId);
                            if (connectConfig == null)
                            {
                                Growl.WarningGlobal(new GrowlInfo { Message = $"当前连接不存在或已被删除", WaitTime = 1, ShowDateTime = false });
                                return;
                            }
                            var connectAny = sqLiteHelper.db.Table<ConnectConfigs>().FirstOrDefault(x => x.ConnectName == connectName && x.ID != connectId);
                            if (connectAny != null)
                            {
                                Growl.WarningGlobal(new GrowlInfo { Message = $"已存在相同名称的连接名", WaitTime = 1, ShowDateTime = false });
                                return;
                            }
                            connectConfig.ConnectName = connectName;
                            connectConfig.DbType = DbType.SqlServer;
                            connectConfig.ServerAddress = serverAddress;
                            connectConfig.ServerPort = Convert.ToInt32(serverPort);
                            connectConfig.UserName = userName;
                            connectConfig.Password = EncryptHelper.Encode(password);
                            connectConfig.DefaultDatabase = defaultDataBase.DbName;
                            connectConfig.Authentication = authentication;
                            sqLiteHelper.db.Update(connectConfig);
                        }
                        else
                        {
                            var connect = sqLiteHelper.db.Table<ConnectConfigs>().FirstOrDefault(x => x.ConnectName.ToLower() == connectName.ToLower());
                            if (connect != null)
                            {
                                Growl.WarningGlobal(new GrowlInfo { Message = $"已存在相同名称的连接名", WaitTime = 1, ShowDateTime = false });
                                return;
                            }
                            connectConfig = new ConnectConfigs()
                            {
                                ConnectName = connectName,
                                DbType = DbType.SqlServer,
                                ServerAddress = serverAddress,
                                ServerPort = Convert.ToInt32(serverPort),
                                Authentication = authentication,
                                UserName = userName,
                                Password = EncryptHelper.Encode(password),
                                CreateDate = DateTime.Now,
                                DefaultDatabase = defaultDataBase.DbName

                            };
                            sqLiteHelper.db.Insert(connectConfig);
                        }

                        Task.Run(() =>
                        {
                            var datalist = sqLiteHelper.db.Table<ConnectConfigs>().
                                ToList();
                            Dispatcher.Invoke(() =>
                            {
                                mainWindow.DataList = datalist;
                                if (!isConnect)
                                {
                                    Growl.SuccessGlobal(new GrowlInfo { Message = $"保存成功", WaitTime = 1, ShowDateTime = false });
                                }
                                //if (isConnect && mainWindow.ChangeRefreshEvent != null)
                                //{
                                //   mainWindow.ChangeRefreshEvent(connectConfig);
                                //    mainWindow.Close();
                                //}
                            });
                        });
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        mainWindow.LoadingG.Visibility = Visibility.Collapsed;
                        Growl.WarningGlobal(new GrowlInfo { Message = $"连接失败\r" + ex.ToMsg(), WaitTime = 1, ShowDateTime = false });
                    });
                }
            }); 
            #endregion
        }
        
        /// <summary>
        /// 刷新数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFresh_OnClick(object sender, RoutedEventArgs e)
        {
            TestConnect(false);
        }
    }
}
