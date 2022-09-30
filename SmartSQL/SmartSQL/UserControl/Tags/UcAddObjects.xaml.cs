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
    public partial class UcAddObjects : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region PropertyFiled
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcAddObjects), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 当前选中连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(UcAddObjects), new PropertyMetadata(default(string)));
        /// <summary>
        /// 当前选中数据库
        /// </summary>
        public string SelectedDataBase
        {
            get => (string)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }

        public static readonly DependencyProperty SelectedTagProperty = DependencyProperty.Register(
            "SelectedTag", typeof(TagInfo), typeof(UcAddObjects), new PropertyMetadata(default(TagInfo)));
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
            "TagObjectList", typeof(List<TagObjectDTO>), typeof(UcAddObjects), new PropertyMetadata(default(List<TagObjectDTO>)));
        public List<TagObjectDTO> TagObjectList
        {
            get => (List<TagObjectDTO>)GetValue(TagObjectListProperty);
            set
            {
                SetValue(TagObjectListProperty, value);
                OnPropertyChanged(nameof(TagObjectList));
            }
        }
        #endregion

        public UcAddObjects()
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
                SelectedConnection.SelectedDbConnectString(SelectedDataBase), SelectedDataBase);
            var model = dbInstance.Init();
            var list = new List<TagObjectDTO>();
            var sqLiteInstance = SQLiteHelper.GetInstance();
            foreach (var table in model.Tables)
            {
                var isAny = sqLiteInstance.IsAny<TagObjects>(x =>
                    x.ConnectId == SelectedConnection.ID &&
                    x.DatabaseName == SelectedDataBase &&
                    x.TagId == SelectedTag.TagId &&
                    x.ObjectId == table.Value.Id
                );
                if (isAny)
                {
                    continue;
                }
                var tb = new TagObjectDTO()
                {
                    ObjectId = table.Value.Id,
                    Name = table.Value.Name,
                    ObjectType = 1,
                    Comment = table.Value.Comment
                };
                list.Add(tb);
            }
            MainNoDataText.Visibility = list.Any() ? Visibility.Collapsed : Visibility.Visible;

            TagObjectList = list;
        }

        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var checkedObjects = TagObjectList.Where(x => x.IsChecked == true).ToList();
            var sqLiteInstance = SQLiteHelper.GetInstance();
            var listObjects = new List<TagObjects>();
            foreach (var obj in checkedObjects)
            {
                var isAny = sqLiteInstance.IsAny<TagObjects>(x =>
                x.ConnectId == SelectedConnection.ID &&
                x.DatabaseName == SelectedDataBase &&
                x.TagId == SelectedTag.TagId &&
                x.ObjectId == obj.ObjectId
                );
                if (!isAny)
                {
                    listObjects.Add(new TagObjects
                    {
                        ConnectId = SelectedConnection.ID,
                        DatabaseName = SelectedDataBase,
                        ObjectId = obj.ObjectId,
                        ObjectName = obj.Name,
                        ObjectType = obj.ObjectType,
                        TagId = SelectedTag.TagId
                    });
                }
            }
            sqLiteInstance.Add(listObjects);
        }

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = (TagsView)Window.GetWindow(this);
            var ucTagObjects = new UcTagObjects();
            ucTagObjects.SelectedConnection = SelectedConnection;
            ucTagObjects.SelectedDataBase = SelectedDataBase;
            ucTagObjects.SelectedTag = SelectedTag;
            ucTagObjects.LoadPageData();
            parentWindow.MainContent = ucTagObjects;
        }


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

        private void SearchComObjType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            var selItem = (ComboBoxItem)SearchComObjType.SelectedItem;
            var dbInstance = ExporterFactory.CreateInstance(SelectedConnection.DbType,
                SelectedConnection.SelectedDbConnectString(SelectedDataBase), SelectedDataBase);
            var model = dbInstance.Init();
                var list = new List<TagObjectDTO>();
                var sqLiteInstance = SQLiteHelper.GetInstance();
            if ((string) selItem.Tag == "Table")
            {
                foreach (var table in model.Tables)
                {
                    var isAny = sqLiteInstance.IsAny<TagObjects>(x =>
                        x.ConnectId == SelectedConnection.ID &&
                        x.DatabaseName == SelectedDataBase &&
                        x.TagId == SelectedTag.TagId &&
                        x.ObjectId == table.Value.Id
                    );
                    if (isAny)
                    {
                        continue;
                    }
                    var tb = new TagObjectDTO()
                    {
                        ObjectId = table.Value.Id,
                        Name = table.Value.Name,
                        ObjectType = 1,
                        Comment = table.Value.Comment
                    };
                    list.Add(tb);
                }
            }
            else if ((string)selItem.Tag == "View")
            {
                foreach (var view in model.Views)
                {
                    var isAny = sqLiteInstance.IsAny<TagObjects>(x =>
                        x.ConnectId == SelectedConnection.ID &&
                        x.DatabaseName == SelectedDataBase &&
                        x.TagId == SelectedTag.TagId &&
                        x.ObjectId == view.Value.Id
                    );
                    if (isAny)
                    {
                        continue;
                    }
                    var tb = new TagObjectDTO()
                    {
                        ObjectId = view.Value.Id,
                        Name = view.Value.Name,
                        ObjectType = 1,
                        Comment = view.Value.Comment
                    };
                    list.Add(tb);
                }
            }
            else
            {
                foreach (var proc in model.Procedures)
                {
                    var isAny = sqLiteInstance.IsAny<TagObjects>(x =>
                        x.ConnectId == SelectedConnection.ID &&
                        x.DatabaseName == SelectedDataBase &&
                        x.TagId == SelectedTag.TagId &&
                        x.ObjectId == proc.Value.Id
                    );
                    if (isAny)
                    {
                        continue;
                    }
                    var tb = new TagObjectDTO()
                    {
                        ObjectId = proc.Value.Id,
                        Name = proc.Value.Name,
                        ObjectType = 1,
                        Comment = proc.Value.Comment
                    };
                    list.Add(tb);
                }
            }
            MainNoDataText.Visibility = list.Any() ? Visibility.Collapsed : Visibility.Visible;
            TagObjectList = list;
        }

        private void CheckedRow_Checked(object sender, RoutedEventArgs e)
        {
            var selectedItem = (TagObjectDTO)TableGrid.SelectedItem;
            if (selectedItem != null)
            {
                foreach (var item in TagObjectList)
                {
                    if (item.ObjectId == selectedItem.ObjectId && item.Name == selectedItem.Name)
                    {
                        item.IsChecked = true;
                    }
                }
            }
        }

        private void CheckedRow_Unchecked(object sender, RoutedEventArgs e)
        {
            var selectedItem = (TagObjectDTO)TableGrid.SelectedItem;
            if (selectedItem != null)
            {
                foreach (var item in TagObjectList)
                {
                    if (item.ObjectId == selectedItem.ObjectId && item.Name == selectedItem.Name)
                    {
                        item.IsChecked = false;
                    }
                }
            }
        }
    }
}
