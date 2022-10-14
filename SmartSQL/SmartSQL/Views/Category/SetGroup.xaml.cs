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
    public partial class SetGroup : INotifyPropertyChanged
    {
        /// <summary>
        /// 定义委托接收的事件
        /// </summary>
        public event ObjChangeRefreshHandler ObjChangeRefreshEvent;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SetGroup()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region MyRegion
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(SetGroup), new PropertyMetadata(default(ConnectConfigs)));
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(SetGroup), new PropertyMetadata(default(string)));
        public string SelectedDataBase
        {
            get => (string)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }

        public static readonly DependencyProperty SelectedObjectsProperty = DependencyProperty.Register(
            "SelectedObjects", typeof(List<TreeNodeItem>), typeof(SetGroup), new PropertyMetadata(default(List<TreeNodeItem>)));
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
            "ObjectGroupList", typeof(List<GroupInfo>), typeof(SetGroup), new PropertyMetadata(default(List<GroupInfo>)));
        public List<GroupInfo> ObjectGroupList
        {
            get => (List<GroupInfo>)GetValue(ObjectGroupListProperty);
            set
            {
                SetValue(ObjectGroupListProperty, value);
                OnPropertyChanged(nameof(ObjectGroupList));
            }
        }
        #endregion

        public ObservableCollection<TreeNodeItem> LeftObjects { get; set; } =
            new ObservableCollection<TreeNodeItem>();


        private List<GroupInfo> OldGroupList = new List<GroupInfo>();

        private void SetObjectGroup_OnLoaded(object sender, RoutedEventArgs e)
        {
            var sqLiteHelper = new SQLiteHelper();
            SelectedObjects.ForEach(t => LeftObjects.Add(t));
            var list = sqLiteHelper.db.Table<GroupInfo>().Where(x =>
                  x.ConnectId == SelectedConnection.ID && x.DataBaseName == SelectedDataBase).ToList();
            if (!list.Any())
            {
                return;
            }
            var selectedNames = SelectedObjects.Select(x => x.DisplayName);
            list.ForEach(x =>
            {
                var listObj = sqLiteHelper.db.Table<GroupObjects>().Count(xx => xx.GroupId == x.Id && selectedNames.Contains(xx.ObjectName));
                x.IsSelected = listObj > 0;// listObj == SelectedObjects.Count;
                //if (listObj == SelectedObjects.Count)
                //{
                //    x.IsSelected = true;
                //    OldGroupList.Add(x);
                //}
            });
            ObjectGroupList = list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (ObjectGroupList == null || !ObjectGroupList.Any())
            {
                Growl.Warning(new GrowlInfo { Message = $"请先添加分组", WaitTime = 1, ShowDateTime = false });
                return;
            }
            var sqLiteHelper = new SQLiteHelper();
            var selectedObjNames = SelectedObjects.Select(x => x.DisplayName).ToList();
            //选中的分组名
            var selectedGroup = ObjectGroupList.Where(x => x.IsSelected).ToList();
            if (selectedGroup.Count > 1)
            {
                Growl.Warning(new GrowlInfo { Message = $"每个对象仅能属于一个分组，要设置多个请选择标签管理", WaitTime = 1, ShowDateTime = false });
                return;
            }
            if (!selectedGroup.Any())
            {
                //var list = sqLiteHelper.db.Table<SObjects>().Where(x =>
                //   x.ConnectId == SelectedConnection.ID &&
                //   x.DatabaseName == SelectedDataBase &&
                //   selectedObjNames.Contains(x.ObjectName)).ToList();
                //foreach (var sobj in list)
                //{
                //    sqLiteHelper.db.Delete<SObjects>(sobj.Id);
                //}
                //if (ObjChangeRefreshEvent != null)
                //{
                //    ObjChangeRefreshEvent();
                //}
                //this.Close();
                return;
            }
            var list = sqLiteHelper.db.Table<GroupObjects>().Where(x =>
                x.ConnectId == SelectedConnection.ID &&
                x.DatabaseName == SelectedDataBase &&
                selectedObjNames.Contains(x.ObjectName)).ToList();
            foreach (var sobj in list)
            {
                sqLiteHelper.db.Delete<GroupObjects>(sobj.Id);
            }
            var listNewObject = new List<GroupObjects>();
            selectedObjNames.ForEach(selObject =>
            {
                var newGroup = new GroupObjects
                {
                    ConnectId = SelectedConnection.ID,
                    DatabaseName = SelectedDataBase,
                    GroupId = selectedGroup[0].Id,
                    ObjectName = selObject
                };
                listNewObject.Add(newGroup);
            });
            sqLiteHelper.db.InsertAll(listNewObject);


            //if (OldGroupList != null)
            //{
            //    var oldGroupIds = OldGroupList.Select(x => x.Id).ToList();
            //    //新增加选中的分组
            //    var newSelectedGroup = selectedGroup.Where(x => !oldGroupIds.Contains(x.Id)).ToList();
            //    if (newSelectedGroup.Any())
            //    {
            //        var listNewObject = new List<SObjects>();
            //        foreach (var objectGroup in newSelectedGroup)
            //        {
            //            foreach (var objName in selectedObjNames)
            //            {
            //                var newGroup = new SObjects
            //                {
            //                    ConnectId = SelectedConnection.ID,
            //                    DatabaseName = SelectedDataBase,
            //                    GroupId = objectGroup.Id,
            //                    ObjectName = objName
            //                };
            //                listNewObject.Add(newGroup);
            //            }
            //        }
            //        sqLiteHelper.db.InsertAll(listNewObject);
            //    }
            //}
            //else
            //{
            //    var listNewObject = new List<SObjects>();
            //    foreach (var objectGroup in selectedGroup)
            //    {
            //        foreach (var objName in selectedObjNames)
            //        {
            //            var isAny = sqLiteHelper.db.Table<SObjects>().Any(x =>
            //                x.ConnectId == SelectedConnection.ID &&
            //                x.DatabaseName == SelectedDataBase &&
            //                x.ObjectName == objName);
            //            if (!isAny)
            //            {
            //                var newGroup = new SObjects
            //                {
            //                    ConnectId = SelectedConnection.ID,
            //                    DatabaseName = SelectedDataBase,
            //                    GroupId = objectGroup.Id,
            //                    ObjectName = objName
            //                };
            //                listNewObject.Add(newGroup);
            //            }
            //        }
            //    }
            //    sqLiteHelper.db.InsertAll(listNewObject);
            //}
            if (ObjChangeRefreshEvent != null)
            {
                ObjChangeRefreshEvent();
            }
            this.Close(); 
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
    }
}
