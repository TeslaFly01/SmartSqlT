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
using HandyControl.Controls;
using HandyControl.Data;
using SmartCode.Framework;
using SmartCode.Framework.SqliteModel;
using SmartCode.Tool.Annotations;

namespace SmartCode.Tool.Views
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

        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(string), typeof(SetObjectGroup), new PropertyMetadata(default(string)));
        public string SelectedObject
        {
            get => (string)GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }

        public static readonly DependencyProperty ObjectGroupListProperty = DependencyProperty.Register(
            "ObjectGroupList", typeof(List<ObjectGroup>), typeof(SetObjectGroup), new PropertyMetadata(default(List<ObjectGroup>)));
        public List<ObjectGroup> ObjectGroupList
        {
            get => (List<ObjectGroup>)GetValue(ObjectGroupListProperty);
            set
            {
                SetValue(ObjectGroupListProperty, value);
                OnPropertyChanged("ObjectGroupList");
            }
        }

        private List<ObjectGroup> OldGroupList = null;

        private void SetObjectGroup_OnLoaded(object sender, RoutedEventArgs e)
        {
            var sqLiteHelper = new SQLiteHelper();
            var list = sqLiteHelper.db.Table<ObjectGroup>().Where(x =>
                  x.ConnectionName == Connection.ConnectName && x.DataBaseName == SelectedDataBase).ToList();
            if (!list.Any())
            {
                SelectAllBtn.Visibility = Visibility.Collapsed;
                return;
            }
            var listGroupId = list.Select(x => x.Id);
            var listObj = sqLiteHelper.db.Table<SObjects>().Where(x => listGroupId.Contains(x.GroupId) && x.ObjectName == SelectedObject).Select(x => x.GroupId).ToList();
            OldGroupList = list.Where(x => listObj.Contains(x.Id)).ToList();
            list.ForEach(x =>
            {
                x.IsSelected = listObj.Contains(x.Id);
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
            bool flag = SelectAllBtn.IsChecked == true ? true : false;
            var sqLiteHelper = new SQLiteHelper();
            var list = sqLiteHelper.db.Table<ObjectGroup>().Where(x =>
                x.ConnectionName == Connection.ConnectName && x.DataBaseName == SelectedDataBase).ToList();
            if (!list.Any())
            {
                return;
            }
            list.ForEach(x =>
            {
                x.IsSelected = flag;
            });
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
            //选中的分组名
            var selectedGroup = ObjectGroupList.Where(x => x.IsSelected).ToList();
            if (!selectedGroup.Any())
            {
                var list = sqLiteHelper.db.Table<SObjects>().Where(x =>
                   x.ConnectionName == Connection.ConnectName &&
                   x.DatabaseName == SelectedDataBase &&
                   x.ObjectName == SelectedObject).ToList();
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
            var oldGroupIds = OldGroupList.Select(x => x.Id).ToList();
            //新增加选中的分组
            var newSelectedGroup = selectedGroup.Where(x => !oldGroupIds.Contains(x.Id)).ToList();
            if (newSelectedGroup.Any())
            {
                var listNewObject = new List<SObjects>();
                foreach (var objectGroup in newSelectedGroup)
                {
                    var newGroup = new SObjects
                    {
                        ConnectionName = Connection.ConnectName,
                        DatabaseName = SelectedDataBase,
                        GroupId = objectGroup.Id,
                        ObjectName = SelectedObject
                    };
                    listNewObject.Add(newGroup);
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
                    var sObject = sqLiteHelper.db.Table<SObjects>().FirstOrDefault(x =>

                        x.ObjectName == SelectedObject &&
                        x.GroupId == sobj.Id
                    );
                    if (sObject != null)
                    {
                        sqLiteHelper.db.Delete<SObjects>(sObject.Id);
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
    }
}
