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
        }

        /// <summary>
        /// 连接页面数据初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectManage_OnLoaded(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded)
            {
                return;
            }
            var ucConnectMain = new ConnectMainUC();
            ucConnectMain.ChangeRefreshEvent += ChangeRefreshEvent;
            MainContent = ucConnectMain;
            Task.Run(() =>
            {
                var sqLiteHelper = new SQLiteHelper();
                var datalist = sqLiteHelper.db.Table<ConnectConfigs>().ToList();
                Dispatcher.Invoke(() =>
                {
                    DataList = datalist;
                    if (!datalist.Any())
                    {
                        NoDataText.Visibility = Visibility.Visible;
                    }
                });
            });
            #endregion
        }

        /// <summary>
        /// 加载连接信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion
            var listBox = (ListBox)sender;
            if (listBox.SelectedItems.Count > 0)
            {
                OprToolGrid.Visibility = Visibility.Visible;
                BtnPrev.Visibility = Visibility.Collapsed;
                var connect = (ConnectConfigs)listBox.SelectedItems[0];
                switch (connect.DbType)
                {
                    case DbType.SqlServer:
                        var ucSqlServer = new SqlServerUC();
                        ucSqlServer.ConnectConfig = connect;
                        ucSqlServer.ChangeRefreshEvent += ChangeRefreshEvent;
                        MainContent = ucSqlServer;
                        break;
                    case DbType.MySql:
                        var ucMySql = new MySqlUC();
                        ucMySql.ConnectConfig = connect;
                        ucMySql.ChangeRefreshEvent += ChangeRefreshEvent;
                        MainContent = ucMySql;
                        break;
                    case DbType.PostgreSQL:
                        var ucPostgreSql = new PostgreSqlUC();
                        ucPostgreSql.ConnectConfig = connect;
                        ucPostgreSql.ChangeRefreshEvent += ChangeRefreshEvent;
                        MainContent = ucPostgreSql;
                        break;
                    case DbType.Sqlite:
                        var ucSqlite = new SqliteUC();
                        ucSqlite.ConnectConfig = connect;
                        ucSqlite.ChangeRefreshEvent += ChangeRefreshEvent;
                        MainContent = ucSqlite;
                        break;
                    case DbType.Oracle:
                        var ucOracle = new OracleUC();
                        ucOracle.ConnectConfig = connect;
                        ucOracle.ChangeRefreshEvent += ChangeRefreshEvent;
                        MainContent = ucOracle;
                        break;
                }
            }
            #endregion
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var tag = ((Button)sender).Tag;
            var isConnect = tag != null && (string)tag == $"Connect";
            //SqlServer
            if (MainContent is SqlServerUC ucSqlServer)
            {
                ucSqlServer.SaveForm(isConnect);
            }
            //MySql
            if (MainContent is MySqlUC ucMySql)
            {
                ucMySql.SaveForm(isConnect);
            }
            //PostgreSql
            if (MainContent is PostgreSqlUC ucPostgreSql)
            {
                ucPostgreSql.SaveForm(isConnect);
            }
            //Sqlite
            if (MainContent is SqliteUC ucSqlite)
            {
                ucSqlite.SaveForm(isConnect);
            }
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
                    if (!datalist.Any())
                    {
                        NoDataText.Visibility = Visibility.Visible;
                    }
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

        /// <summary>
        /// 重置表单
        /// </summary>
        private void ResetData()
        {
            #region MyRegion
            MainContent = new ConnectMainUC();
            ListConnects.SelectedItem = null;
            #endregion
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTestConnect_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            //测试SqlServer
            if (MainContent is SqlServerUC ucSqlServer)
            {
                ucSqlServer.TestConnect(true);
            }
            //测试MySql
            if (MainContent is MySqlUC ucMySql)
            {
                ucMySql.TestConnect(true);
            }
            //测试PostgreSql
            if (MainContent is PostgreSqlUC ucPostgreSql)
            {
                ucPostgreSql.TestConnect(true);
            }
            //测试Sqlite
            if (MainContent is SqliteUC ucSqlite)
            {
                ucSqlite.TestConnect(true);
            }
            #endregion
        }

        /// <summary>
        /// 返回上一步
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPrev_OnClick(object sender, RoutedEventArgs e)
        {
            ResetData();
        }
    }
}
