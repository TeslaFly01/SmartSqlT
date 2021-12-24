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
using SmartCode.Tool.Annotations;

namespace SmartCode.Tool.Views
{
    //定义委托
    public delegate void ConnectChangeRefreshHandler();
    /// <summary>
    /// GroupManage.xaml 的交互逻辑
    /// </summary>
    public partial class ConnectManage : INotifyPropertyChanged
    {
        public event ConnectChangeRefreshHandler ChangeRefreshEvent;
        public ConnectManage()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region DependencyProperty
        public static readonly DependencyProperty ConnectionProperty = DependencyProperty.Register(
            "Connection", typeof(DataBasesConfig), typeof(GroupManage), new PropertyMetadata(default(DataBasesConfig)));
        public DataBasesConfig Connection
        {
            get => (DataBasesConfig)GetValue(ConnectionProperty);
            set => SetValue(ConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(GroupManage), new PropertyMetadata(default(string)));
        public string SelectedDataBase
        {
            get => (string)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }

        public new static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(GroupManage), new PropertyMetadata(default(string)));
        public new string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty DataListProperty = DependencyProperty.Register(
            "DataList", typeof(List<Connects>), typeof(ConnectManage), new PropertyMetadata(default(List<Connects>)));
        public List<Connects> DataList
        {
            get => (List<Connects>)GetValue(DataListProperty);
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
                var datalist = sqLiteHelper.db.Table<Connects>().ToList();
                Dispatcher.Invoke(() =>
                {
                    DataList = datalist;
                });
                IExporter exporter = new SqlServer2008Exporter();
            });
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (listBox.SelectedItems.Count > 0)
            {
                var connect = (Connects)listBox.SelectedItems[0];
                HidId.Text = connect.ID.ToString();
                TextConnectName.Text = connect.ConnectName;
                TextServerAddress.Text = connect.ServerAddress;
                TextServerPort.Text = connect.ServerPort.ToString();
                TextServerName.Text = connect.UserName;
                TextServerPassword.Password = connect.Password;
                ComboAuthentication.SelectedItem = connect.Authentication == 0 ? SQLServer : Windows;
                TextDefaultDataBase.Text = connect.DefaultDatabase;
                BtnDelete.Visibility = Visibility.Visible;
                BtnConnect.IsEnabled = true;
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            //var groupName = TextGourpName.Text.Trim();
            //if (string.IsNullOrEmpty(groupName))
            //{
            //    Growl.Warning(new GrowlInfo { Message = $"请填写分组名", WaitTime = 1, ShowDateTime = false });
            //    return;
            //}
            //var selectedDatabase = (DataBase)SelectDatabase.SelectedItem;
            var sqLiteHelper = new SQLiteHelper();
            var connectId = Convert.ToInt32(HidId.Text);
            var connectName = TextConnectName.Text.Trim();
            var serverAddress = TextServerAddress.Text.Trim();
            var serverPort = Convert.ToInt32(TextServerPort.Text.Trim());
            var authentication = ComboAuthentication.SelectedValue == SQLServer ? 1 : 0;
            var userName = TextServerName.Text.Trim();
            var password = TextServerPassword.Password.Trim();
            var defaultDataBase = TextDefaultDataBase.Text.Trim();
            if (connectId > 0)
            {
                var connect = sqLiteHelper.db.Table<Connects>().FirstOrDefault(x => x.ConnectName == connectName && x.ID != connectId);
                if (connect != null)
                {
                    Growl.Warning(new GrowlInfo { Message = $"已存在相同名称的连接名", WaitTime = 1, ShowDateTime = false });
                    return;
                }
                var selectedConnect = (Connects)ListConnects.SelectedItems[0];
                selectedConnect.ConnectName = connectName;
                selectedConnect.ServerAddress = serverAddress;
                selectedConnect.ServerPort = serverPort;
                selectedConnect.UserName = userName;
                selectedConnect.Password = password;
                selectedConnect.DefaultDatabase = defaultDataBase;
                selectedConnect.Authentication = authentication;
                sqLiteHelper.db.Update(selectedConnect);
            }
            else
            {
                var connect = sqLiteHelper.db.Table<Connects>().FirstOrDefault(x => x.ConnectName.ToLower() == connectName.ToLower());
                if (connect != null)
                {
                    Growl.Warning(new GrowlInfo { Message = $"已存在相同名称的连接名", WaitTime = 1, ShowDateTime = false });
                    return;
                }
                sqLiteHelper.db.Insert(new Connects()
                {
                    ConnectName = connectName,
                    ServerAddress = serverAddress,
                    ServerPort = serverPort,
                    Authentication = authentication,
                    UserName = userName,
                    Password = password,
                    CreateDate = 1212,
                    DefaultDatabase = defaultDataBase
                });
            }
            //BtnDelete.Visibility = Visibility.Collapsed;
            //HidId.Text = "0";
            //TextGourpName.Text = "";
            //BtnSave.IsEnabled = false;
            Task.Run(() =>
            {
                var datalist = sqLiteHelper.db.Table<Connects>().
                    ToList();
                Dispatcher.Invoke(() =>
                {
                    DataList = datalist;
                    //if (ChangeRefreshEvent != null)
                    //{
                    //    ChangeRefreshEvent();
                    //}
                });
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
                return;
            }
            Task.Run(() =>
            {
                sqLiteHelper.db.Delete<Connects>(connectId);
                
                var datalist = sqLiteHelper.db.Table<Connects>().
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            BtnDelete.Visibility = Visibility.Hidden;
            HidId.Text = "0";
            TextConnectName.Text = "";
            TextServerAddress.Text = "";
            TextServerPort.Text = "1433";
            TextServerName.Text = "";
            TextServerPassword.Password = "";
            ComboAuthentication.SelectedItem = SQLServer;
            TextDefaultDataBase.Text = "master";
            ListConnects.SelectedItem = null;
            BtnConnect.IsEnabled = false;
            BtnTestConnect.IsEnabled = false;
        }

        private void SelectDatabase_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnDelete.Visibility = Visibility.Collapsed;
            Task.Run(() =>
            {
                var sqLiteHelper = new SQLiteHelper();
                var datalist = sqLiteHelper.db.Table<Connects>().ToList();
                Dispatcher.Invoke(() =>
                {
                    DataList = datalist;
                });
            });
        }

        /// <summary>
        /// 文本框实时监测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Text_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextConnectName.Text.Trim())&&!string.IsNullOrEmpty(TextServerAddress.Text.Trim())&&!string.IsNullOrEmpty(TextServerPort.Text)&&!string.IsNullOrEmpty(TextServerName.Text.Trim())&&!string.IsNullOrEmpty(TextServerPassword.Password.Trim())&&!string.IsNullOrEmpty(TextDefaultDataBase.Text.Trim()))
            {
                BtnConnect.IsEnabled = true;
                BtnTestConnect.IsEnabled = true;
            }
        }

        /// <summary>
        /// 密码框实时监测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextServerPassword_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextConnectName.Text.Trim()) && !string.IsNullOrEmpty(TextServerAddress.Text.Trim()) && !string.IsNullOrEmpty(TextServerPort.Text) && !string.IsNullOrEmpty(TextServerName.Text.Trim()) && !string.IsNullOrEmpty(TextServerPassword.Password.Trim()) && !string.IsNullOrEmpty(TextDefaultDataBase.Text.Trim()))
            {
                BtnConnect.IsEnabled = true;
                BtnTestConnect.IsEnabled = true;
            }
        }
    }
}
