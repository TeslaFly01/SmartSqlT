using SmartSQL.Annotations;
using SmartSQL.Framework;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Models;
using SmartSQL.Views.Category;
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

namespace SmartSQL.UserControl.Tags
{
    /// <summary>
    /// TagObjects.xaml 的交互逻辑
    /// </summary>
    public partial class AddObjects : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region PropertyFiled
        public static readonly DependencyProperty ConnectionProperty = DependencyProperty.Register(
            "Connection", typeof(ConnectConfigs), typeof(AddObjects), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 当前选中连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(ConnectionProperty);
            set => SetValue(ConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(AddObjects), new PropertyMetadata(default(string)));
        /// <summary>
        /// 当前选中数据库
        /// </summary>
        public string SelectedDataBase
        {
            get => (string)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }

        public static readonly DependencyProperty SelectedTagProperty = DependencyProperty.Register(
            "SelectedTag", typeof(TagInfo), typeof(AddObjects), new PropertyMetadata(default(TagInfo)));
        /// <summary>
        /// 当前选中标签
        /// </summary>
        public TagInfo SelectedTag
        {
            get => (TagInfo)GetValue(SelectedTagProperty);
            set => SetValue(SelectedTagProperty, value);
        }

        /// <summary>
        /// 标签对象数据列表
        /// </summary>
        public static readonly DependencyProperty TagObjectListProperty = DependencyProperty.Register(
            "TagObjectList", typeof(List<TreeNodeItem>), typeof(AddObjects), new PropertyMetadata(default(List<TreeNodeItem>)));
        public List<TreeNodeItem> TagObjectList
        {
            get => (List<TreeNodeItem>)GetValue(TagObjectListProperty);
            set
            {
                SetValue(TagObjectListProperty, value);
                OnPropertyChanged(nameof(TagObjectList));
            }
        }
        #endregion

        public AddObjects()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 初始化加载数据
        /// </summary>
        public void LoadPageData()
        {
            UcTitle.Content = $"设置表/视图/存储过程到标签【{SelectedTag.TagName}】";
            var dbInstance = ExporterFactory.CreateInstance(SelectedConnection.DbType,
                SelectedConnection.SelectedDbConnectString(SelectedDataBase));
            var model = dbInstance.Init();
            var list = new List<TreeNodeItem>();
            foreach (var table in model.Tables)
            {
                var tb = new TreeNodeItem()
                {
                    ObejcetId = table.Value.Id,
                    DisplayName = table.Value.DisplayName,
                    Name = table.Value.Name,
                    Schema = table.Value.SchemaName,
                    Comment = table.Value.Comment,
                    CreateDate = table.Value.CreateDate,
                    ModifyDate = table.Value.ModifyDate,
                    Type = ObjType.Table
                };
                list.Add(tb);
            }
            if (list.Any())
            {
                MainNoDataText.Visibility = Visibility.Collapsed;
            }
            TagObjectList = list;
        }

        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = (TagsView)Window.GetWindow(this);
            var ucTagObjects = new TagObjects();
            ucTagObjects.SelectedConnection = SelectedConnection;
            ucTagObjects.SelectedDataBase = SelectedDataBase;
            ucTagObjects.SelectedTag = SelectedTag;
            parentWindow.MainContent = ucTagObjects;
        }


        private ObservableCollection<TreeNodeItem> _TreeNodeList = new ObservableCollection<TreeNodeItem>();
        public ObservableCollection<TreeNodeItem> TreeNodeList { get { return _TreeNodeList; } }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in TagObjectList)
            {
                item.IsChecked = true;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in TagObjectList)
            {
                item.IsChecked = false;
            }
        }
    }
}
