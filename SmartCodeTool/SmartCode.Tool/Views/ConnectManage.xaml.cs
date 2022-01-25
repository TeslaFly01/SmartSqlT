using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
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
using SmartCode.Framework;
using SmartCode.Framework.Exporter;
using SmartCode.Framework.PhysicalDataModel;
using SmartCode.Framework.SqliteModel;
using SmartCode.Framework.Util;
using SmartCode.Tool.Annotations;
using SmartCode.Tool.Helper;

namespace SmartCode.Tool.Views
{
    //定义委托
    public delegate void ConnectChangeRefreshHandler(ConnectConfigs connectConfig);
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

        public event ConnectChangeRefreshHandler ChangeRefreshEvent;
        public ConnectManage()
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
                HidId.Text = connect.ID.ToString();
                TextConnectName.Text = connect.ConnectName;
                TextServerAddress.Text = connect.ServerAddress;
                TextServerPort.Value = connect.ServerPort;
                TextServerName.Text = connect.UserName;
                ComboAuthentication.SelectedItem = connect.Authentication == 0 ? SQLServer : Windows;
                TextServerPassword.Password = pwd;
                BtnConnect.IsEnabled = true;
                var defaultBase = new List<DataBase> { new DataBase { DbName = connect.DefaultDatabase } };
                ComboDefaultDatabase.ItemsSource = defaultBase;
                ComboDefaultDatabase.SelectedItem = defaultBase.First();
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            if (!CheckConnectForm())
            {
                return;
            }
            var tag = ((Button)sender).Tag;
            var isConnect = tag != null && (string)tag == $"Connect";
            var connectId = Convert.ToInt32(HidId.Text);
            var connectName = TextConnectName.Text.Trim();
            var serverAddress = TextServerAddress.Text.Trim();
            var serverPort = TextServerPort.Value;
            var authentication = ComboAuthentication.SelectedValue == SQLServer ? 1 : 0;
            var userName = TextServerName.Text.Trim();
            var password = TextServerPassword.Password.Trim();
            var defaultDataBase = (DataBase)ComboDefaultDatabase.SelectedItem;
            var sqLiteHelper = new SQLiteHelper();
            ConnectConfigs connectConfig;
            var connectionString = $"server={TextServerAddress.Text.Trim()},{TextServerPort.Value};database=master;uid={TextServerName.Text.Trim()};pwd={TextServerPassword.Password.Trim()};";
            LoadingG.Visibility = Visibility.Visible;
            Task.Run(() =>
            {
                try
                {
                    if (isConnect)
                    {
                        var exporter = ExporterFactory.CreateInstance(DBType.SqlServer, connectionString);
                        exporter.GetDatabases(connectionString);
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
                        Growl.WarningGlobal(new GrowlInfo { Message = $"连接失败\r" + ex.Message, WaitTime = 1, ShowDateTime = false });
                    });
                }
            });
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
            var sqLiteHelper = new SQLiteHelper();
            var connectId = Convert.ToInt32(HidId.Text);
            if (connectId < 1)
            {
                Growl.WarningGlobal(new GrowlInfo { Message = $"请选择需要删除的连接", WaitTime = 1, ShowDateTime = false });
                return;
            }
            Task.Run(() =>
            {
                sqLiteHelper.db.Delete<ConnectConfigs>(connectId);
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
        }

        /// <summary>
        /// 添加重置表单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            if (HidId.Text == "0")
            {

            }
            ResetData();
        }

        private void ResetData()
        {
            HidId.Text = "0";
            TextConnectName.Text = "";
            TextServerAddress.Text = "";
            TextServerPort.Value = 1433;
            TextServerName.Text = "";
            TextServerPassword.Password = "";
            ComboAuthentication.SelectedItem = SQLServer;
            ListConnects.SelectedItem = null;
            BtnConnect.IsEnabled = false;
            BtnTestConnect.IsEnabled = false;
        }

        /// <summary>
        /// 校验表单数据
        /// </summary>
        private bool CheckConnectForm()
        {
            #region MyRegion
            var connectName = TextConnectName.Text.Trim();
            var serverAddress = TextServerAddress.Text.Trim();
            var serverPort = TextServerPort.Value;
            var authentication = ComboAuthentication.SelectedValue == SQLServer ? 1 : 0;
            var userName = TextServerName.Text.Trim();
            var password = TextServerPassword.Password.Trim();
            if (string.IsNullOrEmpty(connectName))
            {
                Growl.WarningGlobal(new GrowlInfo { Message = $"请填写连接名称", WaitTime = 1, ShowDateTime = false });
                return false;
            }
            if (string.IsNullOrEmpty(serverAddress))
            {
                Growl.WarningGlobal(new GrowlInfo { Message = $"请填写服务器地址", WaitTime = 1, ShowDateTime = false });
                return false;
            }
            if (serverPort < 1)
            {
                Growl.WarningGlobal(new GrowlInfo { Message = $"请填写端口号", WaitTime = 1, ShowDateTime = false });
                return false;
            }
            if (string.IsNullOrEmpty(userName))
            {
                Growl.WarningGlobal(new GrowlInfo { Message = $"请填写用户名", WaitTime = 1, ShowDateTime = false });
                return false;
            }
            if (string.IsNullOrEmpty(password))
            {
                Growl.WarningGlobal(new GrowlInfo { Message = $"请填写密码", WaitTime = 1, ShowDateTime = false });
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
            var connectionString = $"server={TextServerAddress.Text.Trim()},{TextServerPort.Value};database=master;uid={TextServerName.Text.Trim()};pwd={TextServerPassword.Password.Trim()};";
            LoadingG.Visibility = Visibility.Visible;
            Task.Run(() =>
            {
                try
                {
                    var exporter = ExporterFactory.CreateInstance(DBType.SqlServer, connectionString);
                    var list = exporter.GetDatabases(connectionString);
                    Dispatcher.Invoke(() =>
                    {
                        var connectId = Convert.ToInt32(HidId.Text);
                        ComboDefaultDatabase.ItemsSource = list;
                        if (connectId < 1)
                        {
                            ComboDefaultDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.Equals("master"));
                        }
                        else
                        {
                            var sqLiteHelper = new SQLiteHelper();
                            var connect = sqLiteHelper.db.Table<ConnectConfigs>().FirstOrDefault(x => x.ID == connectId);
                            if (connect != null)
                            {
                                ComboDefaultDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.Equals(connect.DefaultDatabase));
                            }
                        }
                        LoadingG.Visibility = Visibility.Collapsed;
                        if (flag)
                        {
                            Growl.SuccessGlobal(new GrowlInfo { Message = $"连接成功", WaitTime = 1, ShowDateTime = false });
                        }
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        LoadingG.Visibility = Visibility.Collapsed;
                        Growl.WarningGlobal(new GrowlInfo { Message = $"连接失败\r" + ex.Message, WaitTime = 1, ShowDateTime = false });
                    });
                }
            });
        }

        private void BtnFresh_OnClick(object sender, RoutedEventArgs e)
        {
            TestConnect(false);
        }
    }
}
