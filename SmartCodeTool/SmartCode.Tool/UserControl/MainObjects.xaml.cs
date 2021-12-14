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
using SmartCode.Framework.PhysicalDataModel;
using SmartCode.Tool.Annotations;
using SmartCode.Tool.Models;
using UserControlE = System.Windows.Controls.UserControl;

namespace SmartCode.Tool.UserControl
{
    /// <summary>
    /// MainObjects.xaml 的交互逻辑
    /// </summary>
    public partial class MainObjects : UserControlE, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #region Filds
        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(PropertyNodeItem), typeof(MainObjects), new PropertyMetadata(default(PropertyNodeItem)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(MainObjects), new PropertyMetadata(default(DataBase)));

        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(DataBasesConfig), typeof(MainObjects), new PropertyMetadata(default(DataBasesConfig)));

        public static readonly DependencyProperty ColunmDataProperty = DependencyProperty.Register(
            "ColunmData", typeof(List<Column>), typeof(MainObjects), new PropertyMetadata(default(List<Column>)));

        public static readonly DependencyProperty ObjectsViewDataProperty = DependencyProperty.Register(
            "ObjectsViewData", typeof(List<PropertyNodeItem>), typeof(MainObjects), new PropertyMetadata(default(List<PropertyNodeItem>)));

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(MainObjects), new PropertyMetadata(default(string)));

        /// <summary>
        /// 当前选中对象
        /// </summary>
        public PropertyNodeItem SelectedObject
        {
            get => (PropertyNodeItem)GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }
        /// <summary>
        /// 当前选中数据库
        /// </summary>
        public DataBase SelectedDataBase
        {
            get => (DataBase)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }
        /// <summary>
        /// 当前数据连接
        /// </summary>
        public DataBasesConfig SelectedConnection
        {
            get => (DataBasesConfig)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }
        public List<PropertyNodeItem> ObjectsViewData
        {
            get => (List<PropertyNodeItem>)GetValue(ObjectsViewDataProperty);
            set
            {
                SetValue(ObjectsViewDataProperty, value);
                OnPropertyChanged(nameof(ObjectsViewData));
            }
        }
        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set
            {
                SetValue(PlaceholderProperty, value);
                OnPropertyChanged(nameof(Placeholder));
            }
        }
        #endregion
        public MainObjects()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 加载页面数据
        /// </summary>
        public void LoadPageData()
        {
            NoDataText.Visibility = Visibility.Collapsed;
            if (SelectedObject.Type == ObjType.Type)
            {
                switch (SelectedObject.Name)
                {
                    case "treeTable":
                        ObjHead.Header = "表名称";
                        Placeholder = "请输入表名称或备注说明"; break;
                    case "treeView":
                        ObjHead.Header = "视图名称";
                        Placeholder = "请输入视图名称或备注说明"; break;
                    default:
                        ObjHead.Header = "存储过程名称";
                        Placeholder = "请输入存储过程名称或备注说明"; break;
                }

                if (SelectedObject.Parent == null)
                {
                    ObjectsViewData = ObjectsViewData.First(x => x.Name == SelectedObject.Name).Children;
                }
                else
                {
                    ObjectsViewData = ObjectsViewData.First(x => x.DisplayName == SelectedObject.Parent.DisplayName)
                        .Children;
                    ObjectsViewData = ObjectsViewData.First(x => x.Name == SelectedObject.Name).Children;
                }
                if (!ObjectsViewData.Any())
                {
                    NoDataText.Visibility = Visibility.Visible;
                }
                ObjItems = ObjectsViewData;
            }
        }

        private List<PropertyNodeItem> ObjItems;
        /// <summary>
        /// 实时搜索对象
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchObject_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchObject.Text.Trim();
            var searchData = ObjItems;
            if (!string.IsNullOrEmpty(searchText))
            {
                var obj = ObjItems.Where(x => x.DisplayName.ToLower().Contains(searchText.ToLower()) || (!string.IsNullOrEmpty(x.Comment) && x.Comment.ToLower().Contains(searchText.ToLower())));
                searchData = obj.Any() ? obj.ToList() : new List<PropertyNodeItem>();
            }
            ObjectsViewData = searchData;
        }
    }
}
