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
using HandyControl.Data;
using SmartSQL.Helper;
using SmartSQL.Framework.PhysicalDataModel;

namespace SmartSQL.UserControl.Tags
{
    /// <summary>
    /// TagObjects.xaml 的交互逻辑
    /// </summary>
    public partial class UcAddTagObject : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region PropertyFiled
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcAddTagObject), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(UcAddTagObject), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty DbDataProperty = DependencyProperty.Register(
            "DbData", typeof(Model), typeof(UcAddTagObject), new PropertyMetadata(default(Model)));

        public static readonly DependencyProperty SelectedTagProperty = DependencyProperty.Register(
            "SelectedTag", typeof(TagInfo), typeof(UcAddTagObject), new PropertyMetadata(default(TagInfo)));
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
        /// 当前选中数据库
        /// </summary>
        public Model DbData
        {
            get => (Model)GetValue(DbDataProperty);
            set => SetValue(DbDataProperty, value);
        }
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
            "TagObjectList", typeof(List<DbObjectDTO>), typeof(UcAddTagObject), new PropertyMetadata(default(List<DbObjectDTO>)));
        public List<DbObjectDTO> TagObjectList
        {
            get => (List<DbObjectDTO>)GetValue(TagObjectListProperty);
            set
            {
                SetValue(TagObjectListProperty, value);
                OnPropertyChanged(nameof(TagObjectList));
            }
        }

        /// <summary>
        /// 标签对象数据分页列表
        /// </summary>
        public static readonly DependencyProperty TagObjectPageListProperty = DependencyProperty.Register(
            "TagObjectPageList", typeof(List<DbObjectDTO>), typeof(UcAddTagObject), new PropertyMetadata(default(List<DbObjectDTO>)));
        public List<DbObjectDTO> TagObjectPageList
        {
            get => (List<DbObjectDTO>)GetValue(TagObjectPageListProperty);
            set
            {
                SetValue(TagObjectPageListProperty, value);
                OnPropertyChanged(nameof(TagObjectPageList));
            }
        }
        #endregion

        public UcAddTagObject()
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
            LoadingLine.Visibility = Visibility.Visible;
            UcTitle.Content = $"设置表/视图/存储过程到标签【{SelectedTag.TagName}】";
            var selConnection = SelectedConnection;
            var selDatabase = SelectedDataBase;
            var model = DbData;
            var selTag = SelectedTag;
            var list = new List<DbObjectDTO>();
            Task.Run(() =>
            {
                var sqLiteInstance = SQLiteHelper.GetInstance();
                foreach (var table in model.Tables)
                {
                    var isAny = sqLiteInstance.IsAny<TagObjects>(x =>
                        x.ConnectId == selConnection.ID &&
                        x.DatabaseName == selDatabase &&
                        x.TagId == selTag.TagId &&
                        x.ObjectId == table.Value.Id
                    );
                    var tb = new DbObjectDTO()
                    {
                        ObjectId = table.Value.Id,
                        Name = table.Value.DisplayName,
                        ObjectType = 1,
                        IsEnable = !isAny,
                        IsChecked = isAny,
                        Comment = table.Value.Comment
                    };
                    list.Add(tb);
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MainNoDataText.Visibility = list.Any() ? Visibility.Collapsed : Visibility.Visible;
                    TagObjectList = list;
                    PageData();
                }));
            });
        }

        /// <summary>
        /// 页码切换事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageT_OnPageUpdated(object sender, FunctionEventArgs<int> e)
        {
            LoadingLine.Visibility = Visibility.Visible;
            PageData(PageT.PageIndex);
        }

        /// <summary>
        /// 分页加载数据
        /// </summary>
        /// <param name="pageIndex"></param>
        private void PageData(int pageIndex = 0)
        {
            var tagObjects = TagObjectList;
            var totalCount = TagObjectList.Count;
            var pageSize = PageT.DataCountPerPage;
            Task.Run(() =>
            {
                var maxPageCount = (int)Math.Ceiling((decimal)totalCount / pageSize);
                var pageList = tagObjects.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    PageT.MaxPageCount = maxPageCount;
                    TagObjectPageList = pageList;
                    LoadingLine.Visibility = Visibility.Hidden;
                }));
            });
        }

        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var selConnection = SelectedConnection;
            var selDatabase = SelectedDataBase;
            var selTag = SelectedTag;
            var listObjects = new List<TagObjects>();
            var checkedObjects = TagObjectList.Where(x => x.IsChecked == true).ToList();
            LoadingLine.Visibility = Visibility.Visible;
            Task.Run(() =>
            {
                var sqLiteInstance = SQLiteHelper.GetInstance();
                foreach (var obj in checkedObjects)
                {
                    var isAny = sqLiteInstance.IsAny<TagObjects>(x =>
                        x.ConnectId == selConnection.ID &&
                        x.DatabaseName == selDatabase &&
                        x.TagId == selTag.TagId &&
                        x.ObjectId == obj.ObjectId
                    );
                    if (!isAny)
                    {
                        listObjects.Add(new TagObjects
                        {
                            ConnectId = selConnection.ID,
                            DatabaseName = selDatabase,
                            ObjectId = obj.ObjectId,
                            ObjectName = obj.Name,
                            ObjectType = obj.ObjectType,
                            TagId = selTag.TagId
                        });
                    }
                }
                sqLiteInstance.Add(listObjects);
                selTag.SubCount += listObjects.Count;
                sqLiteInstance.db.Update(selTag);
                Dispatcher.Invoke(new Action(() =>
                {
                    LoadPageData();
                    var parentWindow = (TagsView)Window.GetWindow(this);
                    parentWindow?.ReloadMenu();
                    Oops.Success($"成功设置{ listObjects.Count}条数据到标签【{SelectedTag.TagName}】");
                }));
            });
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
            ucTagObjects.DbData = DbData;
            ucTagObjects.SelectedTag = SelectedTag;
            ucTagObjects.LoadPageData();
            parentWindow.MainContent = ucTagObjects;
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in TagObjectList)
            {
                if (item.IsEnable)
                {
                    item.IsChecked = true;
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in TagObjectList)
            {
                if (item.IsEnable)
                {
                    item.IsChecked = false;
                }
            }
        }

        private void SearchComObjType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            LoadingLine.Visibility = Visibility.Visible;
            var selConnection = SelectedConnection;
            var selDatabase = SelectedDataBase;
            var selTag = SelectedTag;
            var model = DbData;
            var selItem = ((ComboBoxItem)SearchComObjType.SelectedItem).Tag;
            Task.Run(() =>
            {
                var list = new List<DbObjectDTO>();
                var sqLiteInstance = SQLiteHelper.GetInstance();
                if ((string)selItem == "Table")
                {
                    #region Table
                    foreach (var table in model.Tables)
                    {
                        var isAny = sqLiteInstance.IsAny<TagObjects>(x =>
                            x.ConnectId == selConnection.ID &&
                            x.DatabaseName == selDatabase &&
                            x.TagId == selTag.TagId &&
                            x.ObjectId == table.Value.Id
                        );
                        if (isAny)
                        {
                            continue;
                        }
                        var tb = new DbObjectDTO()
                        {
                            ObjectId = table.Value.Id,
                            Name = table.Value.DisplayName,
                            ObjectType = 1,
                            Comment = table.Value.Comment
                        };
                        list.Add(tb);
                    }
                    #endregion
                }
                else if ((string)selItem == "View")
                {
                    #region View
                    foreach (var view in model.Views)
                    {
                        var isAny = sqLiteInstance.IsAny<TagObjects>(x =>
                            x.ConnectId == selConnection.ID &&
                            x.DatabaseName == selDatabase &&
                            x.TagId == selTag.TagId &&
                            x.ObjectId == view.Value.Id
                        );
                        if (isAny)
                        {
                            continue;
                        }
                        var tb = new DbObjectDTO()
                        {
                            ObjectId = view.Value.Id,
                            Name = view.Value.DisplayName,
                            ObjectType = 1,
                            Comment = view.Value.Comment
                        };
                        list.Add(tb);
                    }
                    #endregion
                }
                else
                {
                    #region Proc
                    foreach (var proc in model.Procedures)
                    {
                        var isAny = sqLiteInstance.IsAny<TagObjects>(x =>
                            x.ConnectId == selConnection.ID &&
                            x.DatabaseName == selDatabase &&
                            x.TagId == selTag.TagId &&
                            x.ObjectId == proc.Value.Id
                        );
                        if (isAny)
                        {
                            continue;
                        }
                        var tb = new DbObjectDTO()
                        {
                            ObjectId = proc.Value.Id,
                            Name = proc.Value.DisplayName,
                            ObjectType = 1,
                            Comment = proc.Value.Comment
                        };
                        list.Add(tb);
                    }
                    #endregion
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MainNoDataText.Visibility = list.Any() ? Visibility.Collapsed : Visibility.Visible;
                    TagObjectList = list;
                    PageData();
                }));
            });
        }

        /// <summary>
        /// 实时搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchObjects_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            #region MyRegion
            LoadingLine.Visibility = Visibility.Visible;
            var searchText = SearchObjects.Text.Trim();
            var selConnection = SelectedConnection;
            var selDatabase = SelectedDataBase;
            var selTag = SelectedTag;
            var model = DbData;
            var selItem = ((ComboBoxItem)SearchComObjType.SelectedItem).Tag;
            Task.Run(() =>
            {
                var list = new List<DbObjectDTO>();
                var sqLiteInstance = SQLiteHelper.GetInstance();
                if ((string)selItem == "Table")
                {
                    #region Table
                    foreach (var table in model.Tables)
                    {
                        var isAny = sqLiteInstance.IsAny<TagObjects>(x =>
                            x.ConnectId == selConnection.ID &&
                            x.DatabaseName == selDatabase &&
                            x.TagId == selTag.TagId &&
                            x.ObjectId == table.Value.Id
                        );
                        if (isAny)
                        {
                            continue;
                        }
                        if (!string.IsNullOrEmpty(searchText))
                        {
                            if (!table.Value.Name.ToLower().Contains(searchText.ToLower()) &&
                                !table.Value.Comment.ToLower().Contains(searchText.ToLower()))
                            {
                                continue;
                            }
                        }
                        var tb = new DbObjectDTO()
                        {
                            ObjectId = table.Value.Id,
                            Name = table.Value.DisplayName,
                            ObjectType = 1,
                            Comment = table.Value.Comment
                        };
                        list.Add(tb);
                    }
                    #endregion
                }
                else if ((string)selItem == "View")
                {
                    #region View
                    foreach (var view in model.Views)
                    {
                        var isAny = sqLiteInstance.IsAny<TagObjects>(x =>
                            x.ConnectId == selConnection.ID &&
                            x.DatabaseName == selDatabase &&
                            x.TagId == selTag.TagId &&
                            x.ObjectId == view.Value.Id
                        );
                        if (isAny)
                        {
                            continue;
                        }
                        if (!string.IsNullOrEmpty(searchText))
                        {
                            if (!view.Value.Name.ToLower().Contains(searchText.ToLower()) &&
                                !view.Value.Comment.ToLower().Contains(searchText.ToLower()))
                            {
                                continue;
                            }
                        }
                        var tb = new DbObjectDTO()
                        {
                            ObjectId = view.Value.Id,
                            Name = view.Value.DisplayName,
                            ObjectType = 1,
                            Comment = view.Value.Comment
                        };
                        list.Add(tb);
                    }
                    #endregion
                }
                else
                {
                    #region Proc
                    foreach (var proc in model.Procedures)
                    {
                        var isAny = sqLiteInstance.IsAny<TagObjects>(x =>
                            x.ConnectId == selConnection.ID &&
                            x.DatabaseName == selDatabase &&
                            x.TagId == selTag.TagId &&
                            x.ObjectId == proc.Value.Id
                        );
                        if (isAny)
                        {
                            continue;
                        }
                        if (!string.IsNullOrEmpty(searchText))
                        {
                            if (!proc.Value.Name.ToLower().Contains(searchText.ToLower()) &&
                                !proc.Value.Comment.ToLower().Contains(searchText.ToLower()))
                            {
                                continue;
                            }
                        }
                        var tb = new DbObjectDTO()
                        {
                            ObjectId = proc.Value.Id,
                            Name = proc.Value.DisplayName,
                            ObjectType = 1,
                            Comment = proc.Value.Comment
                        };
                        list.Add(tb);
                    }
                    #endregion
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MainNoDataText.Visibility = list.Any() ? Visibility.Collapsed : Visibility.Visible;
                    TagObjectList = list;
                    PageData();
                }));
            }); 
            #endregion
        }

        private void CheckedRow_Checked(object sender, RoutedEventArgs e)
        {
            var selectedItem = (DbObjectDTO)TableGrid.CurrentItem;
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
            var selectedItem = (DbObjectDTO)TableGrid.CurrentItem;
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
