using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using SmartSQL.Models;
using System.Runtime.CompilerServices;
using SmartSQL.Framework;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Annotations;
using SmartSQL.DocUtils;
using SmartSQL.Framework.Const;
using SmartSQL.Helper;
using SmartSQL.UserControl;
using SmartSQL.Views;
using SqlSugar;
using ComboBox = System.Windows.Controls.ComboBox;
using FontAwesome = FontAwesome.WPF.FontAwesome;
using TabControl = System.Windows.Controls.TabControl;
using TabItem = System.Windows.Controls.TabItem;
using System.IO;
using SmartSQL.Views.Category;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcMainContent : BaseUserControl
    {
        private static readonly string GROUPICON = "pack://application:,,,/Resources/svg/category.svg";
        private static readonly string TAGICON = "pack://application:,,,/Resources/svg/tag.svg";
        private static readonly string TABLEICON = "pack://application:,,,/Resources/svg/table.svg";
        private static readonly string VIEWICON = "pack://application:,,,/Resources/svg/view.svg";
        private static readonly string PROCICON = "pack://application:,,,/Resources/svg/proc.svg";



        private static readonly Dictionary<string, string> IconDic = new Dictionary<string, string>
        {
            {"Type", "pack://application:,,,/Resources/svg/category.svg"},
            {"Table", "pack://application:,,,/Resources/svg/table.svg"},
            {"View", "pack://application:,,,/Resources/svg/view.svg"},
            {"Proc", "pack://application:,,,/Resources/svg/proc.svg"}
        };

        private List<TreeNodeItem> itemList = new List<TreeNodeItem>();

        #region Fields
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcMainContent), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty MenuDataProperty = DependencyProperty.Register(
            "MenuData", typeof(Model), typeof(UcMainContent), new PropertyMetadata(default(Model)));

        public static readonly DependencyProperty TreeViewDataProperty = DependencyProperty.Register(
            "TreeViewData", typeof(List<TreeNodeItem>), typeof(UcMainContent), new PropertyMetadata(default(List<TreeNodeItem>)));

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(int), typeof(UcMainContent), new PropertyMetadata(default(int)));
        /// <summary>
        /// 菜单源数据
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        /// <summary>
        /// 菜单源数据
        /// </summary>
        public Model MenuData
        {
            get => (Model)GetValue(MenuDataProperty);
            set => SetValue(MenuDataProperty, value);
        }

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

        /// <summary>
        /// 选项卡圆角度数
        /// </summary>
        public int CornerRadius
        {
            get => (int)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        #endregion

        public ObservableCollection<MainTabWModel> TabItemData = new ObservableCollection<MainTabWModel>();

        public UcMainContent()
        {
            InitializeComponent();
            CornerRadius = 10;
            DataContext = this;
        }

        /// <summary>
        /// 页面初始化加载
        /// </summary>
        public void PageLoad(ConnectConfigs connectConfig)
        {
            #region MyRegion
            var sqLiteHelper = new SQLiteHelper();
            var leftMenuType = sqLiteHelper.GetSysInt(SysConst.Sys_LeftMenuType);
            TabLeftType.SelectedIndex = leftMenuType - 1;
            var isMultipleTab = sqLiteHelper.GetSysBool(SysConst.Sys_IsMultipleTab);
            CornerRadius = isMultipleTab ? 0 : 10;
            MainW.Visibility = Visibility.Collapsed;
            MainTabW.Visibility = Visibility.Collapsed;
            TabItemData.Clear();
            MainTabW.DataContext = TabItemData;
            MainTabW.SetBinding(ItemsControl.ItemsSourceProperty, new Binding());

            LoadingLine.Visibility = Visibility.Visible;
            SelectedConnection = connectConfig;
            try
            {
                var dbInstance = ExporterFactory.CreateInstance(connectConfig.DbType, connectConfig.DbMasterConnectString);
                var list = dbInstance.GetDatabases(connectConfig.DefaultDatabase);
                SelectDatabase.ItemsSource = list;
                HidSelectDatabase.Text = connectConfig.DefaultDatabase;
                if (connectConfig.DbType == DbType.PostgreSQL)
                {
                    SelectDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.EndsWith("public"));
                }
                else
                {
                    SelectDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName == connectConfig.DefaultDatabase);
                }
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Oops.God($"连接失败 {connectConfig.ConnectName}，原因：" + ex.ToMsg());
                    LoadingLine.Visibility = Visibility.Collapsed;
                }));
            }
            #endregion
        }

        /// <summary>
        /// 选择数据库发生变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectDatabase_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded)
            {
                return;
            }
            var selectDatabase = SelectDatabase.SelectedItem;
            if (selectDatabase != null)
            {
                var selectedDbBase = (DataBase)selectDatabase;
                HidSelectDatabase.Text = ((DataBase)selectDatabase).DbName;
                var sqLiteHelper = new SQLiteHelper();
                sqLiteHelper.SetSysValue(SysConst.Sys_SelectedDataBase, selectedDbBase.DbName);
                MenuBind();
            }
            #endregion
        }

        public void MenuBind()
        {
            #region MyRegion
            LoadingLine.Visibility = Visibility.Visible;
            NoDataText.Visibility = Visibility.Collapsed;
            var selectDataBase = HidSelectDatabase.Text;
            var selectConnection = SelectedConnection;
            var menuData = MenuData;
            Task.Run(() =>
            {
                var sqLiteHelper = new SQLiteHelper();
                var leftMenuType = sqLiteHelper.GetSysInt(SysConst.Sys_LeftMenuType);
                var isContainsObjName = sqLiteHelper.GetSysBool(SysConst.Sys_IsContainsObjName);
                //分组相关列表
                var curObjects = new List<SObjectDTO>();
                var curGroups = new List<GroupInfo>();
                //标签相关列表
                var curTagObjects = new List<MenuTagObjectsDTO>();
                var curTags = new List<TagInfo>();
                var itemParentList = new List<TreeNodeItem>();
                itemList = new List<TreeNodeItem>();
                var nodeTable = new TreeNodeItem
                {
                    ObejcetId = "0",
                    DisplayName = "表",
                    Name = "treeTable",
                    Icon = TABLEICON,
                    Type = ObjType.Type,
                    IsShowCount = Visibility.Visible
                };
                itemList.Add(nodeTable);
                var nodeView = new TreeNodeItem
                {
                    ObejcetId = "0",
                    DisplayName = "视图",
                    Name = "treeView",
                    Icon = VIEWICON,
                    Type = ObjType.Type,
                    IsShowCount = Visibility.Visible
                };
                itemList.Add(nodeView);
                var nodeProc = new TreeNodeItem
                {
                    ObejcetId = "0",
                    DisplayName = "存储过程",
                    Name = "treeProc",
                    Icon = PROCICON,
                    Type = ObjType.Type,
                    IsShowCount = Visibility.Visible
                };
                itemList.Add(nodeProc);

                #region 分组业务处理
                //是否业务分组
                if (leftMenuType == LeftMenuType.Group.GetHashCode())
                {
                    curGroups = sqLiteHelper.db.Table<GroupInfo>().Where(a =>
                        a.ConnectId == selectConnection.ID &&
                        a.DataBaseName == selectDataBase).OrderBy(x => x.OrderFlag).ToList();
                    if (curGroups.Any())
                    {
                        foreach (var group in curGroups)
                        {
                            var itemChildList = new List<TreeNodeItem>();
                            var nodeGroup = new TreeNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = group.GroupName,
                                Name = "treeGroup",
                                Icon = GROUPICON,
                                //FontWeight = "Bold",
                                Type = ObjType.Group,
                                IsExpanded = !(!group.OpenLevel.HasValue || group.OpenLevel == 0),
                                IsShowCount = Visibility.Visible
                            };
                            var nodeTable1 = new TreeNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = "表",
                                Name = "treeTable",
                                Icon = TABLEICON,
                                Parent = nodeGroup,
                                Type = ObjType.Type,
                                IsExpanded = group.OpenLevel == 2,
                                IsShowCount = Visibility.Visible
                            };
                            itemChildList.Add(nodeTable1);
                            var nodeView1 = new TreeNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = "视图",
                                Name = "treeView",
                                Icon = VIEWICON,
                                Parent = nodeGroup,
                                Type = ObjType.Type,
                                IsExpanded = group.OpenLevel == 2,
                                IsShowCount = Visibility.Visible
                            };
                            itemChildList.Add(nodeView1);
                            var nodeProc1 = new TreeNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = "存储过程",
                                Name = "treeProc",
                                Icon = PROCICON,
                                Parent = nodeGroup,
                                Type = ObjType.Type,
                                IsExpanded = group.OpenLevel == 2,
                                IsShowCount = Visibility.Visible
                            };
                            itemChildList.Add(nodeProc1);
                            nodeGroup.Children = itemChildList;
                            itemParentList.Add(nodeGroup);
                        }
                    }
                    curObjects = (from a in sqLiteHelper.db.Table<GroupInfo>()
                                  join b in sqLiteHelper.db.Table<GroupObjects>() on a.Id equals b.GroupId
                                  where a.ConnectId == selectConnection.ID &&
                                        a.DataBaseName == selectDataBase
                                  select new SObjectDTO
                                  {
                                      GroupName = a.GroupName,
                                      ObjectName = b.ObjectName
                                  }).ToList();
                }
                #endregion

                #region 标签业务处理
                //是否业务标签
                if (leftMenuType == LeftMenuType.Tag.GetHashCode())
                {
                    curTags = sqLiteHelper.db.Table<TagInfo>().Where(a =>
                        a.ConnectId == selectConnection.ID &&
                        a.DataBaseName == selectDataBase).ToList();
                    if (curTags.Any())
                    {
                        foreach (var tag in curTags)
                        {
                            var itemChildList = new List<TreeNodeItem>();
                            var nodeGroup = new TreeNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = tag.TagName,
                                Name = "treeTag",
                                Icon = TAGICON,
                                Type = ObjType.Group,
                                IsShowCount = Visibility.Visible
                            };
                            var nodeTable1 = new TreeNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = "表",
                                Name = "treeTable",
                                Icon = TABLEICON,
                                Parent = nodeGroup,
                                Type = ObjType.Type,
                                IsShowCount = Visibility.Visible
                            };
                            itemChildList.Add(nodeTable1);
                            var nodeView1 = new TreeNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = "视图",
                                Name = "treeView",
                                Icon = VIEWICON,
                                Parent = nodeGroup,
                                Type = ObjType.Type,
                                IsShowCount = Visibility.Visible
                            };
                            itemChildList.Add(nodeView1);
                            var nodeProc1 = new TreeNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = "存储过程",
                                Name = "treeProc",
                                Icon = PROCICON,
                                Parent = nodeGroup,
                                Type = ObjType.Type,
                                IsShowCount = Visibility.Visible
                            };
                            itemChildList.Add(nodeProc1);
                            nodeGroup.Children = itemChildList;
                            itemParentList.Add(nodeGroup);
                        }
                    }
                    curTagObjects = (from a in sqLiteHelper.db.Table<TagInfo>()
                                     join b in sqLiteHelper.db.Table<TagObjects>() on a.TagId equals b.TagId
                                     where a.ConnectId == selectConnection.ID &&
                                           a.DataBaseName == selectDataBase
                                     select new MenuTagObjectsDTO
                                     {
                                         TagName = a.TagName,
                                         ObjectName = b.ObjectName
                                     }).ToList();
                }
                #endregion
                var model = new Model();
                try
                {
                    var dbInstance = ExporterFactory.CreateInstance(selectConnection.DbType, selectConnection.SelectedDbConnectString(selectDataBase), selectDataBase);
                    model = dbInstance.Init();
                    menuData = model;
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Oops.God($"连接失败 {selectConnection.ConnectName}，原因：" + ex.ToMsg());
                    }));
                }
                var textColor = "#4f5d79";
                #region 表
                foreach (var table in model.Tables)
                {
                    var isShowComment = !isContainsObjName
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                    var tableItem = new TreeNodeItem()
                    {
                        ObejcetId = table.Value.Id,
                        DisplayName = table.Value.DisplayName,
                        Name = table.Value.Name,
                        Schema = table.Value.SchemaName,
                        Comment = table.Value.Comment,
                        CreateDate = table.Value.CreateDate,
                        ModifyDate = table.Value.ModifyDate,
                        TextColor = textColor,
                        Icon = TABLEICON,
                        Type = ObjType.Table,
                        IsShowComment = isShowComment
                    };
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
                    {
                        #region MyRegion
                        var hasGroup = curObjects.Where(x => x.ObjectName == table.Key).
                                            GroupBy(x => x.GroupName).Select(x => x.Key)
                                            .ToList();
                        foreach (var group in hasGroup)
                        {
                            var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                            if (pGroup != null)
                            {
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == "表");
                                if (ppGroup != null)
                                {
                                    ppGroup.Children.Add(tableItem);
                                }
                            }
                        }
                        #endregion
                    }
                    //是否业务标签
                    else if (leftMenuType == LeftMenuType.Tag.GetHashCode())
                    {
                        #region MyRegion
                        var hasTag = curTagObjects
                                            .Where(x => x.ObjectName == table.Key)
                                            .GroupBy(x => x.TagName)
                                            .Select(x => x.Key)
                                            .ToList();
                        foreach (var tag in hasTag)
                        {
                            var pTag = itemParentList.FirstOrDefault(x => x.DisplayName == tag);
                            if (pTag != null)
                            {
                                var ppTag = pTag.Children.FirstOrDefault(x => x.DisplayName == "表");
                                if (ppTag != null)
                                {
                                    ppTag.Children.Add(tableItem);
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region MyRegion
                        nodeTable.Children.Add(tableItem);
                        #endregion
                    }
                }
                #endregion

                #region 视图
                foreach (var view in model.Views)
                {
                    var isShowComment = !isContainsObjName
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                    var viewItem = new TreeNodeItem()
                    {
                        ObejcetId = view.Value.Id,
                        DisplayName = view.Value.DisplayName,
                        Name = view.Value.Name,
                        Schema = view.Value.SchemaName,
                        Comment = view.Value.Comment,
                        CreateDate = view.Value.CreateDate,
                        ModifyDate = view.Value.ModifyDate,
                        TextColor = textColor,
                        Icon = VIEWICON,
                        Type = ObjType.View,
                        IsShowComment = isShowComment
                    };
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
                    {
                        #region MyRegion
                        var hasGroup = curObjects.Where(x => x.ObjectName == view.Key).
                                            GroupBy(x => x.GroupName).Select(x => x.Key)
                                            .ToList();
                        foreach (var group in hasGroup)
                        {
                            var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                            if (pGroup != null)
                            {
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == "视图");
                                if (ppGroup != null)
                                {
                                    ppGroup.Children.Add(viewItem);
                                }
                            }
                        }
                        #endregion
                    }
                    else if (leftMenuType == LeftMenuType.Tag.GetHashCode())
                    {
                        #region MyRegion
                        var hasTag = curTagObjects.Where(x => x.ObjectName == view.Key).
                                            GroupBy(x => x.TagName).Select(x => x.Key)
                                            .ToList();
                        foreach (var tag in hasTag)
                        {
                            var pTag = itemParentList.FirstOrDefault(x => x.DisplayName == tag);
                            if (pTag != null)
                            {
                                var ppTag = pTag.Children.FirstOrDefault(x => x.DisplayName == "视图");
                                if (ppTag != null)
                                {
                                    ppTag.Children.Add(viewItem);
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region MyRegion
                        nodeView.Children.Add(viewItem);
                        #endregion
                    }
                }
                #endregion

                #region 存储过程
                foreach (var proc in model.Procedures)
                {
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
                    {
                        #region MyRegion
                        var hasGroup = curObjects.Where(x => x.ObjectName == proc.Key).GroupBy(x => x.GroupName)
                                            .Select(x => x.Key)
                                            .ToList();
                        foreach (var group in hasGroup)
                        {
                            var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                            if (pGroup != null)
                            {
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == "存储过程");
                                if (ppGroup != null)
                                {
                                    ppGroup.Children.Add(new TreeNodeItem()
                                    {
                                        ObejcetId = proc.Value.Id,
                                        DisplayName = proc.Value.DisplayName,
                                        Name = proc.Value.Name,
                                        Schema = proc.Value.SchemaName,
                                        Comment = proc.Value.Comment,
                                        CreateDate = proc.Value.CreateDate,
                                        ModifyDate = proc.Value.ModifyDate,
                                        TextColor = textColor,
                                        Icon = PROCICON,
                                        Type = ObjType.Proc
                                    });
                                }
                            }
                        }
                        #endregion
                    }
                    else if (leftMenuType == LeftMenuType.Tag.GetHashCode())
                    {
                        #region MyRegion
                        var hasTag = curTagObjects
                                            .Where(x => x.ObjectName == proc.Key)
                                            .GroupBy(x => x.TagName)
                                            .Select(x => x.Key)
                                            .ToList();
                        foreach (var tag in hasTag)
                        {
                            var pTag = itemParentList.FirstOrDefault(x => x.DisplayName == tag);
                            if (pTag != null)
                            {
                                var ppTag = pTag.Children.FirstOrDefault(x => x.DisplayName == "存储过程");
                                if (ppTag != null)
                                {
                                    ppTag.Children.Add(new TreeNodeItem()
                                    {
                                        ObejcetId = proc.Value.Id,
                                        DisplayName = proc.Value.DisplayName,
                                        Name = proc.Value.Name,
                                        Schema = proc.Value.SchemaName,
                                        Comment = proc.Value.Comment,
                                        CreateDate = proc.Value.CreateDate,
                                        ModifyDate = proc.Value.ModifyDate,
                                        TextColor = textColor,
                                        Icon = PROCICON,
                                        Type = ObjType.Proc
                                    });
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region MyRegion
                        nodeProc.Children.Add(new TreeNodeItem()
                        {
                            ObejcetId = proc.Value.Id,
                            DisplayName = proc.Value.DisplayName,
                            Name = proc.Value.Name,
                            Schema = proc.Value.SchemaName,
                            Comment = proc.Value.Comment,
                            CreateDate = proc.Value.CreateDate,
                            ModifyDate = proc.Value.ModifyDate,
                            TextColor = textColor,
                            Icon = PROCICON,
                            Type = ObjType.Proc
                        });
                        #endregion
                    }
                }
                #endregion

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    #region MyRegion
                    LoadingLine.Visibility = Visibility.Hidden;
                    //编写获取数据并显示在界面的代码
                    if (leftMenuType == LeftMenuType.Group.GetHashCode() || leftMenuType == LeftMenuType.Tag.GetHashCode())
                    {
                        if (!itemParentList.Any())
                        {
                            var typeText = (LeftMenuType)leftMenuType == LeftMenuType.Group ? "分组" : "标签";
                            var tipText = $"暂无{typeText}，请先创建{typeText}";
                            NoDataAreaText.TipText = tipText;
                            NoDataText.Visibility = Visibility.Visible;
                            BtnNoData.Content = $"创建{typeText}";
                            BtnNoData.Visibility = Visibility.Visible;
                        }
                        itemParentList.ForEach(treeItem =>
                        {
                            treeItem.Children.ForEach(obj =>
                            {
                                if (!obj.Children.Any())
                                {
                                    obj.Visibility = nameof(Visibility.Collapsed);
                                }
                                //obj.DisplayName += $"（{obj.Children.Count}）";
                                obj.ChildrenCount = obj.Children.Count;
                            });
                            treeItem.ChildrenCount = treeItem.Children[0].Children.Count + treeItem.Children[1].Children.Count + treeItem.Children[2].Children.Count;
                        });
                        TreeViewData = itemParentList;
                        SearchMenu.Text = string.Empty;
                    }
                    else
                    {
                        if (!itemList.Any(x => x.Children.Count > 0))
                        {
                            NoDataAreaText.TipText = "暂无数据";
                            NoDataText.Visibility = Visibility.Visible;
                            BtnNoData.Visibility = Visibility.Collapsed;
                        }
                        itemList.ForEach(obj =>
                        {
                            if (!obj.Children.Any())
                            {
                                obj.Visibility = nameof(Visibility.Collapsed);
                            }
                            //obj.DisplayName += $"（{obj.Children.Count}）";
                            obj.ChildrenCount = obj.Children.Count;
                        });
                        TreeViewData = itemList;
                        SearchMenu.Text = string.Empty;
                    }
                    MenuData = menuData;
                    #endregion
                }));
            });
            #endregion
        }

        /// <summary>
        /// 刷新菜单列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFresh_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedConnection == null)
            {
                return;
            }
            var searchText = SearchMenu.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                SearchMenuBind();
                return;
            }
            MenuBind();
        }

        /// <summary>
        /// 左侧菜单动态实时搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchMenu_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedConnection == null)
            {
                return;
            }
            SearchMenuBind();
        }

        /// <summary>
        /// 搜索菜单绑定
        /// </summary>
        private void SearchMenuBind()
        {
            #region MyRegion
            NoDataText.Visibility = Visibility.Collapsed;
            itemList = new List<TreeNodeItem>();
            var searchText = SearchMenu.Text.ToLower().Trim();
            var nodeTable = new TreeNodeItem()
            {
                ObejcetId = "0",
                DisplayName = "表",
                Name = "treeTable",
                Icon = TABLEICON,
                Type = ObjType.Type,
                IsExpanded = true,
                IsShowCount = Visibility.Visible
            };
            itemList.Add(nodeTable);
            var nodeView = new TreeNodeItem()
            {
                ObejcetId = "0",
                DisplayName = "视图",
                Name = "treeView",
                Icon = VIEWICON,
                Type = ObjType.Type,
                IsExpanded = true,
                IsShowCount = Visibility.Visible
            };
            itemList.Add(nodeView);
            var nodeProc = new TreeNodeItem()
            {
                ObejcetId = "0",
                DisplayName = "存储过程",
                Name = "treeProc",
                Icon = PROCICON,
                Type = ObjType.Type,
                IsExpanded = true,
                IsShowCount = Visibility.Visible
            };
            itemList.Add(nodeProc);
            var sqLiteHelper = new SQLiteHelper();
            var leftMenuType = sqLiteHelper.GetSysInt(SysConst.Sys_LeftMenuType);
            var isLikeSearch = sqLiteHelper.GetSysBool(SysConst.Sys_IsLikeSearch);
            var isContainsObjName = sqLiteHelper.GetSysBool(SysConst.Sys_IsContainsObjName);
            var selectDataBase = HidSelectDatabase.Text;
            var selectConnection = SelectedConnection;
            //分组相关列表
            var currObjects = new List<SObjectDTO>();
            var currGroups = new List<GroupInfo>();
            //标签相关列表
            var curTagObjects = new List<MenuTagObjectsDTO>();
            var curTags = new List<TagInfo>();
            var itemParentList = new List<TreeNodeItem>();
            #region 分组业务处理
            if (leftMenuType == LeftMenuType.Group.GetHashCode())
            {
                currGroups = sqLiteHelper.db.Table<GroupInfo>().Where(a =>
                    a.ConnectId == selectConnection.ID &&
                    a.DataBaseName == selectDataBase).OrderBy(x => x.OrderFlag).ToList();
                if (!currGroups.Any())
                {
                    NoDataAreaText.TipText = "暂无分组，请先创建分组";
                    NoDataText.Visibility = Visibility.Visible;
                    return;
                }
                foreach (var group in currGroups)
                {
                    var itemChildList = new List<TreeNodeItem>();
                    var nodeGroup = new TreeNodeItem
                    {
                        ObejcetId = "0",
                        DisplayName = group.GroupName,
                        Name = "treeTable",
                        Icon = GROUPICON,
                        Type = ObjType.Group,
                        IsExpanded = true,
                        FontWeight = "Bold",
                        Children = itemChildList,
                        IsShowCount = Visibility.Visible
                    };
                    var nodeTable1 = new TreeNodeItem
                    {
                        ObejcetId = "0",
                        DisplayName = "表",
                        Name = "treeTable",
                        Icon = TABLEICON,
                        Type = ObjType.Type,
                        IsExpanded = true,
                        Parent = nodeGroup,
                        IsShowCount = Visibility.Visible
                    };
                    itemChildList.Add(nodeTable1);
                    var nodeView1 = new TreeNodeItem
                    {
                        ObejcetId = "0",
                        DisplayName = "视图",
                        Name = "treeView",
                        Icon = VIEWICON,
                        Type = ObjType.Type,
                        IsExpanded = true,
                        Parent = nodeGroup,
                        IsShowCount = Visibility.Visible
                    };
                    itemChildList.Add(nodeView1);
                    var nodeProc1 = new TreeNodeItem
                    {
                        ObejcetId = "0",
                        DisplayName = "存储过程",
                        Name = "treeProc",
                        Icon = PROCICON,
                        Type = ObjType.Type,
                        IsExpanded = true,
                        Parent = nodeGroup,
                        IsShowCount = Visibility.Visible
                    };
                    itemChildList.Add(nodeProc1);
                    itemParentList.Add(nodeGroup);
                }
                currObjects = (from a in sqLiteHelper.db.Table<GroupInfo>()
                               join b in sqLiteHelper.db.Table<GroupObjects>() on a.Id equals b.GroupId
                               where a.ConnectId == selectConnection.ID &&
                                     a.DataBaseName == selectDataBase
                               select new SObjectDTO
                               {
                                   GroupName = a.GroupName,
                                   ObjectName = b.ObjectName
                               }).ToList();
            }
            #endregion

            #region 标签业务处理
            //是否业务标签
            if (leftMenuType == LeftMenuType.Tag.GetHashCode())
            {
                curTags = sqLiteHelper.db.Table<TagInfo>().Where(a =>
                    a.ConnectId == selectConnection.ID &&
                    a.DataBaseName == selectDataBase).ToList();
                if (!curTags.Any())
                {
                    NoDataAreaText.TipText = "暂无标签，请先创建标签";
                    NoDataText.Visibility = Visibility.Visible;
                    return;
                }
                foreach (var tag in curTags)
                {
                    var itemChildList = new List<TreeNodeItem>();
                    var nodeTag = new TreeNodeItem
                    {
                        ObejcetId = "0",
                        DisplayName = tag.TagName,
                        Name = "treeTable",
                        Icon = TAGICON,
                        Type = ObjType.Tag,
                        IsExpanded = true,
                        FontWeight = "Bold",
                        Children = itemChildList
                    };
                    var nodeTable1 = new TreeNodeItem
                    {
                        ObejcetId = "0",
                        DisplayName = "表",
                        Name = "treeTable",
                        Icon = TABLEICON,
                        Type = ObjType.Type,
                        IsExpanded = true,
                        Parent = nodeTag,
                        IsShowCount = Visibility.Visible
                    };
                    itemChildList.Add(nodeTable1);
                    var nodeView1 = new TreeNodeItem
                    {
                        ObejcetId = "0",
                        DisplayName = "视图",
                        Name = "treeView",
                        Icon = VIEWICON,
                        Type = ObjType.Type,
                        IsExpanded = true,
                        Parent = nodeTag,
                        IsShowCount = Visibility.Visible
                    };
                    itemChildList.Add(nodeView1);
                    var nodeProc1 = new TreeNodeItem
                    {
                        ObejcetId = "0",
                        DisplayName = "存储过程",
                        Name = "treeProc",
                        Icon = PROCICON,
                        Type = ObjType.Type,
                        IsExpanded = true,
                        Parent = nodeTag,
                        IsShowCount = Visibility.Visible
                    };
                    itemChildList.Add(nodeProc1);
                    nodeTag.Children = itemChildList;
                    itemParentList.Add(nodeTag);
                }
                curTagObjects = (from a in sqLiteHelper.db.Table<TagInfo>()
                                 join b in sqLiteHelper.db.Table<TagObjects>() on a.TagId equals b.TagId
                                 where a.ConnectId == selectConnection.ID &&
                                       a.DataBaseName == selectDataBase
                                 select new MenuTagObjectsDTO
                                 {
                                     TagName = a.TagName,
                                     ObjectName = b.ObjectName
                                 }).ToList();
            }
            #endregion

            #region 表
            if (MenuData.Tables != null)
            {
                foreach (var table in MenuData.Tables)
                {
                    var isStartWith = !table.Key.ToLower().StartsWith(searchText, true, null) &&
                                     !table.Value.Name.ToLower().StartsWith(searchText, true, null);
                    var isContains = !table.Key.ToLower().Contains(searchText) && !table.Value.Name.ToLower().Contains(searchText);
                    var isSearchMode = isLikeSearch ? isContains : isStartWith;
                    if (isSearchMode)
                    {
                        continue;
                    }
                    var isShowComment = !isContainsObjName
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                    var tableItem = new TreeNodeItem()
                    {
                        ObejcetId = table.Value.Id,
                        DisplayName = table.Value.DisplayName,
                        Name = table.Value.Name,
                        Schema = table.Value.SchemaName,
                        Comment = table.Value.Comment,
                        CreateDate = table.Value.CreateDate,
                        ModifyDate = table.Value.ModifyDate,
                        Icon = TABLEICON,
                        Type = ObjType.Table,
                        IsShowComment = isShowComment
                    };
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
                    {
                        #region MyRegion
                        var hasGroup = currObjects.Where(x => x.ObjectName == table.Key).
                                            GroupBy(x => x.GroupName).Select(x => x.Key)
                                            .ToList();
                        foreach (var group in hasGroup)
                        {
                            var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                            if (pGroup != null)
                            {
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == "表");
                                if (ppGroup != null)
                                {
                                    ppGroup.Children.Add(tableItem);
                                }
                            }
                        }
                        #endregion
                    }
                    else if (leftMenuType == LeftMenuType.Tag.GetHashCode())
                    {
                        #region MyRegion
                        var hasTag = curTagObjects
                                            .Where(x => x.ObjectName == table.Key)
                                            .GroupBy(x => x.TagName)
                                            .Select(x => x.Key)
                                            .ToList();
                        foreach (var tag in hasTag)
                        {
                            var pTag = itemParentList.FirstOrDefault(x => x.DisplayName == tag);
                            if (pTag != null)
                            {
                                var ppTag = pTag.Children.FirstOrDefault(x => x.DisplayName == "表");
                                if (ppTag != null)
                                {
                                    ppTag.Children.Add(tableItem);
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region MyRegion
                        nodeTable.Children.Add(tableItem);
                        #endregion
                    }
                }
            }
            #endregion

            #region 视图
            if (MenuData.Views != null)
            {
                foreach (var view in MenuData.Views)
                {
                    var isStartWith = !view.Key.ToLower().StartsWith(searchText, true, null) && !view.Value.Name.ToLower().StartsWith(searchText, true, null);
                    var isContains = !view.Key.ToLower().Contains(searchText) && !view.Value.Name.ToLower().Contains(searchText);
                    var isSearchMode = isLikeSearch ? isContains : isStartWith;
                    if (isSearchMode)
                    {
                        continue;
                    }

                    var isShowComment = !isContainsObjName
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                    var viewItem = new TreeNodeItem()
                    {
                        ObejcetId = view.Value.Id,
                        DisplayName = view.Value.DisplayName,
                        Name = view.Value.Name,
                        Schema = view.Value.SchemaName,
                        Comment = view.Value.Comment,
                        CreateDate = view.Value.CreateDate,
                        ModifyDate = view.Value.ModifyDate,
                        Icon = VIEWICON,
                        Type = ObjType.View,
                        IsShowComment = isShowComment
                    };
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
                    {
                        #region MyRegion
                        var hasGroup = currObjects.Where(x => x.ObjectName == view.Key).
                                            GroupBy(x => x.GroupName).Select(x => x.Key)
                                            .ToList();
                        foreach (var group in hasGroup)
                        {
                            var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                            if (pGroup != null)
                            {
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == "视图");
                                if (ppGroup != null)
                                {
                                    ppGroup.Children.Add(viewItem);
                                }
                            }
                        }
                        #endregion
                    }
                    else if (leftMenuType == LeftMenuType.Tag.GetHashCode())
                    {
                        #region MyRegion
                        var hasTag = curTagObjects
                                            .Where(x => x.ObjectName == view.Key)
                                            .GroupBy(x => x.TagName)
                                            .Select(x => x.Key)
                                            .ToList();
                        foreach (var tag in hasTag)
                        {
                            var pTag = itemParentList.FirstOrDefault(x => x.DisplayName == tag);
                            if (pTag != null)
                            {
                                var ppTag = pTag.Children.FirstOrDefault(x => x.DisplayName == "视图");
                                if (ppTag != null)
                                {
                                    ppTag.Children.Add(viewItem);
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region MyRegion
                        nodeView.Children.Add(viewItem);
                        #endregion
                    }
                }
            }
            #endregion

            #region 存储过程
            if (MenuData.Procedures != null)
            {
                foreach (var proc in MenuData.Procedures)
                {
                    var isStartWith = !proc.Key.ToLower().StartsWith(searchText, true, null) && !proc.Value.Name.ToLower().StartsWith(searchText, true, null);
                    var isContains = !proc.Key.ToLower().Contains(searchText) && !proc.Value.Name.ToLower().Contains(searchText);
                    var isSearchMode = isLikeSearch ? isContains : isStartWith;
                    if (isSearchMode)
                    {
                        continue;
                    }
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
                    {
                        #region MyRegion
                        var hasGroup = currObjects.Where(x => x.ObjectName == proc.Key).GroupBy(x => x.GroupName)
                                           .Select(x => x.Key)
                                           .ToList();
                        foreach (var group in hasGroup)
                        {
                            var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                            if (pGroup != null)
                            {
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == "存储过程");
                                if (ppGroup != null)
                                {
                                    ppGroup.Children.Add(new TreeNodeItem()
                                    {
                                        ObejcetId = proc.Value.Id,
                                        DisplayName = proc.Value.DisplayName,
                                        Name = proc.Value.Name,
                                        Schema = proc.Value.SchemaName,
                                        Comment = proc.Value.Comment,
                                        CreateDate = proc.Value.CreateDate,
                                        ModifyDate = proc.Value.ModifyDate,
                                        Icon = PROCICON,
                                        Type = ObjType.Proc
                                    });
                                }
                            }
                        }
                        #endregion
                    }
                    else if (leftMenuType == LeftMenuType.Tag.GetHashCode())
                    {
                        #region MyRegion
                        var hasTag = curTagObjects
                                            .Where(x => x.ObjectName == proc.Key)
                                            .GroupBy(x => x.TagName)
                                            .Select(x => x.Key)
                                            .ToList();
                        foreach (var tag in hasTag)
                        {
                            var pTag = itemParentList.FirstOrDefault(x => x.DisplayName == tag);
                            if (pTag != null)
                            {
                                var ppTag = pTag.Children.FirstOrDefault(x => x.DisplayName == "存储过程");
                                if (ppTag != null)
                                {
                                    ppTag.Children.Add(new TreeNodeItem()
                                    {
                                        ObejcetId = proc.Value.Id,
                                        DisplayName = proc.Value.DisplayName,
                                        Name = proc.Value.Name,
                                        Schema = proc.Value.SchemaName,
                                        Comment = proc.Value.Comment,
                                        CreateDate = proc.Value.CreateDate,
                                        ModifyDate = proc.Value.ModifyDate,
                                        Icon = PROCICON,
                                        Type = ObjType.Proc
                                    });
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region MyRegion
                        nodeProc.Children.Add(new TreeNodeItem()
                        {
                            ObejcetId = proc.Value.Id,
                            DisplayName = proc.Value.DisplayName,
                            Name = proc.Value.Name,
                            Schema = proc.Value.SchemaName,
                            Comment = proc.Value.Comment,
                            CreateDate = proc.Value.CreateDate,
                            ModifyDate = proc.Value.ModifyDate,
                            Icon = PROCICON,
                            Type = ObjType.Proc
                        });
                        #endregion
                    }
                }
            }
            #endregion

            if (leftMenuType == LeftMenuType.Group.GetHashCode() || leftMenuType == LeftMenuType.Tag.GetHashCode())
            {
                itemParentList.ForEach(treeItem =>
                {
                    if (!treeItem.Children.First(x => x.Name.Equals("treeTable")).Children.Any() && !treeItem.Children.First(x => x.Name.Equals("treeView")).Children.Any() && !treeItem.Children.First(x => x.Name.Equals("treeProc")).Children.Any())
                    {
                        treeItem.Visibility = nameof(Visibility.Collapsed);
                    }
                    treeItem.Children.ForEach(obj =>
                    {
                        if (!obj.Children.Any())
                        {
                            obj.Visibility = nameof(Visibility.Collapsed);
                        }
                        //obj.DisplayName = $"{obj.DisplayName}({obj.Children.Count})";
                        obj.ChildrenCount = obj.Children.Count;
                    });
                });
                if (itemParentList.All(x => x.Visibility != nameof(Visibility.Visible)))
                {
                    NoDataAreaText.TipText = "暂无数据";
                    NoDataText.Visibility = Visibility.Visible;
                    BtnNoData.Visibility = Visibility.Collapsed;
                }
                TreeViewData = itemParentList;
            }
            else
            {
                itemList.ForEach(obj =>
                {
                    if (!obj.Children.Any())
                    {
                        obj.Visibility = nameof(Visibility.Collapsed);
                    }
                    //obj.DisplayName = $"{obj.DisplayName}({obj.Children.Count})";
                    obj.ChildrenCount = obj.Children.Count;
                });
                if (itemList.All(x => x.Visibility != nameof(Visibility.Visible)))
                {
                    NoDataAreaText.TipText = "暂无数据";
                    NoDataText.Visibility = Visibility.Visible;
                    BtnNoData.Visibility = Visibility.Collapsed;
                }
                TreeViewData = itemList;
            }
            #endregion
        }

        /// <summary>
        /// 菜单类型变更事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabLeftType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded)
            {
                return;
            }
            var sqLiteHelper = new SQLiteHelper();
            var selectedItem = (TabItem)((TabControl)sender).SelectedItem;
            if (selectedItem.Name == "TabAllData")
            {
                sqLiteHelper.SetSysValue(SysConst.Sys_LeftMenuType, "1");
                //GroupCategory.Visibility = Visibility.Collapsed;
                //TreeViewTables.Margin = new Thickness(0, 103, 0, 0);
            }
            else if (selectedItem.Name == "TabGroupData")
            {
                sqLiteHelper.SetSysValue(SysConst.Sys_LeftMenuType, "2");
                //GroupCategory.Visibility = Visibility.Visible;
                //TreeViewTables.Margin = new Thickness(0, 135, 0, 0);
            }
            else
            {
                sqLiteHelper.SetSysValue(SysConst.Sys_LeftMenuType, "3");
            }
            if (SelectedConnection == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(SearchMenu.Text))
            {
                SearchMenuBind();
            }
            else
            {
                MenuBind();
            }
            #endregion
        }

        /// <summary>
        /// 选中表加载主内容对应数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedTable_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var selectDatabase = (DataBase)SelectDatabase.SelectedItem;
            if (!(TreeViewTables.SelectedItem is TreeNodeItem objects) || objects.Type == ObjType.Group)
            {
                return;
            }
            var sqLiteHelper = new SQLiteHelper();
            var isMultipleTab = sqLiteHelper.GetSysBool(SysConst.Sys_IsMultipleTab);
            if (!isMultipleTab)
            {
                if (TabItemData.Any())
                {
                    TabItemData.Clear();
                }
                CornerRadius = 10;
                MainW.Visibility = Visibility.Visible;
                MainTabW.Visibility = Visibility.Collapsed;
                MainW.ObjChangeRefreshEvent += ChangeRefreshMenuEvent;
                MainW.MenuData = MenuData;
                MainW.SelectedConnection = SelectedConnection;
                MainW.SelectedDataBase = selectDatabase;
                MainW.SelectedObject = objects;
                MainW.LoadPage(TreeViewData);
                return;
            }
            CornerRadius = 0;
            MainW.Visibility = Visibility.Collapsed;
            MainTabW.Visibility = Visibility.Visible;
            var curItem = TabItemData.FirstOrDefault(x => x.DisplayName == objects.DisplayName);
            if (curItem != null)
            {
                //MainTabW.SelectedItem = curItem;
                //return;
                TabItemData.Remove(curItem);
            }
            var mainW = new UcMainW
            {
                SelectedConnection = SelectedConnection,
                SelectedDataBase = selectDatabase,
                SelectedObject = objects,
                MenuData = MenuData
            };
            mainW.LoadPage(TreeViewData);
            var tabItem = new MainTabWModel
            {
                DisplayName = objects.DisplayName,
                Icon = IconDic[objects.Type],
                MainW = mainW
            };
            TabItemData.Insert(0, tabItem);
            MainTabW.SelectedItem = TabItemData.First();
            #endregion
        }

        /// <summary>
        /// 快捷创建分组、标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNoData_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var selectDatabase = (DataBase)SelectDatabase.SelectedItem;
            if (SelectedConnection == null || selectDatabase == null)
            {
                Oops.Oh("请选择数据库");
                return;
            }
            var mainWindow = Window.GetWindow(this);
            if (BtnNoData.Content.ToString() == "创建分组")
            {
                var group = new GroupsView();
                group.SelectedConnection = SelectedConnection;
                group.SelectedDataBase = selectDatabase.DbName;
                group.DbData = MenuData;
                group.Owner = mainWindow;
                group.ChangeRefreshEvent += ChangeRefreshMenuEvent;
                group.ShowDialog();
            }
            else
            {
                var tags = new TagsView();
                tags.SelectedConnection = SelectedConnection;
                tags.SelectedDataBase = selectDatabase.DbName;
                tags.DbData = MenuData;
                tags.Owner = mainWindow;
                tags.ChangeRefreshEvent += ChangeRefreshMenuEvent;
                tags.ShowDialog();
            }
            #endregion
        }

        /// <summary>
        /// 子窗体刷新左侧菜单
        /// </summary>
        public void ChangeRefreshMenuEvent()
        {
            if (TabGroupData.IsSelected || TabTagData.IsSelected)
            {
                MenuBind();
            }
        }

        /// <summary>
        /// 禁止水平滚动条自动滚动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventSetter_OnHandler(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 复制名称
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuCopyName_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(TreeViewTables.SelectedItem is TreeNodeItem selectedObjects) || selectedObjects.ObejcetId == "0")
            {
                return;
            }
            Clipboard.SetDataObject(selectedObjects.Name);
            Oops.Success("复制成功");
        }

        /// <summary>
        /// 设置标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuSetTag_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (!(TreeViewTables.SelectedItem is TreeNodeItem selectedObject) || selectedObject.ObejcetId == "0" || selectedObject.TextColor.Equals("Red"))
            {
                Oops.Oh("请选择对应的表");
                return;
            }
            var selectDatabase = (DataBase)SelectDatabase.SelectedItem;
            var sqLiteHelper = new SQLiteHelper();
            var list = sqLiteHelper.db.Table<TagInfo>().Where(x =>
                x.ConnectId == SelectedConnection.ID && x.DataBaseName == selectDatabase.DbName).ToList();
            if (!list.Any())
            {
                Oops.Oh("暂无标签，请先创建标签");
                return;
            }
            var mainWindow = Window.GetWindow(this);
            var setTag = new SetTag();
            //setTag.ObjChangeRefreshEvent += ObjChangeRefreshEvent;
            setTag.SelectedConnection = SelectedConnection;
            setTag.SelectedDataBase = selectDatabase.DbName;
            setTag.SelectedObjects = new List<TreeNodeItem>() { selectedObject };
            setTag.Owner = mainWindow;
            setTag.ShowDialog();
            #endregion
        }

        /// <summary>
        /// 设置分组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuSetGroup_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (!(TreeViewTables.SelectedItem is TreeNodeItem selectedObject) || selectedObject.ObejcetId == "0")
            {
                Oops.Oh("请选择对应的表");
                return;
            }
            var selectDatabase = (DataBase)SelectDatabase.SelectedItem;
            var sqLiteHelper = new SQLiteHelper();
            var list = sqLiteHelper.db.Table<GroupInfo>().Where(x =>
                x.ConnectId == SelectedConnection.ID && x.DataBaseName == selectDatabase.DbName).ToList();
            if (!list.Any())
            {
                Oops.Oh("暂无分组，请先创建分组");
                return;
            }
            var mainWindow = Window.GetWindow(this);
            var group = new SetGroup();
            //group.ObjChangeRefreshEvent += ObjChangeRefreshEvent;
            group.SelectedConnection = SelectedConnection;
            group.SelectedDataBase = selectDatabase.DbName;
            group.SelectedObjects = new List<TreeNodeItem>() { selectedObject };
            group.Owner = mainWindow;
            group.ShowDialog();
            #endregion
        }

        /// <summary>
        /// 导出文档
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuExportDoc_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (!(TreeViewTables.SelectedItem is TreeNodeItem selectedObject))
            {
                return;
            }
            var exportData = selectedObject.Children.Any()
                ? selectedObject.Children
                : new List<TreeNodeItem> { selectedObject };
            if (selectedObject.Type == ObjType.Group || selectedObject.Type == ObjType.Tag)
            {
                if (selectedObject.ChildrenCount < 1)
                {
                    var textName = selectedObject.Type == ObjType.Group ? "分组" : "标签";
                    Oops.Oh($"该{textName}无数据，无法导出");
                    return;
                }
                exportData = new List<TreeNodeItem>();
                selectedObject.Children.ForEach(c =>
                {
                    c.Children.ForEach(cz =>
                    {
                        exportData.Add(cz);
                    });
                });
            }
            var selectDatabase = (DataBase)SelectDatabase.SelectedItem;
            var mainWindow = Window.GetWindow(this);
            var exportDoc = new ExportDoc();
            exportDoc.Owner = mainWindow;
            exportDoc.MenuData = MenuData;
            exportDoc.SelectedConnection = SelectedConnection;
            exportDoc.SelectedDataBase = selectDatabase;
            exportDoc.ExportData = exportData;
            exportDoc.ShowDialog();
            #endregion
        }

        /// <summary>
        /// 生成SQL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuCreateSQL_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (!(TreeViewTables.SelectedItem is TreeNodeItem selectedObject) || selectedObject.ObejcetId == "0")
            {
                Oops.Oh("请选择对应的表");
                return;
            }
            var selectDatabase = (DataBase)SelectDatabase.SelectedItem;
            var dbInstance = ExporterFactory.CreateInstance(SelectedConnection.DbType, SelectedConnection.SelectedDbConnectString(selectDatabase.DbName), selectDatabase.DbName);
            var tableColumns = dbInstance.GetColumnInfoById(selectedObject.ObejcetId);
            var list = tableColumns.Values.ToList();
            var mainWindow = Window.GetWindow(this);
            var scriptW = new ScriptWindow();
            scriptW.SelectedConnection = SelectedConnection;
            scriptW.SelectedObject = selectedObject;
            scriptW.SelectedColumns = list;
            scriptW.Owner = mainWindow;
            scriptW.ShowDialog();
            #endregion
        }

        /// <summary>
        /// 生成代码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuGenCode_Click(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (!(TreeViewTables.SelectedItem is TreeNodeItem selectedObject))
            {
                return;
            }
            var exportData = selectedObject.Children.Any()
                ? selectedObject.Children
                : new List<TreeNodeItem> { selectedObject };
            if (selectedObject.Type == ObjType.Group || selectedObject.Type == ObjType.Tag)
            {
                if (selectedObject.ChildrenCount < 1)
                {
                    var textName = selectedObject.Type == ObjType.Group ? "分组" : "标签";
                    Oops.Oh($"该{textName}无数据，无法导出");
                    return;
                }
                exportData = new List<TreeNodeItem>();
                selectedObject.Children.ForEach(c =>
                {
                    c.Children.ForEach(cz =>
                    {
                        exportData.Add(cz);
                    });
                });
            }
            var selectDatabase = (DataBase)SelectDatabase.SelectedItem;
            var mainWindow = Window.GetWindow(this);
            var gc = new GenCode();
            gc.Owner = mainWindow;
            gc.MenuData = MenuData;
            gc.SelectedConnection = SelectedConnection;
            gc.SelectedDataBase = selectDatabase;
            gc.ExportData = exportData;
            gc.ShowDialog();
            #endregion
        }

        private void MainTabW_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded)
            {
                return;
            }
            if (e.Source is HandyControl.Controls.TabControl)
            {
                MainTabW.ShowCloseButton = MainTabW.Items.Count > 1;
                MainTabW.ShowContextMenu = MainTabW.Items.Count > 1;
            }
            #endregion
        }

        /// <summary>
        /// 右键菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventSetter_OnHandler(object sender, MouseButtonEventArgs e)
        {
            #region MyRegion
            if (VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) is TreeViewItem treeViewItem)
            {
                var selectedNode = treeViewItem.Header as TreeNodeItem;
                if (selectedNode == null)
                {
                    return;
                }
                if (selectedNode.ObejcetId == "0")
                {
                    MenuCopyName.Visibility = Visibility.Collapsed;
                    MenuSetGroup.Visibility = Visibility.Collapsed;
                    MenuSetTag.Visibility = Visibility.Collapsed;
                    MenuCreateSQL.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MenuCopyName.Visibility = Visibility.Visible;
                    MenuSetGroup.Visibility = Visibility.Visible;
                    MenuSetTag.Visibility = Visibility.Visible;
                    MenuCreateSQL.Visibility = selectedNode.Type == ObjType.Proc ? Visibility.Collapsed : Visibility.Visible;
                    MenuGenCode.Visibility = selectedNode.Type == ObjType.Proc ? Visibility.Collapsed : Visibility.Visible;
                }
                treeViewItem.Focus();
                e.Handled = true;
            }
            #endregion
        }

        private DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source;
        }
    }
}
