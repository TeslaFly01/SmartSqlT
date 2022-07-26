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
using SmartSQL.UserControl;
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
    public partial class ConnectManage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event ConnectChangeRefreshHandlerExt ChangeRefreshEvent;

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
            "MainContent", typeof(System.Windows.Controls.UserControl), typeof(ConnectManage), new PropertyMetadata(default(System.Windows.Controls.UserControl)));
        /// <summary>
        /// 主界面用户控件
        /// </summary>
        public System.Windows.Controls.UserControl MainContent
        {
            get => (System.Windows.Controls.UserControl)GetValue(MainContentProperty);
            set => SetValue(MainContentProperty, value);
        }
        #endregion

        public ConnectManage()
        {
            InitializeComponent();
            DataContext = this;
            MainContent = new ConnectMainUC();
        }

        /// <summary>
        /// 加载连接信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (listBox.SelectedItems.Count > 0)
            {
                OprToolGrid.Visibility = Visibility.Visible;
                var connect = (ConnectConfigs)listBox.SelectedItems[0];
                switch (connect.DbType)
                {
                    case DbType.SqlServer:
                        var ucSqlServer = new SqlServerUC();
                        ucSqlServer.ConnectConfig = connect;
                        MainContent = ucSqlServer;
                        break;
                    case DbType.MySql:
                        var ucMySql = new MySqlUC();
                        ucMySql.ConnectConfig = connect;
                        MainContent = ucMySql;
                        break;
                    case DbType.PostgreSQL:
                        var ucPostgreSql = new PostgreSqlUC();
                        ucPostgreSql.ConnectConfig = connect;
                        MainContent = ucPostgreSql;
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
            //if (!CheckConnectForm())
            //{
            //    return;
            //}
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
        /// 添加/重置表单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            ResetData();
        }

        private void ResetData()
        {
            MainContent = new ConnectMainUC();
            ListConnects.SelectedItem = null;
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
            #region MyRegion
            //测试SqlServer
            if (MainContent is SqlServerUC ucSqlServer)
            {
                ucSqlServer.TestConnect(flag);
            }
            //测试MySql
            if (MainContent is MySqlUC ucMySql)
            {
                ucMySql.TestConnect(flag);
            }
            //测试PostgreSql
            if (MainContent is PostgreSqlUC ucPostgreSql)
            {
                ucPostgreSql.TestConnect(flag);
            } 
            #endregion
        }

        //private void BtnFresh_OnClick(object sender, RoutedEventArgs e)
        //{
        //    TestConnect(false);
        //}

        private void ConnectManage_OnLoaded(object sender, RoutedEventArgs e)
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
    }
}
