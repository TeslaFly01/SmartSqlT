using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using HandyControl.Controls;
using HandyControl.Data;
using SmartSQL.Framework;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Annotations;
using SmartSQL.Models;

namespace SmartSQL.Views
{
    //定义一个委托去刷新主窗体
    public delegate void ObjChangeRefreshHandler();
    /// <summary>
    /// SetObjectGroup.xaml 的交互逻辑
    /// </summary>
    public partial class SetObjectGroup : INotifyPropertyChanged
    {
        /// <summary>
        /// 定义委托接收的事件
        /// </summary>
        public event ObjChangeRefreshHandler ObjChangeRefreshEvent;
        public SetObjectGroup()
        {
            InitializeComponent();
            DataContext = this;
        }
        public static readonly DependencyProperty ConnectionProperty = DependencyProperty.Register(
            "Connection", typeof(ConnectConfigs), typeof(SetObjectGroup), new PropertyMetadata(default(ConnectConfigs)));
        public ConnectConfigs Connection
        {
            get => (ConnectConfigs)GetValue(ConnectionProperty);
            set => SetValue(ConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(SetObjectGroup), new PropertyMetadata(default(string)));
        public string SelectedDataBase
        {
            get => (string)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }

        //public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
        //    "SelectedObject", typeof(string), typeof(SetObjectGroup), new PropertyMetadata(default(string)));
        //public string SelectedObject
        //{
        //    get => (string)GetValue(SelectedObjectProperty);
        //    set => SetValue(SelectedObjectProperty, value);
        //}

        public static readonly DependencyProperty SelectedObjectsProperty = DependencyProperty.Register(
            "SelectedObjects", typeof(List<TreeNodeItem>), typeof(SetObjectGroup), new PropertyMetadata(default(List<TreeNodeItem>)));
        /// <summary>
        /// 分组目标数据
        /// </summary>
        public List<TreeNodeItem> SelectedObjects
        {
            get => (List<TreeNodeItem>)GetValue(SelectedObjectsProperty);
            set
            {
                SetValue(SelectedObjectsProperty, value);
                OnPropertyChanged(nameof(SelectedObjects));
            }
        }

        public static readonly DependencyProperty ObjectGroupListProperty = DependencyProperty.Register(
            "ObjectGroupList", typeof(List<ObjectGroup>), typeof(SetObjectGroup), new PropertyMetadata(default(List<ObjectGroup>)));
        public List<ObjectGroup> ObjectGroupList
        {
            get => (List<ObjectGroup>)GetValue(ObjectGroupListProperty);
            set
            {
                SetValue(ObjectGroupListProperty, value);
                OnPropertyChanged(nameof(ObjectGroupList));
            }
        }

        public ObservableCollection<TreeNodeItem> LeftObjects { get; set; } =
            new ObservableCollection<TreeNodeItem>();


        private List<ObjectGroup> OldGroupList = new List<ObjectGroup>();

        private void SetObjectGroup_OnLoaded(object sender, RoutedEventArgs e)
        {
            var sqLiteHelper = new SQLiteHelper();
            SelectedObjects.ForEach(t => LeftObjects.Add(t));
            var list = sqLiteHelper.db.Table<ObjectGroup>().Where(x =>
                  x.ConnectId == Connection.ID && x.DataBaseName == SelectedDataBase).ToList();
            if (!list.Any())
            {
                return;
            }
            var selectedNames = SelectedObjects.Select(x => x.DisplayName);
            list.ForEach(x =>
            {
                var listObj = sqLiteHelper.db.Table<SObjects>().Count(xx => xx.GroupId == x.Id && selectedNames.Contains(xx.ObjectName));
                x.IsSelected = listObj == SelectedObjects.Count;
                if (listObj == SelectedObjects.Count)
                {
                    x.IsSelected = true;
                    OldGroupList.Add(x);
                }
            });
            ObjectGroupList = list;
        }

        /// <summary>
        /// 全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAllBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var sqLiteHelper = new SQLiteHelper();
            var list = sqLiteHelper.db.Table<ObjectGroup>().Where(x =>
                x.ConnectId == Connection.ID && x.DataBaseName == SelectedDataBase).ToList();
            if (!list.Any())
            {
                return;
            }
            ObjectGroupList = list;
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            if (ObjectGroupList == null || !ObjectGroupList.Any())
            {
                Growl.WarningGlobal(new GrowlInfo { Message = $"请先添加分组", WaitTime = 1, ShowDateTime = false });
                return;
            }
            var sqLiteHelper = new SQLiteHelper();
            var selectedObjNames = SelectedObjects.Select(x => x.DisplayName);
            //选中的分组名
            var selectedGroup = ObjectGroupList.Where(x => x.IsSelected).ToList();
            if (!selectedGroup.Any())
            {
                var list = sqLiteHelper.db.Table<SObjects>().Where(x =>
                   x.ConnectId == Connection.ID &&
                   x.DatabaseName == SelectedDataBase &&
                   selectedObjNames.Contains(x.ObjectName)).ToList();
                foreach (var sobj in list)
                {
                    sqLiteHelper.db.Delete<SObjects>(sobj.Id);
                }
                if (ObjChangeRefreshEvent != null)
                {
                    ObjChangeRefreshEvent();
                }
                this.Close();
                return;
            }

            if (OldGroupList != null)
            {
                var oldGroupIds = OldGroupList.Select(x => x.Id).ToList();
                //新增加选中的分组
                var newSelectedGroup = selectedGroup.Where(x => !oldGroupIds.Contains(x.Id)).ToList();
                if (newSelectedGroup.Any())
                {
                    var listNewObject = new List<SObjects>();
                    foreach (var objectGroup in newSelectedGroup)
                    {
                        foreach (var objName in selectedObjNames)
                        {
                            var newGroup = new SObjects
                            {
                                ConnectId = Connection.ID,
                                DatabaseName = SelectedDataBase,
                                GroupId = objectGroup.Id,
                                ObjectName = objName
                            };
                            listNewObject.Add(newGroup);
                        }
                    }
                    sqLiteHelper.db.InsertAll(listNewObject);
                }
            }
            else
            {
                var listNewObject = new List<SObjects>();
                foreach (var objectGroup in selectedGroup)
                {
                    foreach (var objName in selectedObjNames)
                    {
                        var isAny = sqLiteHelper.db.Table<SObjects>().Any(x =>
                            x.ConnectId == Connection.ID &&
                            x.DatabaseName == SelectedDataBase &&
                            x.ObjectName == objName);
                        if (!isAny)
                        {
                            var newGroup = new SObjects
                            {
                                ConnectId = Connection.ID,
                                DatabaseName = SelectedDataBase,
                                GroupId = objectGroup.Id,
                                ObjectName = objName
                            };
                            listNewObject.Add(newGroup);
                        }
                    }
                }
                sqLiteHelper.db.InsertAll(listNewObject);
            }

            //取消的分组
            var selectedGroupId = selectedGroup.Select(x => x.Id).ToList();
            var cancelGroupList = OldGroupList.Where(x => !selectedGroupId.Contains(x.Id)).ToList();
            if (cancelGroupList.Any())
            {
                foreach (var sobj in cancelGroupList)
                {
                    var objectList = sqLiteHelper.db.Table<SObjects>().Where(x =>
                        selectedObjNames.Contains(x.ObjectName) &&
                                               x.GroupId == sobj.Id
                    ).ToList();
                    if (objectList != null)
                    {
                        foreach (var obj in objectList)
                        {
                            sqLiteHelper.db.Delete<SObjects>(obj.Id);
                        }
                    }
                }
            }
            if (ObjChangeRefreshEvent != null)
            {
                ObjChangeRefreshEvent();
            }
            this.Close();
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ListGroup_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (listBox.SelectedItem != null)
            {
                var allItems = listBox.Items;
                if (allItems.Count == 1)
                {
                    return;
                }
                var selectedItem = (TreeNodeItem)listBox.SelectedItem;
                LeftObjects.Remove(selectedItem);
            }
        }
    }
}
