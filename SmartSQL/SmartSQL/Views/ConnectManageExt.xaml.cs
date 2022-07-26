using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Controls;
using HandyControl.Data;
using SmartSQL.Framework;
using SmartSQL.Framework.Exporter;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Framework.Util;
using SmartSQL.Annotations;
using SmartSQL.DocUtils;
using SmartSQL.Helper;
using SmartSQL.UserControl.Connect;
using SqlSugar;
using DbType = SqlSugar.DbType;

namespace SmartSQL.Views
{
    //定义委托
    public delegate void ConnectChangeRefreshHandlerExt(ConnectConfigs connectConfig);
    /// <summary>
    /// GroupManage.xaml 的交互逻辑
    /// </summary>
    public partial class ConnectManageExt : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event ConnectChangeRefreshHandlerExt ChangeRefreshEvent;
        public ConnectManageExt()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region DependencyProperty

        public static readonly DependencyProperty DataListProperty = DependencyProperty.Register(
            "DataList", typeof(List<ConnectConfigs>), typeof(ConnectManage), new PropertyMetadata(default(List<ConnectConfigs>)));
        public List<ConnectConfigs> DataList
        {
            get => (List<ConnectConfigs>)GetValue(DataListProperty);
            set
            {
                SetValue(DataListProperty, value);
                OnPropertyChanged(nameof(DataList));
            }
        }


        public static readonly DependencyProperty MainContentProperty = DependencyProperty.Register(
            "MainContent", typeof(System.Windows.Controls.UserControl), typeof(ConnectManageExt), new PropertyMetadata(default(System.Windows.Controls.UserControl)));

        public System.Windows.Controls.UserControl MainContent
        {
            get => (System.Windows.Controls.UserControl)GetValue(MainContentProperty);
            set => SetValue(MainContentProperty, value);
        }
        #endregion

        private void GroupManage_OnLoaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                var sqLiteHelper = new SQLiteHelper();
                var datalist = sqLiteHelper.db.Table<ConnectConfigs>().ToList();
                Dispatcher.Invoke(() =>
                {
                    DataList = datalist;
                });
            });
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (listBox.SelectedItems.Count > 0)
            {
                var connect = (ConnectConfigs)listBox.SelectedItems[0];
                var pwd = EncryptHelper.Decode(connect.Password);
                var defaultBase = new List<DataBase> { new DataBase { DbName = connect.DefaultDatabase } };
                switch (connect.DbType)
                {
                    case DbType.SqlServer:
                        MainContent = new SqlServerUC();
                        //TabSqlServer.IsSelected = true;
                        //MsSql_HidId.Text = connect.ID.ToString();
                        //MsSql_TextConnectName.Text = connect.ConnectName;
                        //MsSql_TextServerAddress.Text = connect.ServerAddress;
                        //MsSql_TextServerPort.Value = connect.ServerPort;
                        //MsSql_TextServerName.Text = connect.UserName;
                        //MsSql_ComboAuthentication.SelectedItem = connect.Authentication == 1 ? SQLServer : Windows;
                        //MsSql_TextServerPassword.Password = pwd;
                        //MsSql_ComboDefaultDatabase.ItemsSource = defaultBase;
                        //MsSql_ComboDefaultDatabase.SelectedItem = defaultBase.First();
                        break;
                    case DbType.MySql:
                        MainContent = new MySqlUC();
                        //TabMySql.IsSelected = true;
                        //MySql_HidId.Text = connect.ID.ToString();
                        //MySql_TextConnectName.Text = connect.ConnectName;
                        //MySql_TextServerAddress.Text = connect.ServerAddress;
                        //MySql_TextServerPort.Value = connect.ServerPort;
                        //MySql_TextServerName.Text = connect.UserName;
                        //MySql_TextServerPassword.Password = pwd;
                        //MySql_ComboDefaultDatabase.ItemsSource = defaultBase;
                        //MySql_ComboDefaultDatabase.SelectedItem = defaultBase.First();
                        break;
                    case DbType.PostgreSQL:
                        MainContent = new PostgreSqlUC();
                        //TabPostgreSql.IsSelected = true;
                        //PostgreSql_HidId.Text = connect.ID.ToString();
                        //PostgreSql_TextConnectName.Text = connect.ConnectName;
                        //PostgreSql_TextServerAddress.Text = connect.ServerAddress;
                        //PostgreSql_TextServerPort.Value = connect.ServerPort;
                        //PostgreSql_TextServerName.Text = connect.UserName;
                        //PostgreSql_TextServerPassword.Password = pwd;
                        //PostgreSql_ComboDefaultDatabase.ItemsSource = defaultBase;
                        //PostgreSql_ComboDefaultDatabase.SelectedItem = defaultBase.First();
                        break;
                }
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (!CheckConnectForm())
            {
                return;
            }
            var tag = ((Button)sender).Tag;
            var isConnect = tag != null && (string)tag == $"Connect";
            var connectId = 0;
            var connectName = "";
            var serverAddress = "";
            var serverPort = 1433d;
            var authentication = 1;
            var userName = "";
            var password = "";
            var defaultDataBase = new DataBase();
            var connectionString = "";
            var dbType = DbType.SqlServer;
            //if (TabSqlServer.IsSelected)
            //{
            //    dbType = DbType.SqlServer;
            //    //connectId = Convert.ToInt32(MsSql_HidId.Text);
            //    //connectName = MsSql_TextConnectName.Text.Trim();
            //    //serverAddress = MsSql_TextServerAddress.Text.Trim().Equals(".")
            //    //    ? $"."
            //    //    : $"{MsSql_TextServerAddress.Text.Trim()},{MsSql_TextServerPort.Value}";
            //    //serverPort = MsSql_TextServerPort.Value;
            //    //authentication = MsSql_ComboAuthentication.SelectedValue == SQLServer ? 1 : 0;
            //    //userName = MsSql_TextServerName.Text.Trim();
            //    //password = MsSql_TextServerPassword.Password.Trim();
            //    //defaultDataBase = (DataBase)MsSql_ComboDefaultDatabase.SelectedItem;
            //    connectionString = $"server={serverAddress};" +
            //                       $"database=master;uid={userName};" +
            //                       $"pwd={password};";
            //}
            //if (TabMySql.IsSelected)
            //{
            //    dbType = DbType.MySql;
            //    //connectId = Convert.ToInt32(MySql_HidId.Text);
            //    //connectName = MySql_TextConnectName.Text.Trim();
            //    //serverAddress = MySql_TextServerAddress.Text.Trim();
            //    //serverPort = MySql_TextServerPort.Value;
            //    //userName = MySql_TextServerName.Text.Trim();
            //    //password = MySql_TextServerPassword.Password.Trim();
            //    //defaultDataBase = (DataBase)MySql_ComboDefaultDatabase.SelectedItem;
            //    connectionString = $"server={serverAddress};" +
            //                       $"port={serverPort};" +
            //                       $"uid={userName};" +
            //                       $"pwd={password};" +
            //                       $"Allow User Variables=True;sslmode=none;";
            //}
            //if (TabPostgreSql.IsSelected)
            //{
            //    dbType = DbType.PostgreSQL;
            //    //connectId = Convert.ToInt32(PostgreSql_HidId.Text);
            //    //connectName = PostgreSql_TextConnectName.Text.Trim();
            //    //serverAddress = PostgreSql_TextServerAddress.Text.Trim();
            //    //serverPort = PostgreSql_TextServerPort.Value;
            //    //userName = PostgreSql_TextServerName.Text.Trim();
            //    //password = PostgreSql_TextServerPassword.Password.Trim();
            //    //defaultDataBase = (DataBase)PostgreSql_ComboDefaultDatabase.SelectedItem;
            //    connectionString = $"HOST={serverAddress};" +
            //                       $"PORT={serverPort};" +
            //                       $"DATABASE=postgres;" +
            //                       $"USER ID={userName};" +
            //                       $"PASSWORD={password}";
            //}
            var sqLiteHelper = new SQLiteHelper();
            ConnectConfigs connectConfig;

            LoadingG.Visibility = Visibility.Visible;
            Task.Run(() =>
            {
                try
                {
                    if (isConnect)
                    {
                        var exporter = ExporterFactory.CreateInstance(dbType, connectionString);
                        exporter.GetDatabases();
                    }
                    Dispatcher.Invoke(() =>
                    {
                        LoadingG.Visibility = Visibility.Collapsed;
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
                            connectConfig.DbType = dbType;
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
                                DbType = dbType,
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
                                DataList = datalist;
                                if (!isConnect)
                                {
                                    Growl.SuccessGlobal(new GrowlInfo { Message = $"保存成功", WaitTime = 1, ShowDateTime = false });
                                }
                                if (isConnect && ChangeRefreshEvent != null)
                                {
                                    ChangeRefreshEvent(connectConfig);
                                    this.Close();
                                }
                            });
                        });
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        LoadingG.Visibility = Visibility.Collapsed;
                        Growl.WarningGlobal(new GrowlInfo { Message = $"连接失败\r" + ex.ToMsg(), WaitTime = 1, ShowDateTime = false });
                    });
                }
            });
            #endregion
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var sqLiteHelper = new SQLiteHelper();
            if (ListConnects.SelectedItem == null)
            {
                Growl.WarningGlobal(new GrowlInfo { Message = $"请选择需要删除的连接", WaitTime = 1, ShowDateTime = false });
                return;
            }
            var selectedConnect = (ConnectConfigs)ListConnects.SelectedItem;
            Task.Run(() =>
            {
                sqLiteHelper.db.Delete<ConnectConfigs>(selectedConnect.ID);
                var datalist = sqLiteHelper.db.Table<ConnectConfigs>().
                   ToList();
                Dispatcher.Invoke(() =>
                {
                    ResetData();
                    DataList = datalist;
                    if (ChangeRefreshEvent != null)
                    {
                        //ChangeRefreshEvent();
                    }
                });
            });
            #endregion
        }

        /// <summary>
        /// 添加重置表单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            MainContent = new ConnectMain();
            ResetData();
        }

        private void ResetData()
        {
            //var sdfs = TabDbType;
            //if (TabSqlServer.IsSelected)
            //{
            //    //MsSql_HidId.Text = "0";
            //    //MsSql_TextConnectName.Text = "";
            //    //MsSql_TextServerAddress.Text = "";
            //    //MsSql_TextServerPort.Value = 1433;
            //    //MsSql_TextServerName.Text = "";
            //    //MsSql_TextServerPassword.Password = "";
            //    //MsSql_ComboAuthentication.SelectedItem = SQLServer;
            //}
            //if (TabMySql.IsSelected)
            //{
            //    //MySql_HidId.Text = "0";
            //    //MySql_TextConnectName.Text = "";
            //    //MySql_TextServerAddress.Text = "";
            //    //MySql_TextServerPort.Value = 3306;
            //    //MySql_TextServerName.Text = "";
            //    //MySql_TextServerPassword.Password = "";
            //}
            //if (TabPostgreSql.IsSelected)
            //{
            //    //PostgreSql_HidId.Text = "0";
            //    //PostgreSql_TextConnectName.Text = "";
            //    //PostgreSql_TextServerAddress.Text = "";
            //    //PostgreSql_TextServerPort.Value = 5432;
            //    //PostgreSql_TextServerName.Text = "";
            //    //PostgreSql_TextServerPassword.Password = "";
            //}
            ListConnects.SelectedItem = null;
        }

        /// <summary>
        /// 校验表单数据
        /// </summary>
        private bool CheckConnectForm()
        {
            #region MyRegion
            var connectName = "";
            var serverAddress = "";
            var serverPort = 1433d;
            var authentication = 1;
            var userName = "";
            var password = "";
            //if (TabSqlServer.IsSelected)
            //{
            //    //connectName = MsSql_TextConnectName.Text.Trim();
            //    //serverAddress = MsSql_TextServerAddress.Text.Trim();
            //    //serverPort = MsSql_TextServerPort.Value;
            //    //authentication = MsSql_ComboAuthentication.SelectedValue == SQLServer ? 1 : 0;
            //    //userName = MsSql_TextServerName.Text.Trim();
            //    //password = MsSql_TextServerPassword.Password.Trim();
            //}
            //if (TabMySql.IsSelected)
            //{
            //    //connectName = MySql_TextConnectName.Text.Trim();
            //    //serverAddress = MySql_TextServerAddress.Text.Trim();
            //    //serverPort = MySql_TextServerPort.Value;
            //    //userName = MySql_TextServerName.Text.Trim();
            //    //password = MySql_TextServerPassword.Password.Trim();
            //}
            //if (TabPostgreSql.IsSelected)
            //{
            //    //connectName = PostgreSql_TextConnectName.Text.Trim();
            //    //serverAddress = PostgreSql_TextServerAddress.Text.Trim();
            //    //serverPort = PostgreSql_TextServerPort.Value;
            //    //userName = PostgreSql_TextServerName.Text.Trim();
            //    //password = PostgreSql_TextServerPassword.Password.Trim();
            //}
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTestConnect_OnClick(object sender, RoutedEventArgs e)
        {
            TestConnect(true);
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        /// <param name="flag"></param>
        private void TestConnect(bool flag)
        {
            if (!CheckConnectForm())
            {
                return;
            }
            var dbType = DbType.SqlServer;

            var connectId = 0;
            var connectionString = @"";
            //if (TabSqlServer.IsSelected)
            //{
            //    dbType = DbType.SqlServer;
            //    //connectId = Convert.ToInt32(MsSql_HidId.Text);
            //    //var serAddr = MsSql_TextServerAddress.Text.Trim().Equals(".")
            //    //    ? $"{MsSql_TextServerAddress.Text.Trim()}"
            //    //    : $"{MsSql_TextServerAddress.Text.Trim()},{MsSql_TextServerPort.Value}";
            //    //connectionString = $"server={serAddr};" +
            //    //                   "database=master;" +
            //    //                   $"uid={MsSql_TextServerName.Text.Trim()};" +
            //    //                   $"pwd={MsSql_TextServerPassword.Password.Trim()};";
            //}
            //if (TabMySql.IsSelected)
            //{
            //    dbType = DbType.MySql;
            //    //connectId = Convert.ToInt32(MySql_HidId.Text);
            //    //connectionString = $"server={MySql_TextServerAddress.Text.Trim()};" +
            //    //                   $"port={MySql_TextServerPort.Value};" +
            //    //                   $"uid={MySql_TextServerName.Text.Trim()};" +
            //    //                   $"pwd={MySql_TextServerPassword.Password.Trim()};" +
            //    //                   $"Allow User Variables=True;sslmode=none;";
            //}
            //if (TabPostgreSql.IsSelected)
            //{
            //    dbType = DbType.PostgreSQL;
            //    //connectId = Convert.ToInt32(PostgreSql_HidId.Text);
            //    //connectionString = $"HOST={PostgreSql_TextServerAddress.Text.Trim()};" +
            //    //                   $"PORT={PostgreSql_TextServerPort.Value};" +
            //    //                   $"DATABASE=postgres;" +
            //    //                   $"USER ID={PostgreSql_TextServerName.Text.Trim()};" +
            //    //                   $"PASSWORD={PostgreSql_TextServerPassword.Password.Trim()}";
            //}
            //LoadingG.Visibility = Visibility.Visible;
            //Task.Run(() =>
            //{
            //    try
            //    {
            //        var exporter = ExporterFactory.CreateInstance(dbType, connectionString);
            //        var list = exporter.GetDatabases();
            //        Dispatcher.Invoke(() =>
            //        {
            //            if (TabSqlServer.IsSelected)
            //            {
            //                //MsSql_ComboDefaultDatabase.ItemsSource = list;
            //            }
            //            if (TabMySql.IsSelected)
            //            {
            //                //MySql_ComboDefaultDatabase.ItemsSource = list;
            //            }
            //            if (TabPostgreSql.IsSelected)
            //            {
            //                //PostgreSql_ComboDefaultDatabase.ItemsSource = list;
            //            }
            //            if (connectId < 1)
            //            {
            //                if (TabSqlServer.IsSelected)
            //                {
            //                   // MsSql_ComboDefaultDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.Equals("master"));
            //                }
            //                if (TabMySql.IsSelected)
            //                {
            //                    //MySql_ComboDefaultDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.Equals("mysql"));
            //                }
            //                if (TabPostgreSql.IsSelected)
            //                {
            //                    //PostgreSql_ComboDefaultDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.Equals("postgres"));
            //                }
            //            }
            //            else
            //            {
            //                var sqLiteHelper = new SQLiteHelper();
            //                var connect = sqLiteHelper.db.Table<ConnectConfigs>().FirstOrDefault(x => x.ID == connectId);
            //                if (connect != null)
            //                {
            //                    if (TabSqlServer.IsSelected)
            //                    {
            //                        //MsSql_ComboDefaultDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.Equals(connect.DefaultDatabase));
            //                    }
            //                    if (TabMySql.IsSelected)
            //                    {
            //                        //MySql_ComboDefaultDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.Equals(connect.DefaultDatabase));
            //                    }
            //                    if (TabPostgreSql.IsSelected)
            //                    {
            //                        //PostgreSql_ComboDefaultDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.Equals(connect.DefaultDatabase));
            //                    }
            //                }
            //            }
            //            LoadingG.Visibility = Visibility.Collapsed;
            //            if (flag)
            //            {
            //                Growl.SuccessGlobal(new GrowlInfo { Message = $"连接成功", WaitTime = 1, ShowDateTime = false });
            //            }
            //        });
            //    }
            //    catch (Exception ex)
            //    {
            //        Dispatcher.Invoke(() =>
            //        {
            //            LoadingG.Visibility = Visibility.Collapsed;
            //            Growl.WarningGlobal(new GrowlInfo { Message = $"连接失败\r" + ex.ToMsg(), WaitTime = 1, ShowDateTime = false });
            //        });
            //    }
            //});
        }

        private void BtnFresh_OnClick(object sender, RoutedEventArgs e)
        {
            TestConnect(false);
        }

        /// <summary>
        /// 数据库类型变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabDbType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            //if (TabDbType.SelectedItem == TabSqlServer)
            //{
            //    //var connectId = Convert.ToInt32(MsSql_HidId.Text);
            //    //ListConnects.SelectedItem = connectId > 0 ? DataList.First(x => x.ID == connectId) : null;
            //}
            //if (TabDbType.SelectedItem == TabMySql)
            //{
            //    //var connectId = Convert.ToInt32(MySql_HidId.Text);
            //    //ListConnects.SelectedItem = connectId > 0 ? DataList.First(x => x.ID == connectId) : null;
            //}
            //if (TabDbType.SelectedItem == TabPostgreSql)
            //{
            //    //var connectId = Convert.ToInt32(PostgreSql_HidId.Text);
            //    //ListConnects.SelectedItem = connectId > 0 ? DataList.First(x => x.ID == connectId) : null;
            //}
        }
    }
}
