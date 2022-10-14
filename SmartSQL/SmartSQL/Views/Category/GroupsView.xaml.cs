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
using SmartSQL.Framework;
using SmartSQL.Framework.Exporter;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Annotations;
using SqlSugar;
using SmartSQL.Views.Category;
using SmartSQL.Helper;
using SmartSQL.UserControl.Tags;

namespace SmartSQL.Views
{
    //定义委托
    public delegate void ChangeRefreshHandler();
    /// <summary>
    /// GroupManage.xaml 的交互逻辑
    /// </summary>
    public partial class GroupsView : INotifyPropertyChanged
    {
        public event ChangeRefreshHandler ChangeRefreshEvent;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public GroupsView()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region DependencyProperty
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(GroupsView), new PropertyMetadata(default(ConnectConfigs)));
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(GroupsView), new PropertyMetadata(default(string)));
        public string SelectedDataBase
        {
            get => (string)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }

        public new static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(GroupsView), new PropertyMetadata(default(string)));
        public new string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// 分组菜单数据列表
        /// </summary>
        public static readonly DependencyProperty GroupMenuListProperty = DependencyProperty.Register(
            "GroupMenuList", typeof(List<ObjectGroup>), typeof(GroupsView), new PropertyMetadata(default(List<ObjectGroup>)));
        public List<ObjectGroup> GroupMenuList
        {
            get => (List<ObjectGroup>)GetValue(GroupMenuListProperty);
            set
            {
                SetValue(GroupMenuListProperty, value);
                OnPropertyChanged(nameof(GroupMenuList));
            }
        }

        public static readonly DependencyProperty MainContentProperty = DependencyProperty.Register(
            "MainContent", typeof(System.Windows.Controls.UserControl), typeof(GroupsView), new PropertyMetadata(default(System.Windows.Controls.UserControl)));
        /// <summary>
        /// 主界面用户控件
        /// </summary>
        public System.Windows.Controls.UserControl MainContent
        {
            get => (System.Windows.Controls.UserControl)GetValue(MainContentProperty);
            set => SetValue(MainContentProperty, value);
        }
        #endregion

        private void GroupManage_OnLoaded(object sender, RoutedEventArgs e)
        {
            Title = $"{SelectedConnection.ConnectName} - 分组管理";
            var selConn = SelectedConnection;
            var selectDataBase = SelectedDataBase;
            Task.Run(() =>
            {
                var sqLiteHelper = new SQLiteHelper();
                var groupMenuList = sqLiteHelper.db.Table<ObjectGroup>()
                    .Where(x => x.ConnectId == selConn.ID && x.DataBaseName == selectDataBase)
                    .OrderBy(x => x.OrderFlag).ToList();
                Dispatcher.Invoke(() =>
                {
                    if (!groupMenuList.Any())
                    {
                        NoDataText.Visibility = Visibility.Visible;
                    }
                    GroupMenuList = groupMenuList;
                });
            });
            MainContent = new UcGroupObjects();
        }

        public void Tag_ChangeRefreshEvent()
        {
            ReloadMenu();
        }

        /// <summary>
        /// 重新加载标签菜单
        /// </summary>
        public void ReloadMenu()
        {
            var sqliteInstance = SQLiteHelper.GetInstance();
            var datalist = sqliteInstance.ToList<ObjectGroup>(x =>
                x.ConnectId == SelectedConnection.ID &&
                x.DataBaseName == SelectedDataBase
            );
            GroupMenuList = datalist;
            NoDataText.Visibility = datalist.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)sender;
            var selConn = SelectedConnection;
            var selectDataBase = SelectedDataBase;
            if (listBox.SelectedItems.Count > 0)
            {
                var group = (ObjectGroup)listBox.SelectedItems[0];

                var ucGroupObjects = new UcGroupObjects();
                ucGroupObjects.SelectedConnection = selConn;
                ucGroupObjects.SelectedDataBase = selectDataBase;
                ucGroupObjects.SelectedGroup = group;
                ucGroupObjects.LoadPageData();
                MainContent = ucGroupObjects;
            }
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            var tagAdd = new GroupAddView();
            tagAdd.SelectedConnection = SelectedConnection;
            tagAdd.SelectedDataBase = SelectedDataBase;
            tagAdd.ChangeRefreshEvent += Tag_ChangeRefreshEvent;
            tagAdd.Owner = this;
            tagAdd.ShowDialog();
        }

        private void ListGroup_OnDrop(object sender, DragEventArgs e)
        {
            #region MyRegion
            var selectedDatabase = SelectedDataBase;// (DataBase)SelectDatabase.SelectedItem;
            var selConn = SelectedConnection;
            var pos = e.GetPosition(ListGroup);
            var result = VisualTreeHelper.HitTest(ListGroup, pos);
            if (result == null)
            {
                return;
            }
            //查找元数据
            if (!(e.Data.GetData(typeof(ObjectGroup)) is ObjectGroup sourceGroup))
            {
                return;
            }
            //查找目标数据
            var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
            if (listBoxItem == null)
            {
                return;
            }
            var targetGroup = listBoxItem.Content as ObjectGroup;
            if (ReferenceEquals(targetGroup, sourceGroup))
            {
                return;
            }
            var sourceOrder = sourceGroup.OrderFlag;
            var targetOrder = targetGroup.OrderFlag;
            sourceGroup.OrderFlag = targetOrder;
            targetGroup.OrderFlag = sourceOrder;
            var sqLiteHelper = new SQLiteHelper();
            var listG = new List<ObjectGroup>()
            {
                sourceGroup,
                targetGroup
            };
            sqLiteHelper.db.UpdateAll(listG);
            var datalist = sqLiteHelper.db.Table<ObjectGroup>().
                Where(x => x.ConnectId == selConn.ID && x.DataBaseName == selectedDatabase).
                OrderBy(x => x.OrderFlag).ToList();
            Dispatcher.Invoke(() =>
            {
                GroupMenuList = datalist;
                if (ChangeRefreshEvent != null)
                {
                    ChangeRefreshEvent();
                }
            });
            #endregion
        }


        private void ListGroup_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            #region MyRegion
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(ListGroup);
                HitTestResult result = VisualTreeHelper.HitTest(ListGroup, pos);
                if (result == null)
                {
                    return;
                }
                var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
                if (listBoxItem == null || listBoxItem.Content != ListGroup.SelectedItem)
                {
                    return;
                }
                DataObject dataObj = new DataObject(listBoxItem.Content as ObjectGroup);
                DragDrop.DoDragDrop(ListGroup, dataObj, DragDropEffects.Move);
            }
            #endregion
        }

        /// <summary>
        /// 修改分组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuEdit_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(ListGroup.SelectedItem is ObjectGroup selectedGroup))
            {
                Oops.Oh("请选择需要修改的分组.");
                return;
            }
            var groupAdd = new GroupAddView();
            groupAdd.SelectedGroup = selectedGroup;
            groupAdd.SelectedConnection = SelectedConnection;
            groupAdd.SelectedDataBase = SelectedDataBase;
            groupAdd.ChangeRefreshEvent += Tag_ChangeRefreshEvent;
            groupAdd.Owner = this;
            groupAdd.ShowDialog();
        }

        /// <summary>
        /// 删除分组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuDelete_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (!(ListGroup.SelectedItem is ObjectGroup selectedGroup))
            {
                Oops.Oh("请选择需要删除的分组.");
                return;
            }
            var msResult = HandyControl.Controls.MessageBox.Show("删除分组将不可恢复，是否继续？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);
            if (msResult == MessageBoxResult.OK)
            {
                var sqlLiteInstance = SQLiteHelper.GetInstance();
                var selectedDatabase = SelectedDataBase;
                var connKey = SelectedConnection.ID;
                Task.Run(() =>
                {
                    sqlLiteInstance.db.Delete<ObjectGroup>(selectedGroup.Id);
                    var datalist = sqlLiteInstance.db.Table<ObjectGroup>().
                        Where(x => x.ConnectId == connKey && x.DataBaseName == selectedDatabase).ToList();
                    var list = sqlLiteInstance.db.Table<SObjects>().Where(x =>
                        x.ConnectId == connKey &&
                        x.DatabaseName == selectedDatabase &&
                        x.GroupId == selectedGroup.Id).ToList();
                    if (list.Any())
                    {
                        foreach (var groupObj in list)
                        {
                            sqlLiteInstance.db.Delete<SObjects>(groupObj.Id);
                        }
                    }
                    Dispatcher.Invoke(() =>
                    {
                        GroupMenuList = datalist;
                        ReloadMenu();
                        MainContent = new UcGroupObjects();
                    });
                });
            }
            #endregion
        }
    }

    internal static class Utils
    {
        //根据子元素查找父元素
        public static T FindVisualParent<T>(DependencyObject obj) where T : class
        {
            while (obj != null)
            {
                if (obj is T)
                    return obj as T;

                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }
    }
}
