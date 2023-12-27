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
using SmartSQL.DocUtils;
using SmartSQL.Framework;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Framework.Util;
using SmartSQL.Helper;
using SmartSQL.Views;
using SqlSugar;
using Window = System.Windows.Window;

namespace SmartSQL.UserControl.Connect
{
    /// <summary>
    /// MySqlUC.xaml 的交互逻辑
    /// </summary>
    public partial class RedisUC : System.Windows.Controls.UserControl
    {
        public event ConnectChangeRefreshHandlerExt ChangeRefreshEvent;

        public static readonly DependencyProperty ConnectConfigProperty = DependencyProperty.Register(
            "ConnectConfig", typeof(ConnectConfigs), typeof(RedisUC), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 连接配置信息
        /// </summary>
        public ConnectConfigs ConnectConfig
        {
            get => (ConnectConfigs)GetValue(ConnectConfigProperty);
            set => SetValue(ConnectConfigProperty, value);
        }
        public RedisUC()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化加载页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MySqlUC_OnLoaded(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded || ConnectConfig == null)
            {
                return;
            }
            var connect = ConnectConfig;
            var pwd = EncryptHelper.Decode(connect.Password);
            var defaultBase = new List<DataBase> { new DataBase { DbName = connect.DefaultDatabase } };
            HidId.Text = connect.ID.ToString();
            TextConnectName.Text = connect.ConnectName;
            TextServerAddress.Text = connect.ServerAddress;
            TextServerPort.Value = connect.ServerPort;
            // TextServerName.Text = connect.UserName;
            TextServerPassword.Password = pwd;
            #endregion
        }

        /// <summary>
        /// 重置表单
        /// </summary>
        public bool VerifyForm()
        {
            #region MyRegion
            var connectName = TextConnectName.Text.Trim();
            var serverAddress = TextServerAddress.Text.Trim();
            var serverPort = TextServerPort.Value;
            var password = TextServerPassword.Password.Trim();
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
            //if (string.IsNullOrEmpty(userName))
            //{
            //    tipMsg.Append("- 请填写登录名" + Environment.NewLine);
            //}
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
            var connectId = Convert.ToInt32(HidId.Text);
            var connectionString = ConnectionStringUtil.RedisString(TextServerAddress.Text.Trim(),
                Convert.ToInt32(TextServerPort.Value), string.Empty,
                "", EncryptHelper.Encode(TextServerPassword.Password.Trim()));
            Task.Run(() =>
            {
                try
                {
                    var exporter = ExporterFactory.CreateInstance(DbType.Redis, connectionString);
                    var list = exporter.GetDatabases();
                    Dispatcher.Invoke(() =>
                    {
                        mainWindow.LoadingG.Visibility = Visibility.Collapsed;
                        if (isTest)
                        {
                            Oops.Success("连接成功");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Oops.God($"连接失败，原因：" + ex.ToMsg());
                        mainWindow.LoadingG.Visibility = Visibility.Collapsed;
                    }));
                }
            });
            #endregion
        }

        /// <summary>
        /// 保存表单
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
            var connectId = Convert.ToInt32(HidId.Text);
            var connectName = TextConnectName.Text.Trim();
            var serverAddress = TextServerAddress.Text.Trim();
            var serverPort = Convert.ToInt32(TextServerPort.Value);
            var userName = "";// TextServerName.Text.Trim();
            var password = EncryptHelper.Encode(TextServerPassword.Password.Trim());
            var defaultDataBase = new DataBase { DbName = "DB0" };
            var connectionString = ConnectionStringUtil.RedisString(serverAddress, serverPort, string.Empty, userName, password);

            var sqLiteHelper = new SQLiteHelper();
            ConnectConfigs connectConfig;

            mainWindow.LoadingG.Visibility = Visibility.Visible;
            Task.Run(() =>
            {
                try
                {
                    if (isConnect)
                    {
                        var exporter = ExporterFactory.CreateInstance(DbType.Redis, connectionString);
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
                            connectConfig.DbType = DbType.Redis;
                            connectConfig.ServerAddress = serverAddress;
                            connectConfig.ServerPort = serverPort;
                            connectConfig.UserName = userName;
                            connectConfig.Password = password;
                            connectConfig.DefaultDatabase = defaultDataBase.DbName;
                            connectConfig.Authentication = 1;
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
                                DbType = DbType.Redis,
                                ServerAddress = serverAddress,
                                ServerPort = serverPort,
                                Authentication = 1,
                                UserName = userName,
                                Password = password,
                                CreateDate = DateTime.Now,
                                DefaultDatabase = defaultDataBase == null ? "MySql" : defaultDataBase.DbName

                            };
                            sqLiteHelper.db.Insert(connectConfig);
                        }

                        Task.Run(() =>
                        {
                            var datalist = sqLiteHelper.db.Table<ConnectConfigs>().
                                ToList();
                            Dispatcher.Invoke(() =>
                            {
                                mainWindow.NoDataText.Visibility = Visibility.Collapsed;
                                mainWindow.DataList = datalist;
                                if (!isConnect)
                                {
                                    Growl.SuccessGlobal(new GrowlInfo { Message = $"保存成功", WaitTime = 1, ShowDateTime = false });
                                }
                                if (isConnect && ChangeRefreshEvent != null)
                                {
                                    ChangeRefreshEvent(connectConfig);
                                    mainWindow.Close();
                                }
                            });
                        });
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Oops.God($"连接失败，原因：" + ex.ToMsg());
                        mainWindow.LoadingG.Visibility = Visibility.Collapsed;
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
