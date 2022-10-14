using SmartSQL.Annotations;
using SmartSQL.Framework;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Helper;
using SmartSQL.Views.Category;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Views;

namespace SmartSQL.UserControl.Tags
{
    /// <summary>
    /// TagObjects.xaml 的交互逻辑
    /// </summary>
    public partial class UcGroupObjects : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region PropertyFiled
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcGroupObjects), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(UcGroupObjects), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty DbDataProperty = DependencyProperty.Register(
            "DbData", typeof(Model), typeof(UcGroupObjects), new PropertyMetadata(default(Model)));
        /// <summary>
        /// 当前选中连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }
        /// <summary>
        /// 当前选中数据库
        /// </summary>
        public string SelectedDataBase
        {
            get => (string)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }
        /// <summary>
        /// 基础数据
        /// </summary>
        public Model DbData
        {
            get => (Model)GetValue(DbDataProperty);
            set => SetValue(DbDataProperty, value);
        }

        public static readonly DependencyProperty SelectedGroupProperty = DependencyProperty.Register(
            "SelectedGroup", typeof(GroupInfo), typeof(UcGroupObjects), new PropertyMetadata(default(GroupInfo)));
        /// <summary>
        /// 当前选中分组
        /// </summary>
        public GroupInfo SelectedGroup
        {
            get => (GroupInfo)GetValue(SelectedGroupProperty);
            set => SetValue(SelectedGroupProperty, value);
        }

        /// <summary>
        /// 分组对象数据列表
        /// </summary>
        public static readonly DependencyProperty GroupObjectListProperty = DependencyProperty.Register(
            "GroupObjectList", typeof(List<GroupObjects>), typeof(UcGroupObjects), new PropertyMetadata(default(List<GroupObjects>)));
        public List<GroupObjects> GroupObjectList
        {
            get => (List<GroupObjects>)GetValue(GroupObjectListProperty);
            set
            {
                SetValue(GroupObjectListProperty, value);
                OnPropertyChanged(nameof(GroupObjectList));
            }
        }
        #endregion

        public UcGroupObjects()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 初始化加载数据
        /// </summary>
        public void LoadPageData()
        {
            var conn = SelectedConnection;
            var selDatabase = SelectedDataBase;
            var selGroup = SelectedGroup;

            Task.Run(() =>
            {
                var sqLiteInstance = SQLiteHelper.GetInstance();
                var groupObjectList = sqLiteInstance.ToList<GroupObjects>(x =>
                    x.ConnectId == conn.ID &&
                    x.DatabaseName == selDatabase &&
                    x.GroupId == selGroup.Id);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (groupObjectList.Any())
                    {
                        MainNoDataText.Visibility = Visibility.Collapsed;
                    }
                    GroupObjectItems = groupObjectList;
                    GroupObjectList = groupObjectList;
                }));
            });
        }
        private List<GroupObjects> GroupObjectItems;

        private void SearchObjects_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchData = GroupObjectItems;
            var searchText = SearchObjects.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                var tagObjs = GroupObjectItems.Where(x => x.ObjectName.ToLower().Contains(searchText.ToLower()));
                if (tagObjs.Any())
                {
                    searchData = tagObjs.ToList();
                }
                else
                {
                    searchData = new List<GroupObjects>();
                }
            }
            MainNoDataText.Visibility = searchData.Any() ? Visibility.Collapsed : Visibility.Visible;
            GroupObjectList = searchData;
        }

        /// <summary>
        /// 行删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRowDelete_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = (GroupObjects)TableGrid.SelectedItem;
            if (selectedItem != null)
            {
                var conn = SelectedConnection;
                var selDatabase = SelectedDataBase;
                var selGroup = SelectedGroup;
                var sqLiteInstance = SQLiteHelper.GetInstance();
                sqLiteInstance.db.Delete(selectedItem);
                if (selGroup.SubCount > 0)
                {
                    selGroup.SubCount -= 1;
                    sqLiteInstance.db.Update(selGroup);
                }
                var groupObjectList = sqLiteInstance.ToList<GroupObjects>(x =>
                    x.ConnectId == conn.ID &&
                    x.DatabaseName == selDatabase &&
                    x.GroupId == selGroup.Id);
                MainNoDataText.Visibility = groupObjectList.Any() ? Visibility.Collapsed : Visibility.Visible;
                GroupObjectItems = groupObjectList;
                GroupObjectList = groupObjectList;
                var parentWindow = (GroupsView)Window.GetWindow(this);
                parentWindow?.ReloadMenu();
            }
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }

        /// <summary>
        /// 设置标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSetGroup_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedGroup == null)
            {
                Oops.Oh("请选择分组.");
                return;
            }
            var parentWindow = (GroupsView)Window.GetWindow(this);
            var ucAddObjects = new UcAddGroupObject();
            ucAddObjects.SelectedConnection = SelectedConnection;
            ucAddObjects.SelectedDataBase = SelectedDataBase;
            ucAddObjects.DbData = DbData;
            ucAddObjects.SelectedGroup = SelectedGroup;
            ucAddObjects.LoadPageData();
            parentWindow.MainContent = ucAddObjects;
        }
    }
}
