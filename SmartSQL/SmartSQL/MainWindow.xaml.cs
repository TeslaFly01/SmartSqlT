using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Controls;
using HandyControl.Data;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Models;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using SmartSQL.Framework;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Annotations;
using SmartSQL.DocUtils;
using SmartSQL.Framework.Const;
using SmartSQL.Helper;
using SmartSQL.UserControl;
using SmartSQL.Views;
using ComboBox = System.Windows.Controls.ComboBox;
using FontAwesome = FontAwesome.WPF.FontAwesome;
using TabControl = System.Windows.Controls.TabControl;
using TabItem = System.Windows.Controls.TabItem;
using SmartSQL.Views.Category;

namespace SmartSQL
{
    public partial class MainWindow : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private ConnectConfigs SelectendConnection = null;

        #region

        public static readonly DependencyProperty TreeViewDataProperty = DependencyProperty.Register(
            "TreeViewData", typeof(List<TreeNodeItem>), typeof(MainWindow), new PropertyMetadata(default(List<TreeNodeItem>)));

        /// <summary>
        /// 左侧菜单数据
        /// </summary>
        public List<TreeNodeItem> TreeViewData
        {
            get => (List<TreeNodeItem>)GetValue(TreeViewDataProperty);
            set
            {
                SetValue(TreeViewDataProperty, value);
                OnPropertyChanged(nameof(TreeViewData));
            }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 页面初始化加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var sqLiteHelper = SQLiteHelper.GetInstance();
            var connectConfigs = sqLiteHelper.ToList<ConnectConfigs>();
            SwitchMenu.ItemsSource = null;
            SwitchMenu.ItemsSource = connectConfigs;
            if (!connectConfigs.Any())
            {
                SwitchMenu.Header = @"新建连接";
            }
        }

        /// <summary>
        /// 切换数据库连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchMenu_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var connectConfig = (ConnectConfigs)menuItem.DataContext;
            SwitchConnect(connectConfig);
        }

        /// <summary>
        /// 切换连接
        /// </summary>
        /// <param name="connectConfig"></param>
        public void SwitchConnect(ConnectConfigs connectConfig)
        {
            #region MyRegion
            SwitchMenu.Header = connectConfig.ConnectName;
            SelectendConnection = connectConfig;
            var sqLiteHelper = SQLiteHelper.GetInstance();
            sqLiteHelper.SetSysValue(SysConst.Sys_SelectedConnection, connectConfig.ConnectName);
            var connectConfigs = sqLiteHelper.ToList<ConnectConfigs>();
            SwitchMenu.ItemsSource = null;
            SwitchMenu.ItemsSource = connectConfigs;
            //加载主界面
            MainContent.PageLoad(connectConfig);
            #endregion
        }

        /// <summary>
        /// 分组管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuGroup_OnClick(object sender, RoutedEventArgs e)
        {
            var selectDatabase = (DataBase)MainContent.SelectDatabase.SelectedItem;
            if (SelectendConnection == null || selectDatabase == null)
            {
                Oops.Oh("请选择数据库");
                return;
            }
            var group = new GroupManage();
            group.Connection = SelectendConnection;
            group.SelectedDataBase = selectDatabase.DbName;
            group.Owner = this;
            group.ChangeRefreshEvent += MainContent.Group_ChangeRefreshEvent;
            group.ShowDialog();
        }

        /// <summary>
        /// 标签管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuTag_Click(object sender, RoutedEventArgs e)
        {
            var selectDatabase = (DataBase)MainContent.SelectDatabase.SelectedItem;
            if (SelectendConnection == null || selectDatabase == null)
            {
                Oops.Oh("请选择数据库");
                return;
            }
            var tags = new TagsView();
            tags.Connection = SelectendConnection;
            tags.SelectedDataBase = selectDatabase.DbName;
            tags.Owner = this;
            tags.ShowDialog();
        }

        /// <summary>
        /// 全局设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuSetting_OnClick(object sender, RoutedEventArgs e)
        {
            var set = new SettingWindow();
            set.Owner = this;
            set.ShowDialog();
        }

        /// <summary>
        /// 新建连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchMenu_OnClick(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            if (menuItem.Header.Equals("新建连接"))
            {
                var connect = new ConnectManage();
                connect.Owner = this;
                connect.ChangeRefreshEvent += SwitchConnect;
                connect.ShowDialog();
            }
        }

        /// <summary>
        /// 新建连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddConnect_OnClick(object sender, RoutedEventArgs e)
        {
            var connect = new ConnectManage();
            connect.Owner = this;
            connect.ChangeRefreshEvent += SwitchConnect;
            connect.ShowDialog();
        }

        /// <summary>
        /// 导出文档
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportDoc_OnClick(object sender, RoutedEventArgs e)
        {
            var selectDatabase = (DataBase)MainContent.SelectDatabase.SelectedItem;
            if (SelectendConnection == null || selectDatabase == null)
            {
                Oops.Oh("请选择数据库");
                return;
            }
            var exportDoc = new ExportDoc
            {
                Owner = this,
                MenuData = MainContent.MenuData,
                SelectedConnection = SelectendConnection,
                SelectedDataBase = selectDatabase
            };
            exportDoc.ShowDialog();
        }

        /// <summary>
        /// 导入备注
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportMark_OnClick(object sender, RoutedEventArgs e)
        {
            var selectDatabase = (DataBase)MainContent.SelectDatabase.SelectedItem;
            if (SelectendConnection == null || selectDatabase == null)
            {
                Oops.Oh("请选择数据库");
                return;
            }
            var importMark = new ImportMark();
            importMark.Owner = this;
            importMark.SelectedConnection = SelectendConnection;
            importMark.SelectedDataBase = selectDatabase;
            importMark.ShowDialog();
        }

        /// <summary>
        /// 关于SmartSQL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuAbout_OnClick(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow();
            about.Owner = this;
            about.ShowDialog();
        }

        /// <summary>
        /// 打赏作者
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuDonation_OnClick(object sender, RoutedEventArgs e)
        {
            var donation = new Donation();
            donation.Owner = this;
            donation.ShowDialog();
        }

        private void MenuFontAwesome_OnClick(object sender, RoutedEventArgs e)
        {
            var donation = new Fontawesome();
            donation.Owner = this;
            donation.ShowDialog();
        }

        /// <summary>
        /// 联系作者
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuCallMe_Click(object sender, RoutedEventArgs e)
        {
            var donation = new Contact();
            donation.Owner = this;
            donation.ShowDialog();
        }

        private void MenuManager_Selected(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            DockUcManager.Visibility=Visibility.Visible;
            DockUcTools.Visibility=Visibility.Collapsed;
            DockUcDbCompare.Visibility=Visibility.Collapsed;
        }

        private void MenuTool_Selected(object sender, RoutedEventArgs e)
        {
            UcMainTools.Content=new UcMainTools();
            DockUcTools.Visibility=Visibility.Visible;
            DockUcManager.Visibility=Visibility.Collapsed;
            DockUcDbCompare.Visibility=Visibility.Collapsed;
        }

        private void MenuDbCompare_Selected(object sender, RoutedEventArgs e)
        {
            DockUcDbCompare.Visibility=Visibility.Visible;
            DockUcTools.Visibility=Visibility.Collapsed;
            DockUcManager.Visibility=Visibility.Collapsed;
        }
    }
}
